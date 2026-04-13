namespace RougelikeGame.Core.Systems;

/// <summary>
/// 勢力地変動システム - 領地の支配勢力が戦況に応じて変動
/// Ver.α: タイルレベルの勢力影響度を追加。勢力に応じたフィールド敵種別を提供。
/// </summary>
public class TerritoryInfluenceSystem
{
    /// <summary>勢力情報</summary>
    public record FactionInfluence(string FactionName, float Influence);

    /// <summary>勢力名定数</summary>
    public static class FactionNames
    {
        public const string Kingdom = "王国";
        public const string Bandit = "賊";
        public const string Goblin = "ゴブリン";
        public const string Wildlife = "野生動物";
        public const string Undead = "アンデッド";
        public const string Demon = "魔族";
        public const string Elf = "エルフ";
        public const string Dwarf = "ドワーフ";
    }

    /// <summary>領地ごとの勢力割合</summary>
    private readonly Dictionary<TerritoryId, Dictionary<string, float>> _influence = new();

    /// <summary>勢力を初期化</summary>
    public void Initialize(TerritoryId territory, Dictionary<string, float> factions)
    {
        _influence[territory] = new Dictionary<string, float>(factions);
    }

    /// <summary>勢力値を変動させる</summary>
    public void ModifyInfluence(TerritoryId territory, string factionName, float delta)
    {
        if (!_influence.ContainsKey(territory))
            _influence[territory] = new Dictionary<string, float>();

        var factions = _influence[territory];
        factions[factionName] = Math.Clamp(factions.GetValueOrDefault(factionName) + delta, 0f, 1f);
        NormalizeInfluence(territory);
    }

    /// <summary>支配勢力を取得</summary>
    public string? GetDominantFaction(TerritoryId territory)
    {
        if (!_influence.TryGetValue(territory, out var factions)) return null;
        return factions.OrderByDescending(f => f.Value).FirstOrDefault().Key;
    }

    /// <summary>勢力一覧を取得</summary>
    public IReadOnlyDictionary<string, float>? GetInfluenceMap(TerritoryId territory)
    {
        return _influence.TryGetValue(territory, out var factions) ? factions : null;
    }

    /// <summary>特定勢力の影響度を取得</summary>
    public float GetInfluence(TerritoryId territory, string factionName)
    {
        if (!_influence.TryGetValue(territory, out var factions)) return 0;
        return factions.GetValueOrDefault(factionName);
    }

    /// <summary>
    /// 安全圏距離の基本値。GetSafeZoneDistance()で取得すること。
    /// </summary>
    public const int DefaultSafeZoneDistance = 30;

    /// <summary>
    /// マップサイズに連動した安全圏距離を算出する。
    /// 対角線の1/10をベースに、最低10マス、最大50マスの範囲。
    /// </summary>
    public static int GetSafeZoneDistance(int mapWidth, int mapHeight)
    {
        int diag = (int)Math.Sqrt(mapWidth * mapWidth + mapHeight * mapHeight);
        return Math.Clamp(diag / 10, 10, 50);
    }

    /// <summary>
    /// タイルレベルの支配勢力を取得する。
    /// ロケーション近くは「野生動物」（安全圏）、それ以外は領地全体の支配勢力を返す。
    /// 安全圏距離はマップサイズに連動。
    /// </summary>
    public string GetDominantFactionForTile(
        TerritoryId territory, Position tilePos,
        IReadOnlyDictionary<Position, LocationDefinition>? locationPositions,
        int mapWidth = 220, int mapHeight = 160)
    {
        int safeZone = GetSafeZoneDistance(mapWidth, mapHeight);
        return GetDominantFactionForTileInternal(territory, tilePos, locationPositions, safeZone);
    }

    /// <summary>
    /// タイルレベルの支配勢力を取得する（後方互換性のためのオーバーロード）。
    /// </summary>
    public string GetDominantFactionForTile(
        TerritoryId territory, Position tilePos,
        IReadOnlyDictionary<Position, LocationDefinition>? locationPositions)
    {
        return GetDominantFactionForTileInternal(territory, tilePos, locationPositions, DefaultSafeZoneDistance);
    }

    /// <summary>内部実装: 安全圏距離を指定して支配勢力を取得</summary>
    private string GetDominantFactionForTileInternal(
        TerritoryId territory, Position tilePos,
        IReadOnlyDictionary<Position, LocationDefinition>? locationPositions,
        int safeZoneDistance)
    {
        return GetDominantFactionForTileWithDays(territory, tilePos, locationPositions, safeZoneDistance, 0);
    }

    /// <summary>
    /// 日数経過を考慮した安全圏/危険圏の重複処理付き支配勢力取得。
    /// A1仕様:
    /// - 安全圏同士の重複: そのまま処理（安全圏のまま）
    /// - 安全圏と危険圏の重なり: 日数経過ごとに危険圏が拡大（安全圏距離が縮小）
    /// - 危険圏同士の重複: 派閥勢力によって縮小拡大が決定
    /// - BorderGate: 常に安全圏
    /// - Dungeon/BanditDen/GoblinNest: 判定対象外（圏域計算に含めない）
    /// </summary>
    public string GetDominantFactionForTileWithDays(
        TerritoryId territory, Position tilePos,
        IReadOnlyDictionary<Position, LocationDefinition>? locationPositions,
        int safeZoneDistance, int elapsedDays)
    {
        if (locationPositions != null)
        {
            // --- BorderGateは常に安全圏 ---
            foreach (var (pos, loc) in locationPositions)
            {
                if (loc.Type == LocationType.BorderGate)
                {
                    int dist = Math.Abs(pos.X - tilePos.X) + Math.Abs(pos.Y - tilePos.Y);
                    if (dist < safeZoneDistance) return FactionNames.Wildlife;
                }
            }

            // --- 安全圏判定: 集落/施設/宗教施設近傍（Dungeon系は判定対象外） ---
            // 日数経過で安全圏距離が縮小（危険圏が拡大）: 30日ごとに1マス縮小、最小は元の半分
            int adjustedSafeZone = safeZoneDistance;
            if (elapsedDays > 0)
            {
                int shrink = elapsedDays / DangerExpansionDaysPerTile;
                adjustedSafeZone = Math.Max(safeZoneDistance / 2, safeZoneDistance - shrink);
            }

            foreach (var (pos, loc) in locationPositions)
            {
                // Dungeon/BanditDen/GoblinNestは圏域判定対象外
                if (loc.Type is LocationType.Dungeon or LocationType.BanditDen or LocationType.GoblinNest)
                    continue;

                if (loc.Type is LocationType.Town or LocationType.Village or LocationType.Capital
                    or LocationType.Facility or LocationType.ReligiousSite)
                {
                    int dist = Math.Abs(pos.X - tilePos.X) + Math.Abs(pos.Y - tilePos.Y);
                    if (dist < adjustedSafeZone) return FactionNames.Wildlife;
                }
            }
        }

        // --- 危険圏同士の重複: 派閥勢力による判定 ---
        // 領地内の複数危険圏が重なる場合、勢力値の高い派閥が支配
        var dominant = GetDominantFaction(territory);
        return dominant ?? GetDefaultFaction(territory);
    }

    /// <summary>危険圏が拡大するまでの日数（この日数ごとに安全圏距離が1マス縮小）</summary>
    public const int DangerExpansionDaysPerTile = 30;

    /// <summary>
    /// 危険圏同士の重複における勢力境界を計算する。
    /// 2つの危険圏拠点間で、各派閥の勢力比によって支配範囲が決定される。
    /// </summary>
    /// <returns>tilePos が faction1 の支配下なら true、faction2 なら false</returns>
    public bool IsInFactionTerritory(
        TerritoryId territory,
        Position tilePos,
        Position faction1Center, string faction1Name,
        Position faction2Center, string faction2Name)
    {
        float influence1 = GetInfluence(territory, faction1Name);
        float influence2 = GetInfluence(territory, faction2Name);

        // 勢力が等しい場合は距離で判定
        if (Math.Abs(influence1 - influence2) < 0.01f)
        {
            int dist1 = Math.Abs(faction1Center.X - tilePos.X) + Math.Abs(faction1Center.Y - tilePos.Y);
            int dist2 = Math.Abs(faction2Center.X - tilePos.X) + Math.Abs(faction2Center.Y - tilePos.Y);
            return dist1 <= dist2;
        }

        // 勢力比で境界を決定: 勢力が強い方がより広い範囲を支配
        float totalInfluence = influence1 + influence2;
        if (totalInfluence <= 0) return true;

        float ratio1 = influence1 / totalInfluence;
        int dist1Total = Math.Abs(faction1Center.X - tilePos.X) + Math.Abs(faction1Center.Y - tilePos.Y);
        int dist2Total = Math.Abs(faction2Center.X - tilePos.X) + Math.Abs(faction2Center.Y - tilePos.Y);
        int totalDist = Math.Abs(faction1Center.X - faction2Center.X) + Math.Abs(faction1Center.Y - faction2Center.Y);

        if (totalDist <= 0) return true;

        // faction1 の支配範囲 = 全距離 × faction1 の勢力比率
        float faction1Range = totalDist * ratio1;
        return dist1Total <= faction1Range;
    }

    /// <summary>
    /// 勢力名に基づくフィールド敵タイプ名を返す。
    /// 賊→野盗、ゴブリン→ゴブリン、野生動物→動物、アンデッド→骸骨etc
    /// </summary>
    public static string GetEnemyTypeForFaction(string factionName)
    {
        return factionName switch
        {
            FactionNames.Bandit => "野盗",
            FactionNames.Goblin => "ゴブリン",
            FactionNames.Wildlife => "野生動物",
            FactionNames.Undead => "骸骨兵",
            FactionNames.Demon => "小悪魔",
            FactionNames.Kingdom => "野良犬",
            FactionNames.Elf => "森の精霊",
            FactionNames.Dwarf => "岩トカゲ",
            _ => "野生動物"
        };
    }

    /// <summary>領地のデフォルト勢力を取得</summary>
    public static string GetDefaultFaction(TerritoryId territory)
    {
        return territory switch
        {
            TerritoryId.Capital => FactionNames.Kingdom,
            TerritoryId.Forest => FactionNames.Elf,
            TerritoryId.Mountain => FactionNames.Dwarf,
            TerritoryId.Coast => FactionNames.Kingdom,
            TerritoryId.Southern => FactionNames.Kingdom,
            TerritoryId.Frontier => FactionNames.Bandit,
            TerritoryId.Desert => FactionNames.Bandit,
            TerritoryId.Swamp => FactionNames.Goblin,
            TerritoryId.Tundra => FactionNames.Wildlife,
            TerritoryId.Lake => FactionNames.Kingdom,
            TerritoryId.Volcanic => FactionNames.Demon,
            TerritoryId.Sacred => FactionNames.Kingdom,
            _ => FactionNames.Wildlife
        };
    }

    /// <summary>
    /// 全勢力情報をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、プレイ中に変動した勢力は初期状態に戻る。
    /// </summary>
    public void Reset()
    {
        _influence.Clear();
    }

    /// <summary>勢力値を正規化（合計1.0に）</summary>
    private void NormalizeInfluence(TerritoryId territory)
    {
        var factions = _influence[territory];
        float total = factions.Values.Sum();
        if (total <= 0) return;
        var keys = factions.Keys.ToList();
        foreach (var key in keys)
            factions[key] /= total;
    }

    /// <summary>時間経過による勢力変動（支配勢力は成長、弱小勢力は衰退）</summary>
    public void UpdateInfluence(int turnsElapsed)
    {
        float growthRate = 0.01f * turnsElapsed;
        float decayRate = 0.005f * turnsElapsed;

        foreach (var territory in _influence.Keys.ToList())
        {
            var factions = _influence[territory];
            if (factions.Count == 0) continue;

            var dominant = factions.OrderByDescending(f => f.Value).First().Key;

            foreach (var faction in factions.Keys.ToList())
            {
                if (faction == dominant)
                    factions[faction] = Math.Min(1f, factions[faction] + growthRate);
                else
                    factions[faction] = Math.Max(0f, factions[faction] - decayRate);
            }

            NormalizeInfluence(territory);
        }
    }

    /// <summary>BQ-19: セーブデータから勢力影響を復元</summary>
    public void RestoreInfluences(Dictionary<TerritoryId, Dictionary<string, float>> influences)
    {
        foreach (var (territory, factions) in influences)
        {
            _influence[territory] = new Dictionary<string, float>(factions);
        }
    }

    // === 敵対派閥消失システム ===

    /// <summary>派閥ダンジョンクリアで派閥消失に必要なクリア数</summary>
    public static readonly Dictionary<string, int> FactionEliminationThresholds = new()
    {
        [FactionNames.Goblin] = 1000,
        [FactionNames.Bandit] = 800,
        [FactionNames.Undead] = 1200,
        [FactionNames.Demon] = 1500,
    };

    /// <summary>領地×派閥ごとのダンジョンクリア数</summary>
    private readonly Dictionary<(TerritoryId Territory, string Faction), int> _factionClearCounts = new();

    /// <summary>消失済み派閥の領地別記録</summary>
    private readonly HashSet<(TerritoryId Territory, string Faction)> _eliminatedFactions = new();

    /// <summary>
    /// 敵対派閥ダンジョンをクリアした際に呼び出す。
    /// 該当領地での派閥クリア数をインクリメントし、閾値到達で派閥消失を判定する。
    /// </summary>
    /// <returns>派閥が消失した場合はtrue</returns>
    public bool RecordFactionDungeonClear(TerritoryId territory, string factionName)
    {
        var key = (territory, factionName);
        _factionClearCounts[key] = _factionClearCounts.GetValueOrDefault(key) + 1;

        if (!FactionEliminationThresholds.TryGetValue(factionName, out int threshold))
            return false;

        if (_factionClearCounts[key] >= threshold && !_eliminatedFactions.Contains(key))
        {
            _eliminatedFactions.Add(key);
            // 領地での該当派閥の勢力をゼロにする
            if (_influence.TryGetValue(territory, out var factions) && factions.ContainsKey(factionName))
            {
                factions[factionName] = 0f;
                NormalizeInfluence(territory);
            }
            return true;
        }
        return false;
    }

    /// <summary>指定領地で派閥が消失済みかどうか判定</summary>
    public bool IsFactionEliminated(TerritoryId territory, string factionName)
    {
        return _eliminatedFactions.Contains((territory, factionName));
    }

    /// <summary>指定領地でのダンジョンクリア数を取得</summary>
    public int GetFactionClearCount(TerritoryId territory, string factionName)
    {
        return _factionClearCounts.GetValueOrDefault((territory, factionName));
    }

    /// <summary>派閥消失データをリセットする（死に戻り用）</summary>
    public void ResetFactionElimination()
    {
        _factionClearCounts.Clear();
        _eliminatedFactions.Clear();
    }
}