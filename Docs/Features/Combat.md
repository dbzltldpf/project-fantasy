# Combat

## 개요
체력·피격 무적·방어(가드), 액션 타임라인(프레임 단위 공격 이벤트), 칼날 궤적 타격 판정, 히트스톱, 피격 반응 데이터, 이펙트 풀링, 훈련용 허수아비. 플레이어와 적이 공용으로 사용한다. 원거리 투사체는 [Ranged Combat](RangedCombat.md) 참고.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Health.cs](../../Assets/Project/Scripts/Combat/Health.cs) | 체력, 피격 후 무적 시간, 방어 판정 위임, `RestoreFull`(사망 포함 복구), `HealthChanged` / `Damaged` / `Blocked` / `Died` 이벤트, `SetMaxHealth`(데이터 기반 최대 체력), `TakeDamage`가 결과(`DamageResult`) 반환 |
| [TrainingDummy.cs](../../Assets/Project/Scripts/Combat/TrainingDummy.cs) | 훈련용 허수아비: 피격 직후 체력 복구로 죽지 않음, 가상 체력 처치 판정, 피해 로그(에디터/개발 빌드) |
| [KillReward.cs](../../Assets/Project/Scripts/Combat/KillReward.cs) | 처치 보상: 피해를 준 공격자 기록 → 처치 시 각자에게 경험치 전액 (`IKillRewardReceiver`), `SetExperience`·`ClearContributors`(적 데이터·귀환/리스폰) |
| [EffectPool.cs](../../Assets/Project/Scripts/Combat/Effects/EffectPool.cs) / [PooledEffect.cs](../../Assets/Project/Scripts/Combat/Effects/PooledEffect.cs) | 파티클 이펙트 풀링: 1회 재생(파티클 종료 시 반환) / 시간 지정 / 수동 종료 |
| [Effects/Editor/EffectRecolorWindow.cs](../../Assets/Project/Scripts/Combat/Effects/Editor/EffectRecolorWindow.cs) | `Tools → ProjectFantasy → Recolor Effect`: 이펙트 Prefab Variant를 만들어 파티클·라이트 색조만 변경 (원본 유지) |
| [ShieldGuard.cs](../../Assets/Project/Scripts/Combat/ShieldGuard.cs) | `IDamageBlocker`: 가드 중 정면 각도 내 공격 완전 방어 / `IDamageReducer`: 방패 활성 중 방어력·마법 방어력으로 피해 감소 |
| [MeleeAttacker.cs](../../Assets/Project/Scripts/Combat/MeleeAttacker.cs) | 칼날 궤적 스윕 또는 몸 기준 구체 판정, 판정 구간마다 대상당 1회 타격, 명중 시 히트스톱 요청 |
| [WeaponTrace.cs](../../Assets/Project/Scripts/Combat/WeaponTrace.cs) | 칼날 위 샘플 지점의 이전/현재 월드 위치 추적 |
| [HitStop.cs](../../Assets/Project/Scripts/Combat/HitStop.cs) | 명중 순간 애니메이터 일시 정지 (공격자·피격자 공용), `IsActive`로 로직 정지 판단 |
| [Action/ActionData.cs](../../Assets/Project/Scripts/Combat/Action/ActionData.cs) | 공격 한 동작: 클립, 길이(프레임), 재생 속도, 크로스페이드, 다형 이벤트 목록 (SO) |
| [Action/ActionEvent.cs](../../Assets/Project/Scripts/Combat/Action/ActionEvent.cs) | 이벤트 베이스: `[startFrame, endFrame)` 구간, `OnBegin` / `OnTick` / `OnEnd`, 검증 |
| [Action/Events/](../../Assets/Project/Scripts/Combat/Action/Events/) | `HitWindowEvent`(타격 구간), `ComboWindowEvent`(입력 예약·전이 프레임), `MoveEvent`(전진) |
| [Action/ActionPlayer.cs](../../Assets/Project/Scripts/Combat/Action/ActionPlayer.cs) | 프레임 시계 진행, 이벤트 진입·유지·종료 호출 (GC 없음, 프레임 드랍 시에도 구간 1회 실행 보장) |
| [Action/IActionContext.cs](../../Assets/Project/Scripts/Combat/Action/IActionContext.cs) | 이벤트가 소유자에게 요청하는 동작 (판정, 콤보 창, 이동) – 플레이어는 `PlayerAttackState`가 구현 |
| [Action/Editor/ActionDataEditor.cs](../../Assets/Project/Scripts/Combat/Action/Editor/ActionDataEditor.cs) | ActionData 인스펙터: 타임라인 열기, 이벤트 추가·삭제, 검증 경고 표시 |
| [Action/Editor/ActionTimelineWindow.cs](../../Assets/Project/Scripts/Combat/Action/Editor/ActionTimelineWindow.cs) | 타임라인 창: 대상·캐릭터·무기 선택, 재생 제어, 선택 이벤트 상세 |
| [Action/Editor/ActionTimelineTrackView.cs](../../Assets/Project/Scripts/Combat/Action/Editor/ActionTimelineTrackView.cs) | 눈금자·트랙 그리기, 드래그 편집 (프레임 스냅, 드래그 1회 = Undo 1회) |
| [Action/Editor/ActionPreview.cs](../../Assets/Project/Scripts/Combat/Action/Editor/ActionPreview.cs) | AnimationMode 프레임 샘플링, 임시 무기 부착, 칼날 궤적 캐싱·Scene 뷰 표시 |
| [Action/Editor/ActionEditorUtility.cs](../../Assets/Project/Scripts/Combat/Action/Editor/ActionEditorUtility.cs) / [ActionEventStyle.cs](../../Assets/Project/Scripts/Combat/Action/Editor/ActionEventStyle.cs) | 인스펙터·창 공용 (이벤트 추가 메뉴, 필드 그리기, 검증) / 이벤트 종류별 색 |
| [AttackComboData.cs](../../Assets/Project/Scripts/Combat/Data/AttackComboData.cs) | 무기 종류별 콤보 = ActionData 배열 (SO) |
| [HitReactionData.cs](../../Assets/Project/Scripts/Combat/Data/HitReactionData.cs) | 피격 경직·넉백 (SO) |

## 동작 흐름
### 액션 타임라인 (프레임 단위, 30fps 클립 기준 예시)
```
frame 0 ──── 6 ────── 10 ── 14 ──────────── 21 (lengthFrames)
             [HitWindow )
                    [ComboWindow ────────────)
                            ◆ transitionFrame: 예약 입력이 있으면 다음 액션
```
- **시계**: `경과 프레임 = 경과 시간 × 재생 속도 × 클립 frameRate`. 애니메이터 상태 Speed도 `ActionSpeed` 파라미터로 같은 배율 → 모션과 판정이 어긋나지 않음
- **이벤트 구간**: `[startFrame, endFrame)` 진입 시 `OnBegin`+`OnTick`, 유지 중 `OnTick`, 이탈 시 `OnEnd`. 한 프레임에 구간 전체를 건너뛰어도 1회 실행
- **HitWindowEvent**: 구간마다 `BeginSwing` → 매 프레임 `TickHit` → `EndSwing`. 한 액션에 여러 개 두면 다단 히트(대상 목록은 구간마다 초기화)
- **ComboWindowEvent**: 구간 안에서만 공격 입력을 예약, 예약되면 `transitionFrame`에 다음 액션 시작 → 히트·팔로스루가 잘리지 않음
- **MoveEvent**: 구간 동안 정면으로 `speed` 전진 (재생 속도 배율 적용)
- **종료**: 예약이 없으면 `lengthFrames`에서 Locomotion 복귀
- 이벤트는 에셋 공유 데이터 → 런타임 상태는 `IActionContext` 구현체(플레이어는 `PlayerAttackState`)가 보관. 적 AI도 같은 인터페이스로 재사용

### 타격 판정 (MeleeAttacker)
1. `BeginSwing(damage, hitStopDuration)` → 타격 목록 초기화, 칼날 현재 위치 기록
2. `TickHit()`
   - **칼날 있음**: 칼날 위 샘플 지점(간격 ≤ 지름, 2~8개)마다 이전 → 현재 위치를 `OverlapCapsuleNonAlloc` → 빠르게 휘둘러도 궤적 전체 판정
   - **칼날 없음**(맨손 등): 몸 기준 `OverlapSphereNonAlloc`
3. 콜라이더 부모에서 `IDamageable` 탐색 → 자기 자신/사망/이미 맞은 대상 제외 → `DamageInfo` 전달
4. 명중 시 자신과 대상의 `HitStop.Apply` (대상 탐색은 명중 시에만)
5. `EndSwing()` → 종료

### 히트스톱 (HitStop)
- 명중 순간 공격자·피격자 `Animator.speed = 0`, 시간이 지나면 복구 (겹치면 더 늦은 종료 시각으로 연장)
- 플레이어는 `PlayerController.Update`에서 `IsActive` 동안 상태 시계·이동을 모두 정지 → 액션 프레임 시계도 멈춤

### 피격 / 방어 (Health)
```
TakeDamage
 ├─ 사망·무적·0 이하 데미지 → 무시
 ├─ IDamageBlocker.TryBlock == true → Blocked 이벤트 후 종료 (피해 없음)
 ├─ IDamageReducer.Reduce → 피해 × 100 / (100 + 방어력), 최소 1 (물리 = 방어력, 마법 = 마법 방어력)
 └─ 감소된 피해 적용 → 무적 시작 → HealthChanged → Damaged(감소 후 피해) → (체력 0) Died
```
- `ShieldGuard.TryBlock`: 가드 중이고 가해자 방향이 정면 `guardAngle/2` 이내면 방어.
- 피해 종류(`DamageInfo.Type`): 근접·화살 = 물리, Wand·Staff 마법 = 마법. 방어력은 방패 개체에서 굴린 값 ([Inventory](Inventory.md)).

## 데이터 파라미터
### ActionData
| 필드 | 기본값 | 의미 |
|---|---|---|
| clip | — | 기준 클립 (frameRate, Phase 2 미리보기) |
| stateName | (비움) | 애니메이터 상태 이름, 비우면 클립 이름 |
| lengthFrames | 30 | 액션 종료 프레임 (클립보다 짧으면 나머지는 생략) |
| playbackSpeed | 1 | 애니메이션·이벤트 공통 재생 속도 |
| crossFadeDuration | 0.1 | 이 액션으로 전환하는 시간 |
| events | — | 다형 이벤트 목록 (`[SerializeReference]`) |

### 이벤트
| 이벤트 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| 공통 | startFrame / endFrame | 0 / 1 | 활성 구간 `[start, end)` |
| HitWindowEvent | damageMultiplier | 1 | 데미지 = 무기 공격력 × 배율 |
| HitWindowEvent | hitStopDuration | 0.06 | 명중 시 정지 시간 (초, 0이면 없음) |
| ComboWindowEvent | transitionFrame | 0 | 예약 입력 시 다음 액션으로 전이하는 프레임 |
| MoveEvent | speed | 2 | 전진 속도 (m/s) |

- 검증 경고(빈 구간, 액션 길이 초과, 전이 프레임이 입력 시작보다 빠르거나 길이 초과)는 콘솔이 아닌 인스펙터 HelpBox·타임라인 라벨 ⚠ 아이콘으로 표시 (드래그 편집 중 콘솔 도배 방지)

### AttackComboData
| 필드 | 기본값 | 의미 |
|---|---|---|
| actions | — | 콤보 단계별 ActionData (무기 종류별 에셋: 1H / 2H / Unarmed) |

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
| MeleeAttacker | hitOrigin / gizmoColor | 자신 / 빨강 | 몸 기준 판정 원점, 선택 시 Gizmo (판정 위치·반경은 장착 무기 데이터가 설정, 인스펙터 비노출) |
| MeleeAttacker | targetLayers | Everything | 타격 대상 레이어 |
| HitStop | animator | 자식 자동 탐색 | 정지시킬 애니메이터 |
| ShieldGuard | facingTransform | 자신 | 정면 기준 Transform |

## 에디터 설정
- `Create → ProjectFantasy → Combat → Action Data / Attack Combo Data / Hit Reaction Data`
- ActionData: `clip` 지정(상태 이름 = 클립 이름이면 `stateName` 비움) → **타임라인 열기**로 구간 편집 (인스펙터 **이벤트 추가**로도 가능)
- 기존 액션: `Assets/Project/Data/ComboData/Actions/` (`{콤보}_{단계}_{상태}.asset`)
- **애니메이터**: Float 파라미터 `ActionSpeed`(기본 1)를 공격 상태의 Speed Multiplier에 연결. 없으면 시작 시 경고, 재생 속도 배율 무시
- **HitStop**: 공격자(플레이어 필수)·피격자에 추가. 피격자에 없으면 공격자만 정지
- MeleeAttacker `targetLayers`에서 **Player 레이어 제외** 권장

## Action Timeline 창
**열기**: ActionData 인스펙터 **타임라인 열기** 또는 `Window → ProjectFantasy → Action Timeline`. 창이 열려 있으면 프로젝트에서 선택한 ActionData를 따라감.

```
Action / 미리보기 캐릭터(씬 플레이어 자동) / 미리보기 무기(이 액션을 쓰는 무기 자동) / Scene 미리보기
[◀][▶][▶|] Frame 12 / 21 (0.40s)  Clip 30f @ 30fps  Speed x1
눈금자 0    5    10   15   20   25   30
0. HitWindowEvent    ░░░░[■■■■]░░░░░░░░░░░░
1. ComboWindowEvent  ░░░░░░░░[■■■■|■■■■■■]░   | = 전이 프레임 마커
Length               ─────────────────────▌▒▒ (이후 구간 어둡게)
선택 이벤트 상세 필드
```

| 조작 | 동작 |
|---|---|
| 눈금자 클릭·드래그 / ◀ ▶ | 프레임 이동 → Scene 뷰 캐릭터 자세 갱신 |
| ▶ 재생 | 재생 속도 배율로 액션 길이 안에서 반복 |
| 막대 드래그 / 양 끝 드래그 | 구간 이동(전이 마커 함께) / 시작·끝 조절 |
| 흰 마커 / Length 노란 선 | 콤보 전이 프레임 / 액션 길이 |
| 우클릭 / 트랙 클릭 | 이벤트 추가·삭제 / 선택 후 상세 편집 |

**Scene 뷰 미리보기** (편집 모드 전용)
- 씬 캐릭터를 `AnimationMode`로 샘플링 → 씬 데이터는 바뀌지 않고, 창을 닫거나 플레이 시작 시 원래 자세로 복구
- 선택 무기 모델을 손 소켓에 임시 부착 (`HideAndDontSave`, 저장 안 됨)
- 회색 선: 클립 전체 칼끝 경로 / 빨간 면: 타격 구간 동안 칼날이 쓸고 지나가는 면 / 현재 칼날: 노란 선(타격 구간 중 빨강)
- 칼날이 없는 무기는 몸 기준 판정 구체 표시
- 궤적은 클립·칼날 값이 바뀔 때만 재계산 (캐릭터 로컬 좌표로 캐싱 → 캐릭터를 옮겨도 유효)
- 크로스페이드 블렌딩은 반영하지 않음 (클립 단독)

## 이펙트 풀링 (`EffectPool`)
| 재생 모드 | 사용처 | 반환 시점 |
|---|---|---|
| `EffectPool.OneShot` | 명중, 범위 발동 | 모든 파티클이 끝났을 때 (5초 넘으면 강제 반환 + 경고) |
| 초 단위 시간 | 시전 위치 마법진 | 지정 시간 후 |
| `EffectPool.Infinite` | 조준 중 마법진 | `PooledEffect.Stop()` 호출 시 |
- 원본 서드파티 프리팹은 수정하지 않고 `Assets/Project/Art/Prefabs/Effects/`의 **Prefab Variant**를 사용. 1회 재생용은 루트 포함 모든 파티클 Looping 끄기.
- 인스턴스 생성 시 `PooledEffect`가 자동 부착되므로 프리팹에 따로 붙일 필요 없음.
- 인스턴스는 `Pools/Effects/프리팹 이름` 아래에 생성 (`RangedAttacker`·`SpellCaster`의 `effectRoot`를 지정하면 그 아래).
- **색 바꾼 이펙트**: `Recolor Effect` 창에서 Source·Target Color·Suffix 지정 → 같은 폴더에 Variant 생성 (시작 색·수명/속도별 색·트레일·Light의 색조만 교체, 채도·밝기·HDR·알파 유지, 흰색·회색은 그대로). 머티리얼 색이 흰색이고 파티클 색으로 칠하는 이펙트(Hovl 등)에 적합.

## 처치 보상 (`KillReward`)
- 피격 시 가해자에게 `IKillRewardReceiver`가 있으면 기여자로 기록 → 사망 시 기여자마다 `experience` 전액 지급 후 초기화.
- 플레이어는 `PlayerMasteryRewarder`가 받아 무기 숙련 경험치로 반영 ([Mastery](Mastery.md)).
- 적은 `EnemyController`가 `EnemyData.experience`로 경험치를 지정하고, 귀환·리스폰 시 기여 기록을 지운다 ([Enemy](Enemy.md)).

## 훈련용 허수아비
- `Health` + `TrainingDummy` + `DamageNumberEmitter`([UI](UI.md)) + 콜라이더 → 프리팹화해 필드에 배치 (실제 빌드 포함).
- `Health.invincibleDuration`은 **0** 권장 (연속 공격 수치 확인).
- 피해 처리 순서: `Damaged` → 허수아비가 `RestoreFull` → 사망 판정 시 생존 → 한 방 피해가 커도 죽지 않음.
- `KillReward`를 붙이면 누적 피해가 `virtualHealth`(100)에 도달할 때 처치로 판정해 보상 지급 후 누적 초기화 (숙련도 테스트용).

## 주의사항 / 확장 포인트
- 새 적은 `Health`만 붙이면 타격 대상, `ShieldGuard`까지 붙이면 방패 방어 가능.
- 칼날 판정은 **실제 칼 위치** 기준 → 타격 구간이 클립의 휘두르는 프레임과 맞아야 명중. 타임라인 창의 빨간 면이 대상 위치를 지나가는지 확인.
- 새 이벤트는 `ActionEvent`를 상속한 `[Serializable]` 클래스만 추가하면 인스펙터·타임라인 추가 메뉴에 자동 노출 (색은 `ActionEventStyle`에 등록, 없으면 회색).
- 이름이 `transitionFrame`인 int 필드가 있는 이벤트는 타임라인에 전이 마커로 표시.
- 예정: Phase 3 무적·캔슬·사운드/VFX(칼날 트레일) 이벤트, 원거리·마법 타이밍 이전, 런타임 판정 궤적 디버그 표시.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-09-30 | 최초 작성 (Health, MeleeAttacker, AttackComboData, HitReactionData) |
| 2026-09-30 | `AttackStep.StateName` 공개 (애니메이터 상태 검증용) |
| 2026-10-01 | 방어 판정(`IDamageBlocker`, `ShieldGuard`, `Blocked` 이벤트), `MeleeAttacker.SetHitShape`, 데미지 → 배율(`damageMultiplier`) |
| 2026-10-01 | 콤보 입력 예약/전이 시점 분리(`comboInputStartTime`, `comboTransitionTime`), 타이밍 순서 검증, 원거리·마법 무기 근접 판정 제외 |
| 2026-10-02 | 이펙트 풀링(`EffectPool`, `PooledEffect`), 훈련용 허수아비, `Health.RestoreFull` |
| 2026-10-02 | 액션 타임라인 Phase 1: `ActionData`/`ActionEvent`/`ActionPlayer`/`IActionContext`, 칼날 궤적 판정(`WeaponTrace`), `HitStop`, `ActionSpeed` 재생 속도. `AttackStep`(초 단위) 제거 → ActionData로 변환 |
| 2026-10-02 | 액션 타임라인 Phase 2: Action Timeline 창(트랙 드래그 편집, 프레임 스크럽·재생, Scene 뷰 자세·칼날 궤적 미리보기), 검증 경고를 인스펙터 표시로 이동 |
| 2026-10-06 | 피해 종류(물리/마법), `IDamageReducer`·방패 방어력 감소, 공격력·마법력은 장비 개체 능력치 기준, `MeleeAttacker` 판정 위치·반경 인스펙터 비노출 |
| 2026-10-07 | 처치 보상(`KillReward`), 허수아비 가상 체력 처치 판정, 이펙트 풀 부모 폴더 |
| 2026-10-07 | `Health.SetMaxHealth`, `KillReward.SetExperience`·`ClearContributors` (적 데이터 연동), 이펙트 Variant 위치 `Art/Prefabs/Effects` |
| 2026-10-08 | `DamageResult`(적용/막힘/무시) 반환, 이펙트 색 변환 도구(`Recolor Effect`) |
