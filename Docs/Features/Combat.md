# Combat

## 개요
체력·피격 무적, 근접 타격 판정, 콤보/피격 반응 데이터. 플레이어와 적이 공용으로 사용한다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Health.cs](../../Assets/Project/Scripts/Combat/Health.cs) | 체력, 피격 후 무적 시간, `HealthChanged` / `Damaged` / `Died` 이벤트 |
| [MeleeAttacker.cs](../../Assets/Project/Scripts/Combat/MeleeAttacker.cs) | 구체 범위 타격 판정, 한 번 휘두를 때 대상당 1회 타격 |
| [AttackStep.cs](../../Assets/Project/Scripts/Combat/Data/AttackStep.cs) | 콤보 한 단계의 타이밍·데미지·전진 스텝 |
| [AttackComboData.cs](../../Assets/Project/Scripts/Combat/Data/AttackComboData.cs) | 무기별 콤보 단계 배열 (SO) |
| [HitReactionData.cs](../../Assets/Project/Scripts/Combat/Data/HitReactionData.cs) | 피격 경직·넉백 (SO) |

## 동작 흐름
### 공격 한 단계 타임라인 (기본값, 초)
```
0.00 ──── 0.15 ── 0.20 ─── 0.30 ── 0.35 ────────── 0.70
|전진 스텝|       |히트 윈도우 ────────|              |종료
                           |콤보 입력 가능 ─────────────|
```
- **전진 스텝**: `lungeDuration` 동안 `lungeSpeed`로 바라보는 방향 이동
- **히트 윈도우**: `hitStartTime ~ hitEndTime` 동안 매 프레임 `MeleeAttacker.TickHit()`
- **콤보 입력**: `comboInputTime` 이후 공격 입력(선입력 포함)이 있으면 즉시 다음 단계
- **종료**: `duration` 도달 시 Locomotion 복귀

### 타격 판정 (MeleeAttacker)
1. `BeginSwing(damage)` → 이번 스윙 타격 목록 초기화
2. `TickHit()` → `Physics.OverlapSphereNonAlloc`(버퍼 16개, 트리거 무시)
3. 콜라이더 부모에서 `IDamageable` 탐색 → 자기 자신/사망/이미 맞은 대상 제외
4. `DamageInfo`(데미지, 타격 지점, 수평 방향, 가해자) 전달
5. `EndSwing()` → 종료

### 피격 (Health)
- 사망 상태, 무적 중, 0 이하 데미지는 무시
- 피해 적용 → 무적 시간 시작 → `HealthChanged` → `Damaged` → (체력 0이면) `Died`

## 데이터 파라미터
### AttackStep (콤보 단계별)
| 필드 | 기본값 | 의미 |
|---|---|---|
| stateName | Melee_1H_Attack_Slice_Horizontal | 애니메이터 상태 이름 (전체 이름) |
| damage | 10 | 데미지 |
| duration | 0.7 | 단계 전체 길이 |
| hitStartTime / hitEndTime | 0.2 / 0.35 | 히트 윈도우 |
| comboInputTime | 0.3 | 다음 단계 연계 가능 시점 |
| lungeSpeed / lungeDuration | 3 / 0.15 | 전진 스텝 |

### AttackComboData
| 필드 | 기본값 | 의미 |
|---|---|---|
| steps | — | 콤보 단계 배열 (현재 3단: Slice_Horizontal → Slice_Diagonal → Chop) |
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
| MeleeAttacker | hitOffset / hitRadius | (0, 1, 1) / 0.8 | 판정 구체 위치(로컬)·반경, 선택 시 Gizmo 표시 |
| MeleeAttacker | targetLayers | Everything | 타격 대상 레이어 |

## 에디터 설정
- `Project/Data`에서 Create → ProjectFantasy → Combat → Attack Combo Data / Hit Reaction Data 생성
- 콤보 단계의 `stateName`은 애니메이터 상태의 **전체 이름**으로 입력 (예: `Diagonal` ❌ → `Melee_1H_Attack_Slice_Diagonal` ✅)
- MeleeAttacker `targetLayers`에서 **Player 레이어 제외** 권장

## 주의사항 / 확장 포인트
- 새 적은 `Health`만 붙이면 바로 타격 대상이 된다 (`IDamageable`).
- 무기 교체 시 `AttackComboData` 에셋만 바꾸면 된다 (한손검/양손검 콤보 분리).
- 예정: 방패 가드(Melee_Block 클립 존재), 차지 공격, 점프 공격(Melee_1H_Attack_Jump_Chop).

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-09-30 | 최초 작성 (Health, MeleeAttacker, AttackComboData, HitReactionData) |
| 2026-09-30 | `AttackStep.StateName` 공개 (애니메이터 상태 검증용) |
