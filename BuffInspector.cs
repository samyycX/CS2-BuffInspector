using System.Collections;
using System.Reflection;
using System.Security.Cryptography;
using System.Timers;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Commands;
using CounterStrikeSharp.API.Core.Plugin.Host;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;
using Newtonsoft.Json;

namespace BuffInspector;

public class Config : BasePluginConfig {
    public bool UseSync {get; set;} = false;
    public bool EnableImagePreview {get; set;} = true;
    public float ImagePreviewTime {get; set;} = 5f;
    public bool EnableSticker {get; set;} = true;
}
public partial class BuffInspector : BasePlugin, IPluginConfig<Config>
{


    public override string ModuleName => "Buff Inspector";
    public override string ModuleVersion => "3";
    public override string ModuleAuthor => "samyyc";
    private DatabaseConnection Database;
    public Config Config {get; set;}

    public static Dictionary<int,string> KnifeDefIndexToName { get; } = new Dictionary<int, string>
	{
		{ 500, "weapon_bayonet" },
		{ 503, "weapon_knife_css" },
		{ 505, "weapon_knife_flip" },
		{ 506, "weapon_knife_gut" },
		{ 507, "weapon_knife_karambit" },
		{ 508, "weapon_knife_m9_bayonet" },
		{ 509, "weapon_knife_tactical" },
		{ 512, "weapon_knife_falchion" },
		{ 514, "weapon_knife_survival_bowie" },
		{ 515, "weapon_knife_butterfly" },
		{ 516, "weapon_knife_push" },
		{ 517, "weapon_knife_cord" },
		{ 518, "weapon_knife_canis" },
		{ 519, "weapon_knife_ursus" },
		{ 520, "weapon_knife_gypsy_jackknife" },
		{ 521, "weapon_knife_outdoor" },
		{ 522, "weapon_knife_stiletto" },
		{ 523, "weapon_knife_widowmaker" },
		{ 525, "weapon_knife_skeleton" },
		{ 526, "weapon_knife_kukri" }
	};

    
    public Dictionary<ulong, string> CenterImages = new Dictionary<ulong, string>();
    private List<PlayerSticker> TempStickers = new List<PlayerSticker>();
    private MemoryFunctionVoid<nint, string, float> CAttributeList_SetOrAddAttributeValueByName = new(GameData.GetSignature("CAttributeList_SetOrAddAttributeValueByName"));
    
    public void OnConfigParsed(Config config) {
        Config = config;
    }

    public override void Load(bool hotReload)
    {
        var CS2WeaponPaintsConfigPath = Path.Join(ModuleDirectory, "../../configs/plugins/WeaponPaints/WeaponPaints.json");
        if (!File.Exists(CS2WeaponPaintsConfigPath)) {
            throw new FileNotFoundException($"Weapon Paints Config {CS2WeaponPaintsConfigPath} Not Found.");
        }

        StreamReader reader = File.OpenText(CS2WeaponPaintsConfigPath);
        string content = reader.ReadToEnd();
        DatabaseInfo info = JsonConvert.DeserializeObject<DatabaseInfo>(content)!;

        Database = new DatabaseConnection(info);

        RegisterListener<Listeners.OnTick>(OnTick);
        RegisterListener<Listeners.OnEntityCreated>(OnEntityCreated);
        
        Console.WriteLine("Buff Inspector Loaded.");
    }

    public override void Unload(bool hotReload)
    {
        RemoveListener<Listeners.OnEntityCreated>(OnEntityCreated);
        RemoveListener<Listeners.OnTick>(OnTick);
    }

    private void OnTick() {
         if (!Config.EnableImagePreview) {
            return;
        }
        foreach (var (steamid, imgurl) in CenterImages) {
            var player = Utilities.GetPlayerFromSteamId(steamid);
            if (player != null && imgurl != null && player.IsValid && player.Pawn.IsValid && player.PlayerPawn.IsValid) {
                player.PrintToCenterHtml($"<img src=\"{imgurl}\" width=100 height=100></img>", 10);
            }
        }   
    }

    private float ViewAsFloat(uint value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return BitConverter.ToSingle(bytes, 0);
    }

    private void OnEntityCreated(CEntityInstance entity) {
        if (!entity.IsValid) {
            return;
        }
        if (!entity.DesignerName.Contains("weapon")) {
            return;
        }
        Server.NextFrame(() => {
            var weapon = new CBasePlayerWeapon(entity.Handle);
            if (!weapon.IsValid || weapon.OriginalOwnerXuidLow == 0) return;

            var player = Utilities.GetPlayerFromSteamId((ulong)weapon.OriginalOwnerXuidLow);
            if (player == null || !player.IsValid || player.Pawn == null || !player.Pawn.IsValid || player.PlayerPawn == null || !player.PlayerPawn.IsValid || player.IsBot) {
                return;
            }

            var stickersData = TempStickers.Where(d => d.Steamid == player.SteamID && d.DefIndex == weapon.AttributeManager.Item.ItemDefinitionIndex);
            if (stickersData.Count() == 0) {
                return;
            }
            var stickers = stickersData.First();
            foreach (var sticker in stickers.Stickers) {
                CAttributeList_SetOrAddAttributeValueByName.Invoke(weapon.AttributeManager.Item.NetworkedDynamicAttributes.Handle, $"sticker slot {sticker.Slot} id", ViewAsFloat((uint) sticker.Id));
                CAttributeList_SetOrAddAttributeValueByName.Invoke(weapon.AttributeManager.Item.NetworkedDynamicAttributes.Handle, $"sticker slot {sticker.Slot} rotation", 0f);
                CAttributeList_SetOrAddAttributeValueByName.Invoke(weapon.AttributeManager.Item.NetworkedDynamicAttributes.Handle, $"sticker slot {sticker.Slot} offset x", sticker.OffsetX);
                CAttributeList_SetOrAddAttributeValueByName.Invoke(weapon.AttributeManager.Item.NetworkedDynamicAttributes.Handle, $"sticker slot {sticker.Slot} offset y", sticker.OffsetY);
                CAttributeList_SetOrAddAttributeValueByName.Invoke(weapon.AttributeManager.Item.NetworkedDynamicAttributes.Handle, $"sticker slot {sticker.Slot} wear", sticker.Wear);
            }

            TempStickers.RemoveAll(d => d.Steamid == player.SteamID && d.DefIndex == weapon.AttributeManager.Item.ItemDefinitionIndex);
           
        });
    }

    [ConsoleCommand("css_buff")]
    [CommandHelper(minArgs: 1, usage: "[buff分享链接]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public async void OnBuffCommand(CCSPlayerController player, CommandInfo commandInfo) {
        var splited = commandInfo.GetCommandString.Split(" ");
        splited = splited.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        if (splited.Length < 2) {
            player.PrintToChat(Localizer["failed"]);
            return;
        }
        string url = splited[1];
        player.PrintToChat(Localizer["parsing"]);

        var steamid = player.AuthorizedSteamID!.SteamId64;

        if (!Config.UseSync) { 
            var task = new Task(async() => {
                SkinInfo? skinInfo = await scrapeUrl(url);

                if (skinInfo == null) { 
                    Server.NextFrame(() => player.PrintToChat(Localizer["failed"]));
                    return;
                }

                if (KnifeDefIndexToName.ContainsKey(skinInfo.DefIndex)) {
                    await Database.SetKnifeInfo(steamid, KnifeDefIndexToName[skinInfo.DefIndex]);
                }

                // gloves
                if (skinInfo.DefIndex > 600) {
                    await Database.SetGloveInfo(steamid, skinInfo.DefIndex);
                }

                await Database.SetSkinInfo(steamid, skinInfo);

                if (skinInfo.Stickers.Count() > 0) {
                    TempStickers.Add(new( steamid, skinInfo.DefIndex, skinInfo.Stickers ));
                }
                
                await Server.NextFrameAsync(() => {
                    player.PrintToChat(Localizer["success"]);
                    player.PrintToChat(Localizer["hint.name", skinInfo.title]);
                    player.PrintToChat(Localizer["hint.seed", skinInfo.PaintSeed]);
                    player.PrintToChat(Localizer["hint.index", skinInfo.PaintIndex]);
                    player.PrintToChat(Localizer["hint.wear", skinInfo.PaintWear]);
                    foreach (var sticker in skinInfo.Stickers) {  
                        player.PrintToChat(Localizer["hint.sticker", sticker.Slot, sticker.Name]);
                    }
                    player.PrintToChat(Localizer["hint.update"]);
                    if (CenterImages.ContainsKey(steamid)) {
                        CenterImages.Remove(steamid);
                    }
                    CenterImages.Add(steamid, skinInfo.img);
                    AddTimer(Config.ImagePreviewTime, () => {
                        CenterImages.Remove(steamid);
                    });
                });
                    
            });
            task.Start();
        } else {
            SkinInfo? skinInfo = scrapeUrl(url).Result;

            if (skinInfo == null) { 
                player.PrintToChat(Localizer["failed"]);
                return;
            }

            if (KnifeDefIndexToName.ContainsKey(skinInfo.DefIndex)) {
                Database.SetKnifeInfo(steamid, KnifeDefIndexToName[skinInfo.DefIndex]).Wait();
            }

            // gloves
            if (skinInfo.DefIndex > 600) {
                Database.SetGloveInfo(steamid, skinInfo.DefIndex).Wait();
            }

            Database.SetSkinInfo(steamid, skinInfo).Wait();

            if (skinInfo.Stickers.Count() > 0) {
                TempStickers.Add(new( steamid, skinInfo.DefIndex, skinInfo.Stickers ));
            }
            
            player.PrintToChat(Localizer["success"]);
            player.PrintToChat(Localizer["hint.name", skinInfo.title]);
            player.PrintToChat(Localizer["hint.seed", skinInfo.PaintSeed]);
            player.PrintToChat(Localizer["hint.index", skinInfo.PaintIndex]);
            player.PrintToChat(Localizer["hint.wear", skinInfo.PaintWear]);
            foreach (var sticker in skinInfo.Stickers) {  
                player.PrintToChat(Localizer["hint.sticker", sticker.Slot, sticker.Name]);
            }
            if (CenterImages.ContainsKey(steamid)) {
                CenterImages.Remove(steamid);
            }
            CenterImages.Add(steamid, skinInfo.img);
            AddTimer(Config.ImagePreviewTime, () => {
                CenterImages.Remove(steamid);
            });
        
            player.ExecuteClientCommandFromServer("css_wp");
        }
    }
}

public class PlayerSticker {
    public ulong Steamid {get; set;}
    public int DefIndex {get; set;}
    public List<Sticker> Stickers {get; set;}

    public PlayerSticker(ulong steamid, int defIndex, List<Sticker> stickers) {
        this.Steamid = steamid;
        this.DefIndex = defIndex;
        this.Stickers = stickers;
    }
}
