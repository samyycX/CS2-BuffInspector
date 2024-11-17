
using CounterStrikeSharp.API.Core;

namespace BuffInspector;

public class BuffApiServiceImpl : IBuffApiService
{
    public SkinInfo? GetSkinInfoByBuffShareLink(string shareLink)
    {
        return GetSkinInfoByBuffShareLinkAsynchronously(shareLink).Result;
    }

    public Task<SkinInfo?> GetSkinInfoByBuffShareLinkAsynchronously(string shareLink)
    {
        return Scraper.scrapeUrl(shareLink);
    }

    public void ShowSkinInfoToPlayer(CCSPlayerController player, SkinInfo? skinInfo, bool EnableImagePreview = true, float ImagePreviewTime = 5, bool NotifyUserToUpdate = true)
    {
        BuffInspector.SkinInfoDisplayer.ShowSkinInfoToPlayer(player, skinInfo, EnableImagePreview, ImagePreviewTime, !NotifyUserToUpdate);
    }
}