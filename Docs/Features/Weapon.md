# Weapon

## 개요
손에 드는 무기와 보조 장비(방패·마법서)의 데이터, 모델 장착, 무기 교체. 무기 종류별로 쥐는 손·콤보·대기 모션·판정 범위가 다르다. 플레이어·적 공용.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [WeaponType.cs](../../Assets/Project/Scripts/Weapon/WeaponType.cs) | `Unarmed` `OneHanded` `TwoHanded` `Wand` `Staff` `Bow` `Crossbow` |
| [EquipHand.cs](../../Assets/Project/Scripts/Weapon/EquipHand.cs) | `Right` / `Left` (쥐는 손) |
| [EquipmentData.cs](../../Assets/Project/Scripts/Weapon/Data/EquipmentData.cs) | 장비 공통 베이스 (이름, 모델 프리팹, 손 위치/회전 보정) |
| [WeaponData.cs](../../Assets/Project/Scripts/Weapon/Data/WeaponData.cs) | 무기 종류, 쥐는 손, 보조 손 점유, 공격력, 콤보, 판정 범위, 대기 모션 (SO) |
| [OffHandData.cs](../../Assets/Project/Scripts/Weapon/Data/OffHandData.cs) | 보조 장비 베이스, `CanEquipWith(weapon)`로 동시 장착 가능 여부 판정 |
| [ShieldData.cs](../../Assets/Project/Scripts/Weapon/Data/ShieldData.cs) | 방패: 가드 각도, 막기 넉백/경직 (SO) |
| [SpellbookData.cs](../../Assets/Project/Scripts/Weapon/Data/SpellbookData.cs) | 마법서: 마법 위력 배율 (SO) |
| [EquipmentVisual.cs](../../Assets/Project/Scripts/Weapon/EquipmentVisual.cs) | `handslot.r` / `handslot.l`에 모델 부착, 미리 생성 후 활성/비활성 전환 |
| [PlayerLoadout.cs](../../Assets/Project/Scripts/Player/PlayerLoadout.cs) | 플레이어 보유 무기/보조 장비, 무기 순환, 장착 적용 |
| [TransformExtensions.cs](../../Assets/Project/Scripts/Utils/TransformExtensions.cs) | `FindDeepChild` (손 본 자동 탐색) |

## 동작 흐름
### 장착 (`PlayerLoadout.EquipIndex`)
```
무기 선택
 ├─ EquipmentVisual.Show(gripHand)   // 쥐는 손에 모델 활성 (활은 왼손 → 오른손 비움)
 ├─ MeleeAttacker.SetHitShape        // 무기별 근접 판정 위치·반경
 ├─ 보조 장비 갱신                     // CanEquipWith == false 면 숨김, 방패일 때만 가드 활성
 └─ WeaponChanged → PlayerAnimator.SetIdleState (무기별 대기 모션)
```
- 보유 무기가 없으면 맨손(`unarmedWeapon`) 장착.
- 모델은 `Awake`에서 쥐는 손 슬롯에 한 번 생성해 캐시, 교체 시 `SetActive`만 전환 → 교체 중 GC/Instantiate 없음.

### 보조 장비 동시 장착 규칙
| 무기 | 방패 | 마법서 |
|---|---|---|
| 맨손, 한손 근접, 한손 석궁 | ✅ | ❌ |
| Wand / Staff (한손 마법) | ✅ | ✅ |
| 양손검, 양손 석궁 (`occupiesOffHand`) | ❌ | ❌ |
| 활 (왼손) | ❌ | ❌ |

- 규칙은 각 `OffHandData.CanEquipWith`가 판정 → 새 보조 장비는 클래스만 추가.
- 보조 장비 교체(방패 ↔ 마법서)는 인벤토리 기능에서 구현 예정. 현재는 `startingOffHand` 하나.

### 무기 교체
- 입력: **Next(키보드 2 / 패드 D-pad 오른쪽)**, **Previous(1 / D-pad 왼쪽)**
- 지상 Locomotion 상태에서만 적용 (공격·가드·공중 중 불가), 선입력 버퍼 0.2초
- 보유 무기가 2개 이상일 때만 순환

### 데미지
`최종 데미지 = WeaponData.attackPower × AttackStep.damageMultiplier` (반올림)
- 같은 종류 무기는 콤보 에셋 하나를 공유하고 공격력만 다르게 설정.
- 원거리·마법 무기(`HasMeleeHit == false`)는 근접 판정 없이 모션만 재생 (발사체는 원거리 전투 기능에서 처리).

## 데이터 파라미터
### WeaponData
| 필드 | 기본값 | 의미 |
|---|---|---|
| displayName | — | 게임 내 표시 이름 (UI 용, 현재 미사용) |
| modelPrefab | — | 무기 모델 (맨손은 비움) |
| gripPosition / gripRotation | 0 / 0 | 손 소켓 기준 보정 (KayKit 무기는 0 유지) |
| weaponType | OneHanded | 무기 종류 |
| gripHand | Right | 쥐는 손 (활은 Left) |
| occupiesOffHand | false | 보조 손까지 사용 (양손검, 양손 석궁) |
| attackPower | 10 | 공격력 |
| comboData | — | 사용할 콤보 (AttackComboData) |
| hitOffset / hitRadius | (0, 1, 1) / 0.8 | 근접 판정 구체 (캐릭터 로컬) |
| idleStateName | Idle_A | 이 무기를 들었을 때 대기 모션 |

### ShieldData / SpellbookData
| 에셋 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| Shield | guardAngle | 120 | 정면 기준 방어 각도 (전체 각도) |
| Shield | blockKnockbackSpeed / blockStunDuration | 2 / 0.3 | 막았을 때 밀림·경직 |
| Spellbook | magicPowerMultiplier | 1.2 | 마법 위력 배율 (원거리 전투 기능에서 적용 예정) |

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
   | 지팡이 | `wand` | Wand | Right | ❌ | `Idle_A` | `Ranged_Magic_Shoot` |
   | 스태프 | `staff` | Staff | Right | ❌ | `Idle_A` | `Ranged_Magic_Spellcasting` (공격력↑, duration↑) |
   | 활 | `bow` | Bow | **Left** | (자동) | `Ranged_Bow_Idle` | `Ranged_Bow_Release` |
   | 한손 석궁 | `crossbow_1handed` | Crossbow | Right | ❌ | `Idle_A` | `Ranged_1H_Shoot` |
   | 양손 석궁 | `crossbow_2handed` | Crossbow | Right | ✅ | `Ranged_2H_Aiming` | `Ranged_2H_Shoot` |

3. **애니메이터** – 위 콤보·대기 상태를 `Player.controller`에 추가 (상태 이름 = 클립 이름)
4. **Rogue 프리팹/씬** – `PlayerLoadout`, `EquipmentVisual`, `ShieldGuard` 추가
   - EquipmentVisual 손 본은 컴포넌트 추가 시 자동 탐색
   - PlayerLoadout에 Unarmed / Starting Weapons / Starting Off Hand 지정

## 주의사항 / 확장 포인트
- `unarmedWeapon`은 **필수**. 비어 있으면 시작 시 콘솔 에러.
- 무기 모델이 손에서 어긋나면 먼저 클립 마스크(1번)를 확인. grip 보정은 마지막 수단.
- 적(스켈레톤 등)은 `EquipmentVisual` + `WeaponData`를 그대로 재사용 가능.
- 예정: 원거리 전투(조준·발사·화살 수량·석궁 장전), 보조 장비 교체/인벤토리, 필드 무기 줍기.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-10-01 | 최초 작성 (무기/방패 데이터, 손 소켓 장착, 무기 교체, 종류별 콤보·대기 모션, WeaponSocketMask 임포트 절차) |
| 2026-10-01 | Wand/Staff/Bow/Crossbow 추가, 쥐는 손(`gripHand`)·보조 손 점유(`occupiesOffHand`), 보조 장비 일반화(방패/마법서) |
