# 인스펙터 설정 가이드

에셋·컴포넌트별로 **필드가 하는 일**과 **넣어야 할 값**을 정리한다. 인스펙터에서 필드 이름에 마우스를 올리면 같은 설명이 툴팁으로 나온다.
값 단위: 거리 m, 속도 m/s, 시간 초, 각도 도. "기본값"은 새로 만들었을 때 들어가는 값이다.

## 목차
- [빠른 체크리스트](#빠른-체크리스트)
- 데이터(SO): [무기](#무기-weapondata) · [방패·마법서](#방패마법서) · [화살](#화살-ammodata) · [마법](#마법-spelldata) · [소모품](#소모품-consumabledata) · [등급표](#등급표-itemgradetable) · [티어표](#티어표-itemtiertable) · [숙련도](#숙련도-masterydata) · [인벤토리](#인벤토리-inventorydata) · [액션·콤보](#액션콤보) · [이동](#이동-playermovementdata) · [애니메이션](#애니메이션-playeranimationdata) · [카메라](#카메라-cameradata) · [피격 반응](#피격-반응-hitreactiondata)
- [플레이어(Rogue) 컴포넌트](#플레이어rogue-컴포넌트)
- [씬 오브젝트](#씬-오브젝트)

---

## 빠른 체크리스트

### 새 근접 무기 (검·도끼)
1. `Create → ProjectFantasy → Weapon → Weapon Data`
2. **기본 정보**: 이름, 설명 (아이콘은 선택)
3. **장착**: Weapon Type(OneHanded/TwoHanded), 양손이면 Occupies Off Hand ✔, Model Prefab
4. **능력치**: Grade Table 지정, Attack Power Range 입력 (예: 한손 10~15, 양손 16~24)
5. **근접**: Combo Data(같은 종류 무기 공유), Blade Base/Tip — 타임라인 창 Scene 미리보기의 노란 선이 칼날과 겹치게
6. **애니메이션**: Idle State Name
7. 인스펙터 상단 요약 줄과 경고가 없는지 확인 → 시작 아이템 또는 필드 `WorldItem`으로 배치

### 새 활·석궁
- Weapon Type: Bow(Grip Hand **Left**) / Crossbow. 원거리 섹션: Ammo, Can Aim(한손 석궁 ✖), Requires Reload(석궁 ✔), Launch Speed, 모션 이름·시간

### 새 Wand·Staff
- `Create → ProjectFantasy → Magic → Magic Weapon Data`, Staff는 Allows Shield ✖ (마법서만), Spell 지정, Magic Power Range 입력

---

## 무기 (WeaponData)
인스펙터는 **무기 종류에 맞는 섹션만** 보인다(활엔 근접 섹션 없음, Wand·Staff엔 공격력 대신 마법력). 맨 위 요약 줄: `종류 · 쥐는 손 · 보조 장비 · 능력치 범위 · 등급표`.

### 기본 정보
| 필드 | 하는 일 | 넣을 값 |
|---|---|---|
| Display Name | 게임 내 이름 | 비우면 에셋 이름 |
| Icon | 슬롯 아이콘 | 없으면 슬롯에 이름 표시 |
| Description | 상세 패널 설명 | 자유 |
| World Model Prefab | 필드에 놓였을 때 모델 | 비우면 Model Prefab 사용 |

### 장착
| 필드 | 하는 일 | 넣을 값 |
|---|---|---|
| Weapon Type | 공격 방식·섹션 결정 | Unarmed / OneHanded / TwoHanded / Wand / Staff / Bow / Crossbow |
| Grip Hand | 쥐는 손 | 활만 **Left**, 나머지 Right |
| Occupies Off Hand | 보조 손 사용 → 보조 장비 불가 | 양손검·양손 석궁 ✔ (활은 왼손이라 자동) |
| Allows Shield | 방패 허용 | Staff ✖, 나머지 ✔ |
| Model Prefab | 손에 쥐는 모델 | KayKit `Weapons/*` (맨손은 비움) |
| Grip Position / Rotation | 손 소켓 보정 | KayKit은 **0 유지** (어긋나면 먼저 클립 마스크 확인) |

**보조 장비 조합** (자동 판정): 한손검·한손 석궁 = 방패 / Wand = 방패·마법서 / Staff = 마법서 / 양손검·양손 석궁·활 = 불가

### 능력치
| 필드 | 하는 일 | 넣을 값 |
|---|---|---|
| Tier Table | 티어 접두어·배율·외형 | 공용 `ItemTierTable` (맨손은 비움) |
| Tier Materials | 티어별 모델 머티리얼 교체 (선택) | 보통 비움 (아우라·테두리로 구분), 순서 = T1, T2 … |
| Grade Table | 등급 추첨 표 | 공용 `ItemGradeTable` (맨손은 비움) |
| Attack Power Range | 공격력 범위 (검·활·석궁) | 아래 밸런스 표 |
| Magic Power Range | 마법력 범위 (Wand·Staff) | 아래 밸런스 표 |

최종값 = `범위에서 균등 랜덤 × 티어 배율 × 등급 배율` (반올림). 개체가 생길 때 1회만 굴린다. 범위는 **T1 기준**으로 입력한다.

#### 밸런스 기준 (일반 등급, 한손검 1타 평균 12 = 기준)
| 무기 | 범위 | 1회 피해 | 주기 | DPS | 역할 |
|---|---|---|---|---|---|
| 맨손 | 5~5 (등급 없음) | 5 | 콤보 1.7초 | ≈6 | 최소 대응 |
| 한손검 | 10~14 | 12 / 막타 18 | 콤보 2.3초 | ≈18 | 기준, 빠름, 방패 |
| 양손검 | 18~24 | 21 / 막타 31 | 콤보 4.7초 | ≈16 | 큰 한 방, 회전 베기 다수 타격 |
| 활 | 10~14 | 12 | 비조준 1.6초 / 조준 0.6초 | ≈8 / ≈20 | 거리 유지, 화살 소모 |
| 한손 석궁 | 14~18 | 16 | 발사+장전 1.65초 | ≈10 | 방패와 함께 원거리 |
| 양손 석궁 | 26~34 | 30 | 발사+장전 2.6초 | ≈12 | 한 발 저격 |
| Wand | 마법력 10~14 | 12 | 쿨타임 1초 | ≈12 (마법서 ≈14) | 직선 단일 마법 |
| Staff | 마법력 14~18 | 24 (AreaSpell 배율 1.5) | 쿨타임 3초 | ≈8 × 대상 수 | 범위 공격 |

- 범위 폭은 평균 ±15% → 같은 등급 안의 편차는 있지만 등급을 뒤집지 않는다.
- 등급 배율 상한 1.5 → 전설 한손검(15~21)이 일반 양손검(18~24)을 넘지 않아 무기 종류별 역할 유지.
- 모션 길이(ActionData)·쿨타임을 바꾸면 DPS가 달라지므로 이 표를 함께 갱신.

### 근접 (맨손·한손·양손)
| 필드 | 하는 일 | 넣을 값 |
|---|---|---|
| Combo Data | 콤보 (1타·2타…) | 무기 종류별 공유 에셋 (`1H/2H/UnarmedAttackComboData`) |
| Hit Offset / Hit Radius | 칼날이 없을 때 몸 기준 판정 구체 | 맨손 (0, 1, 1) / 0.8 |
| Blade Base / Blade Tip | 칼날 시작·끝 (모델 로컬) | Sword1H (0,0.2,0)~(0,1,0), Sword2H (0,0.65,0)~(0,1.88,0) |
| Blade Radius | 칼날 판정 두께 | 0.05~0.15 (기본 0.1) |

### 원거리 (활·석궁)
| 필드 | 하는 일 | 넣을 값 |
|---|---|---|
| Ammo | 소모 화살 | 활 `ArrowBow`, 석궁 `ArrowCrossBow` |
| Can Aim | 우클릭 조준 모드 | 한손 석궁 ✖ (우클릭 = 방패 가드) |
| Requires Reload | 발사 후 장전 | 석궁 ✔ |
| Muzzle Offset | 발사 지점 (캐릭터 로컬) | 조준 모드 경로선 시작점으로 확인 |
| Launch Speed | 발사 속도 | 활 40, 석궁 50 |
| Windup State / Duration | 비조준 발사 전 당기기 | 활 `Ranged_Bow_Draw` 0.5, 석궁은 비움 |
| Fire State / Release Time / Fire Duration | 발사 모션, 화살이 나가는 시점, 전체 시간 | 활 `Ranged_Bow_Release` 0.05 / 0.5 |
| Aim Idle State / Aim Hold Time | 조준 대기 자세, 자세 고정 시점(정규화) | 마지막 프레임이 조준 자세면 1, 올렸다 내리는 클립은 0.4~0.6 |
| Reload State / Duration | 장전 모션 (Requires Reload일 때만 보임) | 석궁 장전 클립 길이 |
| Loaded Ammo Position / Rotation | 장전된 볼트 위치 (석궁 모델 로컬) | 플레이 중 볼트가 홈에 맞게 |

### 마법 (Wand·Staff)
| 필드 | 하는 일 | 넣을 값 |
|---|---|---|
| Spell | 좌클릭 기본 마법 | Wand = ProjectileSpell, Staff = AreaSpell |
| Muzzle Offset | 마법탄 발사 지점 | 기본 (0.3, 1.4, 0.6) |

### 애니메이션
| 필드 | 하는 일 | 넣을 값 |
|---|---|---|
| Idle State Name | 이 무기를 든 대기 모션 | 맨손 `Melee_Unarmed_Idle`, 한손 `Idle_A`, 양손 `Melee_2H_Idle`, 활 `Ranged_Bow_Idle` |

### 피해 공식
| 공격 | 피해 |
|---|---|
| 근접 | 공격력 × HitWindowEvent 배율 |
| 화살 | 공격력 |
| 마법 | 마법력 × Spell 배율 × (활성 마법서 배율) |
| 받는 쪽 방패 | `피해 × 100 / (100 + 방어력)` (마법은 마법 방어력), 최소 1 |

---

## 방패·마법서
| 에셋 | 필드 | 하는 일 | 넣을 값 |
|---|---|---|---|
| Shield | Guard Angle | 가드로 막는 정면 각도 | 120 |
| Shield | Block Knockback Speed / Block Stun Duration | 막았을 때 밀림·경직 | 2 / 0.3 |
| Shield | Defense Range / Magic Defense Range | 방어력 (장착 활성 중 항상 감소, 100 = 피해 절반) | 아래 표 |
| Spellbook | Magic Power Multiplier | 마법 피해 배율 (고정) | 1.2 |

| 방패 | 방어력 | 마법 방어력 | 평균 감소 (물리 / 마법) |
|---|---|---|---|
| 철 방패 (`Shield`) | 20~30 | 3~6 | 20% / 4% |
| 마법 방패 (`MagicShield`) | 8~12 | 20~30 | 9% / 20% |

공통: Model Prefab, Grade Table(방패), Display Name 등은 무기와 같다.

## 화살 (AmmoData)
| 필드 | 하는 일 | 넣을 값 |
|---|---|---|
| Max Stack | 한 칸 최대 수 | 99 |
| Model Prefab | 시위에 건 화살·장전 볼트·필드 모델 | `arrow_bow` / `arrow_crossbow` |
| Projectile Prefab | 날아가는 화살 | `Prefabs/Combat` 화살 |
| Gravity | 낙하 (클수록 빨리 떨어짐) | 9.81 |
| Lifetime | 최대 비행 시간 | 5 |
| Stick Duration | 박힌 뒤 사라지는 시간 | 10 |
| Impact Effect Prefab | 명중 이펙트 | Looping 끈 Prefab Variant |

## 마법 (SpellData)
| 에셋 | 필드 | 하는 일 | 넣을 값 |
|---|---|---|---|
| 공통 | Cast State / Release Time / Cast Duration | 시전 모션, 발동 시점, 전체 시간 | `Ranged_Magic_Shoot` 0.3 / 0.8 |
| 공통 | Cooldown | 재사용 대기 | Wand(ProjectileSpell) 1, Staff(AreaSpell) 3 |
| 공통 | Damage Multiplier | 마법력 배율 | Wand 1, Staff 1.5 |
| Projectile | Projectile Prefab / Speed / Lifetime / Impact Effect | 직선 마법탄 | 30 / 3 |
| Area | Max Range | 마법진 사거리 | 15 |
| Area | Targeting Idle State / Hold Time | 조준 자세, 고정 시점 | `Ranged_Magic_Raise` 0.4~0.6 |
| Area | Indicator Prefab / Scale | 바닥 마법진 | 반투명 이펙트 Variant |
| Area | Radius / Activation Delay / Area Effect | 범위, 지연, 발동 이펙트 | 3 / 0.6 |

## 소모품 (ConsumableData)
| 필드 | 하는 일 | 넣을 값 |
|---|---|---|
| Max Stack | 한 칸 최대 수 | 20 |
| Use State Name | 사용 모션 | `Use_Item` |
| Effect Time | 효과 적용 시점 (그 전에 맞으면 소모 안 됨) | 0.6 |
| Duration | 사용 동작 전체 시간 | 1.2 (Effect Time 이상) |
| Move Speed | 사용 중 이동 속도 | 1 |
| Effects | 효과 목록 (드롭다운에서 종류 선택) | `HealEffect` Amount 30 |

## 등급표 (ItemGradeTable)
| 필드 | 하는 일 | 기본값 |
|---|---|---|
| Display Name / Color | 등급 이름·이름 색 | 일반 흰 / 희귀 파랑 / 영웅 보라 / 전설 주황 |
| Weight | 뽑힐 가중치 (확률 = 가중치 / 합) | 70 / 22 / 7 / 1 |
| Stat Multiplier | 능력치 배율 | 1.0 / 1.15 / 1.3 / 1.5 |

상자·보스 전용으로 다른 가중치 표를 만들어 무기에 지정할 수 있다.

## 티어표 (ItemTierTable)
| 필드 | 하는 일 | 넣을 값 |
|---|---|---|
| Tiers | 티어 목록 (첫 줄 = T1) | 아래 표 |
| Aura Prefab / Outline Material | 공용 불씨 파티클 / 테두리 발광 머티리얼 | `Tools → ProjectFantasy → Create Weapon Tier Effects`가 생성·연결 |

티어 한 줄의 필드:

| 필드 | 하는 일 |
|---|---|
| Prefix / Stat Multiplier | 이름 접두어 / T1 범위에 곱하는 배율 |
| Use Aura / Aura Color / Aura Rate | 불씨 파티클 사용 / HDR 색(강도 높을수록 Bloom 번짐) / 초당 입자 수 |
| Use Outline / Outline Color / Outline Width | 테두리 발광 사용 / HDR 색 / 두께 m (0.004~0.012, 두꺼우면 모서리 끊김) |
| Apply Tint 이하 | 원본 텍스처에 색조 곱하기 (손잡이까지 물들어 기본 끔) |

| 티어 | 접두어 | 배율 | Aura Color / Rate | Outline Color / Width |
|---|---|---|---|---|
| T1 | 낡은 | 1.0 | 없음 | 없음 |
| T2 | 철 | 1.3 | (2, 2, 2, 0.9) / 10 | (1.5, 1.5, 1.5) / 0.004 |
| T3 | 강철 | 1.6 | (1, 1.8, 3, 0.95) / 15 | (0.8, 1.5, 2.5) / 0.006 |
| T4 | 미스릴 | 2.0 | (2, 1, 4, 1) / 25 | (1.6, 0.8, 3.2) / 0.008 |
| T5 | 용의 | 2.5 | (6, 2.4, 0.5, 1) / 40 | (5, 2, 0.4) / 0.01 |

- 빛 번짐이 안 보이면: 카메라 **HDR 켜짐**, URP Volume에 Bloom, Threshold가 색 강도보다 낮은지 확인.
- 배율 2.5에서도 T5 한손검(25~35)이 T4 양손검(36~48)보다 낮도록 무기 범위를 유지 → 상위 티어가 항상 역할을 뒤집지는 않음.

## 숙련도 (MasteryData)
| 필드 | 하는 일 | 넣을 값 |
|---|---|---|
| Max Level | 최대 레벨 | 50 |
| Experience Base / Exponent | 다음 레벨 필요 경험치 = 기본값 × 레벨^지수 | 100 / 1.5 (Lv 1→2: 100, Lv 49→50: 34,300) |
| Tier Unlock Levels | 티어별 해금 레벨 (개수 = 최대 티어) | 1, 10, 20, 30, 40 |
| Bonus Per Level | 레벨당 보너스 (무기·마법서 피해, 방패 방어력) | 0.01 (1%) |
| Display Names | 창·알림 이름 (`MasteryType` 순서) | 맨손, 한손검, 양손검, 활, 한손 석궁, 양손 석궁, Wand, Staff, 방패, 마법서 |

처치 경험치는 대상의 `KillReward.experience`(기본 30)로 정한다.

## 인벤토리 (InventoryData)
| 필드 | 하는 일 | 넣을 값 |
|---|---|---|
| Capacity | 가방 칸 수 | 30 (시작 시 1회만 읽음) |
| Quick Slot Count | 퀵슬롯 수 | 8 (키보드 1~8 바인딩 수와 맞출 것) |
| Starting Items | 시작 아이템·수량·티어 | 장비는 수량만큼 개체 생성(최하 등급), 티어는 숙련 Lv 1로 낄 수 있는 1 권장, **맨손은 넣지 않음** |
| Starting Quick Slots | 퀵슬롯 순서 | 무기·소모품만 (같은 무기는 아직 등록 안 된 개체 순) |

## 액션·콤보
| 에셋 | 필드 | 하는 일 | 넣을 값 |
|---|---|---|---|
| AttackComboData | Actions | 콤보 순서 | ActionData 1타, 2타 … |
| ActionData | Clip / State Name | 기준 클립 / 애니메이터 상태 (비우면 클립 이름) | 공격 클립 |
| ActionData | Length Frames | 액션 종료 프레임 | 휘두른 뒤 복귀까지 (클립보다 짧게 가능) |
| ActionData | Playback Speed | 모션·판정 공통 배율 | 1 |
| ActionData | Cross Fade Duration | 전환 블렌드 | 0.1 |
| HitWindowEvent | 구간 / Damage Multiplier / Hit Stop | 타격 프레임, 배율, 멈춤 | 타임라인 창의 빨간 면이 대상 위치를 지나는 구간 / 1~1.5 / 0.06 |
| ComboWindowEvent | 구간 / Transition Frame | 입력 예약 구간, 다음 타로 넘어가는 프레임 | 타격 구간 이후 |
| MoveEvent | 구간 / Speed | 앞으로 내딛기 | 제자리 클립은 0~2 |

편집은 **ActionData 인스펙터 → 타임라인 열기**에서 드래그로 한다 ([Combat](Features/Combat.md)).

## 이동 (PlayerMovementData)
| 필드 | 하는 일 | 기본값 |
|---|---|---|
| Walk / Run Speed | 걷기 / 달리기 버튼 속도 | 2 / 5 |
| Guard / Aim Move Speed | 가드·조준 중 이동 속도 | 1.2 / 1.5 |
| Acceleration / Deceleration | 지상 가속·감속 | 30 / 40 |
| Air Acceleration | 공중 제어 | 8 |
| Rotation Speed | 회전 속도 (도/초) | 720 |
| Jump Height / Gravity | 점프 높이·중력 | 1.2 / 25 |
| Grounded Stick Force / Max Fall Speed | 경사 밀착·최대 낙하 | 2 / 50 |
| Coyote Time | 지면 이탈 후 점프 허용 | 0.15 |

Walk Speed는 애니메이션의 **Run Speed Threshold(3.5)보다 작게**, Run Speed는 크게 둘 것.

## 애니메이션 (PlayerAnimationData)
| 필드 | 하는 일 | 기본값 |
|---|---|---|
| Pose Speed / Action Speed Parameter | 조준 자세 고정 / 공격 재생 속도용 Float 파라미터 | `PoseSpeed` / `ActionSpeed` |
| 상태 이름 13개 | 대기·걷기·달리기·공중·피격·사망·가드·막기·줍기·조준 이동 | 애니메이터 상태 이름과 **정확히 일치** (누락 시 시작할 때 에러) |
| Idle / Run Speed Threshold | 대기 → 걷기 → 달리기 분기 속도 | 0.1 / 3.5 |
| Locomotion / Action Cross Fade | 이동 / 동작 전환 시간 | 0.15 / 0.1 |

## 카메라 (CameraData)
현재 프로젝트 값: `CameraData`(기본) / `AimCameraData`(조준 숄더뷰)

| 필드 | 하는 일 | 기본 / 조준 |
|---|---|---|
| Mouse / Gamepad Sensitivity | 감도 | 0.1 / 0.06 (조준은 낮게), 패드 180 |
| Default / Min / Max Pitch | 시작·하한·상한 상하 각도 | 15 / -30 / 70 |
| Distance / Target Height | 거리·바라보는 높이 | 7 / 3 · 2.2 / 2 |
| Shoulder Offset | 오른쪽 어깨 오프셋 | 0 · 1 |
| Field Of View | 시야각 | 60 · 50 |
| Follow Smooth Time | 추적 지연 | 0.05 |
| Min / Max Zoom Distance | 휠 줌 범위 (기본 프로필만, 조준 중엔 고정) | 3 / 12 |
| Zoom Step / Zoom Speed | 휠 한 칸당 거리 / 따라가는 속도 | 1 / 15 |
| Collision Layers / Radius / Min Distance / Recover Speed | 벽 충돌 | Player·Projectile 제외 / 0.25 / 0.5 / 8 |
| Lock Cursor | 플레이 중 커서 숨김 | ✔ |

## 피격 반응 (HitReactionData)
| 필드 | 하는 일 | 기본값 |
|---|---|---|
| Stun Duration | 피격 경직 | 0.4 |
| Knockback Speed / Deceleration | 넉백 속도·감속 | 4 / 12 |

---

## 플레이어(Rogue) 컴포넌트
레이어는 **Player**. 대부분은 비워 두면 자동 탐색되거나 기본값 그대로 쓴다. **굵게** 표시한 항목만 꼭 연결한다.

| 컴포넌트 | 필드 | 하는 일 | 넣을 값 |
|---|---|---|---|
| PlayerController | Camera Transform | 이동·조준 기준 | 비우면 Main Camera |
| PlayerController | **Hit Reaction Data** | 피격 경직·넉백 | `HitReactionData` |
| PlayerController | Min Aim Facing Distance | 조준점이 이보다 가까우면 카메라 정면을 봄 | 1.5 |
| PlayerController | Trajectory Preview | 조준 경로선 | 씬의 LineRenderer (선택) |
| PlayerInputHandler | **Action Asset** | 입력 액션 (이름으로 탐색) | `InputSystem_Actions` |
| PlayerInputHandler | Input Buffer Time | 선입력 보관 시간 | 0.2 |
| PlayerMotor | **Movement Data** | 이동·점프·중력 | `PlayerMovementData` |
| PlayerAnimator | Animator / **Data** | 애니메이터 / 상태 이름·전환 | 자동 / `PlayerAnimationData` |
| PlayerLoadout | **Unarmed Weapon** | 맨손 데이터 | `Unarmed` |
| PlayerLoadout | Starting Weapon / Off Hand | 시작 장착 (가방의 첫 개체) | 예: Sword1H / Shield |
| Inventory | **Data** | 가방·퀵슬롯·시작 아이템 | `InventoryData` |
| PlayerInteractor | Radius / **Interactable Layers** / Scan Interval | 줍기 탐색 | 1.5 / **Interactable** / 0.1 |
| PlayerItemHandler | **World Item Prefab** | 버린 아이템 프리팹 | `Prefabs/WorldItem` |
| PlayerItemHandler | Drop Distance / Height | 버리는 위치 | 1 / 0.3 |
| PlayerItemHandler | Pick Up Grab Time / Duration | 가방에 들어가는 시점 / 줍기 전체 시간 | 0.4 / 0.9 |
| Health | Max Health / Invincible Duration | 체력·피격 후 무적 | 100 / 0.5 |
| MeleeAttacker | Hit Origin / **Target Layers** | 판정 기준 / 타격 대상 | 자신 / Player 제외 |
| RangedAttacker | Max Aim Distance | 허공 조준 시 거리 | 100 |
| RangedAttacker | **Aim Layers** / **Projectile Hit Layers** | 조준 레이 / 화살 충돌 | 둘 다 Player·Projectile 제외 |
| RangedAttacker | Preview Time Step | 경로선 정밀도 | 0.03 |
| RangedAttacker | Projectile Root / Effect Root | 화살·명중 이펙트 부모 | 비움 (`Pools/Projectiles`, `Pools/Effects` 자동) |
| SpellCaster | **Area Damage Layers** / **Ground Layers** | 범위 피해 대상 / 마법진을 붙일 지면 | Player 제외 / Default·Water |
| SpellCaster | Max Targeting Ray Distance / Ground Probe Height | 마법진 조준 레이 / 사거리 밖 보정 | 100 / 10 |
| SpellCaster | Effect Root | 마법진·마법 이펙트 부모 | 비움 (`Pools/Effects` 자동) |
| WeaponMastery | **Data** | 숙련 규칙 | `MasteryData` |
| WeaponMastery | Is Test Mode / Test Experience Multiplier / Start At Max Level | 테스트용 (에디터·개발 빌드만) | 끔 / 200 / 끔 |
| ShieldGuard | Facing Transform | 가드 정면 | 자신 |
| HitStop | Animator | 멈출 애니메이터 | 자동 |
| EquipmentVisual | Right / Left Hand Slot | 손 소켓 | 자동 (`handslot.r/l`) |
| PlayerAmmoVisual · PlayerRangedWeapon · PlayerMagicCaster · PlayerMasteryRewarder · QuickSlots | — | 설정 없음 | — |

## 씬 오브젝트
| 오브젝트 | 컴포넌트 | 필드 | 넣을 값 |
|---|---|---|---|
| Main Camera | ThirdPersonCamera | **Target** / **Camera Data** / Aim Camera Data / **Look Action** / **Zoom Action** | Rogue / `CameraData` / `AimCameraData` / Player/Look / Player/Zoom |
| Main Camera | ThirdPersonCamera | Aim Blend Speed | 6 |
| Canvas / UI Presenters | PlayerAimPresenter · PlayerMenuPresenter · PlayerHudPresenter | 플레이어·카메라·UI 참조 (Menu는 Inventory Window·**Mastery Window**) | `Create Inventory UI` → `Create Mastery UI`가 자동 연결 |
| UI Presenters | PlayerHudPresenter | Experience / Level Up / Tier Unlock Format | 숙련 안내 문구, 기본값 (`{0}` 이름, `{1}` 값, `{2}` 티어) |
| 데미지 숫자 스포너 | DamageNumberSpawner | Pool Root | 비움 (`Pools/DamageNumbers` 자동) |
| 필드 아이템 | WorldItem | Item / Count / Tier / Model Root | 아이템 / 수량 (장비는 1) / 티어 / 비움 |
| 허수아비 | Health · TrainingDummy · DamageNumberEmitter · KillReward | Invincible Duration / Virtual Health / Experience | 0 / 100 / 30 |
