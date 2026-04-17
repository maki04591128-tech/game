# 実装計画書 Ver.prt.0.22 — 建物入口リファクタリング・建物間移動

**ステータス**: ✅ 全タスク完了
**目的**: 建物入口タイルを建物壁の外側に配置し、建物内部マップでの建物間移動（階段）機能を実装
**テスト結果**: 5,669テスト全合格（Core: 5,521 + Gui: 148）

---

## タスク一覧

| # | タスク | カテゴリ | ステータス |
|---|--------|---------|-----------|
| 1 | PlaceBuildingの入口位置を壁の外側に移動 | バグ修正 | ✅ 完了 |
| 2 | 城入口位置の修正 | バグ修正 | ✅ 完了 |
| 3 | GenerateBuildingInteriorに訪問済み建物パラメータ追加 | 新規実装 | ✅ 完了 |
| 4 | GameControllerに建物訪問追跡・建物間移動を実装 | 新規実装 | ✅ 完了 |
| 5 | テスト追加（5件） | テスト | ✅ 完了 |

---

## 詳細

### タスク1: PlaceBuildingの入口位置を壁の外側に移動

- **問題**: `PlaceBuilding()`が建物入口(⌂)を`y + h - 1`（壁の最下行）に配置していたため、入口タイルが建物の壁と重なって見えていた
- **修正内容**:
  - 入口位置を`y + h - 1`→`y + h`（壁の1マス外側）に変更
  - `IsInBounds`チェックを追加して範囲外配置を防止
  - アクセス確保用の床タイル配置（`y + h + 1`）を追加
- **変更ファイル**: `LocationMapGenerator.cs` (`PlaceBuilding`メソッド)

### タスク2: 城入口位置の修正

- **問題**: `GenerateCapitalCastleTown`の城入口が`(midX, 11)`に直接配置されており、壁と重なっていた
- **修正内容**: `(midX, 11)` → `(midX, 12)` に修正（壁の1マス外側）
- **変更ファイル**: `LocationMapGenerator.cs` (`GenerateCapitalCastleTown`メソッド)

### タスク3: GenerateBuildingInteriorに訪問済み建物パラメータ追加

- **問題**: 建物内部マップに他の建物への移動手段がなく、毎回町マップに戻る必要があった
- **修正内容**:
  - `GenerateBuildingInterior(string buildingId, IReadOnlyList<string>? visitedBuildings = null)` にパラメータ追加
  - `visitedBuildings`から自分自身を除いた他の建物について、左壁(x=0)と右壁(x=width-1)に交互にBuildingEntranceタイルを配置
  - 各タイルに対応する`BuildingId`を設定
  - スロット位置: y=2から開始、間隔2、壁際到達で終了
- **変更ファイル**: `LocationMapGenerator.cs` (`GenerateBuildingInterior`メソッド)

### タスク4: GameControllerに建物訪問追跡・建物間移動を実装

- **問題**: 建物間を移動する仕組みがなく、訪問状態の管理もなかった
- **修正内容**:
  - `_visitedBuildings: HashSet<string>` フィールドを追加
  - `EnterBuilding()`: 初回入場時のみ町マップを保存（建物間移動時の上書き防止）、`_visitedBuildings`に追加、`GenerateBuildingInterior`に訪問リストを渡す
  - `TryLeaveTown()`: `_visitedBuildings.Clear()` で町離脱時にリセット
  - 建物内部でBuildingEntranceタイルを踏んだ場合も`EnterBuilding()`が呼ばれ、建物間移動が成立
- **変更ファイル**: `GameController.cs`

### タスク5: テスト追加（5件）

- **追加テスト**:
  1. `TownMap_BuildingEntrance_IsOutsideWalls` — 町マップの全建物入口が壁の外側にあることを検証
  2. `BuildingInterior_WithoutVisitedBuildings_HasNoExtraBuildingEntrance` — visitedBuildings未指定時に追加階段がないことを検証
  3. `BuildingInterior_WithVisitedBuildings_HasInterBuildingStairs` — 訪問済み建物への階段が正しく配置されることを検証（3建物分の配置、自分自身除外、壁上配置）
  4. `BuildingInterior_WithVisitedBuildings_SelfNotIncluded` — 自分自身のみ訪問済みの場合に階段が0であることを検証
  5. `VillageMap_HasBuildingEntrances`（既存テスト、変更なし — 入口位置変更後も合格を確認）
- **変更ファイル**: `VersionPrt019SystemTests.cs` (Task 14リージョン拡張)

---

## 変更ファイル一覧

| ファイル | 変更種別 | 概要 |
|---------|---------|------|
| `src/RougelikeGame.Core/Map/Generation/LocationMapGenerator.cs` | 修正 | PlaceBuilding入口位置変更、城入口修正、GenerateBuildingInterior拡張 |
| `src/RougelikeGame.Gui/GameController.cs` | 修正 | _visitedBuildings追加、EnterBuilding/TryLeaveTown修正 |
| `tests/RougelikeGame.Core.Tests/VersionPrt019SystemTests.cs` | 修正 | 5テスト追加、Task 14リージョン名更新 |
