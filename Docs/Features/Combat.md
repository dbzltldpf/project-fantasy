# Combat

## 개요
체력·피격 무적·방어(가드), 근접 타격 판정, 콤보/피격 반응 데이터, 이펙트 풀링, 훈련용 허수아비. 플레이어와 적이 공용으로 사용한다. 원거리 투사체는 [Ranged Combat](RangedCombat.md) 참고.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Health.cs](../../Assets/Project/Scripts/Combat/Health.cs) | 체력, 피격 후 무적 시간, 방어 판정 위임, `RestoreFull`(사망 포함 복구), `HealthChanged` / `Damaged` / `Blocked` / `Died` 이벤트 |
| [TrainingDummy.cs](../../Assets/Project/Scripts/Combat/TrainingDummy.cs) | 훈련용 허수아비: 피격 직후 체력 복구로 죽지 않음, 피해 로그(에디터/개발 빌드) |
| [EffectPool.cs](../../Assets/Project/Scripts/Combat/Effects/EffectPool.cs) / [PooledEffect.cs](../../Assets/Project/Scripts/Combat/Effects/PooledEffect.cs) | 파티클 이펙트 풀링: 1회 재생(파티클 종료 시 반환) / 시간 지정 / 수동 종료 |
| [ShieldGuard.cs](../../Assets/Project/Scripts/Combat/ShieldGuard.cs) | `IDamageBlocker` 구현, 가드 중 정면 각도 내 공격 방어 |
| [MeleeAttacker.cs](../../Assets/Project/Scripts/Combat/MeleeAttacker.cs) | 구체 범위 타격 판정, 한 번 휘두를 때 대상당 1회 타격, 무기별 판정 범위 교체 |
| [AttackStep.cs](../../Assets/Project/Scripts/Combat/Data/AttackStep.cs) | 콤보 한 단계의 타이밍·데미지 배율·전진 스텝, 타이밍 순서 검증 |
| [AttackComboData.cs](../../Assets/Project/Scripts/Combat/Data/AttackComboData.cs) | 무기 종류별 콤보 단계 배열 (SO) |
| [HitReactionData.cs](../../Assets/Project/Scripts/Combat/Data/HitReactionData.cs) | 피격 경직·넉백 (SO) |

## 동작 흐름
### 공격 한 단계 타임라인 (기본값, 초)
```
0.00 ─── 0.15 ── 0.20 ──── 0.35 ── 0.45 ──────── 0.70
         |입력 예약 가능 ─────────────────────────|
                 |히트 윈도우 ─|
                                   ▲ 예약 입력이 있으면 여기서 다음 단계
```
- **입력 예약**: `comboInputStartTime` 이후 공격 입력(선입력 포함)은 예약만 함
- **전이 시점**: `comboTransitionTime`에 예약이 있으면 다음 단계 시작 → 히트·팔로스루가 잘리지 않음
- **히트 윈도우**: `hitStartTime ~ hitEndTime` 동안 매 프레임 `MeleeAttacker.TickHit()` (근접 무기만)
- **전진 스텝**: `lungeDuration` 동안 `lungeSpeed`로 이동 (기본 0, 제자리 클립이라 미끄러짐 주의)
- **종료**: 예약이 없으면 `duration`까지 재생 후 Locomotion 복귀
- 값 변경 시 `OnValidate`가 순서(히트 시작 ≤ 히트 끝 ≤ 전이 ≤ 전체 길이)를 검사해 경고

### 타격 판정 (MeleeAttacker)
1. `BeginSwing(damage)` → 이번 스윙 타격 목록 초기화
2. `TickHit()` → `Physics.OverlapSphereNonAlloc`(버퍼 16개, 트리거 무시)
3. 콜라이더 부모에서 `IDamageable` 탐색 → 자기 자신/사망/이미 맞은 대상 제외
4. `DamageInfo`(데미지, 타격 지점, 수평 방향, 가해자) 전달
5. `EndSwing()` → 종료

### 피격 / 방어 (Health)
```
TakeDamage
 ├─ 사망·무적·0 이하 데미지 → 무시
 ├─ IDamageBlocker.TryBlock == true → Blocked 이벤트 후 종료 (피해 없음)
 └─ 피해 적용 → 무적 시작 → HealthChanged → Damaged → (체력 0) Died
```
- `ShieldGuard.TryBlock`: 가드 중이고 가해자 방향이 정면 `guardAngle/2` 이내면 방어.

## 데이터 파라미터
### AttackStep (콤보 단계별)
| 필드 | 기본값 | 의미 |
|---|---|---|
| stateName | Melee_1H_Attack_Slice_Horizontal | 애니메이터 상태 이름 (전체 이름) |
| damageMultiplier | 1 | 데미지 배율 (무기 공격력 × 배율) |
| duration | 0.7 | 단계 전체 길이 |
| hitStartTime / hitEndTime | 0.2 / 0.35 | 히트 윈도우 |
| comboInputStartTime | 0.15 | 다음 공격 입력 예약 시작 |
| comboTransitionTime | 0.45 | 예약된 다음 단계로 전이하는 시점 |
| lungeSpeed / lungeDuration | 0 / 0.15 | 전진 스텝 |

### AttackComboData
| 필드 | 기본값 | 의미 |
|---|---|---|
| steps | — | 콤보 단계 배열 (무기 종류별 에셋: 1H / 2H / Unarmed / 원거리) |
| crossFadeDuration | 0.1 | 공격 애니메이션 전환 시간 |

### HitReactionData
| 필드 | 기본값 | 의미 |
|---|---|---|
| stunDuration | 0.4 | 경직 시간 |
| knockbackSpeed | 4 | 넉백 초기 속도 |
| knockbackDeceleration | 12 | 넉백 감속 |

### 컴포넌트 인스펙터 값
| 컴포넌트 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| Health | maxHealth | 100 | 최대 체력 |
| Health | invincibleDuration | 0.5 | 피격 후 무적 시간 |
| MeleeAttacker | hitOffset / hitRadius | (0, 1, 1) / 0.8 | 판정 구체 기본값 (장착 무기 데이터로 덮어씀), 선택 시 Gizmo |
| MeleeAttacker | targetLayers | Everything | 타격 대상 레이어 |
| ShieldGuard | facingTransform | 자신 | 정면 기준 Transform |

## 에디터 설정
- `Create → ProjectFantasy → Combat → Attack Combo Data / Hit Reaction Data`
- 콤보 단계의 `stateName`은 애니메이터 상태 **전체 이름** (예: `Diagonal` ❌ → `Melee_1H_Attack_Slice_Diagonal` ✅)
- 빈 배열에 단계를 추가하면 Unity가 코드 기본값 대신 **0으로 채움** → 타이밍 값 직접 입력 필요
- MeleeAttacker `targetLayers`에서 **Player 레이어 제외** 권장

## 이펙트 풀링 (`EffectPool`)
| 재생 모드 | 사용처 | 반환 시점 |
|---|---|---|
| `EffectPool.OneShot` | 명중, 범위 발동 | 모든 파티클이 끝났을 때 (5초 넘으면 강제 반환 + 경고) |
| 초 단위 시간 | 시전 위치 마법진 | 지정 시간 후 |
| `EffectPool.Infinite` | 조준 중 마법진 | `PooledEffect.Stop()` 호출 시 |
- 원본 서드파티 프리팹은 수정하지 않고 `Assets/Project/Prefabs/Effects/`의 **Prefab Variant**를 사용. 1회 재생용은 루트 포함 모든 파티클 Looping 끄기.
- 인스턴스 생성 시 `PooledEffect`가 자동 부착되므로 프리팹에 따로 붙일 필요 없음.

## 훈련용 허수아비
- `Health` + `TrainingDummy` + `DamageNumberEmitter`([UI](UI.md)) + 콜라이더 → 프리팹화해 필드에 배치 (실제 빌드 포함).
- `Health.invincibleDuration`은 **0** 권장 (연속 공격 수치 확인).
- 피해 처리 순서: `Damaged` → 허수아비가 `RestoreFull` → 사망 판정 시 생존 → 한 방 피해가 커도 죽지 않음.

## 주의사항 / 확장 포인트
- 새 적은 `Health`만 붙이면 타격 대상, `ShieldGuard`까지 붙이면 방패 방어 가능.
- 히트 타이밍은 클립마다 Animation 창에서 칼이 지나가는 구간을 보고 맞출 것.
- 예정: **액션 타임라인**(프레임 기반 이벤트 구간, 칼날 스윕 판정, 히트스톱, 재생 속도 배율, 에디터 미리보기)으로 초 단위 타이밍 대체.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-09-30 | 최초 작성 (Health, MeleeAttacker, AttackComboData, HitReactionData) |
| 2026-09-30 | `AttackStep.StateName` 공개 (애니메이터 상태 검증용) |
| 2026-10-01 | 방어 판정(`IDamageBlocker`, `ShieldGuard`, `Blocked` 이벤트), `MeleeAttacker.SetHitShape`, 데미지 → 배율(`damageMultiplier`) |
| 2026-10-01 | 콤보 입력 예약/전이 시점 분리(`comboInputStartTime`, `comboTransitionTime`), 타이밍 순서 검증, 원거리·마법 무기 근접 판정 제외 |
| 2026-10-02 | 이펙트 풀링(`EffectPool`, `PooledEffect`), 훈련용 허수아비, `Health.RestoreFull` |
