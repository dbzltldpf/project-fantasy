# Player

## 개요
카메라 기준 이동, 걷기/달리기, 점프, 회피(무적·쿨타임), 콤보 공격, 방패 가드, 원거리 조준·발사, 마법 시전, 줍기·아이템 사용, 퀵슬롯 장착, 피격/사망을 처리하는 플레이어 캐릭터. 기준 캐릭터는 KayKit **Rogue**.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [PlayerController.cs](../../Assets/Project/Scripts/Player/PlayerController.cs) | 컴포넌트 조립, 상태 머신 구동, 카메라 기준 이동 벡터 계산, 체력/무기 이벤트 → 상태 전이, 부활(`Revive`: 체력 회복 → 순간이동 → 이동 상태) |
| [PlayerInputHandler.cs](../../Assets/Project/Scripts/Player/PlayerInputHandler.cs) | 입력 액션 에셋에서 이름으로 액션 탐색, 공격/점프/회피/줍기/퀵슬롯 선입력 버퍼, 메뉴 중 게임플레이 입력 차단 |
| [PlayerMotor.cs](../../Assets/Project/Scripts/Player/PlayerMotor.cs) | CharacterController 이동, 가속/감속, 중력, 접지, 회전, 순간이동(`Teleport`: CharacterController 끄고 이동 후 속도 초기화) |
| [PlayerAnimator.cs](../../Assets/Project/Scripts/Player/PlayerAnimator.cs) | 애니메이터 상태 재생(해시 캐싱, 중복 CrossFade 방지), 무기별 대기 모션, 상태 누락 검증 (상태 이름은 `PlayerAnimationData`), 자세 고정은 `AnimatorPoseHold`(적과 공용) |
| [PlayerLoadout.cs](../../Assets/Project/Scripts/Player/PlayerLoadout.cs) | 장착 무기·보조 장비 **개체** 적용, 무기와 맞지 않는 보조 장비 비활성, `IWeaponWielder` 구현(현재 무기 종류, 없으면 Unarmed) ([Weapon](Weapon.md), [Inventory](Inventory.md)) |
| [PlayerRangedWeapon.cs](../../Assets/Project/Scripts/Player/PlayerRangedWeapon.cs) / [PlayerAmmoVisual.cs](../../Assets/Project/Scripts/Player/PlayerAmmoVisual.cs) | 원거리 발사·장전 규칙, 화살 표시 ([Ranged Combat](RangedCombat.md)) |
| [PlayerMagicCaster.cs](../../Assets/Project/Scripts/Player/PlayerMagicCaster.cs) | 마법 쿨타임·데미지 ([Magic](Magic.md)) |
| [PlayerAimPresenter.cs](../../Assets/Project/Scripts/Player/PlayerAimPresenter.cs) | 조준 뷰 → 카메라 숄더뷰·조준점, 조준 중 활 당김 게이지 |
| [AimHeadFader.cs](../../Assets/Project/Scripts/Player/AimHeadFader.cs) | 조준 뷰 동안 머리 메시를 디더 반투명 머티리얼로 교체 (숄더뷰에서 화면 가림 방지) |
| [RangedDraw.cs](../../Assets/Project/Scripts/Player/RangedDraw.cs) | 활 당김 비율 (`PlayerController.Draw`, [Ranged Combat](RangedCombat.md)) |
| [PlayerItemHandler.cs](../../Assets/Project/Scripts/Player/PlayerItemHandler.cs) / [PlayerInteractor.cs](../../Assets/Project/Scripts/Player/PlayerInteractor.cs) | 아이템 장착·사용·버리기·줍기, 주변 아이템 탐색 ([Inventory](Inventory.md)) |
| [PlayerMenuPresenter.cs](../../Assets/Project/Scripts/Player/PlayerMenuPresenter.cs) / [PlayerHudPresenter.cs](../../Assets/Project/Scripts/Player/PlayerHudPresenter.cs) | 인벤토리·숙련도 창 ↔ 입력·카메라 (하나라도 열리면 차단, 일시정지 중 Tab·K 무시), HUD(체력 상태바·퀵슬롯·화살·안내) 연결 |
| [PausePresenter.cs](../../Assets/Project/Scripts/Player/PausePresenter.cs) | ESC 일시정지·해제: `Time.timeScale` 0, 게임플레이 입력·시점 정지, 일시정지 메뉴 표시. 재개 시 `GameSpeed`(배속)로 복귀, 인벤토리·숙련도 창이 열려 있으면 ESC 무시 |
| [PlayerMasteryRewarder.cs](../../Assets/Project/Scripts/Player/PlayerMasteryRewarder.cs) / [WeaponMastery.cs](../../Assets/Project/Scripts/Mastery/WeaponMastery.cs) | 처치 보상 → 장착 무기·보조 장비 숙련 경험치, 숙련 레벨·티어 장착 조건·보너스 ([Mastery](Mastery.md)) |
| [PlayerMovementData.cs](../../Assets/Project/Scripts/Player/Data/PlayerMovementData.cs) | 이동/점프/회피 튜닝 데이터 (SO) |
| [PlayerAnimationData.cs](../../Assets/Project/Scripts/Player/Data/PlayerAnimationData.cs) | 애니메이터 상태 이름·파라미터·전환 시간·이동 모션 분기 비율·클립 속도 (SO) |
| [States/](../../Assets/Project/Scripts/Player/States/) | `PlayerStateBase`, `Locomotion`, `Air`, `Attack`, `Guard`, `Aim`, `RangedFire`, `Reload`, `Cast`, `SpellTarget`, `PickUp`, `UseItem`, `Dodge`, `Hit`, `Dead` |

## 동작 흐름
### 프레임 처리 순서
```
PlayerController.Update
 ├─ StateMachine.Tick   // 상태가 입력을 해석해 모터에 목표 속도/회전 지시
 └─ PlayerMotor.Tick    // 가속·중력 적용 후 CharacterController.Move 1회
ThirdPersonCamera.LateUpdate  // 이동이 끝난 뒤 카메라 갱신
```

### 상태 전이
```mermaid
flowchart LR
    Start((시작)) --> Locomotion
    Locomotion -->|"공격 입력"| Attack
    Attack -->|"예약 입력 + 전이 시점"| Attack
    Attack -->|"단계 종료"| Locomotion
    Locomotion -->|"가드 홀드 (방패)"| Guard
    Guard -->|"공격 입력"| Attack
    Guard -->|"가드 해제"| Locomotion
    Locomotion -->|"점프 / 코요테 타임 초과"| Air
    Air -->|"착지"| Locomotion
    Locomotion -->|"회피 입력 + 쿨 완료"| Dodge
    Guard -->|"회피 입력 + 쿨 완료"| Dodge
    Aim -->|"회피 입력 + 쿨 완료"| Dodge
    Reload -->|"회피 입력 + 쿨 완료"| Dodge
    RangedFire -->|"발사 후 회피"| Dodge
    Dodge -->|"종료 + 우클릭 유지"| Guard
    Dodge -->|"종료 + 우클릭 유지"| Aim
    Dodge -->|"종료"| Locomotion
    Any["모든 상태<br/>(Dead 제외)"] -->|"피격"| Hit
    Hit -->|"재피격 → 재진입"| Hit
    Hit -->|"경직 종료"| Locomotion
    Any -->|"체력 0"| Dead
```

| 상태 | 역할 |
|---|---|
| Locomotion | 지상 이동, 줍기(E), 퀵슬롯(1~8) 장착·사용, 미장전 석궁이면 멈춰 있을 때만 자동 장전. 애니메이션은 수평 속도로 Idle/Walk/Run 자동 분기 |
| PickUp / UseItem | 줍기 모션 후 가방에 추가 / 소모품 사용(느린 이동, 효과 시점에 소모) ([Inventory](Inventory.md)) |
| Air | 점프·낙하. 공중 가속(airAcceleration)으로 제어, 코요테 타임 내 점프 허용 |
| Attack | 조준점 방향(카메라 기준, `GetAimFacingDirection`)으로 즉시 회전 → `ActionPlayer`로 액션 타임라인 재생(`IActionContext` 구현: 타격 구간·콤보 창·전진) → 예약 입력 시 전이 프레임에 다음 액션, 캔슬 구간에서 회피(최우선)·이동(다음 타가 없거나 콤보 창이 닫힌 뒤)으로 후딜레이 캔슬 ([Combat](Combat.md)) |
| Dodge | 진입 시 입력 방향(없으면 뒤)으로 고정해 `dodgeSpeed`로 `dodgeDuration` 동안 이동. 몸 방향 기준 4방향 애니, 회피 내내 무적. 종료 시 우클릭 유지면 `TryStartSecondaryAction`(가드·조준), 아니면 Locomotion |
| Guard | 방패 가드 홀드. 느린 이동(guardMoveSpeed), 막으면 Block_Hit + 넉백 경직, 가드 중 공격 가능 |
| Aim / RangedFire / Reload | 활·석궁 조준·발사·장전 ([Ranged Combat](RangedCombat.md)) |
| Cast / SpellTarget | 마법 시전 / Staff 마법진 조준 ([Magic](Magic.md)) |

**좌클릭·우클릭 분기** (`PlayerStateBase.TryStartPrimaryAction / TryStartSecondaryAction`)
| 장착 무기 | 좌클릭 | 우클릭 홀드 |
|---|---|---|
| 근접 / 맨손 | 콤보 공격 | 방패 가드 |
| 활 / 양손 석궁 | 발사 | 조준 모드 |
| 한손 석궁 | 발사 | 방패 가드 |
| Wand | 마법탄 | 방패 가드 |
| Staff | (조준 모드에서) 시전 | 마법진 조준 모드 |

- 상태마다 `UsesAimView`를 선언하고, 상태 전이 시 `PlayerController.AimViewChanged`로 숄더뷰·조준점을 전환한다.
| Hit | 입력 버퍼 초기화, 가해 방향을 바라보며 넉백 후 경직 |
| Dead | 종료 상태. 입력 무시, 사망 애니메이션 |

### 이동 속도
- 기본은 **걷기(WalkSpeed)**, 달리기 버튼(Sprint)을 누르고 있으면 **달리기(RunSpeed)**, 가드 중은 **GuardMoveSpeed**.
- 최종 속도 = 기본 속도 × 입력 크기 → 패드 스틱은 기울기에 비례, 키보드는 항상 최대.
- 이동 방향은 카메라 forward/right를 수평 투영해 계산 (`PlayerController.GetCameraRelativeMove`).
- 걷기 → 달리기 모션 전환 속도 = 걷기~달리기 속도 사이 `runThresholdBlend` 지점 (5~8의 0.5 = 6.5). 시작 시 `PlayerAnimator.SetSpeedRange`로 1회 계산 → 이동 속도를 바꿔도 따로 맞출 필요 없음.
- 걷기·달리기 재생 속도 = 실제 수평 속도 ÷ 클립 속도(`walkAnimationSpeed` 2.5 / `runAnimationSpeed` 4) → 발이 미끄러지지 않음. 조준 중 앞으로 걷기도 같은 배율 적용.

### 입력
| 액션 | 키보드/마우스 | 게임패드 | 처리 |
|---|---|---|---|
| Move | WASD | 왼쪽 스틱 | 매 프레임 읽기 |
| Attack | 마우스 왼쪽 | West 버튼 | 선입력 버퍼 |
| Jump | Space | South 버튼 | 선입력 버퍼 |
| Dodge | Left Ctrl | East 버튼 | 선입력 버퍼 (Locomotion·Guard·Aim에서 쿨 완료 시) |
| Sprint | Left Shift | 왼쪽 스틱 누름 | 홀드 |
| Secondary (구 Guard) | 마우스 오른쪽 | 왼쪽 트리거 | 홀드 (무기에 따라 가드/조준) |
| Interact | E | North | 선입력 버퍼 (줍기) |
| Inventory | Tab | Start | 인벤토리 창 토글 (메뉴 중에도 동작) |
| Mastery | K | — | 숙련도 창 토글 (메뉴 중에도 동작) |
| QuickSlot | 1~8 | — | 선입력 버퍼 (바인딩 순서 = 슬롯 번호) |
| Zoom | 마우스 휠 | — | 카메라 줌 ([Camera](Camera.md)) |

### 선입력 / 코요테 타임
- 공격·점프·회피·줍기·퀵슬롯 입력은 `inputBufferTime`(0.2초) 동안 보관 후 `Consume*()` 시 소모.
- 지면을 벗어난 뒤 `coyoteTime`(0.15초)까지는 지상 판정 유지 → 경사·계단에서 Air 전환 떨림 방지 + 늦은 점프 허용.

## 데이터 파라미터
### PlayerMovementData
| 필드 | 기본값 | 의미 |
|---|---|---|
| walkSpeed | 5 | 기본 이동 속도 (m/s), 적 배회·추격 속도의 기준 ([Enemy](Enemy.md)) |
| runSpeed | 8 | 달리기 버튼 입력 시 속도, 적 추격 속도의 기준 |
| guardMoveSpeed | 1.2 | 가드 중 이동 속도 |
| aimMoveSpeed | 1.5 | 조준 모드 이동 속도 |
| acceleration / deceleration | 30 / 40 | 지상 가속·감속 (m/s²) |
| airAcceleration | 8 | 공중 가감속 |
| rotationSpeed | 720 | 회전 속도 (°/s) |
| jumpHeight | 1.2 | 점프 높이 (m), 초기 속도 = √(2gh) |
| gravity | 25 | 중력 가속도 |
| groundedStickForce | 2 | 접지 유지용 하강 속도 |
| maxFallSpeed | 50 | 최대 낙하 속도 |
| coyoteTime | 0.15 | 코요테 타임 (초) |
| dodgeSpeed | 7 | 회피 이동 속도 (m/s, 달리기보다 빠르게) |
| dodgeDuration | 0.4 | 회피 시간 (초, 클립 길이와 맞춤) |
| dodgeCooldown | 1 | 회피 종료 후 다시 회피 가능까지 (초) |

### 컴포넌트 인스펙터 값
| 컴포넌트 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| PlayerInputHandler | inputBufferTime | 0.2 | 선입력 유지 시간 |
| PlayerAnimationData | idleSpeedThreshold / runThresholdBlend | 0.1 / 0.5 | 대기 → 걷기 분기 속도(m/s) / 걷기(0)~달리기(1) 속도 사이 달리기 모션 전환 지점 |
| PlayerAnimationData | walk / runAnimationSpeed | 2.5 / 4 | 재생 배율 1일 때 발이 맞는 이동 속도 (m/s) |
| PlayerAnimationData | locomotion / actionCrossFade | 0.15 / 0.1 | 전환 시간 |
| PlayerAnimationData | pose / action / locomotionSpeedParameter | PoseSpeed / ActionSpeed / LocomotionSpeed | 조준 자세 고정 / 공격 재생 속도 / 걷기·달리기 재생 속도 Float 파라미터 |
| PlayerController | minAimFacingDistance | 1.5 | 조준 지점이 가까우면 카메라 정면을 바라봄 |

### 조준 자세 고정
- 조준 대기 클립의 **마지막 프레임이 조준 자세**면 Loop만 끄면 된다 (활, 양손 석궁).
- 들어올림→내림이 한 클립인 경우(`Ranged_Magic_Raise`): 상태 Speed Multiplier를 `PoseSpeed`로 연결하고 데이터의 hold time(정규화 시간)을 지정 → 도달 시 `PoseSpeed = 0`, 다른 모션 전환 시 1로 복구.

## 에디터 설정
1. **애니메이터(`Player.controller`)** – 아래 상태가 있어야 한다. 전이 화살표는 불필요(코드에서 CrossFade). 무기별 콤보·대기 상태는 [Weapon](Weapon.md) 참고.

   | 용도 | 상태 이름 | 클립 출처 |
   |---|---|---|
   | Idle / Walk / Run | `Idle_A` / `Walking_B` / `Running_B` | Rig_Medium_General, MovementBasic |
   | 공중 | `Jump_Idle` | Rig_Medium_MovementBasic |
   | 피격 / 사망 | `Hit_A` / `Death_A` | Rig_Medium_General |
   | 가드 / 막기 | `Melee_Blocking` / `Melee_Block_Hit` | Rig_Medium_CombatMelee |
   | 조준 이동 | `Walking_Backwards` / `Running_Strafe_Left` / `Running_Strafe_Right` (Loop) | Rig_Medium_MovementAdvanced |
   | 회피 | `Dodge_Forward` / `Dodge_Backward` / `Dodge_Left` / `Dodge_Right` (0.4초, Root Bake Into Pose) | `Animation/Clip/Movement/` (재추출 .anim) |

   Float 파라미터 `PoseSpeed`(기본 1) 추가, 자세 고정이 필요한 조준 대기 상태의 Speed Multiplier에 연결.
   Float 파라미터 `ActionSpeed`(기본 1) 추가, 모든 공격 상태의 Speed Multiplier에 연결.
   Float 파라미터 `LocomotionSpeed`(기본 1) 추가, `Walking_B`·`Running_B` 상태의 Speed Multiplier에 연결.

2. **Rogue 프리팹** – `PlayerController` 추가 시 필요한 컴포넌트 자동 추가 (Input, Motor, Animator, MeleeAttacker, Health, PlayerLoadout, ShieldGuard, PlayerRangedWeapon, RangedAttacker, PlayerAmmoVisual, PlayerMagicCaster, SpellCaster, HitStop, CharacterController). 이미 있는 오브젝트는 RequireComponent가 자동 추가되지 않으므로 수동 추가.
   - CharacterController Height/Center를 캐릭터 크기에 맞춤
   - InputHandler **Action Asset**에 `InputSystem_Actions` 1개 연결 (Player 맵의 액션을 이름으로 탐색, 이름이 다르면 시작 시 에러), PlayerAnimator **Data**에 `PlayerAnimationData` 연결
   - Movement / HitReaction(`PlayerHitReactionData`) 데이터 연결 (콤보는 무기 데이터에서 가져옴), Layer를 **Player**로 지정

3. **조준 머리 페이드** – Rogue에 `AimHeadFader` → Head Renderers(머리·모자 메시), Fade Material(`RogueAimFade`, `AimFade` Shader Graph: Lit·Opaque·Alpha Clipping·Dither). 반투명(Transparent)은 머리 안쪽 면이 비쳐 보여 디더로 처리.

## 주의사항 / 확장 포인트
- 애니메이터 상태 이름은 **정확히 일치**해야 한다. 누락 시 시작할 때 콘솔에 `[PlayerAnimator] 애니메이터에 'XXX' 상태가 없습니다.` 에러 (에디터/개발 빌드 전용 `[Conditional]` 검증, 보유 무기의 콤보·대기 상태 포함).
- 걷기·달리기 클립은 제자리 애니메이션(루트 모션 없음)이라 클립 속도를 FBX에서 읽을 수 없다. 플레이 중 `walk/runAnimationSpeed`를 바꿔 발이 바닥에 붙어 보이는 값으로 맞춘다 (앞으로 미끄러지면 낮추고, 뒤로 끌리면 높임). 적과 같은 클립이라 같은 값 사용.
- 전환 속도는 시작 시 1회 계산이라 플레이 중 이동 속도를 바꾸면 다시 플레이해야 반영된다 (재생 배율은 즉시 반영).
- 루트 모션은 사용하지 않음 (`applyRootMotion = false`), 이동은 전부 PlayerMotor가 담당.
- 가드 중 이동은 전신 가드 모션이라 발이 미끄러져 보임 → 상체 레이어(Avatar Mask) 도입 시 개선.
- 회피 무적은 `Health.SetInvincible`(강제 무적 플래그)로 켜고 `Exit`에서 끈다 → 피격·사망으로 끊겨도 무적이 남지 않음. 쿨타임도 `Exit`에서 기록.
- 이동 중에는 몸이 입력 방향을 보므로 항상 앞 회피 애니가 나온다. 좌·우·후방은 몸 방향이 고정된 가드·조준 중이나 입력 없이(뒤로) 회피할 때 나온다.
- `PlayerStateBase.HasMoveInput`: 이동 입력 판단 공용 (공격 후딜 캔슬·장전 취소·자동 장전). `TryHandleQuickSlot`: 퀵슬롯 처리 공용 (Locomotion·Reload).
- `PlayerController.CanUseItems`: 장비 변경·아이템 사용 가능 상태 = Locomotion·Reload (그 외 상태에서는 "지금은 사용할 수 없습니다"). 상태 비교는 `IsCurrentState`.
- 피격 경직은 플레이어 `PlayerHitReactionData`(0.45초), 적 `EnemyHitReactionData`(0.7초)로 분리 ([Combat](Combat.md)).
- `AimHeadFader`는 `sharedMaterial`만 교체 → 인스턴스 머티리얼이 생기지 않고, 비활성화 시 원래 머티리얼로 복구.
- 회피 진입은 `PlayerStateBase.TryStartDodge`(쿨 확인 후 입력 소비 → 쿨 중 입력은 버퍼에 남음). 공격 중 회피 캔슬은 지원하지 않는다.
- 예정: 주목(락온), 저스트 회피, 스태미나, 점프 시작/착지 애니메이션.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-09-30 | 최초 작성 (이동, 점프, 3단 콤보, 피격, 사망) |
| 2026-09-30 | 기본 걷기 / 달리기 버튼 시 달리기로 변경 (`sprintSpeed`, `walkInputThreshold` 제거), 애니메이터 상태 누락 검증 추가 |
| 2026-10-01 | Guard 상태, 무기 교체 입력, `PlayerLoadout` 연동, 무기별 대기 모션, 콤보 입력 예약 방식 |
| 2026-10-02 | 원거리(Aim/RangedFire/Reload)·마법(Cast/SpellTarget) 상태, 좌/우클릭 무기별 분기, 조준 뷰 이벤트, 스트레이프 이동, 조준 자세 고정, Guard 입력 → Secondary |
| 2026-10-02 | Attack 상태를 액션 타임라인(`ActionPlayer`) 기반으로 전환, `HitStop` 중 상태·이동 정지, `ActionSpeed` 파라미터 |
| 2026-10-06 | 줍기·소모품 사용 상태, 퀵슬롯·줍기·인벤토리·줌 입력, 무기 순환 입력 삭제, 입력 액션 에셋 일괄 탐색, `PlayerAnimationData` 분리, 아이템·HUD Presenter |
| 2026-10-07 | 무기 숙련도(`WeaponMastery`·`PlayerMasteryRewarder`), 숙련도 창 입력(K), 숙련 보너스가 공격력·마법력·방패 방어력에 반영, 숙련 부족 장비 장착 거부 |
| 2026-10-07 | HUD 체력 상태바 연결 (`PlayerHudPresenter.healthBar`), 플레이어 버리기가 `WorldItem.Spawn` 사용 |
| 2026-10-08 | 조준 자세 고정 로직을 `Utils/AnimatorPoseHold`로 분리 (동작 동일) |
| 2026-11-13 | 회피(Dodge): 입력 방향 4방향 회피, 회피 중 무적, 쿨타임, Locomotion·Guard·Aim에서 진입, 종료 시 우클릭 유지면 가드·조준 복귀 |
| 2026-11-17 | 이동 속도 표기를 실제 값(걷기 5 / 달리기 8)으로 수정, 적 이동 속도의 기준이 됨 |
| 2026-11-17 | 달리기 모션 전환 속도를 걷기~달리기 속도 비율(`runThresholdBlend`)로 자동 계산(`runSpeedThreshold` 제거), 걷기·달리기 재생 속도를 이동 속도에 맞춤(`LocomotionSpeed`) |
| 2026-11-18 | `PlayerLoadout`이 `IWeaponWielder` 구현 (적 무기 제한 판정) |
| 2026-11-19 | `PlayerMotor.Teleport` (스테이지 이동, [Stage](Stage.md)) |
| 2026-11-19 | `PlayerController.Revive` (사망 후 스테이지 재시작, [Stage](Stage.md)) |
| 2026-11-19 | 공격 후딜레이 캔슬: 캔슬 구간에서 회피·이동으로 공격 상태 종료 (`TryCancel`, `CanChainCombo`) |
| 2026-11-19 | ESC 일시정지(`PausePresenter`, 입력 `Pause` 액션: Escape / Start), 일시정지 중 Tab·K 무시, 숙련도 창 연결 필수로 변경 |
| 2026-11-25 | 근접 공격을 조준점(카메라) 방향으로, 조준 머리 페이드(`AimHeadFader`), 석궁 장전 중 회피·이동·무기 교체, 발사 후 회피 캔슬, `HasMoveInput`·`TryHandleQuickSlot`·`CanUseItems` 공용화, 활 당김(`Draw`), 플레이어 피격 경직 0.45초(전용 에셋) |
