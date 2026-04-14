using RougelikeGame.Core.Interfaces;
using RougelikeGame.Core.Map;

namespace RougelikeGame.Core.Systems;

/// <summary>
/// 徘徊ボスモンスター定義
/// シンボルマップ上を徘徊する強力なボスモンスター。
/// 各領地の生態系に合わせた生息地で徘徊し、接触すると強制戦闘。
/// </summary>
public record WanderingBossDefinition(
    /// <summary>ボスID</summary>
    string Id,
    /// <summary>ボス名</summary>
    string Name,
    /// <summary>ボスの説明</summary>
    string Description,
    /// <summary>所属領地</summary>
    TerritoryId Territory,
    /// <summary>ボスレベル</summary>
    int Level,
    /// <summary>HP</summary>
    int Hp,
    /// <summary>攻撃力</summary>
    int Attack,
    /// <summary>防御力</summary>
    int Defense,
    /// <summary>徘徊可能な地形タイプ</summary>
    TileType[] WalkableTerrain,
    /// <summary>最低高度（この値以上のタイルでのみ徘徊）</summary>
    int MinAltitude,
    /// <summary>最高高度（この値以下のタイルでのみ徘徊）</summary>
    int MaxAltitude,
    /// <summary>表示文字</summary>
    char DisplayChar = '龍',
    /// <summary>中立モブかどうか（trueの場合、先制攻撃しない）</summary>
    bool IsNeutral = false,
    /// <summary>領地の信仰対象かどうか（trueの場合、討伐時に領地評判が低下する）</summary>
    bool IsWorshipped = false,
    /// <summary>カルマ連動かどうか（trueの場合、プレイヤーのカルマで敵味方が変わる）</summary>
    bool IsKarmaLinked = false,
    /// <summary>カルマ連動時の味方閾値（カルマがこの値以上で味方行動する）</summary>
    int KarmaAllyThreshold = 50,
    /// <summary>カルマ連動時の敵対閾値（カルマがこの値以下で強制敵対する）</summary>
    int KarmaHostileThreshold = -20,
    /// <summary>討伐時のカルマペナルティ（0以外の場合、討伐時にカルマが変動する）</summary>
    int DefeatKarmaPenalty = 0
);

/// <summary>
/// シンボルマップ上のボスモンスター実体（位置・状態管理）
/// </summary>
public class WanderingBossInstance
{
    public WanderingBossDefinition Definition { get; }
    public Position Position { get; set; }
    /// <summary>撃破済みかどうか</summary>
    public bool IsDefeated { get; set; }

    public WanderingBossInstance(WanderingBossDefinition definition, Position position)
    {
        Definition = definition;
        Position = position;
    }
}

/// <summary>
/// 徘徊ボスモンスターシステム
/// 各領地に配置されるシンボルエンカウント型ボスの定義・配置・移動を管理する。
/// </summary>
public static class WanderingBossSystem
{
    /// <summary>ボスの徘徊移動距離（1ターンあたりの最大マス数）</summary>
    private const int BossMoveRange = 1;

    /// <summary>全領地のボス定義一覧</summary>
    private static readonly Dictionary<TerritoryId, WanderingBossDefinition> BossDefinitions = new()
    {
        [TerritoryId.Mountain] = new WanderingBossDefinition(
            "wandering_boss_mountain", "バハムート", "山岳を支配する古竜王。息吹は山をも砕く",
            TerritoryId.Mountain, Level: 50, Hp: 5000, Attack: 200, Defense: 150,
            WalkableTerrain: new[] { TileType.SymbolMountain },
            MinAltitude: 3, MaxAltitude: 5, DisplayChar: '龍',
            IsNeutral: true, IsWorshipped: true),

        [TerritoryId.Coast] = new WanderingBossDefinition(
            "wandering_boss_coast", "リヴァイアサン", "深海に潜む大海蛇。津波を起こす力を持つ",
            TerritoryId.Coast, Level: 48, Hp: 4800, Attack: 180, Defense: 160,
            WalkableTerrain: new[] { TileType.SymbolWater },
            MinAltitude: -4, MaxAltitude: -2, DisplayChar: '龍',
            IsNeutral: true, IsWorshipped: true),

        [TerritoryId.Volcanic] = new WanderingBossDefinition(
            "wandering_boss_volcanic", "イフリート", "火山に棲む炎の魔王。溶岩を従え灼熱の嵐を呼ぶ",
            TerritoryId.Volcanic, Level: 52, Hp: 4500, Attack: 220, Defense: 120,
            WalkableTerrain: new[] { TileType.SymbolLava, TileType.SymbolMountain },
            MinAltitude: 2, MaxAltitude: 4, DisplayChar: '龍',
            IsNeutral: true, IsWorshipped: true),

        [TerritoryId.Forest] = new WanderingBossDefinition(
            "wandering_boss_forest", "ヨルムンガンド", "太古の森に棲む世界蛇。世界樹に巻きつく巨大な蛇であり、森の生態系そのものを司る",
            TerritoryId.Forest, Level: 45, Hp: 4200, Attack: 160, Defense: 180,
            WalkableTerrain: new[] { TileType.SymbolForest },
            MinAltitude: 0, MaxAltitude: 2, DisplayChar: '龍',
            IsNeutral: true, IsWorshipped: true),

        [TerritoryId.Tundra] = new WanderingBossDefinition(
            "wandering_boss_tundra", "フェンリル", "凍土の王。吹雪を纏い氷原を駆ける巨狼",
            TerritoryId.Tundra, Level: 47, Hp: 4600, Attack: 190, Defense: 140,
            WalkableTerrain: new[] { TileType.SymbolIce, TileType.SymbolMountain },
            MinAltitude: 1, MaxAltitude: 3, DisplayChar: '龍',
            IsNeutral: true, IsWorshipped: true),

        [TerritoryId.Desert] = new WanderingBossDefinition(
            "wandering_boss_desert", "サンドワーム", "砂漠の支配者。砂丘を泳ぐ巨大蟲",
            TerritoryId.Desert, Level: 44, Hp: 4000, Attack: 170, Defense: 130,
            WalkableTerrain: new[] { TileType.SymbolDune },
            MinAltitude: 1, MaxAltitude: 3, DisplayChar: '龍'),

        [TerritoryId.Swamp] = new WanderingBossDefinition(
            "wandering_boss_swamp", "ヒュドラ", "沼沢に潜む九頭竜。切り落とした首から新たな首が生える",
            TerritoryId.Swamp, Level: 46, Hp: 4400, Attack: 175, Defense: 135,
            WalkableTerrain: new[] { TileType.SymbolSwamp },
            MinAltitude: -2, MaxAltitude: 0, DisplayChar: '龍'),

        [TerritoryId.Lake] = new WanderingBossDefinition(
            "wandering_boss_lake", "ウンディーネ", "湖底に眠る水精霊の王。穏やかな湖面の下に潜む太古の存在",
            TerritoryId.Lake, Level: 43, Hp: 3800, Attack: 165, Defense: 145,
            WalkableTerrain: new[] { TileType.SymbolWater },
            MinAltitude: -3, MaxAltitude: -1, DisplayChar: '龍',
            IsNeutral: true, IsWorshipped: true),

        [TerritoryId.Sacred] = new WanderingBossDefinition(
            "wandering_boss_sacred", "ライブラ", "聖域を守護する天秤の裁定者。善悪を見極め正義を執行する",
            TerritoryId.Sacred, Level: 55, Hp: 5500, Attack: 210, Defense: 170,
            WalkableTerrain: new[] { TileType.SymbolMountain, TileType.SymbolGrass },
            MinAltitude: 1, MaxAltitude: 2, DisplayChar: '龍',
            IsNeutral: true, IsWorshipped: true,
            IsKarmaLinked: true, KarmaAllyThreshold: 50, KarmaHostileThreshold: -20,
            DefeatKarmaPenalty: -30),

        [TerritoryId.Frontier] = new WanderingBossDefinition(
            "wandering_boss_frontier", "ベヒーモス", "辺境の荒野を支配する巨獣。大地を揺るがす咆哮",
            TerritoryId.Frontier, Level: 42, Hp: 4000, Attack: 185, Defense: 125,
            WalkableTerrain: new[] { TileType.SymbolGrass, TileType.SymbolMountain },
            MinAltitude: 0, MaxAltitude: 3, DisplayChar: '龍'),

        [TerritoryId.Southern] = new WanderingBossDefinition(
            "wandering_boss_southern", "キマイラ", "南部の荒野を徘徊する合成獣。獅子の頭、山羊の胴、蛇の尾を持つ",
            TerritoryId.Southern, Level: 44, Hp: 4100, Attack: 175, Defense: 140,
            WalkableTerrain: new[] { TileType.SymbolGrass, TileType.SymbolForest },
            MinAltitude: -1, MaxAltitude: 1, DisplayChar: '龍'),
    };

    /// <summary>
    /// 指定領地のボス定義を取得する。ボスが存在しない領地はnullを返す。
    /// </summary>
    public static WanderingBossDefinition? GetBossForTerritory(TerritoryId territory)
    {
        return BossDefinitions.GetValueOrDefault(territory);
    }

    /// <summary>
    /// 指定マップ上でボスの初期配置位置を探す。
    /// ボスの生息条件（地形タイプ＋高度範囲）を満たすランダムな位置を返す。
    /// </summary>
    public static Position? FindBossSpawnPosition(
        DungeonMap map, WanderingBossDefinition boss, bool[,] shapeMask,
        HashSet<Position> usedPositions, Random random, int maxAttempts = 500)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int x = random.Next(3, map.Width - 3);
            int y = random.Next(3, map.Height - 3);

            if (x >= shapeMask.GetLength(0) || y >= shapeMask.GetLength(1)) continue;
            if (!shapeMask[x, y]) continue;

            var tile = map.GetTile(new Position(x, y));
            if (!CanBossOccupy(boss, tile)) continue;

            var pos = new Position(x, y);

            // 既存ロケーションから十分離れているか
            bool tooClose = false;
            foreach (var used in usedPositions)
            {
                if (pos.ChebyshevDistanceTo(used) < 10)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            return pos;
        }
        return null;
    }

    /// <summary>
    /// ボスが指定タイルに存在可能かどうか判定する。
    /// 地形タイプと高度範囲の両方を満たす必要がある。
    /// </summary>
    public static bool CanBossOccupy(WanderingBossDefinition boss, Tile tile)
    {
        // 地形タイプチェック
        bool terrainMatch = false;
        foreach (var terrain in boss.WalkableTerrain)
        {
            if (tile.Type == terrain)
            {
                terrainMatch = true;
                break;
            }
        }
        if (!terrainMatch) return false;

        // 高度範囲チェック
        return tile.Altitude >= boss.MinAltitude && tile.Altitude <= boss.MaxAltitude;
    }

    /// <summary>
    /// ボスを1ステップ移動させる（毎ターン呼び出し）。
    /// 生息条件を満たす隣接タイルにランダム移動する。
    /// </summary>
    public static void MoveBoss(WanderingBossInstance bossInstance, DungeonMap map, IRandomProvider random)
    {
        if (bossInstance.IsDefeated) return;

        // 8方向からランダムに移動先を選ぶ
        var directions = new (int dx, int dy)[]
        {
            (-1, -1), (0, -1), (1, -1),
            (-1, 0),           (1, 0),
            (-1, 1),  (0, 1),  (1, 1)
        };

        // シャッフル
        for (int i = directions.Length - 1; i > 0; i--)
        {
            int j = random.Next(0, i + 1);
            (directions[i], directions[j]) = (directions[j], directions[i]);
        }

        // 30%の確率で移動しない（ボスは悠然としている）
        if (random.NextDouble() < 0.3) return;

        foreach (var (dx, dy) in directions)
        {
            int nx = bossInstance.Position.X + dx;
            int ny = bossInstance.Position.Y + dy;

            if (!map.IsInBounds(nx, ny)) continue;

            var tile = map.GetTile(new Position(nx, ny));
            if (CanBossOccupy(bossInstance.Definition, tile))
            {
                bossInstance.Position = new Position(nx, ny);
                return;
            }
        }
    }

    /// <summary>
    /// プレイヤー位置とボス位置が一致（接触）しているかチェックする。
    /// </summary>
    public static bool IsPlayerContactingBoss(Position playerPos, WanderingBossInstance? boss)
    {
        if (boss == null || boss.IsDefeated) return false;
        return playerPos == boss.Position;
    }

    /// <summary>
    /// 中立ボスとの接触時に戦闘が発生するかを判定する。
    /// 中立ボスは通常は戦闘にならないが、カルマ連動ボスは条件により強制敵対する。
    /// </summary>
    /// <param name="boss">対象ボスの定義</param>
    /// <param name="playerKarma">プレイヤーの現在のカルマ値</param>
    /// <returns>true: 戦闘発生、false: 戦闘なし（中立のまま通過）</returns>
    public static bool ShouldEngageCombat(WanderingBossDefinition boss, int playerKarma)
    {
        // 中立ボスでなければ常に戦闘
        if (!boss.IsNeutral) return true;

        // カルマ連動ボスの場合、低カルマで強制敵対
        if (boss.IsKarmaLinked && playerKarma <= boss.KarmaHostileThreshold)
        {
            return true;
        }

        // 中立ボスは通常戦闘しない
        return false;
    }

    /// <summary>
    /// カルマ連動ボスが味方として行動するかを判定する。
    /// 高カルマのプレイヤーに対して味方行動を取る。
    /// </summary>
    /// <param name="boss">対象ボスの定義</param>
    /// <param name="playerKarma">プレイヤーの現在のカルマ値</param>
    /// <returns>true: 味方として行動する</returns>
    public static bool IsAllyBehavior(WanderingBossDefinition boss, int playerKarma)
    {
        if (!boss.IsKarmaLinked) return false;
        return playerKarma >= boss.KarmaAllyThreshold;
    }

    /// <summary>
    /// ボス討伐時の評判ペナルティを計算する。
    /// 信仰対象ボスの討伐は所属領地の評判を大きく低下させる。
    /// </summary>
    /// <param name="boss">討伐されたボスの定義</param>
    /// <returns>評判変動値（負の値）。信仰対象でなければ0</returns>
    public static int GetDefeatReputationPenalty(WanderingBossDefinition boss)
    {
        if (!boss.IsWorshipped) return 0;
        // 信仰対象ボス討伐: 領地評判-30
        return -30;
    }

    /// <summary>
    /// ボス討伐時のカルマペナルティを取得する。
    /// </summary>
    /// <param name="boss">討伐されたボスの定義</param>
    /// <returns>カルマ変動値（定義されたペナルティ値、通常は負の値）</returns>
    public static int GetDefeatKarmaPenalty(WanderingBossDefinition boss)
    {
        return boss.DefeatKarmaPenalty;
    }

    /// <summary>
    /// ボス討伐時のペナルティ理由テキストを取得する。
    /// </summary>
    public static string GetDefeatPenaltyReason(WanderingBossDefinition boss)
    {
        if (boss.IsWorshipped)
        {
            return $"{boss.Territory}領の信仰対象「{boss.Name}」を討伐した";
        }
        return $"徘徊ボス「{boss.Name}」を討伐した";
    }
}
