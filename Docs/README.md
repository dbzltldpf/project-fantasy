# Project Fantasy 기능 문서

젤다의 전설(3D 오픈월드)을 레퍼런스로 한 액션 어드벤처. 기능이 추가/변경될 때마다 해당 문서와 아래 변경 이력을 함께 갱신한다.

## 목차
| 문서 | 내용 |
|---|---|
| [Core](Features/Core.md) | 상태 머신, 피해 인터페이스 등 공용 코드 |
| [Player](Features/Player.md) | 플레이어 입력·이동·애니메이션·상태(FSM) |
| [Combat](Features/Combat.md) | 체력, 근접 타격 판정, 콤보/피격 데이터 |
| [Camera](Features/Camera.md) | 3인칭 오빗 카메라 |
| [Git 워크플로우](GitWorkflow.md) | 브랜치·커밋 규칙, `/feature-start` `/push` `/feature-finish` |

## 스크립트 폴더 구조
기능별 폴더가 기본이며, 여러 기능이 공유하는 범용 코드만 `Core`/`Utils`에 둔다.
```
Assets/Project/Scripts/
├─ Core/          StateMachine/, Interfaces/, Types/
├─ Player/        PlayerController, PlayerInputHandler, PlayerMotor, PlayerAnimator, States/, Data/
├─ Combat/        Health, MeleeAttacker, Data/
├─ CameraSystem/  ThirdPersonCamera, Data/
├─ Enemy/         (예정)
├─ UI/            (예정)
└─ Utils/         (예정)
```

## 네임스페이스
`ProjectFantasy.Core` · `ProjectFantasy.Player` · `ProjectFantasy.Combat` · `ProjectFantasy.CameraSystem`

## 변경 이력
| 날짜 | 문서 | 내용 |
|---|---|---|
| 2026-09-30 | Core, Player, Combat, Camera | 1단계: 이동·점프·3단 콤보·피격/사망·3인칭 카메라 구현 |
| 2026-09-30 | Player | 기본 걷기 / 달리기 버튼 시 달리기로 변경, 애니메이터 상태 누락 검증 추가 |
| 2026-09-30 | GitWorkflow | Git 브랜치/커밋 규칙 및 자동화 커맨드 추가 |
