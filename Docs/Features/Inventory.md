# Inventory

## 개요
일반 RPG식 가방·퀵슬롯·필드 아이템. 장비는 **개체(`ItemInstance`) 단위**로 관리되어 같은 이름이라도 등급·능력치가 다르고 각각 장착·등록·버리기가 된다. 게임은 멈추지 않는 실시간 창(Tab)이며, 퀵슬롯은 숫자 키 1~8로 즉시 장착·사용한다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Items/Data/ItemData.cs](../../Assets/Project/Scripts/Items/Data/ItemData.cs) | 아이템 공통: 이름·아이콘·설명·최대 중첩·필드 모델, `Category`, `CanQuickSlot`, `HasStat`, `CreateInstance` |
| [Items/Data/ConsumableData.cs](../../Assets/Project/Scripts/Items/Data/ConsumableData.cs) | 소모품: 사용 모션·효과 시점·전체 시간·이동 속도, 다형 효과 목록 |
| [Items/Effects/](../../Assets/Project/Scripts/Items/Effects/) | `ConsumableEffect` 베이스, `HealEffect` |
| [Items/ItemInstance.cs](../../Assets/Project/Scripts/Items/ItemInstance.cs) | 장비 개체: 데이터·티어·등급·굴린 능력치, `DisplayName`(티어 접두어 + 이름) (내구도 등 개별 상태 확장 지점) |
| [Items/Tier/](../../Assets/Project/Scripts/Items/Tier/) | `ItemTier`·`ItemTierTable`(접두어·능력치 배율·외형), 티어 외형은 [Weapon](Weapon.md#티어-t1t5-무기-데이터-하나로-공용) |
| [InventorySystem/EquipRequirement.cs](../../Assets/Project/Scripts/InventorySystem/EquipRequirement.cs) | 장착 조건 표시 정보 (`IItemActions.TryGetRequirement`, [Mastery](Mastery.md)) |
| [Items/Grade/](../../Assets/Project/Scripts/Items/Grade/) | `ItemGrade`(이름·색·가중치·배율), `ItemGradeTable`(가중치 추첨, SO) |
| [Items/Stats/](../../Assets/Project/Scripts/Items/Stats/) | `StatType`, `StatRange`(범위 랜덤), `ItemStats`(공격력·마법력·방어력·마법 방어력) |
| [Items/WorldItem.cs](../../Assets/Project/Scripts/Items/WorldItem.cs) | 필드 아이템 (씬 배치 장비는 시작 시 개체 생성, 버린 개체는 그대로 보관) |
| [InventorySystem/Inventory.cs](../../Assets/Project/Scripts/InventorySystem/Inventory.cs) | 고정 칸 가방: 중첩·개체 추가·소모·꺼내기·교환, `Changed` |
| [InventorySystem/ItemStack.cs](../../Assets/Project/Scripts/InventorySystem/ItemStack.cs) | 칸 내용: 겹치는 아이템은 종류+수량, 장비는 개체 1개 |
| [InventorySystem/QuickSlots.cs](../../Assets/Project/Scripts/InventorySystem/QuickSlots.cs) / [QuickSlotEntry.cs](../../Assets/Project/Scripts/InventorySystem/QuickSlotEntry.cs) | 퀵슬롯: 장비는 개체, 소모품은 종류 등록, 사라진 개체 자동 해제 |
| [InventorySystem/IItemActions.cs](../../Assets/Project/Scripts/InventorySystem/IItemActions.cs) | UI가 요청하는 아이템 조작 (UI는 플레이어를 모름) |
| [InventorySystem/Data/InventoryData.cs](../../Assets/Project/Scripts/InventorySystem/Data/InventoryData.cs) | 가방 칸 수·퀵슬롯 수·시작 아이템·시작 퀵슬롯 (SO) |
| [Player/PlayerItemHandler.cs](../../Assets/Project/Scripts/Player/PlayerItemHandler.cs) | `IItemActions` 구현: 장착·해제·사용·버리기·줍기, 보조 장비 호환 안내, `Noticed` |
| [Player/PlayerInteractor.cs](../../Assets/Project/Scripts/Player/PlayerInteractor.cs) | 주변 필드 아이템 탐색 (NonAlloc, 일정 간격) |
| [Player/States/PlayerPickUpState.cs](../../Assets/Project/Scripts/Player/States/PlayerPickUpState.cs) / [PlayerUseItemState.cs](../../Assets/Project/Scripts/Player/States/PlayerUseItemState.cs) | 줍기 / 소모품 사용 상태 |
| [Player/PlayerMenuPresenter.cs](../../Assets/Project/Scripts/Player/PlayerMenuPresenter.cs) / [PlayerHudPresenter.cs](../../Assets/Project/Scripts/Player/PlayerHudPresenter.cs) | 창 열림 → 입력·시점 차단 / HUD 연결 |
| [UI/Inventory/](../../Assets/Project/Scripts/UI/Inventory/) | `InventoryWindow`, `ItemSlotView`, `ItemContextMenu`, `ItemDragGhost`, `ItemStatFormatter` |
| [UI/HUD/](../../Assets/Project/Scripts/UI/HUD/) | `QuickSlotBarView`, `AmmoCounterView`, `InteractPromptView`, `NoticeView` |
| [UI/Editor/InventoryUIBuilder.cs](../../Assets/Project/Scripts/UI/Editor/InventoryUIBuilder.cs) | `Tools → ProjectFantasy → Create Inventory UI`: 전체 UI 계층·슬롯 프리팹 생성, 참조 자동 연결 |

## 동작 흐름
### 아이템 구조
```
ItemData (SO, 종류)
 ├─ EquipmentData ── WeaponData(Ranged/Magic) · OffHandData(Shield/Spellbook) · AmmoData
 └─ ConsumableData ── [SerializeReference] ConsumableEffect (HealEffect …)

가방 칸 = ItemStack
 ├─ 겹치는 아이템(화살·소모품): ItemData + 수량
 └─ 장비: ItemInstance(데이터 + 등급 + 굴린 능력치) 1개
```

### 장비 개체 생성 (등급·능력치)
```
CreateInstance(tier) → 등급표 가중치 추첨 → T1 범위 균등 랜덤 × 티어 배율 × 등급 배율 → 반올림
```
- 티어는 아이템 데이터가 아니라 개체에 있다 (같은 데이터로 T1~T5). 시작 아이템(`StartingItem.tier`)·필드 아이템(`WorldItem.tier`)에서 지정.
- 시작 아이템은 최하 등급으로 생성 (`useLowestGrade`).
- 시점: 시작 지급, 씬 배치 `WorldItem`(Start), 가방에 종류로 추가될 때. **한 번만** 굴리고, 버렸다 다시 주우면 같은 개체.
- 수치 기준·밸런스 표: [인스펙터 가이드](../InspectorGuide.md#무기-weapondata)

### 장착 (`PlayerLoadout`, 개체 기준)
- 장착 무기 개체 1 + 보조 장비 개체 1. 장착 개체가 가방에서 사라지면 해제(무기는 맨손).
- 개체 티어가 해당 숙련 해금 티어보다 높으면 거부 + "숙련도가 부족합니다" ([Mastery](Mastery.md)).
- 장비 변경·소모품 사용은 **이동 상태에서만** (`IsInLocomotion`), 아니면 "지금은 사용할 수 없습니다".
- 보조 장비 호환 (`OffHandData.CanEquipWith`):

  | 주무기 | 방패 | 마법서 |
  |---|---|---|
  | 한손검·한손 석궁 | ✅ | ❌ |
  | Wand | ✅ | ✅ |
  | Staff | ❌ | ✅ |
  | 양손검·양손 석궁·활 | ❌ | ❌ |
- 호환되지 않는 보조 장비 장착 시도 → 거부 + "현재 무기와 함께 장착할 수 없습니다".
- 무기를 바꿔 호환되지 않게 되면 **해제 없이 비활성**(모델·가드·방어력 꺼짐, 장비 칸 흐림) + "보조 장비가 비활성화되었습니다". 맞는 무기로 바꾸면 자동 활성.

### 퀵슬롯
- 숫자 키 1~8: 장비 개체면 장착(이미 장착이면 무시), 소모품이면 사용. 선입력 버퍼(0.2초)로 동작 중 입력도 이동 상태가 되면 실행.
- 등록: 가방 칸을 퀵슬롯으로 드래그 (같은 개체·종류가 다른 칸에 있으면 이동). 퀵슬롯끼리 드래그 = 교환, 슬롯 밖으로 드래그 = 해제.
- 소모품은 0개가 되어도 칸 유지(흐리게), 장비 개체는 가방에서 사라지면 자동 해제.

### 줍기·버리기·사용
```
E → PlayerInteractor.Target → 가방에 자리 없음? "가방이 가득 찼습니다" (모션 없음)
                           → PickUp 상태 → grabTime에 가방 추가 (남은 수량은 필드에 유지)
버리기(우클릭) → 칸 전체 꺼냄 → 플레이어 앞에 WorldItem 생성 (장비는 개체 그대로)
소모품 사용 → UseItem 상태(느린 이동) → effectTime에 1개 소모 + 효과 (그 전에 피격되면 소모 안 됨)
```

### 인벤토리 창 (Tab, 실시간)
- 열리면 이동·공격·점프·줍기·퀵슬롯 입력과 시점 회전·줌이 막히고 커서 표시. 게임은 계속 진행(피격 적용).
- 필터: 전체/무기/장비/화살/소모품 (전체만 빈 칸 표시). 칸끼리 드래그 = 교환.
- 더블클릭: 장착 중 개체면 해제, 아니면 장착·사용. 우클릭 메뉴: 장착/해제/사용/버리기.
- 상세 패널: 등급 색 이름(티어 접두어 포함) `T#` `[등급]`, 능력치, **같은 부위 장착 장비 대비 차이**(▲ 초록 / ▼ 빨강, 비어 있으면 0 기준), 요구 숙련(부족하면 빨강).
- 칸: 아이콘(없으면 이름을 등급 색으로) + 모서리 티어 표시 `T#` (장비만).

### HUD
| 요소 | 표시 |
|---|---|
| 퀵슬롯 바 | 하단 8칸, 키 번호, 소모품 수량, 장착 중 E |
| 화살 수 | 활·석궁 장착 시만, 석궁은 "장전" 표시 |
| 줍기 안내 | "E 줍기 — 이름 x수량" (능력치는 표시 안 함) |
| 안내 문구 | 가방 가득 참 / 사용 불가 / 보조 장비 호환 불가 / 비활성화 / 숙련 부족 / 숙련 경험치·레벨업 (1.5초, 여러 줄 동시 표시) |

## 데이터 파라미터
필드별 역할·권장값은 [인스펙터 가이드](../InspectorGuide.md)의 소모품·등급표·인벤토리 항목 참고.

| 에셋 | 주요 값 |
|---|---|
| InventoryData | Capacity 30, Quick Slot Count 8, Starting Items(장비는 수량만큼 개체, Tier), Starting Quick Slots |
| ItemTierTable | 낡은 ×1 / 철 ×1.3 / 강철 ×1.6 / 미스릴 ×2 / 용의 ×2.5 |
| ItemGradeTable | 일반 70 ×1.0 / 희귀 22 ×1.15 / 영웅 7 ×1.3 / 전설 1 ×1.5 |
| ConsumableData | Max Stack 20, Effect Time 0.6, Duration 1.2, Move Speed 1, Effects |

## 에디터 설정
1. 레이어 `Interactable` 추가, 애니메이터에 `PickUp`·`Use_Item` 상태.
2. `InventoryData`·`ItemGradeTable`·`ConsumableData` 생성, 무기·방패에 등급표와 능력치 범위 지정.
3. Rogue: `PlayerItemHandler` 추가(→ `Inventory`·`QuickSlots`·`PlayerInteractor` 자동), Inventory Data, Interactable Layers, World Item Prefab 연결.
4. `WorldItem` 프리팹: Interactable 레이어 + SphereCollider(Trigger) + `WorldItem`.
5. `Tools → ProjectFantasy → Create Inventory UI` (EventSystem은 Input System UI Input Module).
6. 한글: TMP Settings Default Font = `Pretendard-Regular SDF` (Dynamic).

## 주의사항 / 확장 포인트
- 가방 칸 수는 시작 시 1회만 읽는다 (플레이 중 Capacity 변경 무효).
- 새 소모품 효과는 `ConsumableEffect`를 상속한 `[Serializable]` 클래스만 추가하면 효과 드롭다운(`SubclassSelector`)에 자동 노출.
- 새 아이템 종류는 `ItemData` 상속 + `Category` 지정. 개별 상태(내구도·강화)는 `ItemInstance`에 추가.
- 예정: 저장·불러오기, 상점·제작.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-10-06 | 최초 작성: 아이템 데이터·개체, 가방·퀵슬롯, 줍기·버리기·사용, 실시간 인벤토리 창, HUD, 등급·랜덤 능력치, 보조 장비 호환 규칙, UI 빌더 |
| 2026-10-07 | 개체 티어(접두어·배율, 시작·필드 아이템 티어 지정, 시작 아이템 최하 등급), 숙련 부족 장착 거부·요구 숙련 표시, 칸 티어 표시, 안내 문구 여러 줄 |
