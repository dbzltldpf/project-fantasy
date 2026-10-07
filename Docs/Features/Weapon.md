# Weapon

## 개요
손에 드는 무기와 보조 장비(방패·마법서)의 데이터, 모델 장착, 개체별 등급·능력치. 무기 종류별로 쥐는 손·콤보·대기 모션·판정 범위가 다르다. 장비는 아이템이기도 하며 가방·퀵슬롯으로 장착한다([Inventory](Inventory.md)). 플레이어·적 공용.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [WeaponType.cs](../../Assets/Project/Scripts/Weapon/WeaponType.cs) | `Unarmed` `OneHanded` `TwoHanded` `Wand` `Staff` `Bow` `Crossbow` |
| [EquipHand.cs](../../Assets/Project/Scripts/Weapon/EquipHand.cs) | `Right` / `Left` (쥐는 손) |
| [EquipmentData.cs](../../Assets/Project/Scripts/Weapon/Data/EquipmentData.cs) | 장비 공통 (`ItemData` 상속): 모델 프리팹, 손 위치/회전 보정, 티어표·등급표, 티어별 머티리얼(선택), 개체 생성 시 능력치 굴림, `ApplyModelVisual`(티어 외형) |
| [Items/Tier/](../../Assets/Project/Scripts/Items/Tier/) | `ItemTier`·`ItemTierTable`(SO), `TierAura`(불씨 파티클), `TierOutline`(테두리 발광), `TierTintCache`(선택 색조) |
| [Shaders/WeaponOutline.shader](../../Assets/Project/Shaders/WeaponOutline.shader) | Inverted Hull 테두리 발광 (URP, 가산, 맥동) |
| [Items/Editor/WeaponAuraBuilder.cs](../../Assets/Project/Scripts/Items/Editor/WeaponAuraBuilder.cs) | `Tools → ProjectFantasy → Create Weapon Tier Effects`: 아우라 프리팹·파티클/아웃라인 머티리얼 생성, 비어 있는 티어표에 연결 |
| [WeaponData.cs](../../Assets/Project/Scripts/Weapon/Data/WeaponData.cs) | 무기 종류, 쥐는 손, 보조 손 점유, 공격력 범위, 콤보, 판정 범위·칼날, 대기 모션 (SO) |
| [OffHandData.cs](../../Assets/Project/Scripts/Weapon/Data/OffHandData.cs) | 보조 장비 베이스, `CanEquipWith(weapon)`로 동시 장착 가능 여부 판정 |
| [ShieldData.cs](../../Assets/Project/Scripts/Weapon/Data/ShieldData.cs) | 방패: 가드 각도, 막기 넉백/경직, 방어력·마법 방어력 범위 (SO) |
| [SpellbookData.cs](../../Assets/Project/Scripts/Weapon/Data/SpellbookData.cs) | 마법서: 마법 위력 배율 (SO) |
| [RangedWeaponData.cs](../../Assets/Project/Scripts/Weapon/Data/RangedWeaponData.cs) / [AmmoData.cs](../../Assets/Project/Scripts/Weapon/Data/AmmoData.cs) | 활·석궁, 화살 (화살 수는 가방 기준, [Ranged Combat](RangedCombat.md)) |
| [MagicWeaponData.cs](../../Assets/Project/Scripts/Magic/Data/MagicWeaponData.cs) | Wand·Staff, 마법력 범위 ([Magic](Magic.md)) |
| [EquipmentVisual.cs](../../Assets/Project/Scripts/Weapon/EquipmentVisual.cs) | `handslot.r` / `handslot.l`에 모델 부착, 미리 생성 후 활성/비활성 전환, `GetHandSlot`(에디터 미리보기 공용) |
| [PlayerLoadout.cs](../../Assets/Project/Scripts/Player/PlayerLoadout.cs) | 장착 무기·보조 장비 개체 적용, 맞지 않는 보조 장비 비활성, 가방에서 사라지면 해제 |
| [Editor/WeaponDataEditor.cs](../../Assets/Project/Scripts/Weapon/Editor/WeaponDataEditor.cs) | 무기 인스펙터: 요약 줄·설정 경고, 무기 종류에 맞는 섹션만 표시 |
| [TransformExtensions.cs](../../Assets/Project/Scripts/Utils/TransformExtensions.cs) | `FindDeepChild` (손 본 자동 탐색) |

## 동작 흐름
### 장착 (`PlayerLoadout.EquipWeapon(ItemInstance)`)
```
무기 개체 장착 (퀵슬롯 1~8 / 인벤토리 더블클릭·우클릭)
 ├─ EquipmentVisual.Show(gripHand)   // 쥐는 손에 모델 활성 (활은 왼손 → 오른손 비움)
 ├─ MeleeAttacker.SetHitShape        // 무기별 근접 판정 위치·반경
 ├─ MeleeAttacker.SetBlade / ClearBlade  // 칼날이 설정된 무기는 모델 칼날 궤적 판정
 ├─ 보조 장비 갱신                     // 맞지 않으면 비활성(숨김·가드·방어력 해제, 장착은 유지)
 └─ WeaponChanged → PlayerAnimator.SetIdleState (무기별 대기 모션)
```
- 장착 무기가 없으면 맨손(`unarmedWeapon`), 맨손 공격력은 범위 최솟값.
- 모델은 가방에 들어올 때 미리 생성해 캐시, 교체 시 `SetActive`만 전환 → 교체 중 GC/Instantiate 없음.
- 장비 변경은 이동 상태에서만 ([Inventory](Inventory.md)).

### 등급·능력치
- 장비 개체가 생길 때 `등급표 추첨 → 범위 균등 랜덤 × 등급 배율`로 한 번 굴린다 (무기 공격력, Wand·Staff 마법력, 방패 방어력·마법 방어력).
- 밸런스 기준표: [인스펙터 가이드](../InspectorGuide.md#무기-weapondata).

### 티어 (T0~T4, 무기 데이터 하나로 공용)
- 같은 무기 데이터에서 개체마다 티어가 다르다 (`ItemInstance.Tier`). 장착 조건은 무기 숙련 ([Mastery](Mastery.md)).
- 이름 = 티어 접두어 + 이름 (예: `강철 한손검`), 능력치 = `T0 범위 랜덤 × 티어 배율 × 등급 배율`.

  | 티어 | 접두어 | 배율 | 아우라 / 아웃라인 |
  |---|---|---|---|
  | T0 | 낡은 | 1.0 | 없음 |
  | T1 | 철 | 1.3 | 흰색 |
  | T2 | 강철 | 1.6 | 하늘색 |
  | T3 | 미스릴 | 2.0 | 보라 |
  | T4 | 용의 | 2.5 | 주황 (가장 진함) |
- 모델 외형 (`EquipmentData.ApplyModelVisual`, 모델 생성 시 1회):
  1. 머티리얼: `tierMaterials[티어]` 지정 시 교체 > 티어 색조(`applyTint`, 기본 끔) > 원본
  2. `TierAura`: 가장 큰 렌더러의 로컬 경계 상자에서 불씨 방출 (메시 Read/Write 불필요). 파티클 정점 색은 8비트라 HDR 색은 티어별 머티리얼 복사본으로 전달
  3. `TierOutline`: 메시마다 `TierOutline` 자식(메시 공유)에 테두리 발광 머티리얼
- 머티리얼 복사본은 티어별 1회만 생성해 공유, `EquipmentVisual` 모델 캐시 키는 (데이터, 티어).
- 빛 번짐은 URP Volume Bloom + 카메라 HDR 필요.

### 보조 장비 동시 장착 규칙
| 무기 | 방패 | 마법서 |
|---|---|---|
| 맨손, 한손 근접, 한손 석궁 | ✅ | ❌ |
| Wand (한손 마법) | ✅ | ✅ |
| Staff (`allowsShield = false`) | ❌ | ✅ |
| 양손검, 양손 석궁 (`occupiesOffHand`) | ❌ | ❌ |
| 활 (왼손) | ❌ | ❌ |

- 규칙은 각 `OffHandData.CanEquipWith`가 판정 → 새 보조 장비는 클래스만 추가.
- 맞지 않는 보조 장비 장착 시도는 거부 + 안내, 무기 교체로 맞지 않게 되면 비활성 + 안내 ([Inventory](Inventory.md)).

### 데미지
`근접 피해 = 장착 개체 공격력 × HitWindowEvent.damageMultiplier` (반올림)
- 같은 종류 무기는 콤보 에셋 하나를 공유하고 능력치 범위만 다르게 설정.
- 활·석궁은 화살 피해 = 공격력 ([Ranged Combat](RangedCombat.md)), Wand·Staff는 `마법력 × 마법 배율 × 활성 마법서 배율` ([Magic](Magic.md)).
- 받는 쪽 방패(활성)는 `피해 × 100 / (100 + 방어력)` (마법은 마법 방어력), 최소 1 ([Combat](Combat.md)).

## 데이터 파라미터
### WeaponData
| 필드 | 기본값 | 의미 |
|---|---|---|
| displayName / icon / description | — | 게임 내 이름·아이콘·설명 (인벤토리 표시) |
| tierTable | — | 공용 `ItemTierTable` (맨손은 비움) |
| tierMaterials | 비움 | 티어별 모델 머티리얼 교체 (선택, 순서 = T0, T1 …, 빈 칸은 원본) |
| gradeTable | — | 등급 추첨 표 (맨손은 비움) |
| modelPrefab | — | 무기 모델 (맨손은 비움) |
| gripPosition / gripRotation | 0 / 0 | 손 소켓 기준 보정 (KayKit 무기는 0 유지) |
| weaponType | OneHanded | 무기 종류 |
| gripHand | Right | 쥐는 손 (활은 Left) |
| occupiesOffHand | false | 보조 손까지 사용 (양손검, 양손 석궁) |
| allowsShield | true | 보조 손 방패 허용 (Staff false → 마법서만) |
| attackPowerRange | 10~10 | 공격력 범위 (Wand·Staff는 `magicPowerRange`) |
| comboData | — | 사용할 콤보 (AttackComboData) |
| hitOffset / hitRadius | (0, 1, 1) / 0.8 | 근접 판정 구체 (캐릭터 로컬, 칼날 미설정 시 사용) |
| bladeBase / bladeTip | 0 / 0 | 칼날 시작·끝 (무기 모델 로컬), 같으면 칼날 판정 안 함 |
| bladeRadius | 0.1 | 칼날 판정 두께 (반지름) |
| idleStateName | Idle_A | 이 무기를 들었을 때 대기 모션 |

### ShieldData / SpellbookData
| 에셋 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| Shield | guardAngle | 120 | 정면 기준 방어 각도 (전체 각도) |
| Shield | blockKnockbackSpeed / blockStunDuration | 2 / 0.3 | 막았을 때 밀림·경직 |
| Shield | defenseRange / magicDefenseRange | 10~10 / 10~10 | 방어력·마법 방어력 범위 (철 방패 20~30 / 3~6, 마법 방패 8~12 / 20~30) |
| Spellbook | magicPowerMultiplier | 1.2 | 마법 위력 배율 (활성일 때만) |

## 에디터 설정
1. **애니메이션 임포트 (필수)** – KayKit 리그는 Humanoid의 Hand가 `wrist`에 매핑되어 있어 `hand`·`handslot` 본 회전이 기본적으로 버려진다 → 무기 싱크 깨짐.
   - `Assets/Project/Animation/WeaponSocketMask.mask`: Humanoid 전체 + Transform `hand.l` `hand.r` `handslot.l` `handslot.r` 가중치 1
   - 모든 `Rig_Medium_*.fbx` 클립의 Mask를 **Copy From Other → WeaponSocketMask**로 지정 후 클립 재추출
   - FBX meta를 외부에서 수정할 때는 Unity 인스펙터에서 해당 FBX 선택 해제 (Apply 시 덮어씀)
2. **데이터 생성** – `Create → ProjectFantasy → Weapon → Weapon / Shield / Spellbook Data`

   | 무기 | 모델 | Type | Grip Hand | Occupies Off Hand | 대기 모션 | 콤보 |
   |---|---|---|---|---|---|---|
   | 맨손 | — | Unarmed | Right | ❌ | `Melee_Unarmed_Idle` | Punch_A → Kick |
   | 한손검 | `sword_1handed` | OneHanded | Right | ❌ | `Idle_A` | 1H Slice_Horizontal → Slice_Diagonal → Chop |
   | 양손검 | `sword_2handed` | TwoHanded | Right | ✅ | `Melee_2H_Idle` | 2H Slice → Chop → Spin |
   | 지팡이 | `wand` | Wand (**MagicWeaponData**) | Right | ❌ | `Idle_A` | — (Spell: 직선 마법탄) |
   | 스태프 | `staff` | Staff (**MagicWeaponData**, allowsShield ❌) | Right | ❌ | `Idle_A` | — (Spell: 범위 마법) |
   | 활 | `bow` | Bow (**RangedWeaponData**) | **Left** | (자동) | `Ranged_Bow_Idle` | — (발사 모션은 무기 데이터) |
   | 한손 석궁 | `crossbow_1handed` | Crossbow (**RangedWeaponData**) | Right | ❌ | `Idle_A` | — |
   | 양손 석궁 | `crossbow_2handed` | Crossbow (**RangedWeaponData**) | Right | ✅ | `Ranged_2H_Aiming` | — |

3. **애니메이터** – 위 콤보·대기 상태를 `Player.controller`에 추가 (상태 이름 = 클립 이름)
4. **Rogue 프리팹/씬** – `PlayerLoadout`, `EquipmentVisual`, `ShieldGuard` 추가
   - EquipmentVisual 손 본은 컴포넌트 추가 시 자동 탐색
   - PlayerLoadout에 Unarmed / Starting Weapon / Starting Off Hand 지정 (가방 시작 아이템의 첫 개체를 장착)
5. **능력치** – 등급표(`ItemGradeTable`)와 공격력·마법력·방어력 범위 입력 (인스펙터 상단 요약·경고 확인)

## 주의사항 / 확장 포인트
- `unarmedWeapon`은 **필수**. 비어 있으면 시작 시 콘솔 에러.
- 무기 모델이 손에서 어긋나면 먼저 클립 마스크(1번)를 확인. grip 보정은 마지막 수단.
- 칼날 값은 Action Timeline 창 Scene 미리보기(노란 선)로 확인하며 조정 ([Combat](Combat.md)). 플레이 중 값 변경은 무기 교체 시 반영.
  - Sword1H: Base (0, 0.2, 0), Tip (0, 1, 0) / Sword2H: Base (0, 0.65, 0), Tip (0, 1.88, 0) / Radius 0.1
- 적(스켈레톤 등)은 `EquipmentVisual` + `WeaponData`를 그대로 재사용 가능.
- 클립 Root Transform은 Rotation/Y/XZ 모두 **Bake Into Pose ✓ + Based Upon: Original** (FBX 재추출 시 .anim에 재적용).
- 필드별 역할·권장값: [인스펙터 가이드](../InspectorGuide.md).
- 아웃라인이 각진 모서리에서 끊기면 두께를 줄인다 (부드러운 법선 굽기는 추후 검토).
- 서드파티 원본(FBX·이펙트)은 수정하지 않고 티어 외형은 런타임에 덧붙인다.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-10-01 | 최초 작성 (무기/방패 데이터, 손 소켓 장착, 무기 교체, 종류별 콤보·대기 모션, WeaponSocketMask 임포트 절차) |
| 2026-10-01 | Wand/Staff/Bow/Crossbow 추가, 쥐는 손(`gripHand`)·보조 손 점유(`occupiesOffHand`), 보조 장비 일반화(방패/마법서) |
| 2026-10-02 | `WeaponData` 상속 허용(RangedWeaponData, MagicWeaponData), `allowsShield` 추가, 클립 Root Transform 설정 |
| 2026-10-02 | 칼날 궤적 판정용 `bladeBase` / `bladeTip` / `bladeRadius` 추가 |
| 2026-10-06 | 장비 = 아이템(`ItemData` 상속)·개체 장착, 무기 순환 삭제(퀵슬롯), 등급·능력치 범위(공격력/마법력/방어력), 방패 방어력, 보조 장비 비활성 규칙, 무기 인스펙터, `AmmoPouch` 삭제 |
| 2026-10-07 | 티어(T1~T5, 데이터 하나로 공용): 접두어·능력치 배율, 티어 외형(불씨 아우라·테두리 발광·선택 머티리얼/색조), 티어 이펙트 생성 메뉴, 무기 인스펙터 섹션을 Foldout으로 변경 |
| 2026-10-07 | 티어를 0부터로 변경 (T0~T4, 데이터·화면 표시 동일) |
