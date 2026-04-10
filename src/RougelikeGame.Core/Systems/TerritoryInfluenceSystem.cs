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
    /// タイルレベルの支配勢力を取得する。
    /// ロケーション近くは「野生動物」、それ以外は領地全体の支配勢力を返す。
    /// </summary>
    public string GetDominantFactionForTile(
        TerritoryId territory, Position tilePos,
        IReadOnlyDictionary<Position, LocationDefinition>? locationPositions)
    {
        // 集落（村/町/都）近くのタイルは野生動物
        if (locationPositions != null)
        {
            foreach (var (pos, loc) in locationPositions)
            {
                if (loc.Type is LocationType.Town or LocationType.Village or LocationType.Capital)
                {
                    int dist = Math.Abs(pos.X - tilePos.X) + Math.Abs(pos.Y - tilePos.Y);
                    if (dist < 10) return FactionNames.Wildlife;
                }
            }
        }

        // 領地全体の支配勢力
        var dominant = GetDominantFaction(territory);
        return dominant ?? GetDefaultFaction(territory);
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
}