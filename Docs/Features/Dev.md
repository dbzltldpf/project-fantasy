# Dev

## 개요
통플레이 테스트용 도구. ESC 일시정지 메뉴 안의 테스트 패널 버튼으로 다음 스테이지 이동, 플레이어 무적, 현재 스테이지 적 전멸, 플레이어 즉사, 배속 순환을 실행한다. 제출 빌드에서는 `PauseMenuView`의 `Show Dev Panel`을 꺼서 패널을 숨긴다.

버튼(View)은 명령 이름만 알리고, 실행은 `DevCommands`가 Stage·Combat·Player를 호출해 처리한다. 다른 기능은 Dev를 모른다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Dev/DevCommand.cs](../../Assets/Project/Scripts/Dev/DevCommand.cs) | 명령 종류 enum (`NextStage`, `ToggleInvincible`, `KillStageEnemies`, `KillPlayer`, `CycleGameSpeed`) |
| [Dev/DevCommands.cs](../../Assets/Project/Scripts/Dev/DevCommands.cs) | `CommandChosen` 구독 → 명령 실행, 실행 후 일시정지 해제 (배속은 유지) |
| [UI/Dev/DevPanelView.cs](../../Assets/Project/Scripts/UI/Dev/DevPanelView.cs) | 버튼 5개 클릭 → `CommandChosen(DevCommand)` 알림, 배속 글자 표시(`SetSpeedLabel`) |

연동: [Stage](Stage.md) `StageDirector.GoToStage`·`CurrentStageIndex`·`StageCount`·`CurrentGroup`, [Combat](Combat.md) `Health.Kill`·`SetInvincible`, [Player](Player.md) `PausePresenter.Resume`·`GameSpeed`.

## 동작 흐름
```
버튼 클릭 → DevPanelView.CommandChosen(명령) → DevCommands.HandleCommand
  NextStage        → 마지막 스테이지가 아니면 StageDirector.GoToStage(현재 + 1)
  ToggleInvincible → 자체 bool 반전 → 플레이어 Health.SetInvincible
  KillStageEnemies → CurrentGroup 아래 켜진 Health 전부 Kill
  KillPlayer       → 무적 bool 해제 → 플레이어 Health.Kill (사망 → 스테이지 재시작)
  CycleGameSpeed   → x1 → x2 → x0.5 순환, PausePresenter.GameSpeed 변경, 배속 글자 갱신
  → (배속 제외) PausePresenter.Resume
```
- 전멸·즉사는 `Health.Kill`을 쓴다. 막기·방어력·무적을 거치지 않고 `Died`를 그대로 발생시켜 처치 목표·재시작이 평소처럼 진행된다.
- 무적 토글은 `Health.IsInvincible`이 아니라 자체 bool로 판단한다 (`IsInvincible`은 피격·회피 무적 시간을 포함). 부활(`RestoreFull`) 시 강제 무적이 풀리므로 즉사 시 bool도 함께 끈다.
- 배속은 일시정지 중에 `Time.timeScale`을 바꾸지 않고 `GameSpeed`만 바꾼다. 재개할 때 반영된다.
- `GetComponentsInChildren`은 꺼진 오브젝트를 건너뛰므로 리스폰 대기 중인 적은 대상에서 빠진다.

## 데이터 파라미터
| 대상 | 필드 | 의미 |
|---|---|---|
| DevCommands | devPanelView / stageDirector / pausePresenter / playerHealth | 버튼 패널 / 스테이지 이동 / 일시정지 해제·배속 / 무적·즉사 대상 |
| DevCommands | SpeedSteps (코드 상수) | 배속 순서 1 → 2 → 0.5 |
| DevPanelView | nextStage / toggleInvincible / killStageEnemies / killPlayer / cycleGameSpeed | 각 버튼 (OnClick은 비움, 코드에서 연결) |
| DevPanelView | speedLabel | 배속 버튼 글자 (초기 `배속 x1`) |

## 에디터 설정
1. 일시정지 패널(`PauseMenuWindow`의 Root) 아래 `DevPanel` + `DevPanelView`, 자식으로 `Button - TextMeshPro` 5개
2. `DevPanelView`에 버튼 5개와 배속 버튼 텍스트 연결
3. 빈 오브젝트 `DevCommands` + `DevCommands` → DevPanel / Stage / PausePresenter / 플레이어 Health 연결
4. `PauseMenuView` → Dev Panel 연결, Show Dev Panel 켬

## 주의사항 / 확장 포인트
- **제출 빌드 전 `Show Dev Panel`을 끈다.** 켜 두면 빌드에서도 테스트 버튼이 보인다.
- 명령 추가: enum 값 추가 → `DevPanelView`에 버튼 필드·연결 한 줄 → `DevCommands` switch에 메서드 하나.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-11-19 | 최초 작성: 일시정지 메뉴 안 테스트 패널(다음 스테이지·무적·적 전멸·즉사·배속) |
