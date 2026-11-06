# Part Break (보스 부위 파괴)

## 개요
보스 몸을 부위(머리·몸통·팔·다리)로 나눠 **맞은 부위마다 내구도**를 따로 깎는다. 내구도는 보스 최대 체력의 비율로 정해진다. 내구도가 0이 되면 부위가 파괴되어 그 부위로 들어오는 피해가 커지고(배율), 빨간 아웃라인이 번쩍인 뒤 약하게 유지되며, 보스가 일정 시간 그로기에 빠진다. 귀환·리스폰 시 모든 부위가 복구된다.
피해는 기존 전투 흐름(`Health`)을 그대로 쓰고, 부위 시스템은 `IDamageMultiplier`(배율)와 `Health.Damaged`(내구도) 두 지점에만 연결된다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Enemy/PartBreak/BreakablePart.cs](../../Assets/Project/Scripts/Enemy/PartBreak/BreakablePart.cs) | 부위 하나의 정의(이름, 히트박스 콜라이더, 연출 렌더러, 내구도 비율, 평소/파괴 배율)와 실행 중 내구도. `ApplyDamage`(이번 피해로 막 파괴되면 true), `ResetState(최대 체력)`, `CurrentMultiplier` |
| [Enemy/PartBreak/BreakablePartManager.cs](../../Assets/Project/Scripts/Enemy/PartBreak/BreakablePartManager.cs) | 콜라이더 → 부위 사전, `IDamageMultiplier` 구현(맞은 부위 배율), `Health.Damaged`로 내구도 감소, `PartBroken` 이벤트, `CombatReset` 시 전 부위 복구, 부위 피해 로그(에디터/개발 빌드) |
| [Enemy/PartBreak/BrokenPartOutline.cs](../../Assets/Project/Scripts/Enemy/PartBreak/BrokenPartOutline.cs) | `PartBroken` 구독: 부위 렌더러 아웃라인 번쩍임 → 약하게 유지, `CombatReset` 시 끔 |
| [Enemy/PartBreak/PartBreakGroggy.cs](../../Assets/Project/Scripts/Enemy/PartBreak/PartBreakGroggy.cs) | `PartBroken` 구독: `EnemyController.StartGroggy`로 그로기 요청 |
| [Enemy/States/EnemyGroggyState.cs](../../Assets/Project/Scripts/Enemy/States/EnemyGroggyState.cs) | 일정 시간 행동 불능 (맞아도 경직 없음), 끝나면 전투 복귀 |
| [Core/Interfaces/IDamageMultiplier.cs](../../Assets/Project/Scripts/Core/Interfaces/IDamageMultiplier.cs) | 피해 배율 (`GetMultiplier`), Health가 방어력 계산 전에 호출 ([Core](Core.md)) |
| [Combat/HitColliderUtility.cs](../../Assets/Project/Scripts/Combat/HitColliderUtility.cs) | 근접·범위 판정에서 대상별로 공격 기준점에 가장 가까운 콜라이더 선택 ([Combat](Combat.md)) |
| [Materials/Effects/PartBreakOutline.mat](../../Assets/Project/Materials/Effects/PartBreakOutline.mat) | `WeaponOutline` 셰이더 복제 머티리얼 (보스 크기에 맞춘 두께, 맥동 없음) |

## 동작 흐름
### 맞은 부위 찾기
```
근접(MeleeAttacker) : 칼날 샘플 지점에 가장 가까운 콜라이더
범위 마법(SpellCaster): 범위 중심에 가장 가까운 콜라이더      → DamageInfo.HitCollider
투사체(Projectile)  : 레이캐스트에 맞은 콜라이더 그대로
```
- 보스는 루트 캡슐 없이 **뼈 아래 히트박스**(Head, Chest, LowerArm_L/R, LowerLeg_L/R)로만 맞는다. 사망 시 `EnemyController`가 자식 콜라이더까지 끈다.

### 피해 적용
```
Health.TakeDamage
 ├─ IDamageBlocker.TryBlock → 막힘
 ├─ IDamageMultiplier.GetMultiplier → 맞은 부위의 배율 (파괴 전 normal / 파괴 후 broken, 부위 아님 = 1)
 ├─ IDamageReducer.Reduce → 방어력
 └─ Damaged(최종 피해)
      └─ BreakablePartManager.HandleDamaged
           ├─ HitCollider 없음 / 부위 아님 → 무시
           ├─ BreakablePart.ApplyDamage(최종 피해) → 내구도 감소 (0 아래로 내려가지 않음, 이미 파괴면 무시)
           └─ 이번에 막 파괴됨 → PartBroken(부위)   ← 파괴 순간 1회만
```
- 내구도는 **배율·방어력이 적용된 최종 피해**만큼 깎인다.
- 최대 내구도 = `최대 체력 × 비율`(반올림, 최소 1). 체력은 `EnemyController.Awake`에서 정해지므로 `ResetState`에서 계산한다.
- 비율 합계가 1보다 충분히 낮아야 모든 부위를 부술 수 있다. 파괴된 부위를 때린 피해와 파괴 타격의 초과분은 체력만 깎기 때문이다.
- 배율은 매 타격마다 `IsBroken`을 새로 확인하므로 파괴 직후부터 자동으로 바뀐다.

### 파괴 연출 (BrokenPartOutline)
```
Awake       : 부위 렌더러마다 아웃라인 머티리얼 슬롯 추가(1회) → 슬롯 인덱스 저장 → 검정(보이지 않음)
PartBroken  : flashColor로 켜고 번쩍임 목록에 (부위, 종료 시각) 추가
Update      : 목록이 비면 바로 return / 진행 중이면 flashColor → idleColor 보간, 끝나면 idleColor 고정 후 제거
CombatReset : 목록 비우기 → 전 부위 검정
```
- 아웃라인 셰이더는 가산 혼합이라 **검정 = 보이지 않음**, 밝기(HDR 색)로 강약을 준다. 알파는 쓰지 않는다.
- 색은 `MaterialPropertyBlock` + 슬롯 인덱스로 아웃라인 슬롯에만 적용 (머티리얼 복사본 생성 없음).

### 그로기
```
PartBroken → PartBreakGroggy → EnemyController.StartGroggy(시간)
 ├─ 사망 / 이미 그로기 → 무시 (시간이 늘어나거나 다시 시작하지 않음)
 └─ EnemyGroggyState: 공격·이동 정지, Hit_B 재생 → 시간이 끝나면 전투 복귀
```
- 그로기 중에는 피해만 받고 경직되지 않는다 (`EnemyController.HandleDamaged`가 `SpawnState`와 같이 제외). 그래서 `Damaged` 구독 순서와 관계없이 파괴 타격 후 상태는 그로기다.
- `EnemyController`는 부위 파괴를 모르고, 시간만 받는 `StartGroggy`를 공개한다.
- `Hit_B`는 반복 없는 클립이라 재생 후 그로기가 끝날 때까지 마지막 자세를 유지한다.

### 초기화 (귀환·리스폰)
- `EnemyController.ResetCombat` 끝에서 `CombatReset` 이벤트 → 매니저는 전 부위 `ResetState`, 아웃라인은 끔.
- `ResetCombat`은 스폰·리스폰(`Spawn`)과 귀환(`EnemyReturnState`) 두 경로에서 호출되므로 둘 다 부위가 복구된다.
- 모든 스폰 경로(씬 배치 `Start`, 스포너 `Instantiate`, 리스폰 `SetActive(true)`)가 구독(`OnEnable`) 이후 `Spawn`을 호출하므로 별도 초기화가 필요 없다.

## 에디터 설정
1. 보스 프리팹 루트(= `Health`·`EnemyController`가 있는 오브젝트)에 `BreakablePartManager`, `BrokenPartOutline`, `PartBreakGroggy` 추가.
2. 뼈 아래에 부위별 히트박스 콜라이더 배치 (루트 캡슐은 제거).
3. `BreakablePartManager` → **Breakable Parts**에 부위마다 이름·콜라이더·렌더러·내구도 비율·배율 지정.
   - 렌더러는 모델의 부위별 메시 (KayKit Skeleton: `Head`·`Body`·`ArmLeft`·`ArmRight`·`LegLeft`·`LegRight` 등).
   - 한 콜라이더를 여러 부위에 넣으면 첫 부위만 쓰고 경고를 띄운다.
4. `BrokenPartOutline` → **Outline Material** = `PartBreakOutline`, 번쩍임 시간·색(HDR)·유지 색 조정.

| 컴포넌트 | 필드 | 하는 일 |
|---|---|---|
| BreakablePartManager | Log Part Damage | 부위 피해 로그 (`[보스] 머리 -12 (88/100) → 파괴!`, 에디터/개발 빌드) |
| BreakablePartManager | Breakable Parts | 부위 목록 (Display Name, Colliders, Renderers, Durability Ratio, Normal / Broken Multiplier) |
| BrokenPartOutline | Outline Material | 부위 렌더러에 덧씌울 아웃라인 머티리얼 |
| BrokenPartOutline | Flash Duration | 파괴 순간 번쩍이는 시간 (초) |
| BrokenPartOutline | Flash Color / Idle Color | 번쩍일 때 / 번쩍임 후 유지되는 아웃라인 색 (HDR, 밝기로 강약) |
| PartBreakGroggy | Groggy Duration | 부위 파괴 시 그로기 시간 (초) |

## 주의사항 / 확장 포인트
- `WeaponOutline.mat`은 무기 티어 테두리와 공유하므로 보스용 값은 `PartBreakOutline.mat`에서만 바꾼다.
- 아웃라인 슬롯 인덱스는 `Awake`에서 저장한 값을 쓴다 (`sharedMaterials`는 호출마다 배열을 새로 할당).
- 부위 파괴에 반응하는 기능은 `PartBroken`을 구독하는 별도 컴포넌트로 추가한다 (매니저는 알리기만 함). 귀환·리스폰 시 되돌릴 상태가 있으면 `CombatReset`도 구독한다.
- 애니메이션 상태를 추가하면 `GetBaseStates`를 쓰는 적 컨트롤러를 모두 `Create Enemy Animator`로 다시 생성한다.
- 예정: 부위 파괴 추가 보상.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-11-05 | 최초 작성: 맞은 콜라이더 전달(`DamageInfo.HitCollider`), 부위 피해 배율(`IDamageMultiplier`), 부위 내구도·파괴 이벤트(`PartBroken`)·피해 로그, 귀환·리스폰 시 복구(`EnemyController.CombatReset`), 파괴 부위 아웃라인 번쩍임(`BrokenPartOutline`) |
| 2026-11-06 | 부위 파괴 그로기(`PartBreakGroggy`, `EnemyGroggyState`, `StartGroggy`), 내구도를 최대 체력 비율로 변경(`durabilityRatio`) |
