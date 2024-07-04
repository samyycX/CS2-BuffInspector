# CS2-BuffInspector
基于WeaponPaints，在游戏内解析buff的分享链接并更换皮肤

也可作为API库使用

支持:
- 枪械
- 刀具
- 手套
- 印花
# 依赖
- [WeaponPaints](https://github.com/Nereziel/cs2-WeaponPaints) **(需要已经生成并配置好配置文件)** *纯API库模式不需要*

# 配置项
- `UseSync` 设置为true时开启同步模式，并且解析完成后自动刷新皮肤 （默认关闭，不会自动刷新皮肤）
> [!WARNING]
> 开启 `UseSync` 选项可能会造成服务器发生严重卡顿，不建议在连接至BUFF或者皮肤数据库时有较大延迟的服务器使用
- `EnableImagePreview` 开启屏幕中心的图片预览
- `ImagePreviewTime` 图片预览的持续时间（秒）
- `EnableSticker` 开启贴纸功能（目前存在一些无法刷新的BUG，有需要请关闭）
- `PureApiMode` 纯API库模式，供其他插件使用（正常情况无需配置）
# 使用方法
1. 从 [Release](https://github.com/samyycX/CS2-BuffInspector/releases/latest) 下载插件压缩包
2. 解压后，将plugins文件夹和shared文件夹放置在 `game/csgo/addons/counterstrikesharp` 文件夹下
3. 使用
`!buff <buff分享链接>`
指令更换BUFF皮肤


# API 库使用方法
API方法请查看 [IBuffApiService.cs](https://github.com/samyycX/CS2-BuffInspector/blob/master/Shared/IBuffApiService.cs)

在您的插件中整合API的样例：
```csharp
public class ExamplePlugin : BasePlugin
{
    public static PluginCapability<IBuffApiService> BuffApiService = new("buffinspector:service");

    // 在你需要使用api的方法内使用
    [ConsoleCommand("css_buff", "buff")]
    public void OnBuffCommand(CCSPlayerController player, CommandInfo commandInfo) {
        
        // 拿到url参数
        string url = ...;

        // 调用API，拿到SkinInfo
        SkinInfo? skinInfo = BuffApiService.Get().GetSkinInfoByBuffShareLink(url);
    }
}