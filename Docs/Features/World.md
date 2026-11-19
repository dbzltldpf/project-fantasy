# World

## 개요
월드에서 일어나는 진행용 장치. 장소(`LocationData`)에 플레이어가 들어오면 전역 알림을 보내고, 게이트(`AreaGate`)는 막는 벽과 통과 가능한 반투명 문을 바꿔 켜서 열고 닫는다. 콜라이더와 기본 도형만으로 구성한다.

World는 목표·스테이지를 모른다. 목표가 장소 알림을 듣고([Objective](Objective.md) `ReachObjectiveData`), 게이트를 언제 열고 닫을지는 [Stage](Stage.md) `StageDirector`가 정한다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [World/LocationData.cs](../../Assets/Project/Scripts/World/LocationData.cs) | 장소 하나 SO: 표시 이름 |
| [World/LocationEvents.cs](../../Assets/Project/Scripts/World/LocationEvents.cs) | 장소 전역 알림 `Reached(LocationData)` (`EnemyEvents`와 같은 형태) |
| [World/LocationTrigger.cs](../../Assets/Project/Scripts/World/LocationTrigger.cs) | 트리거 콜라이더에 대상 레이어가 들어오면 `RaiseReached` (기본 1회, `triggerOnce` 끄면 매번) |
| [World/AreaGate.cs](../../Assets/Project/Scripts/World/AreaGate.cs) | `Open()`/`Close()`/`IsOpen`: 닫힘 = 막는 벽(`blocker`) 켬, 열림 = 반투명 문(`openVisual`) 켬, 시작 시 닫힘으로 맞춤 |

## 동작 흐름
```
LocationTrigger.OnTriggerEnter
  → (triggerOnce면 이미 알렸을 때 무시) → 레이어 비트 검사(targetLayers & (1 << layer))
  → LocationEvents.RaiseReached(LocationData)
     → ObjectiveTracker: 현재 목표가 그 장소의 도착 목표면 진행 ([Objective](Objective.md))
     → StageDirector: 게이트 장소이고 열려 있으면 다음 스테이지로 ([Stage](Stage.md))

AreaGate.SetOpen(open): isOpen 갱신 → blocker.SetActive(!open) → openVisual.SetActive(open)
```
- 머티리얼을 코드로 바꾸지 않고 상태별 오브젝트를 켜고 꺼서 콜라이더와 겉모습을 함께 바꾼다.
- 게이트 트리거는 벽 바로 뒤에 둔다. 닫혀 있으면 벽에 막혀 닿지 않는다.

## 데이터 파라미터
| 대상 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| LocationData | displayName | — | 장소 표시 이름 |
| LocationTrigger | locationData / targetLayers | — | 도착을 알릴 장소 / 도착으로 인정할 레이어 (`Player`) |
| LocationTrigger | triggerOnce | true | 처음 한 번만 알림 (게이트 트리거는 끔) |
| AreaGate | blocker | — | 닫혔을 때 길을 막는 벽 |
| AreaGate | openVisual | — | 열렸을 때 보이는 통과 가능한 문 (콜라이더 없음) |

## 에디터 설정
1. `Create → ProjectFantasy/World/Location Data`로 장소 에셋 생성 (`Data/World/`, 예: `LocationGate`)
2. 게이트 구성
```
GateField          ← AreaGate
 ├ Blocker         ← Cube, BoxCollider, NonStickSurface, 머티리얼 Gate_Closed(불투명), Layer Default
 ├ OpenVisual      ← Cube, 콜라이더 없음, 머티리얼 Gate(반투명)
 └ TriggerGate     ← BoxCollider(Is Trigger) + LocationTrigger(LocationGate, Player, Trigger Once 끔), Layer Ignore Raycast
```

## 주의사항 / 확장 포인트
- Blocker를 `Ignore Raycast`에 두면 화살·마법탄·카메라 충돌·적 시야가 벽을 통과한다. 벽은 Default, 트리거는 Ignore Raycast로 레이어가 달라 오브젝트를 나눈다.
- 트리거 이벤트는 둘 중 하나에 Rigidbody 또는 CharacterController가 있어야 발생한다 (플레이어는 CharacterController).
- 런타임 스크립트에 `using UnityEditor`가 있으면 빌드에서만 컴파일 에러가 난다.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-11-18 | 최초 작성: 장소 SO·도착 전역 알림·도착 트리거, 목표 완료 시 구역 개방(`AreaGate`) |
| 2026-11-19 | 게이트 닫기·`IsOpen`, 열림 상태 반투명 문(`openVisual`), 트리거 반복 알림(`triggerOnce`), 게이트 개방을 `StageDirector`가 담당 |
