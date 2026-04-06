namespace RougelikeGame.Core.Systems;

/// <summary>
/// 疲労度システム — 蓄積・回復・段階判定・SP修正・行動コスト加算・行動制限を管理する。
/// 疲労度は0（快調）から100（疲労困憊）まで蓄積し、段階に応じてペナルティが発生する。
/// </summary>
public static class FatigueSystem
{
    /// <summary>
    /// 移動またはスキル使用に応じて疲労度を蓄積する。
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    /// <param name="actionCost">行動のターンコスト</param>
    /// <param name="isSkill">スキル使用の場合true、移動の場合false</param>
    public static void AccumulateFatigue(Entities.Player player, double actionCost, bool isSkill)
    {
        double rate = isSkill ? FatigueConstants.SkillFatigueRate : FatigueConstants.MovementFatigueRate;
        double accumulation = actionCost * rate;
        if (accumulation > 0)
        {
            player.ModifyFatigue(accumulation);
        }
    }

    /// <summary>
    /// 待機時の段階別回復処理。段階に応じて回復量倍率が異なる。
    /// </summary>
    /// <param name="player">対象プレイヤー</param>
    public static void RecoverFatigue(Entities.Player player)
    {
        var stage = player.FatigueStage;
        double recovery = GetRecoveryAmount(stage);
        if (recovery > 0)
        {
            player.ModifyFatigue(-recovery);
        }
    }

    /// <summary>
    /// 待機時の回復量を段階に応じて算出する。
    /// </summary>
    public static double GetRecoveryAmount(FatigueStage stage) => stage switch
    {
        FatigueStage.Refreshed => FatigueConstants.BaseRestRecovery,
        FatigueStage.Normal => FatigueConstants.BaseRestRecovery,
        FatigueStage.Lethargy => FatigueConstants.BaseRestRecovery * FatigueConstants.LethargyRecoveryMultiplier,
        FatigueStage.LightFatigue => FatigueConstants.BaseRestRecovery * FatigueConstants.LightFatigueRecoveryMultiplier,
        FatigueStage.Fatigue => FatigueConstants.BaseRestRecovery * FatigueConstants.SevereRecoveryMultiplier,
        FatigueStage.HeavyFatigue => FatigueConstants.BaseRestRecovery * FatigueConstants.SevereRecoveryMultiplier,
        FatigueStage.Exhaustion => FatigueConstants.BaseRestRecovery * FatigueConstants.SevereRecoveryMultiplier,
        FatigueStage.TotalExhaustion => FatigueConstants.BaseRestRecovery * FatigueConstants.SevereRecoveryMultiplier,
        _ => FatigueConstants.BaseRestRecovery
    };

    /// <summary>
    /// 疲労度値から段階を判定する。
    /// </summary>
    public static FatigueStage GetFatigueStage(double fatigue) => fatigue switch
    {
        >= FatigueConstants.ExhaustionMax => FatigueStage.TotalExhaustion,
        >= FatigueConstants.HeavyFatigueMax => FatigueStage.Exhaustion,
        >= FatigueConstants.FatigueMax => FatigueStage.HeavyFatigue,
        >= FatigueConstants.LightFatigueMax => FatigueStage.Fatigue,
        >= FatigueConstants.LethargyMax => FatigueStage.LightFatigue,
        >= FatigueConstants.NormalMax => FatigueStage.Lethargy,
        >= FatigueConstants.RefreshedMax => FatigueStage.Normal,
        _ => FatigueStage.Refreshed
    };

    /// <summary>
    /// SP上限修正率を返却する。EffectiveMaxSP = BaseMaxSP × (1 + この値)
    /// </summary>
    public static double GetSpModifier(FatigueStage stage) => stage switch
    {
        FatigueStage.Refreshed => FatigueConstants.RefreshedSpBonus,
        FatigueStage.Normal => 0.0,
        FatigueStage.Lethargy => FatigueConstants.LethargySpPenalty,
        FatigueStage.LightFatigue => FatigueConstants.LightFatigueSpPenalty,
        FatigueStage.Fatigue => FatigueConstants.FatigueSpPenalty,
        FatigueStage.HeavyFatigue => FatigueConstants.HeavyFatigueSpPenalty,
        FatigueStage.Exhaustion => FatigueConstants.ExhaustionSpPenalty,
        FatigueStage.TotalExhaustion => FatigueConstants.TotalExhaustionSpPenalty,
        _ => 0.0
    };

    /// <summary>
    /// 行動コスト加算値を返却する。
    /// </summary>
    public static int GetActionCostBonus(FatigueStage stage) => stage switch
    {
        FatigueStage.LightFatigue => FatigueConstants.LightFatigueCostBonus,
        FatigueStage.Fatigue => FatigueConstants.FatigueCostBonus,
        FatigueStage.HeavyFatigue => FatigueConstants.HeavyFatigueCostBonus,
        FatigueStage.Exhaustion => FatigueConstants.ExhaustionCostBonus,
        FatigueStage.TotalExhaustion => FatigueConstants.TotalExhaustionCostBonus,
        _ => 0
    };

    /// <summary>
    /// 行動制限判定。気付け薬による一時解除を考慮する。
    /// </summary>
    /// <param name="stage">現在の疲労段階</param>
    /// <param name="actionType">実行しようとする行動タイプ</param>
    /// <param name="hasFatigueRestrictionRelief">気付け薬による制限解除中かどうか</param>
    /// <returns>行動が実行可能な場合true</returns>
    public static bool CanPerformAction(FatigueStage stage, TurnActionType actionType, bool hasFatigueRestrictionRelief = false)
    {
        // 気付け薬で行動制限が一時解除中の場合、制限をスキップ
        if (hasFatigueRestrictionRelief)
            return true;

        return stage switch
        {
            // 疲労困憊: 待機・インベントリ操作・アイテム使用のみ可能
            FatigueStage.TotalExhaustion => actionType is
                TurnActionType.Wait or
                TurnActionType.Rest or
                TurnActionType.UseItem or
                TurnActionType.Interact,

            // 疲弊: 移動・攻撃・スキル使用不能
            FatigueStage.Exhaustion => actionType is not (
                TurnActionType.Move or
                TurnActionType.Attack or
                TurnActionType.UseSkill or
                TurnActionType.CastSpell),

            // 疲労（重）: 攻撃・スキル使用不能
            FatigueStage.HeavyFatigue => actionType is not (
                TurnActionType.Attack or
                TurnActionType.UseSkill or
                TurnActionType.CastSpell),

            // その他: 制限なし
            _ => true
        };
    }

    /// <summary>
    /// 宿屋回復後の疲労度開始値を返却する。
    /// </summary>
    public static double GetInnRecoveryStart(FatigueStage stage) => stage switch
    {
        FatigueStage.TotalExhaustion => FatigueConstants.InnRecoveryTotalExhaustionStart,
        FatigueStage.Exhaustion => FatigueConstants.InnRecoveryExhaustionStart,
        FatigueStage.HeavyFatigue => FatigueConstants.InnRecoveryHeavyFatigueStart,
        FatigueStage.Fatigue => FatigueConstants.InnRecoveryFatigueStart,
        _ => 0.0  // 疲労未満は完全回復
    };

    /// <summary>
    /// 疲労段階の日本語名を返却する。
    /// </summary>
    public static string GetFatigueStageName(FatigueStage stage) => stage switch
    {
        FatigueStage.Refreshed => "快調",
        FatigueStage.Normal => "通常",
        FatigueStage.Lethargy => "倦怠",
        FatigueStage.LightFatigue => "疲労（軽）",
        FatigueStage.Fatigue => "疲労",
        FatigueStage.HeavyFatigue => "疲労（重）",
        FatigueStage.Exhaustion => "疲弊",
        FatigueStage.TotalExhaustion => "疲労困憊",
        _ => "不明"
    };
}
