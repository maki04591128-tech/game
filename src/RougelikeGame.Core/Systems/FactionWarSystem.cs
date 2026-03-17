namespace RougelikeGame.Core.Systems;

/// <summary>
/// 陣営・派閥間戦争イベントシステム - 領地間戦争、陣営選択、世界変化
/// 参考: Mount &amp; Blade、Kenshi
/// </summary>
public class FactionWarSystem
{
    /// <summary>戦争イベント定義</summary>
    public record WarEvent(
        string WarId,
        string Name,
        TerritoryId Attacker,
        TerritoryId Defender,
        WarPhase Phase,
        int TurnStarted,
        int Duration,
        FactionAlignment PlayerAlignment
    );

    /// <summary>戦争参加結果</summary>
    public record WarParticipationResult(
        string WarId,
        FactionAlignment ChosenSide,
        int ReputationChange,
        int GoldReward,
        string Message
    );

    /// <summary>戦争結果</summary>
    public record WarOutcome(
        string WarId,
        TerritoryId Winner,
        TerritoryId Loser,
        int TerritoryInfluenceChange,
        string Description
    );

    private readonly List<WarEvent> _activeWars = new();
    private readonly List<WarOutcome> _warHistory = new();

    /// <summary>現在進行中の戦争一覧</summary>
    public IReadOnlyList<WarEvent> ActiveWars => _activeWars;

    /// <summary>戦争履歴</summary>
    public IReadOnlyList<WarOutcome> WarHistory => _warHistory;

    /// <summary>戦争イベントを開始</summary>
    public WarEvent StartWar(string warId, string name, TerritoryId attacker, TerritoryId defender, int currentTurn)
    {
        var war = new WarEvent(warId, name, attacker, defender, WarPhase.Tension, currentTurn, 0, FactionAlignment.Neutral);
        _activeWars.Add(war);
        return war;
    }

    /// <summary>戦争フェーズを進行</summary>
    public WarEvent? AdvancePhase(string warId, int currentTurn)
    {
        for (int i = 0; i < _activeWars.Count; i++)
        {
            if (_activeWars[i].WarId == warId)
            {
                var war = _activeWars[i];
                var nextPhase = war.Phase switch
                {
                    WarPhase.Tension => WarPhase.Skirmish,
                    WarPhase.Skirmish => WarPhase.Battle,
                    WarPhase.Battle => WarPhase.Aftermath,
                    WarPhase.Aftermath => WarPhase.Peace,
                    _ => WarPhase.Peace
                };
                int duration = currentTurn - war.TurnStarted;
                _activeWars[i] = war with { Phase = nextPhase, Duration = duration };
                return _activeWars[i];
            }
        }
        return null;
    }

    /// <summary>陣営を選択</summary>
    public WarParticipationResult? ChooseAlignment(string warId, FactionAlignment alignment)
    {
        for (int i = 0; i < _activeWars.Count; i++)
        {
            if (_activeWars[i].WarId == warId)
            {
                _activeWars[i] = _activeWars[i] with { PlayerAlignment = alignment };
                int repChange = alignment switch
                {
                    FactionAlignment.Faction1 => 20,
                    FactionAlignment.Faction2 => 20,
                    FactionAlignment.Neutral => 0,
                    FactionAlignment.Mercenary => -5,
                    _ => 0
                };
                int goldReward = alignment == FactionAlignment.Mercenary ? 500 : 0;
                return new WarParticipationResult(warId, alignment, repChange, goldReward,
                    $"{GetAlignmentName(alignment)}として参戦");
            }
        }
        return null;
    }

    /// <summary>戦争を終結させる</summary>
    public WarOutcome? ResolveWar(string warId, TerritoryId winner, int influenceChange)
    {
        var war = _activeWars.FirstOrDefault(w => w.WarId == warId);
        if (war == null) return null;

        var loser = winner == war.Attacker ? war.Defender : war.Attacker;
        var outcome = new WarOutcome(warId, winner, loser, influenceChange,
            $"{war.Name}終結: {winner}の勝利（勢力変動: {influenceChange}）");

        _warHistory.Add(outcome);
        _activeWars.RemoveAll(w => w.WarId == warId);
        return outcome;
    }

    /// <summary>指定領地が関与している戦争を取得</summary>
    public IReadOnlyList<WarEvent> GetWarsInvolving(TerritoryId territory)
    {
        return _activeWars.Where(w => w.Attacker == territory || w.Defender == territory).ToList();
    }

    /// <summary>現在のフェーズに応じた影響を取得</summary>
    public static string GetPhaseDescription(WarPhase phase) => phase switch
    {
        WarPhase.Tension => "両国の間に緊張が走っている。戦争の予感...",
        WarPhase.Skirmish => "国境付近で小競り合いが発生している",
        WarPhase.Battle => "全面戦争が勃発！各地で激しい戦闘が行われている",
        WarPhase.Aftermath => "戦闘は収束しつつある。戦後処理が進行中",
        WarPhase.Peace => "和平が成立した。新しい秩序が生まれつつある",
        _ => "不明"
    };

    /// <summary>陣営名を取得</summary>
    public static string GetAlignmentName(FactionAlignment alignment) => alignment switch
    {
        FactionAlignment.Faction1 => "攻撃側陣営",
        FactionAlignment.Faction2 => "防衛側陣営",
        FactionAlignment.Neutral => "中立",
        FactionAlignment.Mercenary => "傭兵",
        _ => "不明"
    };
}
