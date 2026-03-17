namespace RougelikeGame.Core.Systems;

/// <summary>
/// テンプレートマップシステム - 手作りマップの管理
/// </summary>
public static class TemplateMapSystem
{
    /// <summary>テンプレート定義</summary>
    public record TemplateDefinition(
        TemplateMapType Type,
        string Name,
        int Width,
        int Height,
        int MinLevel,
        string Description
    );

    private static readonly List<TemplateDefinition> Templates = new()
    {
        new(TemplateMapType.BossFloor, "魔王の間", 40, 30, 40, "最終ボスが待ち受ける広間"),
        new(TemplateMapType.Town, "王都メインストリート", 60, 40, 1, "商店やNPCが集まる中心地"),
        new(TemplateMapType.Ruins, "古代遺跡", 50, 50, 20, "罠と謎に満ちた遺跡"),
        new(TemplateMapType.Tower, "魔導塔", 30, 30, 30, "各階に試練が待つ魔法の塔"),
        new(TemplateMapType.SpecialDungeon, "隠しダンジョン", 45, 45, 35, "特殊条件で出現する隠し領域"),
    };

    /// <summary>テンプレートを取得</summary>
    public static TemplateDefinition? GetTemplate(TemplateMapType type)
    {
        return Templates.FirstOrDefault(t => t.Type == type);
    }

    /// <summary>全テンプレートを取得</summary>
    public static IReadOnlyList<TemplateDefinition> GetAllTemplates() => Templates;

    /// <summary>レベル制限を満たすか判定</summary>
    public static bool MeetsLevelRequirement(TemplateMapType type, int playerLevel)
    {
        var template = GetTemplate(type);
        return template != null && playerLevel >= template.MinLevel;
    }

    /// <summary>テンプレート種別名を取得</summary>
    public static string GetTypeName(TemplateMapType type) => type switch
    {
        TemplateMapType.BossFloor => "ボスフロア",
        TemplateMapType.Town => "街",
        TemplateMapType.Ruins => "遺跡",
        TemplateMapType.Tower => "塔",
        TemplateMapType.SpecialDungeon => "特殊ダンジョン",
        _ => "不明"
    };
}
