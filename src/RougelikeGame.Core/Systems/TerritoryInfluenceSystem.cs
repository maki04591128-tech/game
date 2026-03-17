namespace RougelikeGame.Core.Systems;

/// <summary>
/// 勢力地変動システム - 領地の支配勢力が戦況に応じて変動
/// </summary>
public class TerritoryInfluenceSystem
{
    /// <summary>勢力情報</summary>
    public record FactionInfluence(string FactionName, float Influence);

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
}
