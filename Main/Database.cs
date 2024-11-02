using System.Text.Json.Serialization;
using Dapper;
using MySqlConnector;
using Newtonsoft.Json;

namespace BuffInspector;

class DatabaseInfo
{
    [JsonProperty("DatabaseHost")] public string host;
    [JsonProperty("DatabasePort")] public int port;
    [JsonProperty("DatabaseUser")] public string user;
    [JsonProperty("DatabasePassword")] public string password;
    [JsonProperty("DatabaseName")] public string database;
}


class DatabaseConnection
{
    private MySqlConnection conn;
    public DatabaseConnection(DatabaseInfo info)
    {
        string connectStr = $"server={info.host};port={info.port};user={info.user};password={info.password};database={info.database};Pooling=true;MinimumPoolSize=0;MaximumPoolsize=640;ConnectionIdleTimeout=30;";
        conn = new MySqlConnection(connectStr);
        conn.Open();
    }

    public async Task SetSkinInfo(ulong steamid, SkinInfo skinInfo)
    {
        var sql = "";
        var select = "SELECT 1 FROM wp_player_skins WHERE steamid=@steamid AND weapon_defindex=@DefIndex;";
        var r = conn.Query(select, new
        {
            steamid,
            skinInfo.DefIndex
        });

        var sticker0 = skinInfo.Stickers.Find(s => s.Slot == 0)?.ToWeaponPaintsDatabaseString() ?? "0;0;0;0;0;1;0";
        var sticker1 = skinInfo.Stickers.Find(s => s.Slot == 1)?.ToWeaponPaintsDatabaseString() ?? "0;0;0;0;0;1;0";
        var sticker2 = skinInfo.Stickers.Find(s => s.Slot == 2)?.ToWeaponPaintsDatabaseString() ?? "0;0;0;0;0;1;0";
        var sticker3 = skinInfo.Stickers.Find(s => s.Slot == 3)?.ToWeaponPaintsDatabaseString() ?? "0;0;0;0;0;1;0";
        var sticker4 = skinInfo.Stickers.Find(s => s.Slot == 4)?.ToWeaponPaintsDatabaseString() ?? "0;0;0;0;0;1;0";

        if (r.Count() > 0)
        {
            sql = $"""
                UPDATE wp_player_skins SET weapon_paint_id=@PaintIndex, weapon_wear=@PaintWear, weapon_seed=@PaintSeed, weapon_nametag=@Nametag, weapon_sticker_0=@sticker0, weapon_sticker_1=@sticker1, weapon_sticker_2=@sticker2, weapon_sticker_3=@sticker3, weapon_sticker_4=@sticker4 WHERE steamid=@steamid AND weapon_defindex=@DefIndex;
            """;
        }
        else
        {
            sql = $"""
                INSERT INTO wp_player_skins (weapon_team, steamid, weapon_defindex, weapon_paint_id, weapon_wear, weapon_seed, weapon_nametag, weapon_sticker_0, weapon_sticker_1, weapon_sticker_2, weapon_sticker_3, weapon_sticker_4) VALUES (0, @steamid, @DefIndex, @PaintIndex, @PaintWear, @PaintSeed, @Nametag, @sticker0, @sticker1, @sticker2, @sticker3, @sticker4)
            """;
        }

        await Task.Run(async () =>
        {
            await conn.ExecuteAsync(sql,
                new
                {
                    steamid,
                    skinInfo.PaintIndex,
                    skinInfo.PaintSeed,
                    skinInfo.PaintWear,
                    skinInfo.DefIndex,
                    skinInfo.Nametag,
                    sticker0,
                    sticker1,
                    sticker2,
                    sticker3,
                    sticker4
                }
            );
        });
    }

    public async Task SetKnifeInfo(ulong steamid, string knife)
    {
        var sql = "";
        var select = "SELECT 1 FROM wp_player_knife WHERE steamid=@steamid";
        var r = conn.Query(select, new
        {
            steamid
        });
        if (r.Count() > 0)
        {
            sql = $"""
                UPDATE wp_player_knife SET knife=@knife WHERE steamid=@steamid
            """;
        }
        else
        {
            sql = $"""
                INSERT INTO wp_player_knife (weapon_team, steamid, knife) VALUES (0, @steamid, @knife)
            """;
        }

        await Task.Run(async () =>
        {
            await conn.ExecuteAsync(sql,
                new
                {
                    steamid,
                    knife
                }
            );
        });
    }

    public async Task SetGloveInfo(ulong steamid, int defindex)
    {
        var sql = "";
        var select = "SELECT 1 FROM wp_player_gloves WHERE steamid=@steamid";
        var r = conn.Query(select, new
        {
            steamid
        });
        if (r.Count() > 0)
        {
            sql = $"""
                UPDATE wp_player_gloves SET weapon_defindex=@defindex WHERE steamid=@steamid;
            """;
        }
        else
        {
            sql = $"""
                INSERT INTO wp_player_gloves (weapon_team, steamid, weapon_defindex) VALUES (0, @steamid, @defindex)
            """;
        }

        await Task.Run(async () =>
        {
            await conn.ExecuteAsync(sql,
                new
                {
                    steamid,
                    defindex
                }
            );
        });
    }
}