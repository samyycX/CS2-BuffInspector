using System.Text.Json.Serialization;
using Dapper;
using MySqlConnector;
using Newtonsoft.Json;

namespace BuffInspector;

class DatabaseInfo {
    [JsonProperty("DatabaseHost")] public string host;
    [JsonProperty("DatabasePort")] public int port;
    [JsonProperty("DatabaseUser")] public string user;
    [JsonProperty("DatabasePassword")] public string password;
    [JsonProperty("DatabaseName")] public string database;
}


class DatabaseConnection {
    private MySqlConnection conn;
    public DatabaseConnection(DatabaseInfo info) {
        string connectStr = $"server={info.host};port={info.port};user={info.user};password={info.password};database={info.database};Pooling=true;MinimumPoolSize=0;MaximumPoolsize=640;ConnectionIdleTimeout=30;";
        conn = new MySqlConnection(connectStr);
        conn.Open();
    }

    public async Task SetSkinInfo(ulong steamid, SkinInfo skinInfo) {
        var sql = "";
        var select = "SELECT 1 FROM wp_player_skins WHERE steamid=@steamid AND weapon_defindex=@DefIndex;";
        var r = conn.Query(select, new {
            steamid, 
            skinInfo.DefIndex
        });
        if (r.Count() > 0) {
            sql =  $"""
                UPDATE wp_player_skins SET weapon_paint_id=@PaintIndex, weapon_wear=@PaintWear, weapon_seed=@PaintSeed WHERE steamid=@steamid AND weapon_defindex=@DefIndex;
            """;
        } else {
            sql = $"""
                INSERT INTO wp_player_skins (steamid, weapon_defindex, weapon_paint_id, weapon_wear, weapon_seed) VALUES (@steamid, @DefIndex, @PaintIndex, @PaintWear, @PaintSeed)
            """;
        }

        await Task.Run(async () => {
            await conn.ExecuteAsync(sql, 
                new {
                    steamid,
                    skinInfo.PaintIndex,
                    skinInfo.PaintSeed,
                    skinInfo.PaintWear,
                    skinInfo.DefIndex
                }
            );
        });
    }

    public async Task SetKnifeInfo(ulong steamid, string knife) {
        var sql = "";
        var select = "SELECT 1 FROM wp_player_knife WHERE steamid=@steamid";
        var r = conn.Query(select, new {
            steamid
        });
        if (r.Count() > 0) {
            sql =  $"""
                UPDATE wp_player_knife SET knife=@knife WHERE steamid=@steamid
            """;
        } else {
            sql = $"""
                INSERT INTO wp_player_knife (steamid, knife) VALUES (@steamid, @knife)
            """;
        }

        await Task.Run(async () => {
            await conn.ExecuteAsync(sql, 
                new {
                    steamid,
                    knife
                }
            );
        });
    }

    public async Task SetGloveInfo(ulong steamid, int defindex) {
        var sql = "";
        var select = "SELECT 1 FROM wp_player_gloves WHERE steamid=@steamid";
        var r = conn.Query(select, new {
            steamid
        });
        if (r.Count() > 0) {
            sql =  $"""
                UPDATE wp_player_gloves SET weapon_defindex=@defindex WHERE steamid=@steamid;
            """;
        } else {
            sql = $"""
                INSERT INTO wp_player_gloves (steamid, weapon_defindex) VALUES (@steamid, @defindex)
            """;
        }

        await Task.Run(async () => {
            await conn.ExecuteAsync(sql, 
                new {
                    steamid,
                    defindex
                }
            );
        });
    }
}