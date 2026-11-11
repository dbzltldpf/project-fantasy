# Objective

## 개요
스토리 진행용 목표 시스템. 목표 단계를 SO로 순서대로 정의하고, 적 처치 알림을 받아 진행도를 센다. 한 단계가 끝나면 HUD에 완료 연출을 보여 준 뒤 다음 목표로 넘어가며, 모든 단계가 끝나면 완료 문구를 띄운다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Objective/ObjectiveData.cs](../../Assets/Project/Scripts/Objective/ObjectiveData.cs) | 목표 한 단계 SO: 화면 문구, 처치 대상(`EnemyData`), 필요 횟수 |
| [Objective/ObjectiveSequence.cs](../../Assets/Project/Scripts/Objective/ObjectiveSequence.cs) | 목표 단계 목록 SO (챕터 단위), `IReadOnlyList`로 읽기 전용 노출 |
| [Objective/ObjectiveTracker.cs](../../Assets/Project/Scripts/Objective/ObjectiveTracker.cs) | `EnemyEvents.EnemyKilled` 구독, 현재 단계·처치 수·완료 여부 관리, `StepCompleted`·`Changed` 알림 |
| [Objective/ObjectivePresenter.cs](../../Assets/Project/Scripts/Objective/ObjectivePresenter.cs) | 트래커 알림 → 뷰 갱신, 단계 완료 시 연출 코루틴 동안 갱신 보류 후 최신 상태로 다시 그림 |
| [UI/HUD/ObjectiveView.cs](../../Assets/Project/Scripts/UI/HUD/ObjectiveView.cs) | 목표 문구·진행도(`SetText`) 표시, 단계 완료 색 연출, 전체 완료 문구 (목표 시스템을 모르고 문자열·숫자만 받음) |
| [Enemy/EnemyEvents.cs](../../Assets/Project/Scripts/Enemy/EnemyEvents.cs) | 적 전역 알림 `EnemyKilled(EnemyData)` (`EnemyController` 사망 시 발생) |

## 동작 흐름
```
EnemyController.HandleDied → EnemyEvents.RaiseEnemyKilled(EnemyData)
  → ObjectiveTracker: 현재 단계 대상과 SO 참조 비교 → 처치 수 증가
     → 필요 횟수 도달 시 다음 단계로 (마지막이면 IsCompleted)
     → 상태를 모두 바꾼 뒤 StepCompleted(끝난 단계) → Changed 순으로 알림
  → ObjectivePresenter
     StepCompleted: 이전 연출 코루틴 중지 → ObjectiveView.PlayCompleted 대기 (그동안 Refresh 보류) → Refresh
     Changed: Refresh (IsCompleted면 ShowCompleted, 아니면 Show(문구, 처치 수, 필요 수))
```
- 로직은 즉시 다음 단계로 넘어가고 화면만 연출 뒤에 따라간다. 연출 중 처치한 수도 연출이 끝나면 반영된다.
- Presenter는 `OnEnable`에서 구독 직후 `Refresh()`를 호출해 컴포넌트 활성 순서와 관계없이 첫 화면을 그린다.
- 연출이 연달아 들어오면 이전 코루틴을 멈추고 새로 시작한다. 뷰는 `Show`·`ShowCompleted`·`PlayCompleted` 시작 시 라벨 색을 원래 값으로 되돌려 끊긴 연출이 남지 않는다.

## 데이터 파라미터
| 대상 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| ObjectiveData | description | — | 화면 목표 문구 |
| ObjectiveData | target | — | 처치 대상 `EnemyData` (참조 비교) |
| ObjectiveData | requiredCount | 1 이상 | 필요 처치 수 |
| ObjectiveSequence | objectiveDatas | — | 진행 순서대로 나열한 단계 |
| ObjectiveView | completed | `완료` | 단계 완료 표시·전체 완료 문구 |
| ObjectiveView | 완료 표시 시간 / 색 | 1.5초 / 금색 | 단계 완료 연출 유지 시간과 라벨 색 (코드 고정) |

## 에디터 설정
1. `Create → ProjectFantasy/Objective/Objective Data`로 단계 에셋을 만들고 문구·대상·횟수 입력 (`Data/Objective/`)
2. `Create → ProjectFantasy/Objective/Objective Sequence`로 챕터 에셋을 만들고 단계를 순서대로 등록 (예: `Chapter1`)
3. 씬에 `ObjectiveTracker` 오브젝트 → Goals에 챕터 에셋 연결
4. HUD Canvas 아래 패널 + TextMeshPro 두 개(문구 `Label`, 진행도 `ProgressLabel`, Pretendard SDF) → 패널에 `ObjectiveView` 추가 후 연결
5. `ObjectivePresenter` 오브젝트 → Tracker·View 연결

## 주의사항 / 확장 포인트
- 현재 조건은 적 처치만 지원한다. 장소 도착·NPC 대화 등 두 번째 조건이 생기면 조건 추상화(`ObjectiveCondition`)로 분리한다.
- `ObjectiveData`의 직렬화 필드 이름을 바꿀 때는 `FormerlySerializedAs`로 기존 에셋 값을 보존한다.
- `CurrentObjective`는 완료 후 null이므로 `IsCompleted`를 먼저 확인한다.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-11-11 | 최초 작성: 목표 단계·챕터 SO, 적 처치 알림(`EnemyEvents`), 트래커·프레젠터, HUD 목표 표시, 단계 완료 색 연출 후 다음 목표 |
