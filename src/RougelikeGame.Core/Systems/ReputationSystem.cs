namespace RougelikeGame.Core.Systems;

/// <summary>
/// 評判変動イベント引数
/// </summary>
public record ReputationChangedEventArgs(
    TerritoryId Territory,
    int OldValue,
    int NewValue,
    string Reason);

/// <summary>
/// 評判・名声システム - 領地ごとの評判を管理
/// </summary>
public class ReputationSystem
{
    private readonly Dictionary<TerritoryId, int> _reputations = new();

    /// <summary>評判変動時に発火するイベント</summary>
    public event Action<ReputationChangedEventArgs>? OnReputationChanged;

    public ReputationSystem()
    {
        foreach (TerritoryId territory in Enum.GetValues<TerritoryId>())
        {
            _reputations[territory] = 0;
        }
    }

    /// <summary>指定領地の評判を変動させる</summary>
    public void ModifyReputation(TerritoryId territory, int amount, string reason)
    {
        int oldValue = _reputations[territory];
        _reputations[territory] = Math.Clamp(oldValue + amount, -100, 100);
        int newValue = _reputations[territory];

        if (oldValue != newValue)
        {
            OnReputationChanged?.Invoke(new ReputationChangedEventArgs(territory, oldValue, newValue, reason));
        }
    }

    /// <summary>指定領地の評判値を取得</summary>
    public int GetReputation(TerritoryId territory) => _reputations[territory];

    /// <summary>指定領地の評判段階を取得</summary>
    public ReputationRank GetReputationRank(TerritoryId territory) => GetRankFromValue(_reputations[territory]);

    /// <summary>全領地の評判データを取得</summary>
    public IReadOnlyDictionary<TerritoryId, int> GetAllReputations() => _reputations;

    /// <summary>評判による割引率を取得</summary>
    public double GetShopDiscount(TerritoryId territory) => GetRankFromValue(_reputations[territory]) switch
    {
        ReputationRank.Revered => 0.8,
        ReputationRank.Trusted => 0.9,
        ReputationRank.Friendly => 0.95,
        ReputationRank.Indifferent => 1.0,
        ReputationRank.Unfriendly => 1.1,
        ReputationRank.Hostile => 1.3,
        ReputationRank.Hated => 1.5,
        _ => 1.0
    };

    /// <summary>評判によるクエスト解放率を取得（0.0～1.0）</summary>
    public double GetQuestAvailability(TerritoryId territory) => GetRankFromValue(_reputations[territory]) switch
    {
        ReputationRank.Revered => 1.0,
        ReputationRank.Trusted => 0.9,
        ReputationRank.Friendly => 0.7,
        ReputationRank.Indifferent => 0.5,
        ReputationRank.Unfriendly => 0.3,
        ReputationRank.Hostile => 0.1,
        ReputationRank.Hated => 0.0,
        _ => 0.5
    };

    /// <summary>指定領地に入場可能か（Hatedで拒否）</summary>
    public bool IsWelcome(TerritoryId territory) =>
        GetRankFromValue(_reputations[territory]) != ReputationRank.Hated;

    /// <summary>BP-2: 評判に基づくイベント発生率修正 (0.5〜1.5)</summary>
    public float GetEventModifier(TerritoryId territory) => GetRankFromValue(_reputations[territory]) switch
    {
        ReputationRank.Revered => 1.5f,
        ReputationRank.Trusted => 1.3f,
        ReputationRank.Friendly => 1.1f,
        ReputationRank.Indifferent => 1.0f,
        ReputationRank.Unfriendly => 0.8f,
        ReputationRank.Hostile => 0.6f,
        ReputationRank.Hated => 0.5f,
        _ => 1.0f
    };

    /// <summary>評判段階の日本語名を取得</summary>
    public static string GetReputationRankName(ReputationRank rank) => rank switch
    {
        ReputationRank.Revered => "崇拝",
        ReputationRank.Trusted => "信頼",
        ReputationRank.Friendly => "友好",
        ReputationRank.Indifferent => "無関心",
        ReputationRank.Unfriendly => "不信",
        ReputationRank.Hostile => "敵意",
        ReputationRank.Hated => "憎悪",
        _ => "不明"
    };

    /// <summary>評判値から段階を判定</summary>
    private static ReputationRank GetRankFromValue(int value) => value switch
    {
        >= 80 => ReputationRank.Revered,
        >= 50 => ReputationRank.Trusted,
        >= 20 => ReputationRank.Friendly,
        >= -19 => ReputationRank.Indifferent,
        >= -49 => ReputationRank.Unfriendly,
        >= -79 => ReputationRank.Hostile,
        _ => ReputationRank.Hated
    };

    /// <summary>
    /// 全評判をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、全領地の評判は初期値に戻る。
    /// </summary>
    public void Reset()
    {
        foreach (var territory in _reputations.Keys.ToList())
        {
            _reputations[territory] = 0;
        }
    }

    /// <summary>
    /// DA-4: 評判の時間減衰。極端な評判値は0に向かって緩やかに収束する。
    /// ProcessTurnEffectsから定期呼び出しされる。
    /// </summary>
    public void DecayReputations()
    {
        foreach (var territory in _reputations.Keys.ToList())
        {
            int value = _reputations[territory];
            if (Math.Abs(value) > 10)
            {
                // 1ポイントずつ中立方向に減衰
                int decay = value > 0 ? -1 : 1;
                _reputations[territory] = value + decay;
            }
        }
    }

    /// <summary>BQ-2: セーブデータから評判値を復元する</summary>
    public void RestoreReputations(Dictionary<string, int> saved)
    {
        foreach (var (key, value) in saved)
        {
            if (Enum.TryParse<TerritoryId>(key, out var territory))
            {
                _reputations[territory] = Math.Clamp(value, -100, 100);
            }
        }
    }
}
