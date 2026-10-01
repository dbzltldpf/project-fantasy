# Core

## 개요
플레이어·적 등 여러 기능이 공유하는 범용 코드. 특정 기능에 종속되지 않는다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [IState.cs](../../Assets/Project/Scripts/Core/StateMachine/IState.cs) | 상태 인터페이스 (`Enter` / `Tick` / `Exit`) |
| [StateMachine.cs](../../Assets/Project/Scripts/Core/StateMachine/StateMachine.cs) | 제네릭 상태 머신, 상태 전이만 담당 |
| [IDamageable.cs](../../Assets/Project/Scripts/Core/Interfaces/IDamageable.cs) | 피해를 받을 수 있는 대상 (`IsAlive`, `TakeDamage`) |
| [IDamageBlocker.cs](../../Assets/Project/Scripts/Core/Interfaces/IDamageBlocker.cs) | 피해 적용 전 방어 여부 판정 (`TryBlock`), Health가 호출 |
| [DamageInfo.cs](../../Assets/Project/Scripts/Core/Types/DamageInfo.cs) | 피해 정보 값 타입 (양, 타격 지점, 방향, 가해자) |
| [TransformExtensions.cs](../../Assets/Project/Scripts/Utils/TransformExtensions.cs) | `FindDeepChild` 하위 계층 이름 탐색 (초기화 전용) |

## 동작 흐름
### StateMachine\<TState\>
- `Initialize(start)` → 시작 상태 `Enter`
- `ChangeState(next)` → 현재 상태 `Exit` → 다음 상태 `Enter` → `StateChanged` 이벤트
- **같은 상태로 전이하면 재진입**(Exit → Enter)한다. 연속 피격 시 HitState 재시작에 사용.
- `Tick` 도중 `ChangeState`를 호출한 상태는 **바로 `return`** 해야 이후 코드가 실행되지 않는다.

### DamageInfo
- `readonly struct` → GC 할당 없이 전달. `in` 파라미터로 복사 비용 최소화.

## 주의사항 / 확장 포인트
- 적 AI도 `StateMachine<EnemyStateBase>` 형태로 그대로 재사용한다.
- 복잡한 판단이 필요한 적(보스 등)은 판단 계층만 BT/유틸리티 AI로 두고, 실행은 FSM 상태가 맡는 하이브리드를 권장.
- 상속이 필요 없는 클래스는 `sealed`가 기본 (설계 의도 명시 + devirtualization).

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-09-30 | 최초 작성 (StateMachine, IState, IDamageable, DamageInfo) |
| 2026-10-01 | `IDamageBlocker`, `Utils/TransformExtensions` 추가 |
