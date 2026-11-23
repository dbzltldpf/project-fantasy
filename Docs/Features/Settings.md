# Settings

## 개요
플레이어가 바꾸는 게임 설정. 지금은 **프레임 제한** 하나다. 기본값은 모니터 주사율(VSync)이고, 일시정지 메뉴의 드롭다운에서 30·60·120·144·제한 없음으로 바꿀 수 있다. 선택값은 `PlayerPrefs`에 저장되어 다음 실행 때도 유지된다. 테스트 패널(DevPanel) 밖에 있어 제출 빌드에서도 보인다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Settings/Data/FrameRateSettingsData.cs](../../Assets/Project/Scripts/Settings/Data/FrameRateSettingsData.cs) | 프레임 제한 선택지 SO (`Options`: 표시 이름·VSync 사용·목표 프레임, `DefaultIndex`) |
| [Settings/FrameRateSettings.cs](../../Assets/Project/Scripts/Settings/FrameRateSettings.cs) | 시작 시 저장값 복원·적용, 드롭다운 선택 시 적용·저장(`Select`), 현재 선택(`CurrentIndex`) |
| [UI/Settings/FrameRateDropdownView.cs](../../Assets/Project/Scripts/UI/Settings/FrameRateDropdownView.cs) | 드롭다운 선택지 표시(`SetOptions`), 선택 알림(`Selected`). 적용·저장은 모름 |

## 동작 흐름
```
Awake  → PlayerPrefs "Settings.FrameRate" 읽기 (없거나 범위 밖이면 DefaultIndex) → Apply
Start  → 선택지 Label 목록 → FrameRateDropdownView.SetOptions(목록, CurrentIndex)  (알림 없이 값만 맞춤)
선택   → TMP_Dropdown.onValueChanged → Selected → FrameRateSettings.Select(index) → Apply → PlayerPrefs 저장

Apply(index)
  → UseVSync 켬: QualitySettings.vSyncCount = 1, Application.targetFrameRate = -1 (모니터 주사율)
  → UseVSync 끔: vSyncCount = 0, targetFrameRate = TargetFrameRate (-1 = 제한 없음)
```
- PC에서는 VSync가 켜져 있으면 `targetFrameRate`를 무시하므로, 숫자 제한을 쓸 때는 VSync를 끈다.
- 적용은 `Awake`(첫 프레임 전), 드롭다운 연결은 `Start`(View의 `Awake` 이후)에서 한다.
- 처음 값을 맞출 때 `SetValueWithoutNotify`를 써서 불필요한 저장이 일어나지 않게 한다.
- `Apply`는 저장하지 않으므로 외부에서는 `Select`만 호출한다.

## 데이터 파라미터
| 대상 | 필드 | 의미 |
|---|---|---|
| FrameRateSettingsData | options (label / useVSync / targetFrameRate) | 드롭다운 순서대로: 표시 이름 / 켜면 모니터 주사율(목표 프레임 무시) / VSync를 끈 경우 목표 프레임(-1 = 제한 없음) |
| FrameRateSettingsData | defaultIndex | 저장값이 없을 때 선택지 (0 = 모니터 주사율) |
| FrameRateSettings | settingsData / dropdownView | 선택지 데이터 / 일시정지 메뉴의 드롭다운 |
| FrameRateDropdownView | dropdown | 선택지를 표시할 TMP 드롭다운 |

현재 선택지 (`Data/Settings/FrameRateSettingsData`):
| # | Label | Use VSync | Target Frame Rate |
|---|---|---|---|
| 0 | 모니터 주사율 | 켬 | -1 |
| 1 | 30 | 끔 | 30 |
| 2 | 60 | 끔 | 60 |
| 3 | 120 | 끔 | 120 |
| 4 | 144 | 끔 | 144 |
| 5 | 제한 없음 | 끔 | -1 |

## 에디터 설정
1. `Data/Settings`에서 `Create → ProjectFantasy → Settings → Frame Rate Settings Data` → 위 표대로 입력, Default Index 0
2. 일시정지 패널 Root 아래(DevPanel 밖)에 `UI → Dropdown - TextMeshPro` + `FrameRateDropdownView` → Dropdown 연결, 옆에 "프레임 제한" 라벨
3. **항상 켜진** 빈 오브젝트 `FrameSettings` + `FrameRateSettings` → Settings Data·Dropdown View 연결
   - 꺼진 일시정지 패널 아래에 두면 메뉴를 처음 열 때까지 `Awake`가 돌지 않아 저장값이 시작 시 적용되지 않는다.
4. 프로젝트 기본값: `Quality → PC → VSync Count = Every V Blank` (설정 스크립트가 돌기 전에도 주사율에 맞춤)

## 주의사항 / 확장 포인트
- 에디터 Game 창은 VSync가 기본으로 꺼져 있다. 해상도 드롭다운의 `VSync (Game view only)`를 켜야 "모니터 주사율"을 확인할 수 있다. 정확한 확인은 빌드에서 한다.
- 선택지를 줄이면 저장된 인덱스가 범위를 벗어날 수 있어 `Awake`에서 기본값으로 되돌린다.
- 새 설정(음량·감도 등)은 같은 구조(데이터 SO + 적용·저장 클래스 + View)로 `Settings/`·`UI/Settings/`에 추가한다.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-11-23 | 최초 작성: 프레임 제한 설정(모니터 주사율 기본, 30·60·120·144·제한 없음), 일시정지 메뉴 드롭다운, PlayerPrefs 저장 |
