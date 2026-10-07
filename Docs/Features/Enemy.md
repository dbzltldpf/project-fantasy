# Enemy

## 개요
KayKit Skeleton 기반 근접 적 2종(Minion·Warrior). NavMeshAgent로 이동하고, 플레이어와 같은 전투 부품(`Health`·`MeleeAttacker`·액션 타임라인 콤보·`ShieldGuard`·`HitStop`·`KillReward`)을 재사용한다. 스폰 지점에서 배회 → 발견·추격 → 공격/막기 → 처치 시 경험치·드랍 → 일정 시간 뒤 리스폰.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Enemy/Data/EnemyData.cs](../../Assets/Project/Scripts/Enemy/Data/EnemyData.cs) | 적 한 종류: 능력치·장비·감지·이동·전투·막기·등장/사망·드랍·애니메이션 (SO) |
| [Enemy/Data/EnemyAnimationData.cs](../../Assets/Project/Scripts/Enemy/Data/EnemyAnimationData.cs) | 애니메이터 상태 이름·전환 시간·이동 모션 분기 속도 (적 공용 SO) |
| [Enemy/Data/EnemyGuardSettings.cs](../../Assets/Project/Scripts/Enemy/Data/EnemyGuardSettings.cs) | 막기 확률·시간·쿨타임·반격 |
| [Enemy/EnemyController.cs](../../Assets/Project/Scripts/Enemy/EnemyController.cs) | 컴포넌트 조립, 상태 머신, 스폰·리스폰, 공격/막기 선택, 경직 면역, `Despawned` |
| [Enemy/EnemyNavigator.cs](../../Assets/Project/Scripts/Enemy/EnemyNavigator.cs) | NavMeshAgent 래퍼: 목적지 이동(재탐색 최소화)·정지·회전(직접 제어)·넉백(`agent.Move`)·히트스톱 정지·순간 이동 |
| [Enemy/EnemyPerception.cs](../../Assets/Project/Scripts/Enemy/EnemyPerception.cs) | 대상 감지(거리 → 시야각 → 가림, NonAlloc·간격), 공격받으면 공격자 추적(도발 시간), `TargetChanged` |
| [Enemy/EnemyAnimator.cs](../../Assets/Project/Scripts/Enemy/EnemyAnimator.cs) | 상태 재생(해시 캐싱·중복 방지), 액션 재생 속도, 누락 상태 검증 |
| [Enemy/EnemyLoadout.cs](../../Assets/Project/Scripts/Enemy/EnemyLoadout.cs) | 무기·방패 모델, 근접 판정(칼날/몸 기준), 방패 방어력 |
| [Enemy/EnemyLoot.cs](../../Assets/Project/Scripts/Enemy/EnemyLoot.cs) | 사망 시 드랍 표 → 주변에 `WorldItem` 생성 |
| [Enemy/EnemySpawner.cs](../../Assets/Project/Scripts/Enemy/EnemySpawner.cs) | 반경 안 N마리 유지, 사라진 적은 대기 후 같은 인스턴스로 리스폰 (풀링) |
| [Enemy/EnemyHealthBarPresenter.cs](../../Assets/Project/Scripts/Enemy/EnemyHealthBarPresenter.cs) | 머리 위 체력바 표시 규칙 |
| [Enemy/States/](../../Assets/Project/Scripts/Enemy/States/) | `Spawn` `Patrol` `Chase` `Attack` `Guard` `Hit` `Return` `Dead` |
| [Enemy/Editor/EnemyAnimatorBuilder.cs](../../Assets/Project/Scripts/Enemy/Editor/EnemyAnimatorBuilder.cs) | `Tools → ProjectFantasy → Create Enemy Animator` |
| [Items/Loot/](../../Assets/Project/Scripts/Items/Loot/) | `DropTable`(SO)·`DropEntry`·`LootDrop` ([Inventory](Inventory.md)) |
| [UI/Enemy/EnemyHealthBarView.cs](../../Assets/Project/Scripts/UI/Enemy/EnemyHealthBarView.cs) | 월드 공간 체력바 표시·빌보드 ([UI](UI.md)) |

## 동작 흐름
### 상태 전이
```
Spawn(Spawn_Ground, spawnDuration) → Patrol
Patrol: 대기(idleTimeRange) ↔ 반경 내 무작위 지점 걷기 ── 대상 발견 → Chase
Chase: 달려서 추격 ── attackRange 안: 정지·바라보기 → 막기(guardChance) 또는 정면이면 Attack
       대상 상실 / 스폰 지점에서 leashRange 초과 → Return
Attack: 무기 콤보를 maxComboActions까지 (대상이 사거리 1.5배 안일 때만 이어 침) → 쿨타임 → Chase/Patrol
Guard(방패): guardDuration 동안 바라보며 천천히 접근 ── 막으면 경직 → 반격(counterChance) 또는 계속 막기
Hit: 넉백·경직 → 반응 막기(reactiveGuardChance) 또는 복귀
Return: 대상 버림, 스폰 지점 도착 시 체력·기여 기록 초기화 → Patrol
Dead: 콜라이더·에이전트 끔, 사망 모션 → corpseDuration 후 비활성화 → 스포너가 respawnDelay 후 리스폰
```
- **경직 면역**: 경직 후 `staggerImmunity`(1초) 동안은 맞아도 피해만 받고 행동 유지 (무한 경직 방지). 등장 중에는 경직 없음.
- **히트스톱** 중에는 상태 시계·에이전트 모두 정지.

### 감지
- `scanInterval`(0.2초)마다 `sightRange` 안 대상 → `sightAngle` 안 → 눈높이 Linecast로 가림 판정, 가장 가까운 대상.
- `sightAngle` 360이면 등 뒤도 감지(일반 MMO 선공 방식), 지형·벽에 가리면 감지 안 함.
- 공격받으면 시야와 무관하게 공격자를 대상으로 잡고, `provokedDuration`(5초, 맞을 때마다 갱신) 동안은 `loseRange`를 무시하고 추적 → 멀리서 활로 맞혀도 추격. 리쉬는 그대로 적용.

### 피해·보상
- 근접 피해 = `attackPower × 액션 damageMultiplier` (플레이어와 같은 액션 타임라인 판정).
- 방패가 있으면 막기 중 정면 공격 완전 방어, 평소에도 `defense`/`magicDefense`로 감소 ([Combat](Combat.md)).
- 처치 시 `KillReward`가 기여자마다 `experience` 지급 → 플레이어 무기·보조 장비 숙련 ([Mastery](Mastery.md)), `EnemyLoot`가 드랍.

### 머리 위 체력바
- 한 번 맞으면 표시, **사망하거나 대상을 잃을 때까지 유지**, 그 뒤 `hideDelay`(4초) 후 숨김. 숨기기 전에 다시 대상을 잡으면 계속 표시.
- 어두운 배경 = 최대 체력, 마스크로 잘린 채움 = 현재 체력 (줄어든 끝은 직선, 둥근 끝은 제자리).

## 데이터 파라미터
| 필드 | Minion | Warrior | 의미 |
|---|---|---|---|
| maxHealth / attackPower / experience | 60 / 8 / 30 | 120 / 12 / 60 | 체력 / 공격력 / 처치 경험치 |
| weapon / shield | SkeletonAxe / — | SkeletonBlade / SkeletonShield | 무기·방패 (WeaponData·ShieldData 재사용) |
| defense / magicDefense | 0 / 0 | 20 / 5 | 방패 방어력 (적은 고정값) |
| sightRange / sightAngle | 10 / 360 | 10 / 360 | 시야 거리 / 각도 |
| loseRange / provokedDuration / leashRange | 18 / 5 / 25 | 18 / 5 / 25 | 추적 포기 / 도발 시간 / 귀환 거리 |
| walkSpeed / runSpeed / turnSpeed | 1.5 / 4 / 540 | 같음 | 이동·회전 |
| patrolRadius / idleTimeRange | 6 / 2~5 | 같음 | 배회 반경 / 대기 시간 |
| attackRange / attackAngle / attackCooldownRange | 1.8 / 40 / 1.2~2 | 같음 | 공격 거리·정면 각도·쿨타임 |
| maxComboActions / staggerImmunity | 2 / 1 | 3 / 1 | 최대 연속 공격 / 경직 면역 |
| guard (chance / reactive / duration / cooldown / counter) | — | 0.35 / 0.5 / 1.5 / 4 / 0.5 | 막기 (방패가 있을 때만) |
| spawnDuration / corpseDuration | 1.5 / 3 | 같음 | 등장 모션 / 시체 유지 |

| 컴포넌트 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| EnemyPerception | targetLayers / obstacleLayers | — / Default | 감지 대상(Player) / 시야 가림(캐릭터 제외) |
| EnemyPerception | eyeHeight / scanInterval | 1.5 / 0.2 | 가림 판정 높이 / 감지 간격 |
| EnemySpawner | count / spawnRadius / respawnDelay | 3 / 5 / 30 | 유지 마리 수 / 반경 / 리스폰 대기 |
| EnemyLoot | scatterRadius / dropHeight | 1 / 0.3 | 드랍 흩어짐 / 높이 |
| EnemyHealthBarPresenter | hideDelay | 4 | 전투 종료 후 체력바 유지 시간 |

## 에디터 설정
1. **무기 데이터** (`Data/Enemy/WeaponData`): Skeleton_Axe·Skeleton_Blade → Weapon Data(OneHanded, 플레이어 1H 콤보 공유), Skeleton_Shield_Large_A → Shield Data. 칼날 값을 비우면 몸 기준 판정.
2. **반격 액션**: Action Data(Clip `Melee_Block_Attack`) + Action Timeline에서 HitWindow.
3. **데이터**: Enemy Animation Data(기본값), Enemy Data(위 표), Drop Table(`Create → ProjectFantasy → Items → Drop Table`).
4. **애니메이터**: EnemyData 선택 → `Tools → ProjectFantasy → Create Enemy Animator` → `Animation/Controller/Enemy_<이름>.controller` (상태 이름 = 클립 이름, 전이선 없음, 다시 실행하면 같은 에셋 갱신). 상태가 같으면 여러 적이 한 컨트롤러를 공유해도 된다.
5. **체력바**: `Tools → ProjectFantasy → Create Enemy Health Bar Prefab` → 적 프리팹 자식으로.
6. **적 프리팹** (레이어 **Enemy**): 루트에 `EnemyController`(나머지 대부분 자동 추가) + `CapsuleCollider`(Center Y 1, Height 2, Radius 0.4) + `DamageNumberEmitter` + `EnemyLoot`(World Item Prefab), 자식에 Skeleton 모델(Animator = 4번 컨트롤러)·체력바.
   - MeleeAttacker Target Layers = Player, EnemyPerception Target = Player / Obstacle = Default.
   - NavMeshAgent Radius 0.4, Height 2, 플레이 중 발이 뜨거나 묻히면 Base Offset으로 보정.
7. **NavMesh**: 빈 오브젝트에 `NavMeshSurface`(Use Geometry: Physics Colliders, Include Layers: 지면만) → Bake. 에이전트 Step Height 0.4~0.5, Max Slope 45. 지형 MeshCollider는 Convex 끔.
8. **스포너**: 빈 오브젝트 + `EnemySpawner`(Prefab, Count, Radius, Respawn Delay).
9. **플레이어 레이어 마스크**: RangedAttacker Aim·Projectile Hit, SpellCaster Area Damage에 Enemy 포함 (Everything에서 Player 등을 빼는 방식 권장, [인스펙터 가이드](../InspectorGuide.md)).

## 주의사항 / 확장 포인트
- `EnemyData.HitReaction`·`AnimationData`는 필수 (비면 시작 시 에러). 애니메이터 상태 누락은 시작 시 콘솔에 표시.
- 씬에 직접 배치한 적은 제자리에서 스폰, 스포너가 만든 적은 스포너 자식으로 생성·재사용.
- 귀환 중 다시 맞으면 재추적하지만 리쉬를 넘으면 다시 귀환.
- 예정: 원거리(Skeleton Rogue 석궁)·마법(Skeleton Mage) 적 상태, 근접 감지 반경(정면/후방 거리 분리), 보스 부위 파괴, 무리 행동.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-10-07 | 최초 작성: 근접 적 2종(Minion·Warrior), 감지·추격·공격·막기·반격·피격·귀환·사망·리스폰, 드랍 표, 머리 위 체력바, 애니메이터 생성 메뉴 |
