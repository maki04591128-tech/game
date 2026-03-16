namespace RougelikeGame.Core.Systems;

/// <summary>
/// 環境パズル・仕掛けシステム - ルーン語/属性/物理パズル
/// </summary>
public static class EnvironmentalPuzzleSystem
{
    /// <summary>パズル定義</summary>
    public record PuzzleDefinition(
        PuzzleType Type,
        string Name,
        int Difficulty,
        string Hint,
        string RewardDescription
    );

    private static readonly List<PuzzleDefinition> Puzzles = new()
    {
        new(PuzzleType.RuneLanguage, "古代碑文の解読", 3, "ルーン語の知識が必要", "隠し部屋が開く"),
        new(PuzzleType.Elemental, "属性の祭壇", 2, "正しい属性の魔法を使え", "属性強化のバフ"),
        new(PuzzleType.Physical, "圧力板の迷路", 1, "正しい順序で踏め", "ショートカット開通"),
        new(PuzzleType.RuneLanguage, "封印の扉", 4, "3つのルーン語を組み合わせよ", "伝説の装備"),
        new(PuzzleType.Physical, "歯車の仕掛け", 2, "レバーを正しい順序で操作せよ", "宝物庫が開く"),
    };

    /// <summary>タイプ別パズルを取得</summary>
    public static IReadOnlyList<PuzzleDefinition> GetByType(PuzzleType type)
    {
        return Puzzles.Where(p => p.Type == type).ToList();
    }

    /// <summary>全パズルを取得</summary>
    public static IReadOnlyList<PuzzleDefinition> GetAllPuzzles() => Puzzles;

    /// <summary>解答可能か判定（INT/知識レベル）</summary>
    public static bool CanAttempt(PuzzleType type, int intelligence, int knowledgeLevel)
    {
        int required = type switch
        {
            PuzzleType.RuneLanguage => 15,
            PuzzleType.Elemental => 12,
            PuzzleType.Physical => 8,
            _ => 10
        };
        return intelligence + knowledgeLevel >= required;
    }

    /// <summary>解答成功率を計算</summary>
    public static float CalculateSuccessRate(int difficulty, int intelligence)
    {
        float rate = 0.5f + (intelligence - difficulty * 5) * 0.05f;
        return Math.Clamp(rate, 0.1f, 0.95f);
    }

    /// <summary>パズル種別名を取得</summary>
    public static string GetTypeName(PuzzleType type) => type switch
    {
        PuzzleType.RuneLanguage => "ルーン語パズル",
        PuzzleType.Elemental => "属性パズル",
        PuzzleType.Physical => "物理パズル",
        _ => "不明"
    };
}
