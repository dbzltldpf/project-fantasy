# Mastery

## 개요
알비온식 무기 숙련도. 직업 없이 **무기 종류별 숙련 레벨**이 장착 가능한 장비 티어와 보너스를 결정한다. 적을 처치하면 그 순간 장착한 무기와 활성 보조 장비가 **각각 경험치 전액**을 받는다. 숙련도 창은 K(실시간).

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Mastery/MasteryType.cs](../../Assets/Project/Scripts/Mastery/MasteryType.cs) | 숙련 종류 10개 (배열 인덱스 = 창 표시 순서) |
| [Mastery/MasteryMapping.cs](../../Assets/Project/Scripts/Mastery/MasteryMapping.cs) | 장비 → 숙련 종류 (`TryGet`, 숙련이 없는 아이템은 false) |
| [Mastery/Data/MasteryData.cs](../../Assets/Project/Scripts/Mastery/Data/MasteryData.cs) | 최대 레벨·경험치 곡선·티어 해금 레벨·레벨당 보너스·표시 이름 (SO) |
| [Mastery/WeaponMastery.cs](../../Assets/Project/Scripts/Mastery/WeaponMastery.cs) | 종류별 레벨·경험치, `CanUse`(티어 장착 판정), `GetBonusMultiplier`, `ExperienceGained`·`LevelChanged`, 테스트 옵션 |
| [Combat/KillReward.cs](../../Assets/Project/Scripts/Combat/KillReward.cs) | 피해를 준 공격자를 기록했다가 처치 시 각자에게 경험치 지급 (적·허수아비 공용) |
| [Core/Interfaces/IKillRewardReceiver.cs](../../Assets/Project/Scripts/Core/Interfaces/IKillRewardReceiver.cs) | 처치 보상 수신 (Combat은 Mastery를 모름) |
| [Player/PlayerMasteryRewarder.cs](../../Assets/Project/Scripts/Player/PlayerMasteryRewarder.cs) | 보상 → 장착 무기(맨손 포함)·활성 보조 장비 숙련에 전액 |
| [InventorySystem/EquipRequirement.cs](../../Assets/Project/Scripts/InventorySystem/EquipRequirement.cs) | UI 표시용 장착 조건 (숙련 이름·필요 레벨·충족 여부) |
| [UI/Mastery/](../../Assets/Project/Scripts/UI/Mastery/) | `MasteryWindow`(변경된 줄만 갱신), `MasteryRowView`(Kenney 3조각 경험치 바) |
| [UI/Editor/MasteryUIBuilder.cs](../../Assets/Project/Scripts/UI/Editor/MasteryUIBuilder.cs) | `Tools → ProjectFantasy → Create Mastery UI` |

## 동작 흐름
### 숙련 종류
| 종류 | 대상 |
|---|---|
| 맨손 · 한손검 · 양손검 · 활 · Wand · Staff | 같은 `WeaponType` 무기 |
| 한손 석궁 / 양손 석궁 | Crossbow 중 `occupiesOffHand`로 구분 (별도 숙련) |
| 방패 / 마법서 | `ShieldData` / `SpellbookData` |

### 경험치 (알비온 방식)
```
공격 적중 → Health.Damaged → KillReward가 공격자(IKillRewardReceiver) 기록
처치(Died, 허수아비는 누적 피해 ≥ virtualHealth) → 기록된 공격자마다 experience 전액
 → PlayerMasteryRewarder: 처치 순간 장착 무기 숙련 + 활성 보조 장비 숙련 각각 전액
```
- 무기를 바꿔 가며 때려도 **처치 순간** 장비 기준. 비활성 보조 장비(무기와 맞지 않음)는 받지 않는다.
- 필요 경험치 = `experienceBase × 현재 레벨^experienceExponent` (기본 100 × Lv^1.5), 한 번에 여러 레벨 상승 가능, 최대 레벨이면 누적 안 함.

### 티어 장착 조건
- 해금 레벨: T0 = 1, T1 = 10, T2 = 20, T3 = 30, T4 = 40 (`tierUnlockLevels`).
- `개체 티어 ≤ 해당 숙련 해금 티어`일 때만 장착. 부족하면 거부 + "숙련도가 부족합니다".
- 숙련이 없는 아이템(화살·소모품)은 항상 가능. 시작 장착은 장착 가능한 첫 개체.
- 인벤토리 상세 패널에 `요구 숙련: 한손검 Lv 10 (T1)` 표시(부족하면 빨강, `ItemStatFormatter.requirementFormat`).

### 보너스
`배율 = 1 + (레벨 − 1) × bonusPerLevel` (기본 1%, Lv 50 = +49%)

| 숙련 | 적용 |
|---|---|
| 무기 | 공격력·마법력 (`PlayerLoadout.AttackPower` / `MagicPower`) |
| 마법서 | 마법서 배율에 곱함 (`PlayerMagicCaster`) |
| 방패 | 방어력·마법 방어력 (레벨업 즉시 갱신) |

### 숙련도 창 (K)
- 열리면 인벤토리 창과 같이 이동·공격·시점 입력 차단, 게임은 계속 진행.
- 줄: 이름 · `Lv` · 해금 티어 · 경험치 바(`현재/필요`, 최대면 MAX) · 보너스 `+%`.
- 획득·레벨업은 HUD 안내 문구: `{0} 숙련 +{1}` / `{0} 숙련 Lv {1}` / 티어 해금 시 `— T{2} 장착 가능` (`PlayerHudPresenter` 포맷 문자열).

## 데이터 파라미터
| 에셋/컴포넌트 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| MasteryData | maxLevel | 50 | 최대 레벨 |
| MasteryData | experienceBase / experienceExponent | 100 / 1.5 | 경험치 곡선 |
| MasteryData | tierUnlockLevels | 1, 10, 20, 30, 40 | 티어별 해금 레벨 (개수 = 최대 티어) |
| MasteryData | bonusPerLevel | 0.01 | 레벨당 보너스 |
| MasteryData | displayNames | 맨손 … 마법서 | 창·알림 이름 (`MasteryType` 순서, 개수 다르면 경고) |
| KillReward | experience | 30 | 처치 시 기여자 각각이 받는 경험치 |
| TrainingDummy | virtualHealth | 100 | 이만큼 누적 피해 시 처치 판정 후 초기화 |

### 테스트 옵션 (`WeaponMastery`, 에디터·개발 빌드에서만 동작)
| 필드 | 의미 |
|---|---|
| isTestMode / testExperienceMultiplier | 획득 경험치 × 배율 (기본 200) |
| startAtMaxLevel | 시작 시 모든 숙련 최대 레벨 |
| ⋮ 메뉴 "테스트: 모든 숙련도 최대" | 플레이 중 즉시 최대 레벨 |

릴리즈 빌드에서는 켜 둬도 무시된다(`IsTestBuild` 상수).

## 에디터 설정
1. `Create → ProjectFantasy → Mastery → Mastery Data` → Rogue에 `WeaponMastery`(Data 연결)·`PlayerMasteryRewarder` 추가.
2. 처치 대상(허수아비·적)에 `KillReward` 추가.
3. 입력 액션 `Mastery`(K) 확인 → `Tools → ProjectFantasy → Create Mastery UI` (Create Inventory UI 이후, `PlayerMenuPresenter.masteryWindow` 자동 연결).

## 주의사항 / 확장 포인트
- 진행 데이터는 저장되지 않는다 (저장·불러오기 기능에서 `levels`·`experiences` 직렬화 예정).
- 새 숙련 종류: `MasteryType` 끝에 추가 + `MasteryMapping` + `displayNames`.
- 적 종류별 경험치는 `KillReward.experience`로 조절.
- 예정: 상세 패널에 숙련 보너스 반영 수치 표시, 숙련 레벨별 무기 스킬 해금.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-10-07 | 최초 작성: 무기 종류별 숙련(석궁 한손/양손 분리), 처치 경험치(장착 무기·보조 장비 각각 전액), 티어 장착 조건, 레벨당 보너스, 숙련도 창(K), 테스트 옵션 |
| 2026-10-07 | 티어 0부터: 해금 T0 = Lv 1 … T4 = Lv 40 |
