namespace RougelikeGame.Core.Systems;

/// <summary>
/// ダンジョン生態系・食物連鎖システム - 敵同士の相互作用・捕食関係・戦闘痕跡
/// 参考: Dwarf Fortress、Caves of Qud
/// </summary>
public class DungeonEcosystemSystem
{
    /// <summary>捕食関係の定義</summary>
    public record PredatorPreyRelation(
        MonsterRace Predator,
        MonsterRace Prey,
        int PredationChance
    );

    /// <summary>生態系イベント記録</summary>
    public record EcosystemEvent(
        EcosystemEventType Type,
        string PredatorId,
        string PreyId,
        int Floor,
        int Turn,
        string Description
    );

    /// <summary>戦闘痕跡</summary>
    public record BattleTrace(
        int X,
        int Y,
        int Floor,
        int DangerLevel,
        string Description,
        int TurnCreated
    );

    private readonly List<PredatorPreyRelation> _relations = new();
    private readonly List<EcosystemEvent> _events = new();
    private readonly List<BattleTrace> _traces = new();

    /// <summary>全捕食関係</summary>
    public IReadOnlyList<PredatorPreyRelation> Relations => _relations;

    /// <summary>全生態系イベント</summary>
    public IReadOnlyList<EcosystemEvent> Events => _events;

    /// <summary>全戦闘痕跡</summary>
    public IReadOnlyList<BattleTrace> Traces => _traces;

    /// <summary>捕食関係を登録</summary>
    public void RegisterRelation(MonsterRace predator, MonsterRace prey, int chance = 70)
    {
        _relations.Add(new PredatorPreyRelation(predator, prey, Math.Clamp(chance, 0, 100)));
    }

    /// <summary>捕食関係が存在するか確認</summary>
    public bool HasPredatorRelation(MonsterRace predator, MonsterRace prey)
    {
        return _relations.Any(r => r.Predator == predator && r.Prey == prey);
    }

    /// <summary>生態系イベントを処理する</summary>
    public EcosystemEvent? ProcessInteraction(string predatorId, MonsterRace predatorRace,
        string preyId, MonsterRace preyRace, int floor, int turn)
    {
        var relation = _relations.FirstOrDefault(r => r.Predator == predatorRace && r.Prey == preyRace);
        if (relation == null)
        {
            if (predatorRace == preyRace)
            {
                var evt = new EcosystemEvent(
                    EcosystemEventType.TerritoryFight, predatorId, preyId, floor, turn,
                    $"{predatorRace}同士の縄張り争い");
                _events.Add(evt);
                return evt;
            }
            return null;
        }

        var predationEvent = new EcosystemEvent(
            EcosystemEventType.Predation, predatorId, preyId, floor, turn,
            $"{predatorRace}が{preyRace}を捕食");
        _events.Add(predationEvent);
        return predationEvent;
    }

    /// <summary>戦闘痕跡を追加</summary>
    public void AddBattleTrace(int x, int y, int floor, int dangerLevel, string description, int turn)
    {
        _traces.Add(new BattleTrace(x, y, floor, dangerLevel, description, turn));
    }

    /// <summary>指定フロアの戦闘痕跡を取得</summary>
    public IReadOnlyList<BattleTrace> GetTracesOnFloor(int floor)
    {
        return _traces.Where(t => t.Floor == floor).ToList();
    }

    /// <summary>フロアの危険度を痕跡から推定</summary>
    public int EstimateDangerLevel(int floor)
    {
        var floorTraces = _traces.Where(t => t.Floor == floor).ToList();
        if (floorTraces.Count == 0) return 0;
        return (int)floorTraces.Average(t => t.DangerLevel);
    }

    /// <summary>
    /// イベント記録・戦闘痕跡をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、ダンジョン内の痕跡は全て消失する。
    /// 捕食関係の定義（マスターデータ）は保持する。
    /// </summary>
    public void Reset()
    {
        _events.Clear();
        _traces.Clear();
    }

    /// <summary>生態系イベント種別名を取得</summary>
    public static string GetEventTypeName(EcosystemEventType type) => type switch
    {
        EcosystemEventType.Predation => "捕食",
        EcosystemEventType.TerritoryFight => "縄張り争い",
        EcosystemEventType.Symbiosis => "共生",
        EcosystemEventType.Scavenging => "漁り",
        _ => "不明"
    };

    /// <summary>BQ-14: セーブデータからイベントを復元</summary>
    public void RestoreEvents(IEnumerable<EcosystemEvent> events)
    {
        foreach (var ev in events)
            _events.Add(ev);
    }
}
