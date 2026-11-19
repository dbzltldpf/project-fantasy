# Objective

## 개요
스토리 진행용 목표 시스템. 목표 목록을 받아 순서대로 진행하며, 적 처치·장소 도착 알림을 받아 진행도를 센다. 한 단계가 끝나면 HUD에 완료 연출을 보여 준 뒤 다음 목표로 넘어가며, 모든 단계가 끝나면 완료 문구를 띄운다. 목표 목록은 [Stage](Stage.md) `StageDirector`가 스테이지 목록에서 만들어 넘긴다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Objective/ObjectiveData.cs](../../Assets/Project/Scripts/Objective/ObjectiveData.cs) | 목표 한 단계 추상 SO: 화면 문구, 필요 횟수(`RequiredCount`), 알림 일치 여부(`IsMatch(EnemyData)`·`IsMatch(LocationData)`, 기본 false), 진행도 표시 여부(`ShowProgress`, 기본 true) |
| [Objective/KillObjectiveData.cs](../../Assets/Project/Scripts/Objective/KillObjectiveData.cs) | 처치 목표: 대상 `EnemyData`, 필요 처치 수 |
| [Objective/ReachObjectiveData.cs](../../Assets/Project/Scripts/Objective/ReachObjectiveData.cs) | 도착 목표: 대상 `LocationData`, 필요 횟수 1 고정, 진행도 숨김 |
| [Objective/ObjectiveTracker.cs](../../Assets/Project/Scripts/Objective/ObjectiveTracker.cs) | `Begin(목록)`으로 시작, `RestartStep`(현재 단계 진행 수만 초기화), `EnemyEvents.EnemyKilled`·`LocationEvents.Reached` 구독, 현재 단계·진행 수·완료 여부 관리, `StepCompleted`·`Changed` 알림 |
| [Objective/ObjectivePresenter.cs](../../Assets/Project/Scripts/Objective/ObjectivePresenter.cs) | 트래커 알림 → 뷰 갱신, 단계 완료 시 연출 코루틴 동안 갱신 보류 후 최신 상태로 다시 그림 |
| [UI/HUD/ObjectiveView.cs](../../Assets/Project/Scripts/UI/HUD/ObjectiveView.cs) | 목표 문구·진행도(`SetText`) 표시, 진행도 표시 여부, 단계 완료 색 연출, 전체 완료 문구 (목표 시스템을 모르고 문자열·숫자·bool만 받음) |
| [Enemy/EnemyEvents.cs](../../Assets/Project/Scripts/Enemy/EnemyEvents.cs) | 적 전역 알림 `EnemyKilled(EnemyData)` (`EnemyController` 사망 시 발생) |
| [World/LocationEvents.cs](../../Assets/Project/Scripts/World/LocationEvents.cs) | 장소 전역 알림 `Reached(LocationData)` (`LocationTrigger` 진입 시 발생) |

## 동작 흐름
```
StageDirector.Start → ObjectiveTracker.Begin(목록) → Changed

EnemyEvents.EnemyKilled(EnemyData) / LocationEvents.Reached(LocationData)
  → ObjectiveTracker: 진행 중이 아니면 무시(IsRunning) → 현재 단계.IsMatch(받은 데이터) → AddProgress
     → 필요 횟수 도달 시 다음 단계로 (마지막이면 IsCompleted)
     → 상태를 모두 바꾼 뒤 StepCompleted(끝난 단계) → Changed 순으로 알림
  → ObjectivePresenter
     StepCompleted: 이전 연출 코루틴 중지 → ObjectiveView.PlayCompleted 대기 (그동안 Refresh 보류) → Refresh
     Changed: Refresh (IsCompleted면 ShowCompleted, 시작 전이면 무시, 아니면 Show(문구, 진행 수, 필요 수, 진행도 표시 여부))
```
- 트래커는 목표 종류를 검사하지 않고 `IsMatch`로 묻는다. 새 목표 종류는 `ObjectiveData`를 상속해 해당 `IsMatch`만 override한다.
- 진행도 표시 여부는 목표 종류가 정한다 (도착 = 숨김, 처치 = 1마리여도 `0 / 1` 표시).
- `Begin` 전에는 `CurrentObjective`가 null이다. Presenter는 `OnEnable`의 첫 `Refresh`에서 null이면 그리지 않고 `Begin`의 `Changed`를 기다린다.
- 로직은 즉시 다음 단계로 넘어가고 화면만 연출 뒤에 따라간다. 연출 중 처치한 수도 연출이 끝나면 반영된다.
- 연출이 연달아 들어오면 이전 코루틴을 멈추고 새로 시작한다. 뷰는 `Show`·`ShowCompleted`·`PlayCompleted` 시작 시 라벨 색을 원래 값으로 되돌려 끊긴 연출이 남지 않는다.

## 데이터 파라미터
| 대상 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| ObjectiveData (공통) | description | — | 화면 목표 문구 (처치 목표는 사용 무기 포함, 예: "활로 해골 궁수 2마리 처치") |
| KillObjectiveData | target | — | 처치 대상 `EnemyData` (참조 비교) |
| KillObjectiveData | requiredCount | 1 이상 | 필요 처치 수 |
| ReachObjectiveData | locationData | — | 도착할 `LocationData` (필요 횟수 1, 진행도 숨김) |
| ObjectiveView | completed | `완료` | 단계 완료 표시·전체 완료 문구 |
| ObjectiveView | 완료 표시 시간 / 색 | 1.5초 / 금색 | 단계 완료 연출 유지 시간과 라벨 색 (코드 고정) |

## 에디터 설정
1. 단계 에셋 생성 (`Data/Objective/`)
   - 처치: `Create → ProjectFantasy/Objective/Kill Objective` → 문구·대상·횟수 (예: `Kill_Rogue_2`)
   - 도착: `Create → ProjectFantasy/Objective/Reach Objective` → 문구·장소 (예: `ReachGate`, "게이트로 이동")
2. 씬에 `ObjectiveTracker` 오브젝트 (목록은 `StageDirector`가 넘김, [Stage](Stage.md))
3. HUD Canvas 아래 패널 + TextMeshPro 두 개(문구 `Label`, 진행도 `ProgressLabel`, Pretendard SDF) → 패널에 `ObjectiveView` 추가 후 연결
4. `ObjectivePresenter` 오브젝트 → Tracker·View 연결

## 주의사항 / 확장 포인트
- `ObjectiveData`는 추상 클래스라 에셋을 만들 수 없다. 기존 처치 에셋은 스크립트 파일을 `KillObjectiveData`로 이름만 바꿔(.meta guid 유지) 그대로 이어졌다.
- 스크립트·에셋 이름을 바꿀 때는 Unity 프로젝트 창에서 바꿔 `.meta`를 함께 옮긴다. 직렬화 필드 이름을 바꿀 때는 `FormerlySerializedAs`로 기존 에셋 값을 보존한다.
- 같은 목표 에셋(`ReachGate`)을 목록에 여러 번 넣어도 된다 (트래커는 현재 인덱스 기준).
- `CurrentObjective`는 시작 전·완료 후 null이다.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-11-11 | 최초 작성: 목표 단계·챕터 SO, 적 처치 알림(`EnemyEvents`), 트래커·프레젠터, HUD 목표 표시, 단계 완료 색 연출 후 다음 목표 |
| 2026-11-18 | 목표 추상화(`ObjectiveData` → `KillObjectiveData`·`ReachObjectiveData`, `IsMatch`), 장소 도착 목표, 진행 수 `ProgressCount`로 변경, 목표별 진행도 표시 여부, 목표 완료 시 구역 개방(`ObjectiveGateOpener`) |
| 2026-11-19 | 트래커가 목록을 받아 시작(`Begin`, `IsRunning`), `ObjectiveSequence`·`Chapter1`·`ObjectiveGateOpener` 제거 (목록 생성·게이트 개방은 `StageDirector`) |
| 2026-11-19 | `RestartStep` (스테이지 재시작 시 현재 단계 진행 수 초기화) |
