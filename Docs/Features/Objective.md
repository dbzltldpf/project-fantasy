# Objective

## 개요
스토리 진행용 목표 시스템. 목표 단계를 SO로 순서대로 정의하고, 적 처치·장소 도착 알림을 받아 진행도를 센다. 한 단계가 끝나면 HUD에 완료 연출을 보여 준 뒤 다음 목표로 넘어가며, 모든 단계가 끝나면 완료 문구를 띄운다. 지정한 단계가 끝나면 구역 문을 연다([World](World.md)).

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Objective/ObjectiveData.cs](../../Assets/Project/Scripts/Objective/ObjectiveData.cs) | 목표 한 단계 추상 SO: 화면 문구, 필요 횟수(`RequiredCount`), 알림 일치 여부(`IsMatch(EnemyData)`·`IsMatch(LocationData)`, 기본 false), 진행도 표시 여부(`ShowProgress`, 기본 true) |
| [Objective/KillObjectiveData.cs](../../Assets/Project/Scripts/Objective/KillObjectiveData.cs) | 처치 목표: 대상 `EnemyData`, 필요 처치 수 |
| [Objective/ReachObjectiveData.cs](../../Assets/Project/Scripts/Objective/ReachObjectiveData.cs) | 도착 목표: 대상 `LocationData`, 필요 횟수 1 고정, 진행도 숨김 |
| [Objective/ObjectiveSequence.cs](../../Assets/Project/Scripts/Objective/ObjectiveSequence.cs) | 목표 단계 목록 SO (챕터 단위), `IReadOnlyList`로 읽기 전용 노출 |
| [Objective/ObjectiveTracker.cs](../../Assets/Project/Scripts/Objective/ObjectiveTracker.cs) | `EnemyEvents.EnemyKilled`·`LocationEvents.Reached` 구독, 현재 단계·진행 수·완료 여부 관리, `StepCompleted`·`Changed` 알림 |
| [Objective/ObjectivePresenter.cs](../../Assets/Project/Scripts/Objective/ObjectivePresenter.cs) | 트래커 알림 → 뷰 갱신, 단계 완료 시 연출 코루틴 동안 갱신 보류 후 최신 상태로 다시 그림 |
| [Objective/ObjectiveGateOpener.cs](../../Assets/Project/Scripts/Objective/ObjectiveGateOpener.cs) | 지정한 목표의 `StepCompleted`를 받으면 `AreaGate.Open()` ([World](World.md)) |
| [UI/HUD/ObjectiveView.cs](../../Assets/Project/Scripts/UI/HUD/ObjectiveView.cs) | 목표 문구·진행도(`SetText`) 표시, 진행도 표시 여부, 단계 완료 색 연출, 전체 완료 문구 (목표 시스템을 모르고 문자열·숫자·bool만 받음) |
| [Enemy/EnemyEvents.cs](../../Assets/Project/Scripts/Enemy/EnemyEvents.cs) | 적 전역 알림 `EnemyKilled(EnemyData)` (`EnemyController` 사망 시 발생) |
| [World/LocationEvents.cs](../../Assets/Project/Scripts/World/LocationEvents.cs) | 장소 전역 알림 `Reached(LocationData)` (`LocationTrigger` 진입 시 발생) |

## 동작 흐름
```
EnemyEvents.EnemyKilled(EnemyData) / LocationEvents.Reached(LocationData)
  → ObjectiveTracker: 완료 상태면 무시 → 현재 단계.IsMatch(받은 데이터) → AddProgress
     → 필요 횟수 도달 시 다음 단계로 (마지막이면 IsCompleted)
     → 상태를 모두 바꾼 뒤 StepCompleted(끝난 단계) → Changed 순으로 알림
  → ObjectivePresenter
     StepCompleted: 이전 연출 코루틴 중지 → ObjectiveView.PlayCompleted 대기 (그동안 Refresh 보류) → Refresh
     Changed: Refresh (IsCompleted면 ShowCompleted, 아니면 Show(문구, 진행 수, 필요 수, 진행도 표시 여부))
  → ObjectiveGateOpener: 끝난 단계가 지정 목표면 AreaGate.Open()
```
- 트래커는 목표 종류를 검사하지 않고 `IsMatch`로 묻는다. 새 목표 종류는 `ObjectiveData`를 상속해 해당 `IsMatch`만 override한다.
- 진행도 표시 여부는 목표 종류가 정한다 (도착 = 숨김, 처치 = 1마리여도 `0 / 1` 표시).
- 로직은 즉시 다음 단계로 넘어가고 화면만 연출 뒤에 따라간다. 연출 중 처치한 수도 연출이 끝나면 반영된다.
- Presenter는 `OnEnable`에서 구독 직후 `Refresh()`를 호출해 컴포넌트 활성 순서와 관계없이 첫 화면을 그린다.
- 연출이 연달아 들어오면 이전 코루틴을 멈추고 새로 시작한다. 뷰는 `Show`·`ShowCompleted`·`PlayCompleted` 시작 시 라벨 색을 원래 값으로 되돌려 끊긴 연출이 남지 않는다.

## 데이터 파라미터
| 대상 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| ObjectiveData (공통) | description | — | 화면 목표 문구 |
| KillObjectiveData | target | — | 처치 대상 `EnemyData` (참조 비교) |
| KillObjectiveData | requiredCount | 1 이상 | 필요 처치 수 |
| ReachObjectiveData | locationData | — | 도착할 `LocationData` (필요 횟수 1, 진행도 숨김) |
| ObjectiveSequence | objectiveDatas | — | 진행 순서대로 나열한 단계 (처치·도착 혼합) |
| ObjectiveGateOpener | objectiveTracker / targetObjective / areaGate | — | 씬 트래커 / 완료 시 문을 열 목표 / 열 문 |
| ObjectiveView | completed | `완료` | 단계 완료 표시·전체 완료 문구 |
| ObjectiveView | 완료 표시 시간 / 색 | 1.5초 / 금색 | 단계 완료 연출 유지 시간과 라벨 색 (코드 고정) |

## 에디터 설정
1. 단계 에셋 생성 (`Data/Objective/`)
   - 처치: `Create → ProjectFantasy/Objective/Kill Objective` → 문구·대상·횟수 (예: `Kill_Minion_2`)
   - 도착: `Create → ProjectFantasy/Objective/Reach Objective` → 문구·장소 (예: `ReachField`, "다음 구역으로 이동")
2. `Create → ProjectFantasy/Objective/Objective Sequence`로 챕터 에셋을 만들고 단계를 순서대로 등록 (예: `Chapter1` = `Kill_Minion_2` → `ReachField` → `Kill_Warrior_1`)
3. 씬에 `ObjectiveTracker` 오브젝트 → Goals에 챕터 에셋 연결
4. HUD Canvas 아래 패널 + TextMeshPro 두 개(문구 `Label`, 진행도 `ProgressLabel`, Pretendard SDF) → 패널에 `ObjectiveView` 추가 후 연결
5. `ObjectivePresenter` 오브젝트 → Tracker·View 연결
6. 구역 문을 열 단계가 있으면 `ObjectiveGateOpener` 설정 ([World](World.md) 에디터 설정 3~4)

## 주의사항 / 확장 포인트
- `ObjectiveData`는 추상 클래스라 에셋을 만들 수 없다. 기존 처치 에셋은 스크립트 파일을 `KillObjectiveData`로 이름만 바꿔(.meta guid 유지) 그대로 이어졌다.
- 스크립트 파일 이름을 바꿀 때는 Unity 프로젝트 창에서 바꿔 `.meta`를 함께 옮긴다. 직렬화 필드 이름을 바꿀 때는 `FormerlySerializedAs`로 기존 에셋 값을 보존한다.
- 도착 목표의 트리거는 해당 단계 전에 밟히지 않도록 배치한다 (트리거는 1회만 알림).
- `CurrentObjective`는 완료 후 null이므로 `IsCompleted`를 먼저 확인한다.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-11-11 | 최초 작성: 목표 단계·챕터 SO, 적 처치 알림(`EnemyEvents`), 트래커·프레젠터, HUD 목표 표시, 단계 완료 색 연출 후 다음 목표 |
| 2026-11-18 | 목표 추상화(`ObjectiveData` → `KillObjectiveData`·`ReachObjectiveData`, `IsMatch`), 장소 도착 목표, 진행 수 `ProgressCount`로 변경, 목표별 진행도 표시 여부, 목표 완료 시 구역 개방(`ObjectiveGateOpener`) |
