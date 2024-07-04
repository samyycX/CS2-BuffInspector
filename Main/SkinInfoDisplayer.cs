using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Localization;

namespace BuffInspector;

public class SkinInfoDisplayer {
    private IStringLocalizer Localizer;

    private Dictionary<ulong, string> CenterImages = new Dictionary<ulong, string>();

    public void OnTick() {
        foreach (var (steamid, imgurl) in CenterImages) {
            var player = Utilities.GetPlayerFromSteamId(steamid);
            if (player != null && imgurl != null && player.IsValid && player.Pawn.IsValid && player.PlayerPawn.IsValid) {
                player.PrintToCenterHtml($"<img src=\"{imgurl}\" width=100 height=100></img>", 10);
            }
        }   
    }

    public SkinInfoDisplayer(IStringLocalizer Localizer) {
        this.Localizer = Localizer;
    }

    public void ShowSkinInfoToPlayer(CCSPlayerController player, SkinInfo? skinInfo, bool EnableImagePreview = true, float ImagePreviewTime = 5f, bool WillAutoUpdate = false) {
    
        if (skinInfo == null) { 
            player.PrintToChat(Localizer["failed"]);
            return;
        }
        
        var steamid = player.AuthorizedSteamID!.SteamId64;
        Server.NextFrame(() => {
            player.PrintToChat(Localizer["success"]);
            player.PrintToChat(Localizer["hint.name", skinInfo.title]);
            if (skinInfo.nametag != null) {
                player.PrintToChat(Localizer["hint.nametag", skinInfo.nametag]);
            }
            player.PrintToChat(Localizer["hint.seed", skinInfo.PaintSeed]);
            player.PrintToChat(Localizer["hint.index", skinInfo.PaintIndex]);
            player.PrintToChat(Localizer["hint.wear", skinInfo.PaintWear]);
            foreach (var sticker in skinInfo.Stickers) {  
                player.PrintToChat(Localizer["hint.sticker", sticker.Slot, sticker.Name]);
            }
            if (!WillAutoUpdate) {
                player.PrintToChat(Localizer["hint.update"]);
            }
            if (CenterImages.ContainsKey(steamid)) {
                CenterImages.Remove(steamid);
            }
            if (EnableImagePreview) {
                CenterImages.Add(steamid, skinInfo.img);
                BuffInspector.INSTANCE.AddTimer(ImagePreviewTime, () => {
                    CenterImages.Remove(steamid);
                });
            }
            
        });
    }
}