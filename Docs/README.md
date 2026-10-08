# Project Fantasy 기능 문서

젤다의 전설(3D 오픈월드)을 레퍼런스로 한 액션 어드벤처. 기능이 추가/변경될 때마다 해당 문서와 아래 변경 이력을 함께 갱신한다.

## 목차
| 문서 | 내용 |
|---|---|
| [Core](Features/Core.md) | 상태 머신, 피해/방어 인터페이스, 유틸 등 공용 코드 |
| [Player](Features/Player.md) | 플레이어 입력·이동·애니메이션·상태(FSM)·가드 |
| [Combat](Features/Combat.md) | 체력·방어, 액션 타임라인(프레임 이벤트), 칼날 궤적 판정, 히트스톱, 콤보/피격 데이터, 이펙트 풀링, 처치 보상 |
| [Weapon](Features/Weapon.md) | 무기·보조 장비 데이터, 손 소켓 장착, 티어(외형 아우라·테두리 발광)·등급·능력치, 보조 장비 규칙, 애니메이션 마스크 임포트 |
| [Mastery](Features/Mastery.md) | 무기 종류별 숙련도(알비온식), 처치 경험치, 티어 장착 조건, 숙련 보너스, 숙련도 창(K) |
| [Enemy](Features/Enemy.md) | 적 4종(근접 Minion·Warrior, 사격 Rogue, 시전 Mage), 전투 방식 드롭다운: 감지·추격·공격·막기·반격·귀환, 스폰·리스폰, 드랍, 머리 위 체력바, 애니메이터 생성 |
| [Inventory](Features/Inventory.md) | 아이템·장비 개체, 가방·퀵슬롯, 줍기·버리기·사용, 실시간 인벤토리 창, 등급·랜덤 능력치 |
| [Ranged Combat](Features/RangedCombat.md) | 활·석궁 조준·발사·장전, 화살 수량, 포물선 투사체·경로 미리보기 |
| [Magic](Features/Magic.md) | Wand 직선 마법탄, Staff 지면 범위 마법, 쿨타임, 마법서 배율 |
| [Camera](Features/Camera.md) | 3인칭 오빗 카메라, 휠 줌, 조준 숄더뷰 |
| [UI](Features/UI.md) | 조준점, 데미지 숫자, 인벤토리·숙련도 창, 퀵슬롯 바·플레이어 상태바·HUD, 적 체력바 |
| [인스펙터 설정 가이드](InspectorGuide.md) | 에셋·컴포넌트별 필드 역할과 넣을 값, 새 무기 만들기 체크리스트 |
| [Git 워크플로우](GitWorkflow.md) | 브랜치·커밋 규칙, `/feature-start` `/push` `/feature-finish` |

## 스크립트 폴더 구조
기능별 폴더가 기본이며, 여러 기능이 공유하는 범용 코드만 `Core`/`Utils`에 둔다.
```
Assets/Project/Scripts/
├─ Core/          StateMachine/, Interfaces/, Types/
├─ Player/        PlayerController, Input, Motor, Animator, Loadout, RangedWeapon, AmmoVisual, MagicCaster, ItemHandler, Interactor, MasteryRewarder, Presenters(Aim/Menu/Hud), States/, Data/
├─ Combat/        Health, ShieldGuard, MeleeAttacker, WeaponTrace, HitStop, RangedAttacker, TrainingDummy, KillReward, Action/(Events/, Editor/), Projectile/, Effects/(Editor/), Data/
├─ Weapon/        WeaponType, EquipHand, EquipmentVisual, Data/, Editor/
├─ Items/         ItemInstance, WorldItem, ItemNotice, Data/, Effects/, Grade/, Stats/, Tier/, Loot/, Editor/
├─ InventorySystem/  Inventory, ItemStack, QuickSlots, IItemActions, EquipRequirement, Data/
├─ Mastery/       MasteryType, MasteryMapping, WeaponMastery, Data/
├─ Magic/         SpellCaster, GroundTargeting, Data/
├─ CameraSystem/  ThirdPersonCamera, Data/
├─ UI/            CrosshairView, Common/, DamageNumbers/, Inventory/, HUD/, Mastery/, Enemy/, Editor/
├─ Enemy/         EnemyController, Navigator, Perception, Animator, Loadout, Loot, Spawner, HealthBarPresenter, Combat/, States/, Data/, Editor/
└─ Utils/         TransformExtensions, RendererExtensions, AnimatorExtensions, AnimatorPoseHold, FloatRange, PrefabPool, PoolContainers, PoolContainerMap, SubclassSelectorAttribute, Editor/
```
셰이더는 `Assets/Project/Shaders/` (`WeaponOutline`).

## 네임스페이스
`ProjectFantasy.Core` · `ProjectFantasy.Player` · `ProjectFantasy.Combat` · `ProjectFantasy.Weapon` · `ProjectFantasy.Items` · `ProjectFantasy.InventorySystem` · `ProjectFantasy.Mastery` · `ProjectFantasy.Enemy` · `ProjectFantasy.Magic` · `ProjectFantasy.CameraSystem` · `ProjectFantasy.UI` · `ProjectFantasy.Utils`
(에디터 전용: `CombatEditor` · `WeaponEditor` · `ItemsEditor` · `EnemyEditor` · `UIEditor` · `UtilsEditor`)

## 변경 이력
| 날짜 | 문서 | 내용 |
|---|---|---|
| 2026-09-30 | Core, Player, Combat, Camera | 1단계: 이동·점프·3단 콤보·피격/사망·3인칭 카메라 구현 |
| 2026-09-30 | Player | 기본 걷기 / 달리기 버튼 시 달리기로 변경, 애니메이터 상태 누락 검증 추가 |
| 2026-09-30 | GitWorkflow | Git 브랜치/커밋 규칙 및 자동화 커맨드 추가 |
| 2026-09-30 | GitWorkflow | 브랜치 번호 제거(`feature/기능명`), 릴리즈는 SemVer 태그로 구분 |
| 2026-10-01 | Weapon, Combat, Player, Core | 2단계: 무기 장착·교체, 무기 종류별 콤보/대기 모션, 방패 가드, 데미지 배율, 콤보 입력 예약 |
| 2026-10-01 | Weapon | Wand/Staff/Bow/Crossbow 추가, 쥐는 손·보조 손 점유, 보조 장비(방패/마법서) 규칙, WeaponSocketMask 임포트 절차 |
| 2026-10-02 | RangedCombat, Magic, UI, Camera, Combat, Player, Weapon, Core | 3단계: 원거리 전투(조준·발사·장전·투사체·경로 미리보기), 마법(직선 마법탄·지면 범위), 숄더뷰, 조준점, 데미지 숫자, 훈련용 허수아비, 이펙트 풀링, 클립 Root 설정 |
| 2026-10-02 | Combat, Player, Weapon | 액션 타임라인 Phase 1: 프레임 단위 ActionData·이벤트, 칼날 궤적 판정, 히트스톱, 재생 속도 배율, 초 단위 AttackStep 제거 |
| 2026-10-02 | Combat, Weapon | 액션 타임라인 Phase 2: Action Timeline 에디터 창(트랙 드래그 편집, 프레임 스크럽, Scene 뷰 자세·칼날 궤적 미리보기) |
| 2026-10-06 | Inventory, Weapon, Player, Combat, RangedCombat, Magic, Camera, UI, Core, InspectorGuide | 인벤토리(가방·퀵슬롯·줍기·사용·실시간 창), 장비 개체·등급·랜덤 능력치, 방패 방어력, 보조 장비 규칙, 휠 줌, 인스펙터 정리(무기 인스펙터·툴팁·입력 에셋·애니메이션 데이터), 인스펙터 가이드 |
| 2026-10-07 | Mastery, Weapon, Inventory, Player, Combat, RangedCombat, Magic, UI, Core, InspectorGuide | 무기 숙련도(종류별 레벨·처치 경험치·티어 장착 조건·보너스·숙련도 창 K·테스트 옵션), 장비 티어 T1~T5(접두어·배율·불씨 아우라·테두리 발광), 처치 보상, UI Presenters 분리, 풀 부모 폴더(`Pools/…`) |
| 2026-10-07 | Enemy, Combat, Inventory, UI, Player, Magic, InspectorGuide | 근접 적 2종(감지·추격·공격·막기·반격·귀환·리스폰), 드랍 표, 적 머리 위 체력바·플레이어 상태바, 애니메이터·체력바·상태바 생성 메뉴, 레이어 마스크 설정 원칙 |
| 2026-10-07 | Weapon, Inventory, Mastery, InspectorGuide | 티어를 0부터로 변경 (T0~T4, 기존 데이터 값 이전) |
| 2026-10-08 | Enemy, Combat, Core, RangedCombat, Magic, Player, InspectorGuide | 원거리·마법 적(Rogue·Mage), 적 전투 방식 전략 패턴, 중력 보정 사격, 근접 콤보 무작위 횟수, 조준 자세 고정 공용화, 방패에 막힌 화살 미부착(`DamageResult`), 범위 값 Min/Max, 이펙트 색 변환 도구 |
