# 実装計画書 Ver.prt.0.20 — バグ修正・機能追加バッチ（8件）

**ステータス**: ✅ 全タスク完了
**目的**: ユーザー報告の8件のバグ修正・機能要望に対応
**テスト結果**: 5,661テスト全合格（Core: 5,513 + Gui: 148）

---

## タスク一覧

| # | タスク | カテゴリ | ステータス |
|---|--------|---------|-----------|
| 1 | 装備着脱時のアイテム重複バグ修正 | バグ修正 | ✅ 完了 |
| 2 | インベントリD&D装備着脱対応 | 機能追加 | ✅ 完了 |
| 3 | フィールド/町の可視性修正 | バグ修正 | ✅ 完了 |
| 4 | 素材アイテムのインベントリ未反映修正 | バグ修正 | ✅ 完了 |
| 5 | 素材アイテムのログ表示名修正 | バグ修正 | ✅ 完了 |
| 6 | ダンジョン通路の柱ブロック修正 | バグ修正 | ✅ 完了 |
| 7 | スキルツリーにアクティブスキル追加 | 機能追加 | ✅ 完了 |
| 8 | メッセージログ1000件制限・番号削除 | 機能改善 | ✅ 完了 |

---

## 詳細

### タスク1: 装備着脱時のアイテム重複バグ修正

- **問題**: インベントリの装備欄にある武器や防具を外す際にアイテムが2つに増え、装備する際は2つが同時に装備される
- **根本原因**: `InventoryWindow` のコールバック `_onUseItem` がインデックス（int）で渡されていたが、グリッドインベントリはソート・配置順で表示しているため、渡されるインデックスが `Inventory.Items` の実際のインデックスと一致していなかった
- **修正内容**:
  - `_onUseItem` と `_onDropItem` のコールバック型を `Action<int>` から `Action<Item>` に変更
  - `GameController.cs` に `UseItem(Item item)` オーバーロードを追加し、`inventory.Items.Contains(item)` で参照一致を確認
  - `MainWindow.xaml.cs` のコールバックも `Item` 参照ベースに更新
- **変更ファイル**: `InventoryWindow.xaml.cs`, `GameController.cs`, `MainWindow.xaml.cs`

### タスク2: インベントリD&D装備着脱対応

- **問題**: インベントリの装備欄からの着脱操作をドラッグアンドドロップでもできるようにしてほしい
- **修正内容**:
  - グリッド→装備パネルへのD&D: `GridCanvas_MouseUp` で装備パネル上へのドロップを検出し、ドラッグ中のアイテムが装備可能なら `_onUseItem` を呼び出して装備
  - 装備パネル→グリッドへのD&D: `EquipSlot_Click` でドラッグ開始、`EquipmentCanvas_MouseMove` でドラッグゴースト表示、`EquipmentCanvas_MouseUp` でグリッド上へのドロップを検出し `_onUnequipItem` で装備解除
  - `IsOverEquipmentPanel()` / `IsOverGridPanel()` ヘルパーメソッド追加
  - `CreateEquipDragGhost()` で装備アイテムのドラッグ表示を生成
  - `EquipmentCanvas` に `AllowDrop="True"` を追加
- **変更ファイル**: `InventoryWindow.xaml.cs`, `InventoryWindow.xaml`

### タスク3: フィールド/町の可視性修正

- **問題**: フィールドや町を探索する際に視界が変化せず、入った瞬間に表示されたマスしか表示されない
- **根本原因**: `_isInLocationMap` が true の場合、メインループでFOV計算と敵行動がスキップされるが、フィールドマップでは視界更新が必要だった。また町マップではRevealAll未対応だった
- **修正内容**:
  - `_isLocationField` フラグを追加し、フィールドマップ（field/terrain_field）と町マップ（town/village）を区別
  - 町/村マップは入場時に `Map.RevealAll()` で全タイルを可視化
  - フィールドマップはFOV計算を継続実行（`!_isInLocationMap || _isLocationField` 条件）
  - `DungeonMap.RevealAll()` メソッドを新規追加
  - 建物内部マップも `Map.RevealAll()` を使用するよう統一
- **変更ファイル**: `GameController.cs`, `DungeonMap.cs`

### タスク4: 素材アイテムのインベントリ未反映修正

- **問題**: 骨片、魔力結晶、ゼリー、装備品の欠片、石ころ、鉄片、呪いのエッセンス、甲殻が拾ってもグリッドインベントリに反映されない
- **根本原因**: `GameController` のハーベストアイテム取得処理で `new Material { Name = itemId }` と生のIDで直接生成していたため、正しいアイテムプロパティ（表示名、レアリティ等）が設定されず、グリッドインベントリの描画に失敗していた
- **修正内容**: `new Material { Name = itemId }` を `ItemDefinitions.Create(itemId)` に置き換え、ItemFactoryで定義された正しいアイテムインスタンスを生成
- **変更ファイル**: `GameController.cs`

### タスク5: 素材アイテムのログ表示名修正

- **問題**: `material_equipment_fragment` を拾った際に「装備品の欠片」ではなく生のID文字列がログに表示される
- **根本原因**: タスク4と同一原因。`Material` を手動生成した際に `Name = itemId` としていたため、生IDがそのまま表示名として使用されていた
- **修正内容**: タスク4の修正で同時に解決。`materialItem.GetDisplayName()` でログメッセージを生成するよう変更
- **変更ファイル**: `GameController.cs`

### タスク6: ダンジョン通路の柱ブロック修正

- **問題**: ダンジョン生成時に通路のど真ん中に柱ができてしまい通過できない箇所が発生する
- **根本原因**: `RoomGenerator.AddPillars()`, `AddPillarsAtCorners()`, `AddRandomDecoration()` がタイルを柱や噴水に変更する際、通路の通行可否をチェックしていなかった
- **修正内容**:
  - `WouldBlockPassage()` メソッドを新規追加。4方向隣接タイルに通路/ドアがあるか、対角方向の歩行可能タイルが分断されるかを判定
  - `AddPillars()`, `AddPillarsAtCorners()`, `AddRandomDecoration()` の3箇所に `WouldBlockPassage()` チェックを適用
- **変更ファイル**: `RoomCorridorGenerator.cs`

### タスク7: スキルツリーにアクティブスキル追加

- **問題**: スキルツリーにアクティブスキルを追加してほしい
- **修正内容**:
  - 武器タブに6つのアクティブスキルノードを追加:
    - 十字斬り（剣）、大地割り（斧）、影縫い（短剣）、貫通矢（弓）、マナバースト（杖）、シールドバッシュ（盾）
  - 魔法タブに5つのアクティブスキルノードを追加:
    - メテオ（火）、ブリザード（氷）、サンダーストーム（雷）、リジェネーション（聖）、ソウルドレイン（闇）
  - 全11ノード: Tier 4, RequiredLevel 12, 各対応するTier 3パッシブを前提条件に設定
  - `SkillNodeType.Active` 型、SkillTreeWindowでは既に ⚡ アイコンで表示対応済み
- **変更ファイル**: `SkillTreeSystem.cs`

### タスク8: メッセージログ1000件制限・番号削除

- **問題**: ログに数値が振られるのをやめてほしい。ログは1000件ごとに一番古いものを削除してほしい
- **修正内容**:
  - `AddMessage()` から `$"[{TurnCount}] "` プレフィックスを削除
  - `_messageHistory` が1000件を超えた場合、`RemoveAt(0)` で最古のメッセージを削除
- **変更ファイル**: `GameController.cs`

---

## テスト

| テストファイル | テスト数 | 結果 |
|---|---|---|
| VersionPrt020SystemTests.cs | 29 | ✅ 全合格 |
| 既存テスト（Core + Gui） | 5,632 | ✅ 全合格 |
| **合計** | **5,661** | **✅ 全合格** |

### テストカバレッジ

- **タスク1**: 装備着脱時のアイテム重複なし確認、参照ベース削除テスト（4テスト）
- **タスク2**: GUI層のためCore層テスト対象外（D&Dはユーザー操作テスト）
- **タスク3**: `RevealAll()` の全タイル可視化テスト、リセット後の再RevealAllテスト（2テスト）
- **タスク4**: 全素材アイテムIDの `ItemDefinitions.Create` 生成テスト、表示名テスト（3テスト + Theory 13ケース）
- **タスク5**: タスク4のテストで同時にカバー
- **タスク6**: 複数シードでダンジョン生成し柱が通路を塞がないことを確認（2テスト）
- **タスク7**: 武器/魔法タブのアクティブスキル存在・属性テスト、前提条件テスト、合計数テスト、日本語名テスト（5テスト）
- **タスク8**: メッセージリスト1000件制限シミュレーション（1テスト）

---

## 変更ファイル一覧

| ファイル | 変更種別 | 変更内容 |
|---|---|---|
| `src/RougelikeGame.Gui/GameController.cs` | 修正 | UseItem(Item)追加、ハーベスト素材生成修正、_isLocationField追加、FOV条件変更、AddMessage修正 |
| `src/RougelikeGame.Gui/InventoryWindow.xaml.cs` | 修正 | Action<Item>化、D&D装備着脱コード追加 |
| `src/RougelikeGame.Gui/InventoryWindow.xaml` | 修正 | EquipmentCanvas AllowDrop追加 |
| `src/RougelikeGame.Gui/MainWindow.xaml.cs` | 修正 | コールバックItem参照化 |
| `src/RougelikeGame.Core/Map/DungeonMap.cs` | 修正 | RevealAll()追加 |
| `src/RougelikeGame.Core/Map/Generation/RoomCorridorGenerator.cs` | 修正 | WouldBlockPassage()追加 |
| `src/RougelikeGame.Core/Systems/SkillTreeSystem.cs` | 修正 | アクティブスキル11ノード追加 |
| `tests/RougelikeGame.Core.Tests/VersionPrt020SystemTests.cs` | 新規 | Phase 20テスト29件 |
