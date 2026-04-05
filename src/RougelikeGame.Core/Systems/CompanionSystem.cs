namespace RougelikeGame.Core.Systems;

/// <summary>
/// 仲間・傭兵システム - パーティ管理（最大4体）と戦闘AI
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
        int HireCost,
        int Hp = 100,
        int MaxHp = 100,
        int Attack = 10,
        int Defense = 5,
        bool IsAlive = true
    );

    /// <summary>仲間の行動結果</summary>
    public record CompanionActionResult(
        string CompanionName,
        string ActionDescription,
        int DamageDealt = 0,
        string? TargetName = null
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

    /// <summary>仲間のターン行動を処理（AIモード別）</summary>
    public IReadOnlyList<CompanionActionResult> ProcessCompanionTurns(
        bool hasNearbyEnemy, string? nearbyEnemyName = null, int enemyDistance = 99)
    {
        var results = new List<CompanionActionResult>();

        foreach (var companion in _party.Where(c => c.IsAlive))
        {
            var result = companion.AIMode switch
            {
                CompanionAIMode.Aggressive => ProcessAttackMode(companion, hasNearbyEnemy, nearbyEnemyName, enemyDistance),
                CompanionAIMode.Defensive => ProcessDefendMode(companion, hasNearbyEnemy, nearbyEnemyName, enemyDistance),
                CompanionAIMode.Support => ProcessFollowMode(companion),
                CompanionAIMode.Wait => new CompanionActionResult(companion.Name, "待機している"),
                _ => new CompanionActionResult(companion.Name, "待機している")
            };
            results.Add(result);
        }

        return results;
    }

    private static CompanionActionResult ProcessAttackMode(
        CompanionData companion, bool hasEnemy, string? enemyName, int distance)
    {
        if (hasEnemy && distance <= 2)
        {
            int damage = CalculateCompanionDamage(companion);
            return new CompanionActionResult(companion.Name,
                $"{enemyName}に攻撃した！", damage, enemyName);
        }
        return new CompanionActionResult(companion.Name, "敵を探している");
    }

    private static CompanionActionResult ProcessDefendMode(
        CompanionData companion, bool hasEnemy, string? enemyName, int distance)
    {
        if (hasEnemy && distance <= 1)
        {
            int damage = CalculateCompanionDamage(companion) / 2;
            return new CompanionActionResult(companion.Name,
                $"防御しながら{enemyName}に反撃した", damage, enemyName);
        }
        return new CompanionActionResult(companion.Name, "プレイヤーの近くで防御態勢をとっている");
    }

    private static CompanionActionResult ProcessFollowMode(CompanionData companion)
    {
        return new CompanionActionResult(companion.Name, "プレイヤーに追従している");
    }

    /// <summary>仲間のダメージ計算</summary>
    public static int CalculateCompanionDamage(CompanionData companion)
    {
        return Math.Max(1, companion.Attack + companion.Level / 2);
    }

    /// <summary>仲間にダメージを与える</summary>
    public bool DamageCompanion(string name, int damage)
    {
        var index = _party.FindIndex(c => c.Name == name);
        if (index < 0) return false;

        var companion = _party[index];
        int newHp = Math.Max(0, companion.Hp - Math.Max(1, damage - companion.Defense));
        bool alive = newHp > 0;
        _party[index] = companion with { Hp = newHp, IsAlive = alive };
        return !alive; // true if companion died
    }

    /// <summary>仲間を回復</summary>
    public void HealCompanion(string name, int amount)
    {
        var index = _party.FindIndex(c => c.Name == name);
        if (index < 0) return;
        var companion = _party[index];
        if (!companion.IsAlive) return;
        _party[index] = companion with { Hp = Math.Min(companion.MaxHp, companion.Hp + amount) };
    }

    /// <summary>死亡した仲間を除去</summary>
    public IReadOnlyList<string> RemoveDeadCompanions()
    {
        var dead = _party.Where(c => !c.IsAlive).Select(c => c.Name).ToList();
        _party.RemoveAll(c => !c.IsAlive);
        return dead;
    }

    /// <summary>生存仲間数</summary>
    public int AliveCount => _party.Count(c => c.IsAlive);

    /// <summary>
    /// 全仲間を解散する（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、仲間関係は全て消失する。
    /// </summary>
    public void Reset()
    {
        _party.Clear();
    }

    /// <summary>AX-2: コンパニオンが経験値を獲得してレベルアップ</summary>
    public bool GainExperience(string companionName, int enemyLevel)
    {
        var idx = _party.FindIndex(c => c.Name == companionName && c.IsAlive);
        if (idx < 0) return false;

        var companion = _party[idx];
        // 敵レベルがコンパニオンレベル以上の場合にレベルアップ判定（確率20%）
        if (enemyLevel >= companion.Level && Random.Shared.Next(5) == 0)
        {
            _party[idx] = companion with
            {
                Level = companion.Level + 1,
                MaxHp = companion.MaxHp + 10,
                Hp = Math.Min(companion.Hp + 10, companion.MaxHp + 10),
                Attack = companion.Attack + 3,
                Defense = companion.Defense + 2
            };
            return true;
        }
        return false;
    }
}
