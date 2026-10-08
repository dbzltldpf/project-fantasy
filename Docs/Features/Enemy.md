# Enemy

## 개요
KayKit Skeleton 기반 적 4종. 근접(Minion·Warrior), 사격(Rogue 석궁), 시전(Mage 마법). NavMeshAgent로 이동하고 플레이어와 같은 전투 부품(`Health`·`MeleeAttacker`·액션 타임라인 콤보·`ShieldGuard`·`RangedAttacker`·`SpellCaster`·`HitStop`·`KillReward`)을 재사용한다. 공격 방식은 EnemyData의 **전투 방식(Combat Style)** 드롭다운으로 고른다(전략 패턴). 스폰 지점에서 배회 → 발견·추격 → 공격 → 처치 시 경험치·드랍 → 일정 시간 뒤 리스폰.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Enemy/Data/EnemyData.cs](../../Assets/Project/Scripts/Enemy/Data/EnemyData.cs) | 적 한 종류: 능력치·장비·감지·이동·전투 방식·공통 전투·근접·막기·등장/사망·드랍·애니메이션 (SO) |
| [Enemy/Data/EnemyAnimationData.cs](../../Assets/Project/Scripts/Enemy/Data/EnemyAnimationData.cs) | 애니메이터 상태 이름·파라미터(ActionSpeed·PoseSpeed)·전환 시간 (적 공용 SO) |
| [Enemy/Data/EnemyGuardSettings.cs](../../Assets/Project/Scripts/Enemy/Data/EnemyGuardSettings.cs) | 막기 확률·시간·쿨타임·반격 |
| [Enemy/Combat/](../../Assets/Project/Scripts/Enemy/Combat/) | 전투 방식: `EnemyCombatStyle`(기반) · `MeleeCombatStyle` · `EnemyRangedStyle`(원거리 공통) · `ShooterCombatStyle` · `CasterCombatStyle` · `CasterSpell` |
| [Enemy/EnemyController.cs](../../Assets/Project/Scripts/Enemy/EnemyController.cs) | 컴포넌트 조립, 상태 머신, 스폰·리스폰, 공격 쿨타임·막기 쿨타임·경직 면역, `Despawned` |
| [Enemy/EnemyNavigator.cs](../../Assets/Project/Scripts/Enemy/EnemyNavigator.cs) | NavMeshAgent 래퍼: 목적지 이동(재탐색 최소화)·정지·회전(직접 제어)·넉백·직접 이동·히트스톱 정지·순간 이동 |
| [Enemy/EnemyPerception.cs](../../Assets/Project/Scripts/Enemy/EnemyPerception.cs) | 대상 감지(거리 → 시야각 → 가림, NonAlloc·간격), 공격받으면 공격자 추적(도발 시간), 사선 확인, `TargetChanged` |
| [Enemy/EnemyAnimator.cs](../../Assets/Project/Scripts/Enemy/EnemyAnimator.cs) | 상태 재생(해시 캐싱·중복 방지), 액션 재생 속도, 자세 고정(`AnimatorPoseHold`), 클립 종료 확인, 누락 상태 검증 |
| [Enemy/EnemyLoadout.cs](../../Assets/Project/Scripts/Enemy/EnemyLoadout.cs) | 무기·방패 모델, 근접 판정(칼날/몸 기준), 방패 방어력 |
| [Enemy/EnemyLoot.cs](../../Assets/Project/Scripts/Enemy/EnemyLoot.cs) | 사망 시 드랍 표 → 주변에 `WorldItem` 생성 |
| [Enemy/EnemySpawner.cs](../../Assets/Project/Scripts/Enemy/EnemySpawner.cs) | 반경 안 N마리 유지, 사라진 적은 대기 후 같은 인스턴스로 리스폰 (풀링) |
| [Enemy/EnemyHealthBarPresenter.cs](../../Assets/Project/Scripts/Enemy/EnemyHealthBarPresenter.cs) | 머리 위 체력바 표시 규칙 |
| [Enemy/States/](../../Assets/Project/Scripts/Enemy/States/) | `Spawn` `Patrol` `Chase` `Attack` `Guard` `Shoot` `Cast` `Hit` `Return` `Dead` |
| [Enemy/Editor/EnemyAnimatorBuilder.cs](../../Assets/Project/Scripts/Enemy/Editor/EnemyAnimatorBuilder.cs) | `Tools → ProjectFantasy → Create Enemy Animator` |
| [Items/Loot/](../../Assets/Project/Scripts/Items/Loot/) | `DropTable`(SO)·`DropEntry`·`LootDrop` ([Inventory](Inventory.md)) |
| [UI/Enemy/EnemyHealthBarView.cs](../../Assets/Project/Scripts/UI/Enemy/EnemyHealthBarView.cs) | 월드 공간 체력바 표시·빌보드 ([UI](UI.md)) |

## 동작 흐름
### 상태 전이 (공통)
```
Spawn(Spawn_Ground, spawnDuration) → Patrol
Patrol: 대기(idleTimeRange) ↔ 반경 내 무작위 지점 걷기 ── 대상 발견 → Chase
Chase: 전투 방식의 공격 거리 밖이거나 사선이 가리면(원거리) 달려서 접근
       공격 거리 안: 정지·바라보기 → 전투 방식이 공격 결정 (가까워도 제자리에서 공격, 물러나지 않음)
       대상 상실 / 스폰 지점에서 leashRange 초과 → Return
Hit: 넉백·경직 → 반응 막기(방패) 또는 복귀
Return: 대상 버림, 스폰 지점 도착 시 체력·기여 기록·마법 쿨타임 초기화 → Patrol
Dead: 콜라이더·에이전트 끔, 사망 모션 → corpseDuration 후 비활성화 → 스포너가 respawnDelay 후 리스폰
```
- **경직 면역**: 경직 후 `staggerImmunity`(1초) 동안은 맞아도 피해만 받고 행동 유지 (무한 경직 방지). 등장 중에는 경직 없음.
- **히트스톱** 중에는 상태 시계·에이전트 모두 정지.

### 전투 방식 (Combat Style)
| 방식 | 공격 거리 | 공격 상태 | 내용 |
|---|---|---|---|
| (비움) / `MeleeCombatStyle` | `attackRange` (Melee 항목) | Attack · Guard | 무기 콤보, 방패가 있으면 확률적 막기·반격 |
| `ShooterCombatStyle` | `attackRange` (스타일 값) | Shoot | 장전 → 조준 → 발사, 무기 `RangedWeaponData` 재사용, 탄약 무한 |
| `CasterCombatStyle` | `attackRange` (스타일 값) | Cast | 쿨타임이 끝난 마법을 가중치로 선택 → 마법탄 또는 범위 마법 |

- 원거리 공통(`EnemyRangedStyle`): 사선(눈높이 Linecast)이 가리면 쏘지 않고 접근, 대상 몸통(`targetHeight`) 조준 + 탄 퍼짐(`spreadAngle`), **이동 예측 없음** → 옆으로 움직이면 피할 수 있다.
- 스타일은 데이터(공유)라 런타임 상태(마법별 쿨타임 등)는 적 개체의 상태가 보관한다.

### 근접 (Attack · Guard)
- 공격마다 **1 ~ maxComboActions 중 무작위 횟수**만큼 이어 친다 (Warrior 1~3타, Minion 1~2타).
- 콤보 창이 열렸을 때 대상이 사거리 1.5배 안이면 다음 공격을 예약 → 전이 프레임에 이어 침 (플레이어와 같은 예약 방식, 창이 먼저 닫혀도 유지).
- 막기(방패): `guardDuration` 동안 바라보며 천천히 접근, 막으면 경직 → `counterChance`로 반격(단일 액션).
- 피해 = `attackPower × 액션 damageMultiplier`.

### 사격 (Shoot, Rogue)
```
(석궁) 장전(Ranged_2H_Reload) → 조준(Ranged_2H_Aiming, 클립이 끝까지 재생된 뒤 aimDuration만큼 유지)
→ 발사(Ranged_2H_Shoot, releaseTime에 화살 생성) → 쿨타임 → 다음 공격은 다시 장전부터
```
- 평소 대기 모션은 무기의 Idle State Name(`Idle_A`), 공격할 때만 장전·조준 자세.
- 발사 속도: `Ballistics.TrySolveLaunchVelocity`로 중력을 보정한 낮은 궤도 → 대상 위치에 닿음, 이후 퍼짐 적용.
- 피해 = `attackPower` (물리). 화살 프리팹·속도·중력은 무기의 Ammo.

### 시전 (Cast, Mage)
```
준비된 마법(쿨타임 끝) 중 가중치 추첨 → 시전 모션(spell.castStateName) → releaseTime에 발동 → castDuration 후 복귀
  마법탄(ProjectileSpellData): castOffset에서 대상 몸통으로 직선 발사 + 퍼짐
  범위 마법(AreaSpellData): 발동 순간 대상 발밑에 마법진 → activationDelay 뒤 폭발 (마법진을 보고 피할 수 있음)
```
- 피해 = `attackPower × spell.damageMultiplier` (마법). 마법별 쿨타임은 SpellData의 cooldown.
- 적은 플레이어와 **별도 마법 에셋**을 쓴다 (`Data/Enemy/ProjectileSpell`, `EnemyAreaSpell`): 예고 시간·쿨타임·빨간 이펙트를 플레이어와 따로 조절.

### 감지
- `scanInterval`(0.2초)마다 `sightRange` 안 대상 → `sightAngle` 안 → 눈높이 Linecast로 가림 판정, 가장 가까운 대상.
- `sightAngle` 360이면 등 뒤도 감지(일반 MMO 선공 방식), 지형·벽에 가리면 감지 안 함.
- 공격받으면 시야와 무관하게 공격자를 대상으로 잡고, `provokedDuration`(5초, 맞을 때마다 갱신) 동안은 `loseRange`를 무시하고 추적 → 멀리서 활로 맞혀도 추격. 리쉬는 그대로 적용.

### 피해·보상
- 방패가 있으면 막기 중 정면 공격 완전 방어, 평소에도 `defense`/`magicDefense`로 감소 ([Combat](Combat.md)).
- 방패에 막힌 화살은 박히지 않고 사라진다 (`DamageResult.Blocked`).
- 처치 시 `KillReward`가 기여자마다 `experience` 지급 → 플레이어 무기·보조 장비 숙련 ([Mastery](Mastery.md)), `EnemyLoot`가 드랍.

### 머리 위 체력바
- 한 번 맞으면 표시, **사망하거나 대상을 잃을 때까지 유지**, 그 뒤 `hideDelay`(4초) 후 숨김. 숨기기 전에 다시 대상을 잡으면 계속 표시.
- 어두운 배경 = 최대 체력, 마스크로 잘린 채움 = 현재 체력 (줄어든 끝은 직선, 둥근 끝은 제자리).

## 데이터 파라미터
### EnemyData
| 필드 | Minion | Warrior | Rogue | Mage | 의미 |
|---|---|---|---|---|---|
| maxHealth / attackPower / experience | 60 / 8 / 30 | 120 / 12 / 60 | 50 / 10 / 40 | 42 / 12 / 45 | 체력 / 공격력 / 처치 경험치 |
| weapon | SkeletonAxe | SkeletonBlade (+ SkeletonShield) | SkeletonCrossBow2H | SkeletonStaff | 무기 (WeaponData·ShieldData 재사용) |
| combatStyle | Melee | Melee | Shooter | Caster | 전투 방식 |
| defense / magicDefense | 0 / 0 | 20 / 5 | 0 / 0 | 0 / 0 | 방패 방어력 (적은 고정값) |
| sightRange / sightAngle | 10 / 360 | 같음 | 같음 | 같음 | 시야 거리 / 각도 |
| loseRange / provokedDuration / leashRange | 18 / 5 / 25 | 같음 | 같음 | 같음 | 추적 포기 / 도발 시간 / 귀환 거리 |
| walkSpeed / runSpeed / turnSpeed | 1.5 / 4 / 540 | 같음 | 같음 | 같음 | 이동·회전 |
| patrolRadius / idleTimeRange | 6 / 2~5 | 같음 | 같음 | 같음 | 배회 반경 / 대기 시간 (Min~Max) |
| attackAngle / attackCooldownRange | 40 / 1.2~2 | 40 / 1.2~2 | 40 / 0~0 | 40 / 1~1.5 | 공격 시작 정면 각도 / 공격 후 쿨타임 (Min~Max) |
| attackRange / maxComboActions | 1.8 / 2 | 1.8 / 3 | — | — | 근접 전용 |
| guard (chance / reactive / duration / cooldown / counter) | — | 0.35 / 0.5 / 1.5 / 4 / 0.5 | — | — | 막기 (근접 + 방패) |
| staggerImmunity / spawnDuration / corpseDuration | 1 / 1.5 / 3 | 같음 | 같음 | 같음 | 경직 면역 / 등장 모션 / 시체 유지 |

### 전투 방식 값
| 방식 | 필드 | 값 | 의미 |
|---|---|---|---|
| 원거리 공통 | attackRange / targetHeight / spreadAngle | 14(Rogue)·15(Mage) / 1.2 / 3 | 공격 거리 / 조준 높이 / 탄 퍼짐(도) |
| Shooter | aimDuration | 0 (기본 0.2) | 조준 클립이 끝난 뒤 추가 유지 시간 |
| Caster | castOffset / spells | (0, 1.5, 0.6) / 마법탄 ×3, 범위 마법 ×1 | 마법탄 발사 위치 / 마법·가중치 |

### 적 전용 마법
| 에셋 | castState | releaseTime / cooldown / damageMultiplier | 기타 |
|---|---|---|---|
| `Data/Enemy/ProjectileSpell` | Ranged_Magic_Shoot | 0.3 / 1 / 1 | 투사체 `Prefabs/Combat/Enemy/ProjectileSpell`(Sparks red), 명중 `Electro hit Red` |
| `Data/Enemy/EnemyAreaSpell` | Ranged_Magic_Spellcasting | 0.3 / 3 / 1.5 | radius 3, **activationDelay 1.2** (예고 시간), 폭발 `Plexus AoE Red` |

### 컴포넌트
| 컴포넌트 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| EnemyPerception | targetLayers / obstacleLayers | — / Default | 감지 대상(Player) / 시야 가림(캐릭터 제외) |
| EnemyPerception | eyeHeight / scanInterval | 1.5 / 0.2 | 가림 판정 높이 / 감지 간격 |
| EnemySpawner | count / spawnRadius / respawnDelay | 3 / 5 / 30 | 유지 마리 수 / 반경 / 리스폰 대기 |
| EnemyLoot | scatterRadius / dropHeight | 1 / 0.3 | 드랍 흩어짐 / 높이 |
| EnemyHealthBarPresenter | hideDelay | 4 | 전투 종료 후 체력바 유지 시간 |

## 에디터 설정
1. **무기 데이터** (`Data/Enemy/WeaponData`): 플레이어 무기를 복제해 Model만 Skeleton 무기로 교체.
   - 근접: Skeleton_Axe·Skeleton_Blade → Weapon Data(OneHanded, 플레이어 1H 콤보 공유), Skeleton_Shield_Large_A → Shield Data.
   - Rogue: `CrossBow2H` 복제 → `SkeletonCrossBow2H`, Model `Skeleton_Crossbow`, **Idle State Name = Idle_A** (플레이어 석궁은 Ranged_2H_Aiming 유지).
   - Mage: `Staff` 복제 → `SkeletonStaff`, Model `Skeleton_Staff`.
2. **반격 액션**: Action Data(Clip `Melee_Block_Attack`) + Action Timeline에서 HitWindow.
3. **적 전용 마법**: 플레이어 마법을 복제해 쿨타임·예고 시간·이펙트 조정. 빨간 이펙트는 `Tools → ProjectFantasy → Recolor Effect`로 생성 ([Combat](Combat.md)).
4. **데이터**: Enemy Animation Data(기본값), Enemy Data(위 표, **Combat Style** 드롭다운), Drop Table.
5. **애니메이터**: EnemyData 선택 → `Tools → ProjectFantasy → Create Enemy Animator` → `Animation/Controller/Enemy_<이름>.controller`.
   - 상태 = 기본 상태 + 전투 방식이 알려 주는 상태(근접 콤보·반격 / 장전·조준·발사 / 마법 시전 모션). 액션 상태는 ActionSpeed, 조준 대기는 PoseSpeed 연결.
   - Spells·무기를 바꾸면 **다시 실행** (같은 에셋 갱신, 프리팹 연결 유지).
6. **체력바**: `Tools → ProjectFantasy → Create Enemy Health Bar Prefab` → 적 프리팹 자식으로.
7. **적 프리팹** (레이어 **Enemy**): 루트에 `EnemyController`(나머지 대부분 자동 추가) + `CapsuleCollider`(Center Y 1, Height 2, Radius 0.4) + `DamageNumberEmitter` + `EnemyLoot`(World Item Prefab), 자식에 Skeleton 모델(Animator = 5번 컨트롤러)·체력바.
   - **EnemyController Data를 해당 적 데이터로** (프리팹 복제 시 이전 데이터가 남지 않게 주의).
   - MeleeAttacker Target Layers = Player, EnemyPerception Target = Player / Obstacle = Default.
   - Rogue·Mage: `RangedAttacker` 추가 (Aim·Projectile Hit Layers = Everything − Enemy·Projectile·Interactable).
   - Mage: `SpellCaster` 추가 (Area Damage Layers = Everything − Enemy, Ground Layers = Default).
   - 누락 시 시작할 때 콘솔에 필요한 컴포넌트·무기 종류가 표시된다.
   - NavMeshAgent Radius 0.4, Height 2, 플레이 중 발이 뜨거나 묻히면 Base Offset으로 보정.
8. **NavMesh**: 빈 오브젝트에 `NavMeshSurface`(Use Geometry: Physics Colliders, Include Layers: 지면만) → Bake. 에이전트 Step Height 0.4~0.5, Max Slope 45. 지형 MeshCollider는 Convex 끔.
9. **스포너**: 빈 오브젝트 + `EnemySpawner`(Prefab, Count, Radius, Respawn Delay).
10. **플레이어 레이어 마스크**: RangedAttacker Aim·Projectile Hit, SpellCaster Area Damage에 Enemy 포함 (Everything에서 Player 등을 빼는 방식 권장, [인스펙터 가이드](../InspectorGuide.md)).

## 주의사항 / 확장 포인트
- `EnemyData.HitReaction`·`AnimationData`는 필수 (비면 시작 시 에러). 애니메이터 상태 누락은 시작 시 콘솔에 표시.
- 새 공격 방식은 `EnemyCombatStyle`을 상속한 `[Serializable]` 클래스만 추가하면 드롭다운에 자동 노출 (공격 거리, 공격 시작, 필요한 애니메이터 상태, 검증).
- 씬에 직접 배치한 적은 제자리에서 스폰, 스포너가 만든 적은 스포너 자식으로 생성·재사용.
- 귀환 중 다시 맞으면 재추적하지만 리쉬를 넘으면 다시 귀환.
- 예정: 근접 감지 반경(정면/후방 거리 분리), 보스 부위 파괴, 무리 행동, 강인도(누적 피해 경직).

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-10-07 | 최초 작성: 근접 적 2종(Minion·Warrior), 감지·추격·공격·막기·반격·피격·귀환·사망·리스폰, 드랍 표, 머리 위 체력바, 애니메이터 생성 메뉴 |
| 2026-10-08 | 전투 방식(근접·사격·시전) 전략 패턴, 원거리 적 Rogue(장전 → 조준 클립 끝까지 → 발사)·마법 적 Mage(마법탄·예고형 범위 마법), 중력 보정 사격, 근접 콤보 무작위 횟수·예약 방식, 조준 자세 고정, 방패에 막힌 화살 미부착, 범위 값 Min/Max 표시, 적 전용 마법·빨간 이펙트 |
