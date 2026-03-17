namespace RougelikeGame.Core.Systems;

/// <summary>
/// アイテム鑑定・呪いシステム - 未識別アイテムの鑑定と呪い装備の管理
/// 参考: NetHack、不思議のダンジョン、Elona
/// </summary>
public class ItemIdentificationSystem
{
    /// <summary>鑑定結果</summary>
    public record IdentificationResult(
        string ItemId,
        string TrueName,
        IdentificationState State,
        CurseType Curse,
        string Description
    );

    /// <summary>呪い解除結果</summary>
    public record CurseRemovalResult(
        bool Success,
        string ItemId,
        CurseType OriginalCurse,
        string Message
    );

    private readonly Dictionary<string, IdentificationResult> _identifiedItems = new();
    private readonly HashSet<string> _knownCurses = new();

    /// <summary>鑑定済みアイテム一覧</summary>
    public IReadOnlyDictionary<string, IdentificationResult> IdentifiedItems => _identifiedItems;

    /// <summary>既知の呪い一覧</summary>
    public IReadOnlyCollection<string> KnownCurses => _knownCurses;

    /// <summary>アイテムを鑑定する</summary>
    public IdentificationResult Identify(string itemId, string trueName, CurseType curse = CurseType.None)
    {
        var state = curse != CurseType.None ? IdentificationState.Cursed : IdentificationState.Identified;
        var description = GetCurseDescription(curse);
        var result = new IdentificationResult(itemId, trueName, state, curse, description);
        _identifiedItems[itemId] = result;
        if (curse != CurseType.None)
            _knownCurses.Add(itemId);
        return result;
    }

    /// <summary>アイテムが鑑定済みか確認</summary>
    public bool IsIdentified(string itemId) => _identifiedItems.ContainsKey(itemId);

    /// <summary>アイテムの鑑定状態を取得</summary>
    public IdentificationState GetState(string itemId)
    {
        return _identifiedItems.TryGetValue(itemId, out var result)
            ? result.State
            : IdentificationState.Unknown;
    }

    /// <summary>呪い装備を装備可能か判定（呪い装備は外せない）</summary>
    public bool CanUnequip(string itemId)
    {
        if (!_identifiedItems.TryGetValue(itemId, out var result))
            return true;
        return result.Curse == CurseType.None;
    }

    /// <summary>呪いを解除する</summary>
    public CurseRemovalResult RemoveCurse(string itemId, int playerLevel)
    {
        if (!_identifiedItems.TryGetValue(itemId, out var result))
            return new CurseRemovalResult(false, itemId, CurseType.None, "アイテムが見つかりません");

        if (result.Curse == CurseType.None)
            return new CurseRemovalResult(false, itemId, CurseType.None, "呪いはかかっていません");

        int requiredLevel = result.Curse switch
        {
            CurseType.Minor => 5,
            CurseType.Major => 15,
            CurseType.Deadly => 30,
            _ => 0
        };

        if (playerLevel < requiredLevel)
            return new CurseRemovalResult(false, itemId, result.Curse,
                $"レベル{requiredLevel}以上が必要です");

        var uncursed = result with { State = IdentificationState.Identified, Curse = CurseType.None, Description = "呪いが解除された" };
        _identifiedItems[itemId] = uncursed;
        _knownCurses.Remove(itemId);
        return new CurseRemovalResult(true, itemId, result.Curse, "呪いが解除されました");
    }

    /// <summary>呪い種別に応じたステータス影響値を取得</summary>
    public static int GetCursePenalty(CurseType curse) => curse switch
    {
        CurseType.None => 0,
        CurseType.Minor => -2,
        CurseType.Major => -5,
        CurseType.Deadly => -10,
        _ => 0
    };

    /// <summary>呪いの説明文を取得</summary>
    public static string GetCurseDescription(CurseType curse) => curse switch
    {
        CurseType.None => "呪いなし",
        CurseType.Minor => "軽い呪い - 装備すると微かな不快感がある",
        CurseType.Major => "重い呪い - 装備すると外せなくなる",
        CurseType.Deadly => "致死の呪い - 装備者の生命力を蝕む",
        _ => "不明"
    };
}
