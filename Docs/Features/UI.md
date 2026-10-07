# UI

## 개요
조준점, 월드 공간 데미지 숫자·적 체력바, 인벤토리 창·숙련도 창·퀵슬롯 바·플레이어 상태바·HUD. 게임플레이 코드는 UI를 직접 참조하지 않고 이벤트(Presenter/Emitter)로 연결한다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [CrosshairView.cs](../../Assets/Project/Scripts/UI/CrosshairView.cs) | 화면 중앙 조준점 표시/숨김 (기본 숨김, 조준 모드에서 표시) |
| [DamageNumberEmitter.cs](../../Assets/Project/Scripts/UI/DamageNumbers/DamageNumberEmitter.cs) | `Health.Damaged` 구독 → 스포너에 표시 요청 (허수아비·적 공용) |
| [DamageNumberSpawner.cs](../../Assets/Project/Scripts/UI/DamageNumbers/DamageNumberSpawner.cs) | 씬 단위 데미지 숫자 생성·풀링, 겹침 방지 랜덤 오프셋 |
| [DamageNumber.cs](../../Assets/Project/Scripts/UI/DamageNumbers/DamageNumber.cs) | 떠오르며 사라지는 TextMeshPro 숫자, 카메라 방향 빌보드, `SetText`로 GC 없음 |
| [Inventory/](../../Assets/Project/Scripts/UI/Inventory/) | 인벤토리 창, 아이템 칸, 우클릭 메뉴, 드래그 고스트, 능력치 비교 문구 ([Inventory](Inventory.md)) |
| [HUD/](../../Assets/Project/Scripts/UI/HUD/) | 상태바(`StatusBarView`: 체력·자원 공용, 수치, 낮으면 깜빡임), 퀵슬롯 바, 화살 수, 줍기 안내, 안내 문구(여러 줄 동시 표시) |
| [Mastery/](../../Assets/Project/Scripts/UI/Mastery/) | 숙련도 창(K)·줄 ([Mastery](Mastery.md)) |
| [Enemy/EnemyHealthBarView.cs](../../Assets/Project/Scripts/UI/Enemy/EnemyHealthBarView.cs) | 적 머리 위 월드 공간 체력바 표시·빌보드 (표시 규칙은 `EnemyHealthBarPresenter`, [Enemy](Enemy.md)) |
| [Common/BarFill.cs](../../Assets/Project/Scripts/UI/Common/BarFill.cs) | 3조각 바 채움: `SetRatio`(양 끝 유지, 숙련 경험치) / `SetMaskedRatio`(마스크로 잘림, 체력·자원) |
| [Editor/InventoryUIBuilder.cs](../../Assets/Project/Scripts/UI/Editor/InventoryUIBuilder.cs) | 인벤토리 UI 계층 일괄 생성 메뉴 |
| [Editor/MasteryUIBuilder.cs](../../Assets/Project/Scripts/UI/Editor/MasteryUIBuilder.cs) | 숙련도 창 생성 메뉴 (Kenney 3조각 경험치 바, 줄 프리팹은 없을 때만 생성) |
| [Editor/PlayerStatusUIBuilder.cs](../../Assets/Project/Scripts/UI/Editor/PlayerStatusUIBuilder.cs) | `Tools → ProjectFantasy → Create Player Status UI`: 하단 중앙(퀵슬롯 위) 상태바, PlayerHudPresenter 연결 |
| [Editor/EnemyHealthBarBuilder.cs](../../Assets/Project/Scripts/UI/Editor/EnemyHealthBarBuilder.cs) | `Tools → ProjectFantasy → Create Enemy Health Bar Prefab` |
| [Editor/UIPresenterSetup.cs](../../Assets/Project/Scripts/UI/Editor/UIPresenterSetup.cs) | `Tools → ProjectFantasy → Gather UI Presenters`: Presenter를 Canvas 아래 `UI Presenters` 한 곳으로 모음 (UI 빌더가 먼저 호출, 다시 생성해도 연결 유지) |
| [Editor/UIBuilderUtility.cs](../../Assets/Project/Scripts/UI/Editor/UIBuilderUtility.cs) | UI 빌더 공용 생성·배치 함수, Kenney 3조각 바(`CreateCappedBar`)·체력 바(`CreateMaskedBar`: 어두운 배경 + 마스크 채움) |

## 동작 흐름
```
조준: PlayerController.AimViewChanged → PlayerAimPresenter → CrosshairView.SetVisible / ThirdPersonCamera.SetAiming
아이템: Inventory·QuickSlots·IItemActions 변경 → PlayerMenuPresenter / PlayerHudPresenter → InventoryWindow · QuickSlotBarView · AmmoCounterView · NoticeView
숙련: WeaponMastery.ExperienceGained·LevelChanged → MasteryWindow(해당 줄만) · PlayerHudPresenter → NoticeView
피해: Health.Damaged → DamageNumberEmitter → DamageNumberSpawner.Spawn(피해량, 타격 지점) → DamageNumber (수명 후 풀 반환)
체력: Health.HealthChanged → PlayerHudPresenter → StatusBarView (플레이어) / EnemyHealthBarPresenter → EnemyHealthBarView (적)
```

## 데이터 파라미터
| 컴포넌트 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| DamageNumber | lifetime / riseSpeed | 0.8 / 1.5 | 표시 시간 / 상승 속도 |
| DamageNumber | alphaOverLifetime / scaleOverLifetime | 1→0 / 1 | 수명 비율별 투명도·크기 커브 |
| DamageNumberSpawner | damageColor | 흰색 | 숫자 색 |
| DamageNumberSpawner | spawnOffset / randomHorizontalRadius | (0, 0.3, 0) / 0.3 | 타격 지점 기준 위치, 겹침 방지 반경 |
| DamageNumberSpawner | poolRoot | 비움 | 숫자 부모 (비우면 `Pools/DamageNumbers/프리팹 이름`) |
| StatusBarView | warningThreshold / blinkSpeed / minBlinkAlpha | 0.3 / 2 / 0.35 | 깜빡임 시작 비율 / 속도 / 가장 흐린 알파 |
| StatusBarView | valueFormat | `{0} / {1}` | 수치 문구 |
| EnemyHealthBarPresenter | hideDelay | 4 | 전투 종료 후 유지 시간 |
| DamageNumberEmitter | spawner | 자동 탐색 | 비우면 씬에서 최초 1회 탐색 |

## 에디터 설정
1. `Window → TextMeshPro → Import TMP Essential Resources` (최초 1회)
2. **데미지 숫자 프리팹**: TextMeshPro(3D, UI 아님) + `DamageNumber` → 씬 오브젝트 `DamageNumberSpawner`에 연결
3. **조준점**: Canvas(Screen Space Overlay) 중앙 Image + `CrosshairView` → `PlayerAimPresenter`에 연결
4. **피격 대상**: `Health` 오브젝트에 `DamageNumberEmitter` 추가 (허수아비는 [Combat](Combat.md)의 `TrainingDummy` 참고)
5. **플레이어 상태바**: Create Inventory UI 이후 `Tools → ProjectFantasy → Create Player Status UI` (다시 만들어도 Presenter 연결 유지)
6. **적 체력바**: `Tools → ProjectFantasy → Create Enemy Health Bar Prefab` → 적 프리팹 자식으로 배치

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-10-02 | 최초 작성 (조준점, 데미지 숫자) |
| 2026-10-06 | 인벤토리 창·퀵슬롯 바·화살 수·줍기 안내·안내 문구, UI 빌더 메뉴, 한글 폰트(Pretendard 기본, Black Han Sans 제목, MaruBuri 대화) |
| 2026-10-07 | 숙련도 창(K)·UI 빌더, Presenter를 `UI Presenters` 오브젝트로 분리, 아이템 칸 티어 표시, 안내 문구 여러 줄, 데미지 숫자 풀 부모 폴더 |
| 2026-10-07 | 플레이어 상태바(하단 중앙, 체력 수치·저체력 깜빡임, 자원 줄 자리), 적 머리 위 체력바(어두운 배경 + 마스크 채움), `BarFill`·3조각 바 생성 공용화 |
