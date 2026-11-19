# Flow

## 개요
한 씬 안에서 타이틀 → 플레이 → 엔딩을 UI 패널 전환으로 처리한다. 게임을 켜면 실제 맵 위에 타이틀이 덮이고, 시작하면 첫 스테이지부터 진행한다. 보스를 처치하고 게이트로 나가면 엔딩 화면에 플레이 시간과 쓰러진 횟수를 보여 주고, 타이틀로 돌아가 다시 시작할 수 있다.

`GameFlowPresenter`는 Stage·Player·Camera·UI를 엮는 최상위 지휘자다. 다른 기능은 Flow를 모른다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Flow/GameFlowPresenter.cs](../../Assets/Project/Scripts/Flow/GameFlowPresenter.cs) | 화면 전환(타이틀·플레이·엔딩), 화면에 따라 플레이어 입력·시점 켜고 끔, 시작 시 기록 시작·`GoToStage(0)`, 전체 클리어 시 기록 정지·엔딩 표시, 종료 |
| [Flow/PlayRecord.cs](../../Assets/Project/Scripts/Flow/PlayRecord.cs) | `Begin`~`End` 사이 플레이 시간(일시정지 제외)·사망 횟수 집계 |
| [UI/Title/TitleView.cs](../../Assets/Project/Scripts/UI/Title/TitleView.cs) | 타이틀 패널 표시/숨김, 시작·종료 버튼 클릭 알림 |
| [UI/Ending/EndingView.cs](../../Assets/Project/Scripts/UI/Ending/EndingView.cs) | 엔딩 패널 표시(플레이 시간 `mm:ss`, 쓰러진 횟수), 타이틀로 버튼 클릭 알림 |

연동: [Stage](Stage.md) `StageDirector.GoToStage`·`AllStagesCleared`, [Player](Player.md) `PlayerInputHandler`, [Camera](Camera.md) `ThirdPersonCamera.SetLookEnabled`, [Combat](Combat.md) 플레이어 `Health.Died`.

## 동작 흐름
```
Start → ShowTitle: 엔딩 숨김 → 입력 끔 → 타이틀 표시
  (StageDirector.Start는 그룹을 모두 꺼 두므로 적·목표·인트로 없음)

타이틀 "시작" → StartGame
  → 타이틀 숨김 → PlayRecord.Begin → StageDirector.GoToStage(0) → 입력 켬
타이틀 "종료" → Application.Quit (에디터에서는 반응 없음)

StageDirector.AllStagesCleared (보스 클리어 후 게이트 진입)
  → PlayRecord.End → 입력 끔 → EndingView.Show(플레이 시간, 쓰러진 횟수)
엔딩 "타이틀로" → ShowTitle (다시 시작하면 GoToStage가 적·게이트·목표·플레이어를 모두 초기화)
```
- 입력 끄기(`SetInputEnabled`)는 `PlayerInputHandler` 컴포넌트를 통째로 끈다. `OnDisable`에서 모든 키 구독이 풀려 ESC·Tab·K까지 막히므로 일시정지·메뉴 쪽 코드를 고치지 않는다. 시점은 `SetLookEnabled`로 멈추고 커서도 함께 풀린다.
- 플레이 시간은 `Time.deltaTime` 누적이라 일시정지(timeScale 0) 시간은 빠진다. 테스트 배속에서는 배속만큼 쌓인다.
- 사망 횟수는 기록 중일 때만 센다.

## 데이터 파라미터
| 대상 | 필드 | 의미 |
|---|---|---|
| GameFlowPresenter | inputHandler / thirdPersonCamera | 타이틀·엔딩 중 입력·시점 정지 |
| GameFlowPresenter | stageDirector | 게임 시작, 전체 클리어 알림 |
| GameFlowPresenter | titleView / endingView / playRecord | 타이틀 UI / 엔딩 UI / 기록 |
| PlayRecord | playerHealth | 사망 횟수를 셀 플레이어 체력 |
| TitleView | root / startButton / quitButton | 패널 / 시작 / 종료 |
| EndingView | root / playTimeLabel / deathCountLabel / titleButton | 패널 / 플레이 시간 / 쓰러진 횟수 / 타이틀로 |

## 에디터 설정
1. Canvas 아래 `TitleWindow`(+ `TitleView`) → `Root`(전체 반투명 배경) → 제목 TMP(Black Han Sans), 시작·종료 버튼
2. Canvas 아래 `EndingWindow`(+ `EndingView`, 항상 켜 둠) → `Root` → 제목, 플레이 시간·쓰러진 횟수 TMP, 타이틀로 버튼
3. 두 창은 일시정지 패널보다 아래쪽 계층(나중에 그려져 위에 보임)
4. 빈 오브젝트 `GameFlow` + `GameFlowPresenter` + `PlayRecord` → 참조 연결
5. 플레이어는 씬에서 PlayerSpawn 근처에 둔다 (타이틀 배경)

## 주의사항 / 확장 포인트
- `EndingView`는 Awake에서 패널을 숨기고, `TitleView`는 숨기지 않는다 (게임을 켜면 바로 보여야 함).
- 버튼 View는 Awake에서 `onClick`을 이벤트로 연결해야 한다. 이벤트를 선언만 하고 `Invoke`가 없으면 구독해도 아무 일도 일어나지 않는다.
- 엔딩 문구·연출을 늘리려면 `HandleAllStagesCleared`에서 [Narration](Narration.md)을 재생하거나 `EndingView`를 확장한다.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-11-19 | 최초 작성: 한 씬 타이틀(시작·종료), 엔딩(플레이 시간·쓰러진 횟수, 타이틀로), 플레이 기록, 화면별 입력 차단 |
