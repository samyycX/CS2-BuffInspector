using CounterStrikeSharp.API.Core;

namespace BuffInspector;

public interface IBuffApiService {

    /// <summary>
    /// 通过buff分享链接获取饰品信息
    /// </summary>
    /// <param name="shareLink">buff分享链接，支持短链</param>
    /// <returns>饰品信息，失败时返回null，目前仅支持枪械，匕首，手套</returns>
    public SkinInfo? GetSkinInfoByBuffShareLink(string shareLink);


    /// <summary>
    /// 通过buff分享链接异步获取饰品信息
    /// </summary>
    /// <param name="shareLink">buff分享链接，支持短链</param>
    /// <returns>饰品信息，失败时返回null，目前仅支持枪械，匕首，手套</returns>
    public Task<SkinInfo?> GetSkinInfoByBuffShareLinkAsynchronously(string shareLink);

    /// <summary>
    /// 给玩家发送获取到的物品信息
    /// </summary>
    /// <param name="player">玩家</param>
    /// <param name="skinInfo">SkinInfo对象</param>
    /// <param name="EnableImagePreview">是否启用图片预览（通过CenterHTML菜单）</param>
    /// <param name="ImagePreviewTime">预览持续时间（默认5秒）</param>
    /// <param name="NotifyUserToUpdate">是否提醒用户更新皮肤</param>
    public void ShowSkinInfoToPlayer(CCSPlayerController player, SkinInfo? skinInfo, bool EnableImagePreview = true, float ImagePreviewTime = 5f, bool NotifyUserToUpdate = true);

}