# Project Fantasy 기능 문서

젤다의 전설(3D 오픈월드)을 레퍼런스로 한 액션 어드벤처. 기능이 추가/변경될 때마다 해당 문서와 아래 변경 이력을 함께 갱신한다.

## 목차
| 문서 | 내용 |
|---|---|
| [Core](Features/Core.md) | 상태 머신, 피해/방어 인터페이스, 유틸 등 공용 코드 |
| [Player](Features/Player.md) | 플레이어 입력·이동·애니메이션·상태(FSM)·가드 |
| [Combat](Features/Combat.md) | 체력·방어, 근접 타격 판정, 콤보/피격 데이터 |
| [Weapon](Features/Weapon.md) | 무기·보조 장비 데이터, 손 소켓 장착, 무기 교체, 애니메이션 마스크 임포트 |
| [Ranged Combat](Features/RangedCombat.md) | 활·석궁 조준·발사·장전, 화살 수량, 포물선 투사체·경로 미리보기 |
| [Magic](Features/Magic.md) | Wand 직선 마법탄, Staff 지면 범위 마법, 쿨타임, 마법서 배율 |
| [Camera](Features/Camera.md) | 3인칭 오빗 카메라, 조준 숄더뷰 |
| [UI](Features/UI.md) | 조준점, 데미지 숫자 |
| [Git 워크플로우](GitWorkflow.md) | 브랜치·커밋 규칙, `/feature-start` `/push` `/feature-finish` |

## 스크립트 폴더 구조
기능별 폴더가 기본이며, 여러 기능이 공유하는 범용 코드만 `Core`/`Utils`에 둔다.
```
Assets/Project/Scripts/
├─ Core/          StateMachine/, Interfaces/, Types/
├─ Player/        PlayerController, Input, Motor, Animator, Loadout, RangedWeapon, AmmoVisual, MagicCaster, AimPresenter, States/, Data/
├─ Combat/        Health, ShieldGuard, MeleeAttacker, RangedAttacker, TrainingDummy, Projectile/, Effects/, Data/
├─ Weapon/        WeaponType, EquipHand, EquipmentVisual, AmmoPouch, Data/
├─ Magic/         SpellCaster, GroundTargeting, Data/
├─ CameraSystem/  ThirdPersonCamera, Data/
├─ UI/            CrosshairView, DamageNumbers/
├─ Enemy/         (예정)
└─ Utils/         TransformExtensions, PrefabPool
```

## 네임스페이스
`ProjectFantasy.Core` · `ProjectFantasy.Player` · `ProjectFantasy.Combat` · `ProjectFantasy.Weapon` · `ProjectFantasy.Magic` · `ProjectFantasy.CameraSystem` · `ProjectFantasy.UI` · `ProjectFantasy.Utils`

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
