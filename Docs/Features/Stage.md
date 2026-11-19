# Stage

## 개요
맵 공간 하나와 게이트 하나를 보스까지 재사용하는 스테이지 진행. 스테이지마다 적 그룹이 켜지고, 클리어 목표를 끝내면 게이트가 열린다. 게이트에 들어가면 플레이어를 `PlayerSpawn`으로 순간이동시키고 게이트를 닫은 뒤 다음 스테이지의 적 그룹을 켠다. 목표 순서는 스테이지 목록 하나에서 자동으로 만든다. 플레이어가 죽으면 잠시 뒤 `PlayerSpawn`에서 현재 스테이지를 다시 시작한다 (체크포인트 대신).

`StageDirector`는 Objective·World·Player·Camera를 엮는 최상위 지휘자다. 다른 기능은 Stage를 모른다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Stage/StageDirector.cs](../../Assets/Project/Scripts/Stage/StageDirector.cs) | 스테이지 목록(적 그룹 + 클리어 목표), 목표 목록 생성, 지정 스테이지로 시작(`GoToStage`), 클리어 시 게이트 개방, 게이트 진입 시 순간이동·게이트 닫기·그룹 교체, 플레이어 사망 시 대기 후 현재 스테이지 재시작 |

연동: [World](World.md) `AreaGate`·`LocationEvents`, [Objective](Objective.md) `ObjectiveTracker.Begin`·`StepCompleted`·`RestartStep`·`ReachObjectiveData`, [Player](Player.md) `PlayerController.Revive`, [Enemy](Enemy.md) `EnemySpawner.ResetEnemies`, [Combat](Combat.md) 플레이어 `Health.Died`, [Camera](Camera.md) `ThirdPersonCamera.SnapToTarget`, [Narration](Narration.md) `NarrationPlayer.Play`.

## 동작 흐름
```
Start
  → BuildObjectives: 스테이지마다 clearObjective, 마지막을 뺀 각 스테이지 뒤에 gateObjective(ReachGate)
     예) Kill_Minion_2 → ReachGate → Kill_Rogue_2 → ReachGate → … → Kill_Boss_1
  → GoToStage(0)

GoToStage(N)  (시작·테스트 패널 다음 스테이지·장 선택 공용)
  → 범위 밖이면 무시 → stageIndex = N
  → N번 그룹만 켜고 나머지는 끔 → N번 스포너 전부 ResetEnemies
  → AreaGate.Close()
  → ObjectiveTracker.Begin(목록, N × ObjectivesPerStage)
  → PlayerController.Revive(PlayerSpawn) → ThirdPersonCamera.SnapToTarget()
  → N번 intro가 있으면 NarrationPlayer.Play(intro)

ObjectiveTracker.StepCompleted(목표)
  → 현재 스테이지의 clearObjective이고 마지막 스테이지가 아니면 AreaGate.Open()

LocationEvents.Reached(장소)
  → gateObjective.IsMatch(장소) && AreaGate.IsOpen 이면 GoToStage(현재 + 1)

플레이어 Health.Died
  → (이미 대기 중이면 무시) → retryDelay 대기 → Retry
     → 게이트가 닫혀 있으면(미클리어): 현재 그룹의 스포너 전부 ResetEnemies → ObjectiveTracker.RestartStep
     → PlayerController.Revive(PlayerSpawn) → ThirdPersonCamera.SnapToTarget()
```
- 목표 순서는 스테이지 목록에서만 만든다 (순서표가 두 곳이면 어긋남).
- 게이트 진입 알림은 반복(`triggerOnce` 끔)이므로 `IsOpen`으로 걸러 스테이지 건너뛰기를 막는다.
- 이전 그룹을 끄면 그 그룹의 스포너도 멈춘다.
- 스테이지 시작 경로는 `GoToStage` 하나다 (게임 시작·게이트 진입·테스트 패널·장 선택). 다음 스테이지로 넘어갈 때도 플레이어 체력이 가득 찬다.
- 재시작(`Retry`)은 `GoToStage`를 거치지 않아 인트로가 다시 나오지 않는다.
- 마지막(보스) 스테이지는 클리어해도 게이트를 열지 않는다 (엔딩에서 처리 예정).
- 트래커 시작은 `Start`에서 한다. 모든 `OnEnable`(Presenter 구독)이 끝난 뒤라 첫 `Changed`를 놓치지 않는다.
- 재시작 시 클리어 여부는 게이트 상태(`IsOpen`)로 판단한다. 이미 클리어한 스테이지는 적을 되살리지 않고 플레이어만 부활.
- 적 리셋을 플레이어 부활보다 먼저 해 이전 상태의 적에게 감지되지 않게 한다.
- `ResetEnemies`는 살아 있는 적도 껐다 켜서 리스폰한다. 일반 리스폰과 같은 경로라 `OnEnable`에서 초기화하는 컴포넌트(체력바 숨김 등)가 모두 초기화된다.

## 데이터 파라미터
| 대상 | 필드 | 의미 |
|---|---|---|
| StageDirector | stages (group / clearObjective / intro) | 진행 순서대로: 적 그룹(스포너 + 구역) / 끝나면 게이트를 열 목표 / 시작 내레이션 (비우면 없음) |
| StageDirector | objectiveTracker / areaGate | 씬의 트래커 / 게이트 |
| StageDirector | gateObjective | 게이트 이동 목표 `ReachGate` (진입 장소 판정에도 사용) |
| StageDirector | playerSpawn | 순간이동 위치·방향 (Z축이 맵 안쪽) |
| StageDirector | thirdPersonCamera / narrationPlayer | 카메라 즉시 맞춤 / 인트로 내레이션 재생 |
| StageDirector | playerController / health | 부활(`Revive`) / 플레이어 사망 감지 |
| StageDirector | retryDelay | 쓰러진 뒤 재시작까지 대기 (2초, 사망 모션 표시) |

현재 스테이지:
| # | 그룹 | 클리어 목표 | 사용 무기 |
|---|---|---|---|
| 1 | CampMinion | Kill_Minion_2 | 한손검 |
| 2 | CampRogue | Kill_Rogue_2 | 활 |
| 3 | CampWarrior | Kill_Warrior_1 | 양손검 |
| 4 | CampMage | Kill_Mage_2 | 지팡이 |
| 5 | CampBoss | Kill_Boss_1 | 전부 |

## 에디터 설정
1. 스테이지마다 빈 부모(예: `CampRogue`) 아래 `EnemySpawner`(**Respawn 끔**, Count = 목표 처치 수) + `EnemyTerritory`
2. 처치 목표 에셋(`Kill_X_N`, 문구에 사용 무기) 생성, 게이트 이동 목표 `ReachGate`(장소 `LocationGate`) 하나
3. 맵 안에 빈 오브젝트 `PlayerSpawn` (바닥에서 0.1 위, Z축이 맵 안쪽)
4. 게이트 구성은 [World](World.md) 에디터 설정
5. `StageDirector` 오브젝트 → Stages를 순서대로 (Intro에 내레이션 에셋), 나머지 참조 연결

## 주의사항 / 확장 포인트
- 스포너 Count와 목표 처치 수를 맞춘다 (리스폰을 끄므로 Count가 곧 처치 수).
- 스테이지 하나당 목표가 2개(처치 + 게이트 이동)라 N번 스테이지 목표 위치는 `N × ObjectivesPerStage`다. 목표 구성을 바꾸면 이 상수도 맞춘다.
- 장 선택은 `GoToStage(N)`을 그대로 호출하면 된다.
- 외부 조회용: `CurrentStageIndex`, `StageCount`, `CurrentGroup` ([Dev](Dev.md) 테스트 패널이 사용).
- 보스 처치 후 엔딩 연결은 아직 없다.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-11-19 | 최초 작성: 스테이지 목록·목표 자동 생성, 클리어 시 게이트 개방, 게이트 진입 시 순간이동·그룹 교체, 5스테이지 배치 |
| 2026-11-19 | 플레이어 사망 시 현재 스테이지 재시작 (미클리어면 적·목표 진행 초기화, 플레이어 부활) |
| 2026-11-19 | `GoToStage(N)`: 지정 스테이지로 시작 (그룹·적·게이트·목표·플레이어 초기화), 현재 스테이지 조회 속성 공개 |
| 2026-11-19 | 스테이지 인트로 내레이션(`Stage.intro`), 게이트 진입도 `GoToStage`로 통합 (`MoveToNextStage` 본문·`playerMotor`·`HasCurrentStage` 제거) |
