namespace RougelikeGame.Core.Systems;

/// <summary>
/// 仲間・傭兵システム - パーティ管理（最大4体）
/// </summary>
public class CompanionSystem
{
    /// <summary>仲間データ</summary>
    public record CompanionData(
        string Name,
        CompanionType Type,
        CompanionAIMode AIMode,
        int Level,
        int Loyalty,
        int HireCost
    );

    private readonly List<CompanionData> _party = new();

    /// <summary>最大パーティサイズ</summary>
    public const int MaxPartySize = 4;

    /// <summary>現在のパーティ</summary>
    public IReadOnlyList<CompanionData> Party => _party;

    /// <summary>仲間を追加</summary>
    public bool AddCompanion(CompanionData companion)
    {
        if (_party.Count >= MaxPartySize) return false;
        if (_party.Any(c => c.Name == companion.Name)) return false;
        _party.Add(companion);
        return true;
    }

    /// <summary>仲間を除去</summary>
    public bool RemoveCompanion(string name)
    {
        var companion = _party.FirstOrDefault(c => c.Name == name);
        if (companion == null) return false;
        _party.Remove(companion);
        return true;
    }

    /// <summary>AIモードを変更</summary>
    public bool SetAIMode(string name, CompanionAIMode mode)
    {
        var index = _party.FindIndex(c => c.Name == name);
        if (index < 0) return false;
        _party[index] = _party[index] with { AIMode = mode };
        return true;
    }

    /// <summary>雇用コストを計算</summary>
    public static int CalculateHireCost(CompanionType type, int level) => type switch
    {
        CompanionType.Mercenary => 100 + level * 50,
        CompanionType.Ally => 0,
        CompanionType.Pet => 50 + level * 20,
        _ => 100
    };

    /// <summary>忠誠度による離脱判定</summary>
    public static bool CheckDesertion(int loyalty, CompanionType type)
    {
        int threshold = type switch
        {
            CompanionType.Mercenary => 10,
            CompanionType.Ally => 5,
            CompanionType.Pet => 15,
            _ => 10
        };
        return loyalty < threshold;
    }

    /// <summary>仲間種別名を取得</summary>
    public static string GetTypeName(CompanionType type) => type switch
    {
        CompanionType.Mercenary => "傭兵",
        CompanionType.Ally => "仲間",
        CompanionType.Pet => "ペット",
        _ => "不明"
    };
}
