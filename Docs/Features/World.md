# World

## 개요
월드에서 일어나는 진행용 장치. 장소(`LocationData`)에 플레이어가 처음 들어오면 전역 알림을 보내고, 막고 있는 벽(`AreaGate`)을 열어 다음 구역으로 갈 수 있게 한다. 방 단위 진행(입장 → 목표 완료 → 출구 개방 → 다음 방 도착)을 콜라이더와 기본 도형만으로 구성한다.

World는 목표 시스템을 모른다. 목표가 장소 알림을 듣고([Objective](Objective.md) `ReachObjectiveData`), 목표 완료로 벽을 여는 연결도 Objective 쪽(`ObjectiveGateOpener`)이 맡는다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [World/LocationData.cs](../../Assets/Project/Scripts/World/LocationData.cs) | 장소 하나 SO: 표시 이름 |
| [World/LocationEvents.cs](../../Assets/Project/Scripts/World/LocationEvents.cs) | 장소 전역 알림 `Reached(LocationData)` (`EnemyEvents`와 같은 형태) |
| [World/LocationTrigger.cs](../../Assets/Project/Scripts/World/LocationTrigger.cs) | 트리거 콜라이더에 대상 레이어가 처음 들어오면 `RaiseReached` (1회) |
| [World/AreaGate.cs](../../Assets/Project/Scripts/World/AreaGate.cs) | `Open()` 시 막고 있는 벽(자식 오브젝트)을 끔 (1회) |

## 동작 흐름
```
LocationTrigger.OnTriggerEnter
  → 이미 알렸으면 무시 → 레이어 비트 검사(targetLayers & (1 << layer))
  → LocationEvents.RaiseReached(LocationData)
     → ObjectiveTracker: 현재 목표가 그 장소의 도착 목표면 진행 ([Objective](Objective.md))

ObjectiveTracker.StepCompleted(끝난 목표)
  → ObjectiveGateOpener: 지정한 목표와 같으면 AreaGate.Open() → Blocker 비활성화
```
- 트리거는 한 번만 알린다. 도착 목표가 오기 전에 밟히면 알림이 지나가므로, 트리거는 **벽 너머**에 배치해 순서를 레벨 구조로 보장한다.
- 벽 오브젝트를 끄고 `AreaGate` 자체는 남겨 둔다 (이후 개방 이펙트·상태 복원용).

## 데이터 파라미터
| 대상 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| LocationData | displayName | — | 장소 표시 이름 |
| LocationTrigger | locationData | — | 도착을 알릴 장소 |
| LocationTrigger | targetLayers | — | 도착으로 인정할 레이어 (`Player`) |
| AreaGate | blocker | — | 길을 막는 벽 오브젝트 |

## 에디터 설정
1. `Create → ProjectFantasy/World/Location Data`로 장소 에셋 생성 (`Data/World/`, 예: `LocationField`)
2. 도착 트리거: 빈 오브젝트(예: `TriggerField`) + BoxCollider(**Is Trigger**) + `LocationTrigger` → 장소·Target Layers(`Player`) 연결, Layer `Ignore Raycast`
3. 구역 문: 빈 오브젝트(예: `GateField`) + `AreaGate` + `ObjectiveGateOpener`, 자식 `Blocker`(Cube, BoxCollider 기본, 반투명 머티리얼 `Gate`, **Layer Default**, `NonStickSurface`)
4. `ObjectiveGateOpener` → 씬의 ObjectiveTracker, 완료 시 열 목표 에셋, 같은 오브젝트의 AreaGate 연결

## 주의사항 / 확장 포인트
- Blocker를 `Ignore Raycast`에 두면 화살·마법탄·카메라 충돌·적 시야가 벽을 통과한다. 벽은 Default 레이어로 둔다.
- 트리거 이벤트는 둘 중 하나에 Rigidbody 또는 CharacterController가 있어야 발생한다 (플레이어는 CharacterController).
- 목표를 이미 끝낸 상태에서 시작(장 선택·체크포인트)하면 `StepCompleted`가 다시 오지 않아 벽이 닫혀 있다. 체크포인트 단계에서 시작 시 상태 맞추기로 해결 예정.
- 순간이동 포탈은 체크포인트·장 선택의 스폰 지점과 함께 추가한다.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-11-18 | 최초 작성: 장소 SO·도착 전역 알림·도착 트리거, 목표 완료 시 구역 개방(`AreaGate`) |
