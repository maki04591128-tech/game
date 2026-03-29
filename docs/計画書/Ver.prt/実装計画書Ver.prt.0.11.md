# 実装計画書 Ver.prt.0.11（スキルスロット6拡張・未実装魔法エフェクト7種実装）

**目標**: スキルスロット数をスキルツリーシステムと統一（5→6）、未実装だった7種の魔法エフェクトを全て実装
**状態**: ✅ 全タスク完了 — テスト全体2,707件（GUIオートテスト除外）全合格
**前提**: Ver.prt.0.10（ダンジョン品質改善・ショップバグ修正・スキルツリー拡張・インベントリUI強化）完了済み
**完了時テスト数**: 全体 = 2,707件（Core 2,559 + GUI 148）

---

## 1. 概要

Ver.prt.0.11 は Ver.prt.0.10 完了後のコードベース調査に基づき、
スキルスロットの不整合修正と未実装魔法エフェクトの実装を行うフェーズである。

主な改善点:
- スキルスロット配列を5→6に拡張（SkillTreeSystem.MaxSkillSlots=6と統一）
- MainWindowのキーバインドをD1-D5→D1-D6に拡張
- Teleport魔法: ランダム位置への転送+視界再計算
- Stealth魔法: Invisibilityステータスエフェクト付与
- Summon魔法: プレイヤー周囲にFriendly陣営の敵を召喚
- Copy魔法: インベントリ最後のアイテムを複製
- Reverse魔法: 全デバフ除去+再生効果付与
- Seal魔法: 対象敵にSilenceステータスエフェクト付与
- Resurrect魔法: HP全回復+全デバフ浄化

---

## 2. タスク一覧

### Phase 11: スキルスロット6拡張・未実装魔法エフェクト7種実装（9タスク）

| # | タスク名 | 内容 | 状態 |
|---|---------|------|------|
| T.1 | スキルスロット6拡張 | _skillSlots配列5→6、MainWindowキーバインドD1-D5→D1-D6、コメント修正 | ✅ 完了 |
| T.2 | Teleport魔法実装 | GetRandomWalkablePositionで転送+ComputeFov再計算 | ✅ 完了 |
| T.3 | Stealth魔法実装 | Invisibilityステータスエフェクト付与（Duration指定） | ✅ 完了 |
| T.4 | Summon魔法実装 | 周囲空きマス探索+Friendly陣営敵生成+Enemiesリスト追加 | ✅ 完了 |
| T.5 | Copy魔法実装 | インベントリ最後アイテムのItemIdからItemDefinitions.Createで複製 | ✅ 完了 |
| T.6 | Reverse魔法実装 | 全デバフ除去（11種対応）+Regenerationバフ付与 | ✅ 完了 |
| T.7 | Seal魔法実装 | 対象敵にSilenceステータス付与（SingleEnemy/AllEnemies対応） | ✅ 完了 |
| T.8 | Resurrect魔法実装 | HP全回復+全デバフ浄化（7種対応） | ✅ 完了 |
| T.9 | テスト修正・追加・ドキュメント | スロット5→6テスト修正、6番目スロットテスト追加、全テスト合格確認 | ✅ 完了 |

---

## 3. タスク詳細

### T.1: スキルスロット6拡張

**目的**: GameControllerのアクティブスキルスロット数（5）がSkillTreeSystemのMaxSkillSlots（6）と不整合だった問題を修正する

**根本原因**: スキルツリーのD&Dスロット実装時（Ver.prt.0.10 T.12）にSkillTreeSystem側は6スロットで実装したが、GameController側の_skillSlots配列とMainWindowのキーバインドが5のまま残っていた

**変更内容**:
1. `GameController._skillSlots` を `new string?[5]` → `new string?[6]` に変更
2. コメントを「1-5キー割当、最大5スロット」→「1-6キー割当、最大6スロット」に修正
3. `TryUseSkillSlot` メソッドのコメントを「1-5キー用」→「1-6キー用」に修正
4. `MainWindow.xaml.cs` のキーバインド範囲を `Key.D5` → `Key.D6` に拡張
5. MainWindowのコメントを「1-5キー」→「1-6キー」に修正

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`
- `src/RougelikeGame.Gui/MainWindow.xaml.cs`

**受入基準**:
- [x] スキルスロットが6つ利用可能
- [x] キー1-6でスキル発動可能
- [x] 既存テスト全合格

### T.2: Teleport魔法実装

**目的**: メッセージ表示のみだったTeleport魔法に実際の転送機能を実装する

**変更内容**:
1. `ApplySpellTeleport()` メソッドを新規作成
2. `Map.GetRandomWalkablePosition(_random)` でランダムな歩行可能位置を取得
3. `Player.Position` を転送先に更新
4. `Map.ComputeFov()` で視界を再計算
5. 転送先が見つからない場合のフォールバックメッセージ

**参考実装**: トラップのTeleport処理（GameController line 2273-2280）と同一パターンを使用

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] Teleport魔法でプレイヤーがランダム位置に転送される
- [x] 転送後の視界が正しく再計算される

### T.3: Stealth魔法実装

**目的**: メッセージ表示のみだったStealth魔法に透明化効果を実装する

**変更内容**:
1. `ApplySpellStealth()` メソッドを新規作成
2. `StatusEffectType.Invisibility` を `effect.Duration`（最低5ターン）で付与
3. 持続ターン数をメッセージに表示

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] Stealth魔法でInvisibilityステータスが付与される
- [x] Duration指定が反映される

### T.4: Summon魔法実装

**目的**: 「未実装」メッセージだったSummon魔法に召喚機能を実装する

**変更内容**:
1. `ApplySpellSummon()` メソッドを新規作成
2. プレイヤー周囲（±2マス）で空きマスを探索（IsWalkable + 敵/プレイヤー非占有）
3. `EnemyDefinitions.GetEnemiesForDepth(CurrentFloor)` で現階層の敵定義を取得
4. `_enemyFactory.CreateEnemy()` で敵を生成し、`Faction.Friendly` に設定
5. 名前に「召喚」プレフィックスを付与しEnemiesリストに追加
6. 空きマスがない場合・敵定義がない場合のフォールバック処理

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] Summon魔法でFriendly陣営の召喚体が出現する
- [x] 空きマスがない場合にエラーメッセージが表示される

### T.5: Copy魔法実装

**目的**: 「未実装」メッセージだったCopy魔法にアイテム複製機能を実装する

**変更内容**:
1. `ApplySpellCopy()` メソッドを新規作成
2. `(Inventory)Player.Inventory` でキャストし具象型にアクセス（既存パターンに準拠）
3. `Items.LastOrDefault()` でインベントリの最後のアイテムを取得
4. `ItemDefinitions.Create(lastItem.ItemId)` で同一アイテムを新規生成
5. `concreteInventory.Add(copy)` で複製品を追加

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] Copy魔法でインベントリのアイテムが複製される
- [x] インベントリが空の場合にメッセージが表示される

### T.6: Reverse魔法実装

**目的**: 「未実装」メッセージだったReverse魔法にデバフ反転機能を実装する

**変更内容**:
1. `ApplySpellReverse()` メソッドを新規作成
2. 全デバフ系ステータスを検出（Poison/Burn/Freeze/Paralysis/Blind/Confusion/Slow/Weakness/Vulnerability/Silence/Curse の11種）
3. 検出した全デバフを `Player.RemoveStatusEffect()` で除去
4. ボーナスとして `StatusEffectType.Regeneration` を付与（Duration指定、最低5ターン）

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] Reverse魔法で全デバフが除去される
- [x] デバフ除去後にRegeneration効果が付与される
- [x] デバフがない場合にメッセージが表示される

### T.7: Seal魔法実装

**目的**: 「未実装」メッセージだったSeal魔法に能力封印機能を実装する

**変更内容**:
1. `ApplySpellSeal()` メソッドを新規作成
2. ターゲットタイプに応じた分岐（SingleEnemy/Forward → 最寄り敵、AllEnemies/All → 範囲内全敵）
3. 対象敵に `StatusEffectType.Silence` を `effect.Duration`（最低5ターン）で付与
4. 対象なし・範囲外のフォールバック処理

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] Seal魔法で対象敵にSilenceが付与される
- [x] SingleEnemy/AllEnemies両方のターゲットタイプに対応

### T.8: Resurrect魔法実装

**目的**: 「未実装」メッセージだったResurrect魔法に全回復機能を実装する

**変更内容**:
1. `ApplySpellResurrect()` メソッドを新規作成
2. `Player.Heal(Player.MaxHp)` でHP全回復
3. 負のステータス異常（Poison/Burn/Freeze/Paralysis/Blind/Confusion/Curse の7種）を全除去
4. 回復量と浄化をメッセージに表示

**変更ファイル**:
- `src/RougelikeGame.Gui/GameController.cs`

**受入基準**:
- [x] Resurrect魔法でHPが全回復する
- [x] 全デバフが浄化される

### T.9: テスト修正・追加・ドキュメント

**目的**: スキルスロット拡張に伴うテスト修正と新規テスト追加

**変更内容**:
1. `GetSkillSlots_ReturnsAllFiveSlots` → `GetSkillSlots_ReturnsAllSixSlots` にリネーム、Assert.Equal(5→6)
2. `AssignSkillSlot_OutOfRange_Ignored` のindex 5→6に修正（5は範囲内になったため）
3. `AssignSkillSlot_SixthSlot_Succeeds` テスト新規追加（index 5に割り当て確認）

**変更ファイル**:
- `tests/RougelikeGame.Gui.Tests/GameControllerTests.cs`

**受入基準**:
- [x] 全テスト合格（2,707件）
- [x] 6番目スロットの割り当てが検証される

---

## 4. 実装状況サマリ

| 項目 | 値 |
|------|-----|
| 総タスク数 | 9 |
| 完了タスク | 9 |
| テスト総数 | 2,707（Core 2,559 + GUI 148） |
| ビルドエラー | 0 |
| 変更ファイル数 | 3（GameController.cs, MainWindow.xaml.cs, GameControllerTests.cs） |
| 追加メソッド数 | 7（ApplySpellTeleport/Stealth/Summon/Copy/Reverse/Seal/Resurrect） |

---

## 5. ブラッシュアップ記録

| 日付 | 対象ドキュメント | 更新内容 |
|------|-----------------|---------|
| Phase 11完了時 | マスター実装計画書 | Ver.prt.0.11エントリ追加 |
| Phase 11完了時 | ドキュメント概要 | Ver.prt.0.11エントリ追加 |
| Phase 11完了時 | プロジェクト構造設計書 | テスト数2,706→2,707更新 |
| Phase 11完了時 | 戦闘システム設計書 | 魔法エフェクト実装状況更新 |
| Phase 11完了時 | デバッグ・テスト設計書 | テスト数更新 |
| Phase 11完了時 | 実装計画書Ver.prt.0.11 | ブラッシュアップ記録セクション追加 |
