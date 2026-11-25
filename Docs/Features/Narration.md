# Narration

## 개요
화자 없이 화면 상단에 문구를 한 줄씩 띄우는 자막형 내레이션. 스테이지가 시작될 때 인트로 문구로 어떤 적을 어떤 무기로 상대할지 알려 준다. 시간을 멈추지 않고 입력도 막지 않는다. 문구는 게임 시간 기준으로 넘어가므로 ESC 일시정지 중에는 함께 멈춘다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Narration/Data/NarrationData.cs](../../Assets/Project/Scripts/Narration/Data/NarrationData.cs) | 내레이션 문구 묶음 SO (`Lines`, 순서대로 표시) |
| [Narration/NarrationPlayer.cs](../../Assets/Project/Scripts/Narration/NarrationPlayer.cs) | `Play(데이터)`: 줄마다 `lineDuration`초 표시 후 숨김, 재생 중 새 요청이 오면 이전 재생을 끊고 새로 시작 |
| [UI/HUD/NarrationView.cs](../../Assets/Project/Scripts/UI/HUD/NarrationView.cs) | 자막 패널 표시(`Show(문구)`)·숨김(`Hide`), 시작 시 숨김 |

연동: [Stage](Stage.md) `StageDirector.GoToStage`가 스테이지의 `intro`를 재생한다.

## 동작 흐름
```
StageDirector.GoToStage(N) → intro가 있으면 NarrationPlayer.Play(intro)
  → 재생 중인 코루틴이 있으면 정지
  → 줄마다: NarrationView.Show(줄) → lineDuration 대기 (WaitForSeconds 재사용)
  → 끝나면 NarrationView.Hide()
```
- `WaitForSeconds`는 게임 시간 기준이라 `Time.timeScale` 0(일시정지)이면 멈췄다가 재개하면 이어진다. 배속도 그대로 적용된다.
- 재시작(`Retry`)은 `GoToStage`를 거치지 않아 인트로가 다시 나오지 않는다.

## 데이터 파라미터
| 대상 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| NarrationData | lines | — | 한 줄씩 표시할 문구 (순서대로) |
| NarrationPlayer | narrationView | — | 자막 UI |
| NarrationPlayer | lineDuration | 3 | 한 줄 표시 시간 (초) |
| NarrationView | root / label | — | 자막 패널 루트 / 문구 TMP |

현재 인트로 (`Data/Narration`):
| 에셋 | 스테이지 |
|---|---|
| Intro_Minion | 1 Minion (한손검, 우클릭 방패 막기 안내) |
| Intro_Rogue | 2 Rogue (활) |
| Intro_Warrior | 3 Warrior (양손검) |
| Intro_Mage | 4 Mage (지팡이, 마법책 강화 안내) |
| Intro_Boss | 5 Boss (전부) |

## 에디터 설정
1. `Data/Narration`에서 `Create → ProjectFantasy → Narration → Narration Data` → Lines 입력
2. Canvas 아래 `NarrationWindow`(+ `NarrationView`, 항상 켜 둠) → 자식 `Root`(반투명 Image, 상단 중앙) → 자식 `Label`(TMP, `MaruBuri-Regular SDF`, 가운데 정렬)
3. `NarrationView`는 Root가 아닌 부모에 붙인다 (Awake에서 Root를 끄므로)
4. 일시정지 패널보다 위쪽 계층(먼저 그려짐)에 두어 일시정지 메뉴가 자막을 덮게 한다
5. 빈 오브젝트 `NarrationPlayer` + `NarrationPlayer` → Narration View 연결
6. `StageDirector` → Narration Player 연결, Stages마다 Intro 연결

## 주의사항 / 확장 포인트
- 보스 처치·엔딩 문구도 `NarrationData`를 만들어 `Play`하면 된다.
- 문구 길이에 따라 표시 시간을 바꾸려면 `NarrationPlayer`에서 줄 길이 기반으로 대기 시간을 계산한다 (지금은 고정).

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-11-19 | 최초 작성: 자막형 내레이션(데이터·재생기·자막 UI), 스테이지 인트로 5종 |
| 2026-11-23 | 인트로 문구 정리 (Warrior: 양손검으로 방패를 깰 수 있다는 안내) |
| 2026-11-25 | Minion 인트로에 방패 막기, Mage 인트로에 마법책 강화 안내 한 줄씩 |
