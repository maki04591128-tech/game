namespace RougelikeGame.Core.Systems;

/// <summary>
/// ペット・騎乗システム - 動物パートナーの育成、戦闘補助、騎乗移動
/// 参考: Elona、Stone Soup、Pokemon Mystery Dungeon
/// </summary>
public class PetSystem
{
    /// <summary>ペット状態</summary>
    public record PetState(
        string PetId,
        string Name,
        PetType Type,
        int Level,
        int Experience,
        int Hunger,
        int Loyalty,
        int MaxHp,
        int CurrentHp,
        bool IsRiding
    );

    /// <summary>ペット種別定義</summary>
    public record PetDefinition(
        PetType Type,
        string DefaultName,
        int BaseHp,
        int BaseAttack,
        int BaseDefense,
        int BaseSpeed,
        string SpecialAbility
    );

    private readonly Dictionary<string, PetState> _pets = new();
    private readonly Dictionary<PetType, PetDefinition> _definitions = new();

    /// <summary>全ペット</summary>
    public IReadOnlyDictionary<string, PetState> Pets => _pets;

    /// <summary>初期化（ペット種別定義の登録）</summary>
    public PetSystem()
    {
        _definitions[PetType.Wolf] = new PetDefinition(PetType.Wolf, "ウルフ", 30, 12, 5, 8, "威嚇（敵の攻撃力低下）");
        _definitions[PetType.Horse] = new PetDefinition(PetType.Horse, "ホース", 40, 6, 8, 15, "騎乗（移動速度2倍）");
        _definitions[PetType.Hawk] = new PetDefinition(PetType.Hawk, "ホーク", 15, 8, 3, 20, "偵察（視野範囲拡大）");
        _definitions[PetType.Cat] = new PetDefinition(PetType.Cat, "キャット", 20, 5, 4, 12, "幸運（ドロップ率UP）");
        _definitions[PetType.Bear] = new PetDefinition(PetType.Bear, "ベアー", 60, 15, 12, 5, "防壁（ダメージ吸収）");
        _definitions[PetType.Dragon] = new PetDefinition(PetType.Dragon, "ドラゴン", 80, 20, 15, 10, "ブレス（範囲攻撃）");
    }

    /// <summary>ペット種別定義を取得</summary>
    public PetDefinition? GetDefinition(PetType type) =>
        _definitions.TryGetValue(type, out var def) ? def : null;

    /// <summary>ペットを入手</summary>
    public PetState AddPet(string petId, string name, PetType type)
    {
        var def = _definitions[type];
        var pet = new PetState(petId, name, type, 1, 0, 100, 50, def.BaseHp, def.BaseHp, false);
        _pets[petId] = pet;
        return pet;
    }

    /// <summary>餌をやる（空腹度回復＋忠誠度上昇）</summary>
    public PetState Feed(string petId, int hungerRecovery = 30, int loyaltyBonus = 5)
    {
        if (!_pets.TryGetValue(petId, out var pet)) return new PetState(petId, "", PetType.Cat, 1, 0, 0, 0, 1, 1, false);
        var updated = pet with
        {
            Hunger = Math.Min(100, pet.Hunger + hungerRecovery),
            Loyalty = Math.Min(100, pet.Loyalty + loyaltyBonus)
        };
        _pets[petId] = updated;
        return updated;
    }

    /// <summary>しつけ（忠誠度上昇、レベルに応じて変動）</summary>
    public PetState Train(string petId, int loyaltyChange = 10)
    {
        if (!_pets.TryGetValue(petId, out var pet)) return new PetState(petId, "", PetType.Cat, 1, 0, 0, 0, 1, 1, false);
        var updated = pet with { Loyalty = Math.Clamp(pet.Loyalty + loyaltyChange, 0, 100) };
        _pets[petId] = updated;
        return updated;
    }

    /// <summary>騎乗状態を切り替え</summary>
    public PetState ToggleRide(string petId)
    {
        if (!_pets.TryGetValue(petId, out var pet)) return new PetState(petId, "", PetType.Cat, 1, 0, 0, 0, 1, 1, false);
        bool canRide = pet.Type == PetType.Horse || pet.Type == PetType.Bear || pet.Type == PetType.Dragon;
        if (!canRide) return pet;
        var updated = pet with { IsRiding = !pet.IsRiding };
        _pets[petId] = updated;
        return updated;
    }

    /// <summary>騎乗中の移動速度倍率を取得</summary>
    public float GetMoveSpeedMultiplier(string petId)
    {
        if (!_pets.TryGetValue(petId, out var pet)) return 1.0f;
        if (!pet.IsRiding) return 1.0f;
        return pet.Type switch
        {
            PetType.Horse => 2.0f,
            PetType.Bear => 1.3f,
            PetType.Dragon => 2.5f,
            _ => 1.0f
        };
    }

    /// <summary>ペットの空腹度を減少（ターン経過）</summary>
    public PetState TickHunger(string petId, int amount = 1)
    {
        if (!_pets.TryGetValue(petId, out var pet)) return new PetState(petId, "", PetType.Cat, 1, 0, 0, 0, 1, 1, false);
        var updated = pet with { Hunger = Math.Max(0, pet.Hunger - amount) };
        if (updated.Hunger == 0)
            updated = updated with { Loyalty = Math.Max(0, updated.Loyalty - 2) };
        _pets[petId] = updated;
        return updated;
    }

    /// <summary>
    /// 全ペット状態をリセットする（死に戻り時に呼び出し）。
    /// 死に戻りは時間巻き戻しであるため、入手したペットは全て消失する。
    /// 種別定義（マスターデータ）は保持する。
    /// </summary>
    public void Reset()
    {
        _pets.Clear();
    }

    /// <summary>AB-1: セーブデータからペット状態を復元</summary>
    public void RestorePetState(string petId, int level, int experience, int hunger, int loyalty, int currentHp)
    {
        if (_pets.TryGetValue(petId, out var pet))
        {
            _pets[petId] = pet with
            {
                Level = level,
                Experience = experience,
                Hunger = hunger,
                Loyalty = loyalty,
                CurrentHp = Math.Min(currentHp, pet.MaxHp)
            };
        }
    }

    /// <summary>忠誠度に基づく命令成功率を取得</summary>
    public int GetObedienceRate(string petId)
    {
        if (!_pets.TryGetValue(petId, out var pet)) return 0;
        return Math.Clamp(pet.Loyalty, 0, 100);
    }

    /// <summary>AY-2: ペットの特殊能力ボーナスを取得</summary>
    public (float DropBonus, int ViewRadiusBonus, float DamageReduction, float AttackDebuff) GetPetAbilityBonuses()
    {
        float dropBonus = 0;
        int viewBonus = 0;
        float dmgReduction = 0;
        float atkDebuff = 0;

        foreach (var pet in _pets.Values.Where(p => p.CurrentHp > 0))
        {
            switch (pet.Type)
            {
                case PetType.Cat: dropBonus += 0.15f; break;     // 幸運: ドロップ率+15%
                case PetType.Hawk: viewBonus += 3; break;         // 偵察: 視野+3
                case PetType.Bear: dmgReduction += 0.10f; break;  // 防壁: 被ダメージ-10%
                case PetType.Wolf: atkDebuff += 0.10f; break;     // 威嚇: 敵攻撃力-10%
            }
        }
        return (dropBonus, viewBonus, dmgReduction, atkDebuff);
    }
}
