namespace RougelikeGame.Core.Systems;

/// <summary>
/// カルマ変動イベント記録
/// </summary>
public record KarmaEvent(int OldValue, int NewValue, string Reason, int TurnNumber);

/// <summary>
/// カルマシステム - プレイヤーの善悪を管理
/// </summary>
public class KarmaSystem
{
    private const string ArrowSeparator = "->";
    private int _karmaValue;

    /// <summary>カルマ値 (-100～100)</summary>
    public int KarmaValue
    {
        get => _karmaValue;
        private set => _karmaValue = Math.Clamp(value, -100, 100);
    }

    /// <summary>現在のカルマ段階</summary>
    public KarmaRank CurrentRank => GetRankFromValue(KarmaValue);

    /// <summary>カルマ変動履歴</summary>
    public List<KarmaEvent> KarmaHistory { get; } = new();

    private int _currentTurn;

    public KarmaSystem()
    {
        KarmaValue = 0;
        _currentTurn = 0;
    }

    /// <summary>カルマ値から段階を判定</summary>
    private static KarmaRank GetRankFromValue(int value) => value switch
    {
        >= 80 => KarmaRank.Saint,
        >= 50 => KarmaRank.Virtuous,
        >= 20 => KarmaRank.Normal,
        >= -19 => KarmaRank.Neutral,
        >= -49 => KarmaRank.Rogue,
        >= -79 => KarmaRank.Criminal,
        _ => KarmaRank.Villain
    };

    /// <summary>カルマを変動させ履歴に記録する</summary>
    public void ModifyKarma(int amount, string reason)
    {
        int oldValue = KarmaValue;
        KarmaValue += amount;
        KarmaHistory.Add(new KarmaEvent(oldValue, KarmaValue, reason, _currentTurn));
    }

    /// <summary>ターン数を設定（履歴記録用）</summary>
    public void SetCurrentTurn(int turn) => _currentTurn = turn;

    /// <summary>カルマによるショップ価格修正率を取得</summary>
    public double GetShopPriceModifier() => CurrentRank switch
    {
        KarmaRank.Saint => 0.8,
        KarmaRank.Virtuous => 0.9,
        KarmaRank.Normal => 1.0,
        KarmaRank.Neutral => 1.0,
        KarmaRank.Rogue => 1.1,
        KarmaRank.Criminal => 1.3,
        KarmaRank.Villain => 1.5,
        _ => 1.0
    };

    /// <summary>NPC態度修正値を取得</summary>
    public double GetNpcDispositionModifier() => CurrentRank switch
    {
        KarmaRank.Saint => 1.5,
        KarmaRank.Virtuous => 1.2,
        KarmaRank.Normal => 1.0,
        KarmaRank.Neutral => 1.0,
        KarmaRank.Rogue => 0.8,
        KarmaRank.Criminal => 0.5,
        KarmaRank.Villain => 0.1,
        _ => 1.0
    };

    /// <summary>闇市へのアクセス可否（悪漢以下で可能）</summary>
    public bool CanAccessDarkMarket() => KarmaValue <= -20;

    /// <summary>聖域への入場可否（悪党以上は制限）</summary>
    public bool CanEnterHolyGround() => KarmaValue > -50;

    /// <summary>
    /// 全状態をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、カルマ値・履歴は全て消失する。
    /// </summary>
    public void Reset()
    {
        KarmaValue = 0;
        KarmaHistory.Clear();
        _currentTurn = 0;
    }

    /// <summary>セーブデータからカルマ履歴を復元</summary>
    public void RestoreHistory(List<string> serializedHistory)
    {
        foreach (var entry in serializedHistory)
        {
            // フォーマット: "oldValue->newValue:reason"
            var arrowIndex = entry.IndexOf(ArrowSeparator);
            if (arrowIndex < 0) continue;
            var colonIndex = entry.IndexOf(':', arrowIndex + ArrowSeparator.Length);
            if (colonIndex < 0) continue;

            if (int.TryParse(entry[..arrowIndex], out var oldVal)
                && int.TryParse(entry[(arrowIndex + ArrowSeparator.Length)..colonIndex], out var newVal))
            {
                var reason = entry[(colonIndex + 1)..];
                KarmaHistory.Add(new KarmaEvent(oldVal, newVal, reason, 0));
            }
        }
    }

    /// <summary>カルマ段階の日本語名を取得</summary>
    public static string GetKarmaRankName(KarmaRank rank) => rank switch
    {
        KarmaRank.Saint => "聖人",
        KarmaRank.Virtuous => "善人",
        KarmaRank.Normal => "普通",
        KarmaRank.Neutral => "中立",
        KarmaRank.Rogue => "悪漢",
        KarmaRank.Criminal => "悪党",
        KarmaRank.Villain => "外道",
        _ => "不明"
    };
}
