# CS2-BuffInspector
基于WeaponPaints，在游戏内解析buff的分享链接并更换皮肤
支持:
- 枪械
- 刀具
- 手套

# 依赖
- [WeaponPaints](https://github.com/Nereziel/cs2-WeaponPaints) **(需要已经生成并配置好配置文件)**

# 配置项
- `UseSync` 设置为true时开启同步模式，并且解析完成后自动刷新皮肤 （默认关闭，不会自动刷新皮肤）
> [!WARNING]
> 开启 `UseSync` 选项可能会造成服务器发生严重卡顿，不建议在连接至BUFF或者皮肤数据库时有较大延迟的服务器使用
- `EnableImagePreview` 开启屏幕中心的图片预览
- `ImagePreviewTime` 图片预览的持续时间（秒）

# 使用方法
安装完插件后，使用
`!buff <buff分享链接>`
指令更换BUFF皮肤

