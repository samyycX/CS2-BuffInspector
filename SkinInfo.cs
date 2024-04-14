using System.Diagnostics.CodeAnalysis;

namespace BuffInspector;

class SkinInfo {
    public string title;
    public string img;
    public required int DefIndex { get; init; }
    public required int PaintIndex { get; init; }
    public required int PaintSeed { get; init; }
    public required float PaintWear { get; init; }

    [SetsRequiredMembers]
    public SkinInfo(string title, string img, int defIndex, int paintIndex, int paintSeed, float wear) {
        this.title = title;
        this.img = img;
        this.DefIndex = defIndex;
        this.PaintIndex = paintIndex;
        this.PaintSeed = paintSeed;
        this.PaintWear = wear;
    }
}