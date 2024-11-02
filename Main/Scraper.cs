using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace BuffInspector;

public class Scraper
{

    private static Dictionary<string, int> ItemNameToDefIndex = new Dictionary<string, int>() {
        { "沙漠之鹰", 1 },
        { "双持贝瑞塔", 2 },
        { "FN57", 3 },
        { "格洛克 18 型", 4 },
        { "AK-47", 7 },
        { "AUG", 8 },
        { "AWP", 9 },
        { "法玛斯", 10 },
        { "G3SG1", 11 },
        { "加利尔 AR", 13 },
        { "M249", 14 },
        { "M4A4", 16 },
        { "MAC-10", 17 },
        { "P90", 19 },
        { "MP5-SD", 23 },
        { "UMP-45", 24 },
        { "XM1014", 25 },
        { "PP-野牛", 26 },
        { "MAG-7", 27 },
        { "内格夫", 28 },
        { "截短霰弹枪", 29 },
        { "Tec-9", 30 },
        { "P2000", 32 },
        { "MP7", 33 },
        { "MP9", 34 },
        { "新星", 35 },
        { "P250", 36 },
        { "SCAR-20", 38 },
        { "SG 553", 39 },
        { "SSG 08", 40 },
        { "M4A1 消音型", 60 },
        { "USP 消音版", 61 },
        { "CZ75 自动手枪", 63 },
        { "R8 左轮手枪", 64 },
        { "刺刀", 500 },
        { "海豹短刀", 503 },
        { "折叠刀", 505 },
        { "穿肠刀", 506 },
        { "爪子刀", 507 },
        { "M9 刺刀", 508},
        { "猎杀者匕首", 509 },
        { "弯刀", 512 },
        { "鲍伊猎刀", 514 },
        { "蝴蝶刀", 515 },
        { "暗影双匕", 516 },
        { "系绳匕首", 517 },
        { "求生匕首", 518 },
        { "熊刀", 519 },
        { "折刀", 520 },
        { "流浪者匕首", 521 },
        { "短剑", 522 },
        { "锯齿爪刀", 523 },
        { "骷髅匕首", 525 },
        { "廓尔喀刀", 526 },
        { "狂牙手套", 4725 },
        { "血猎手套", 5027 },
        { "运动手套", 5030 },
        { "驾驶手套", 5031 },
        { "手部束带", 5032 },
        { "摩托手套", 5033 },
        { "专业手套", 5034 },
        { "九头蛇手套", 5035 }
    };

    private static string? ExtractFirstNumber(string input)
    {
        var result = Regex.Matches(input, "(\\d+)");
        return result.First()?.Value;
    }
    public async static Task<SkinInfo?> scrapeUrl(string url)
    {
        url = url.Replace("https://", "").Replace("http://", "");

        if (!url.StartsWith("buff.163.com"))
        {
            return null;
        }
        url = url.Replace("buff.163.com", "");

        var client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0");
        client.BaseAddress = new Uri("https://buff.163.com");
        var resp = await client.GetAsync(url);

        // short link redirect
        if (resp.StatusCode == System.Net.HttpStatusCode.Found)
        {
            url = resp.Headers.GetValues("location").First();
        }
        else if (resp.StatusCode != System.Net.HttpStatusCode.OK)
        {
            return null;
        }

        url = Regex.Replace(url, "/goods/\\d+", "/market/item_detail");
        url = url.Replace("https://", "").Replace("http://", "").Replace("buff.163.com", "");
        resp = await client.GetAsync(url);

        if (!resp.IsSuccessStatusCode)
        {
            return null;
        }

        var html = await resp.Content.ReadAsStringAsync();
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        var title = htmlDoc.DocumentNode.SelectSingleNode("//h3")?.InnerText;
        if (title == null)
        {
            return null;
        }
        string? nametag = htmlDoc.DocumentNode.SelectSingleNode("//p[@class='name_tag']")?.InnerText;
        if (nametag != null)
        {
            nametag = Regex.Matches(nametag, @"(?<=“)(.*)(?=”)").FirstOrDefault()?.Value;
        }
        var img = htmlDoc.DocumentNode.SelectSingleNode("//img")?.GetAttributeValue("src", "");
        var informations = htmlDoc.DocumentNode.SelectNodes("//div[@class='info-card'][1]/p/span");
        // informations为Null则该链接不为枪械、刀具或手套皮肤
        if (informations == null || informations.Count() < 3)
        {
            return null;
        }
        string? paintSeed = ExtractFirstNumber(informations[0].InnerText);
        string? paintIndex = ExtractFirstNumber(informations[1].InnerText);
        string paintWear = Regex.Replace(informations[2].InnerText, @"[^\d+\.?\d*]+", "");
        if (paintIndex == null || paintSeed == null || paintWear == null)
        {
            return null;
        }
        int DefIndex = -1;
        foreach (var item in ItemNameToDefIndex)
        {
            if (title.StartsWith(item.Key))
            {
                DefIndex = item.Value;
                break;
            }
        }
        if (DefIndex == -1)
        {
            return null;
        }

        SkinInfo skinInfo = null;
        try
        {
            int IntPaintSeed = int.Parse(paintSeed);
            int IntPaintIndex = int.Parse(paintIndex);
            float FloatPaintWear = float.Parse(paintWear);

            skinInfo = new SkinInfo(title, img, nametag, DefIndex, IntPaintIndex, IntPaintSeed, FloatPaintWear);
        }
        catch (Exception _)
        {
            Console.WriteLine("Error when parsing: " + url);
            return null;
        }

        // sticker
        var itemDescDetailUrl = url.Replace("/market/item_detail", "/api/market/item_desc_detail");
        var itemDescDetailResp = await client.GetAsync(itemDescDetailUrl);
        if (itemDescDetailResp.IsSuccessStatusCode)
        {
            var itemDescDetailJson = await itemDescDetailResp.Content.ReadAsStringAsync();
            var itemDescDetail = JObject.Parse(itemDescDetailJson) as dynamic;
            if (itemDescDetail != null && itemDescDetail!.code == "OK")
            {
                try
                {
                    var stickersJson = itemDescDetail!.data.steam_asset_info.stickers;
                    foreach (dynamic obj in stickersJson)
                    {
                        var sticker = new Sticker((int)obj.sticker_id, (int)obj.slot, (float)obj.wear, (float)(obj.offset_x ?? 0f), (float)(obj.offset_y ?? 0f), (string)obj.name);
                        skinInfo!.SetSticker(sticker);
                    }
                }
                catch (Exception _) { }

            }
        }

        skinInfo.Stickers = skinInfo.Stickers.OrderBy((sticker) => sticker.Slot).ToList();

        return skinInfo;

    }
}