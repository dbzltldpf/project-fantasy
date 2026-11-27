# Audio

## 개요
효과음과 BGM. 게임플레이 코드는 소리를 모르고 **"쐈다·맞았다·열렸다" 같은 이벤트만 알린다.** 재생은 Audio의 Emitter가 이벤트를 구독해 `SoundPlayer`에 요청한다 (`DamageNumberEmitter`와 같은 방식). 어떤 소리를 낼지는 데이터(`SoundData` 에셋)가 정하므로, 무기·마법·화살마다 다른 소리를 코드 수정 없이 붙인다.

효과음은 Kenney CC0 팩 3종(Impact Sounds, RPG Audio, Sci-Fi Sounds), BGM은 RandomMind의 CC0 칩튠 루프 1곡.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [Audio/SoundData.cs](../../Assets/Project/Scripts/Audio/SoundData.cs) | 소리 하나의 설정 SO: 클립 배열(무작위 선택), 볼륨, 피치 범위(재생마다 무작위), 2D/3D(`spatialBlend`) |
| [Audio/SoundPlayer.cs](../../Assets/Project/Scripts/Audio/SoundPlayer.cs) | 씬에 하나. AudioSource 풀(16개)을 돌려 쓰며 `Play(SoundData, 위치)`. 다 차면 가장 오래된 소리를 덮어씀, 데이터·클립이 비면 건너뜀 |
| [Audio/HealthSoundEmitter.cs](../../Assets/Project/Scripts/Audio/HealthSoundEmitter.cs) | 맞을 때: `Health.Damaged`(물리만, 양손검이면 무거운 소리)·`Blocked`·`Died`, `ShieldGuard.GuardBroken`. 가드가 깨진 프레임에는 피격음을 건너뜀 |
| [Audio/AttackSoundEmitter.cs](../../Assets/Project/Scripts/Audio/AttackSoundEmitter.cs) | 공격할 때: `MeleeAttacker.SoundCued`(휘두르기), `RangedAttacker.ShotFired`·`Impacted`(발사·명중), `SpellCaster.CastStarted`·`AreaActivated`(시전·폭발). 없는 컴포넌트는 건너뜀 |
| [Audio/PartBreakSoundEmitter.cs](../../Assets/Project/Scripts/Audio/PartBreakSoundEmitter.cs) | 보스 `BreakablePartManager.PartBroken` → 그 부위 히트박스 중심에서 파괴음 |
| [Audio/PlayerSoundEmitter.cs](../../Assets/Project/Scripts/Audio/PlayerSoundEmitter.cs) | 플레이어 행동: 활 당기기(`RangedDraw.Started`), 석궁 장전 완료(`LoadedChanged(true)`), 장비 교체(`Equipped`), 줍기(`PickedUp`) |
| [Audio/GateSoundEmitter.cs](../../Assets/Project/Scripts/Audio/GateSoundEmitter.cs) | `AreaGate.Opened`·`Closed` (2D, 진행 알림) |
| [Audio/WindowSoundEmitter.cs](../../Assets/Project/Scripts/Audio/WindowSoundEmitter.cs) | `InventoryWindow.OpenChanged` → 열기·닫기 (2D) |
| [Combat/Action/Events/SoundEvent.cs](../../Assets/Project/Scripts/Combat/Action/Events/SoundEvent.cs) | 액션 타임라인 소리 트랙: 지정 프레임에 `IActionContext.PlaySound` → `MeleeAttacker.CueSound` |
| [Combat/DamageInfoExtensions.cs](../../Assets/Project/Scripts/Combat/DamageInfoExtensions.cs) | `IsFromWeapon(WeaponType)`: 가해자 무기 종류 확인 (가드 파괴·무거운 타격음 공용) |

알리는 쪽(새 이벤트): `ShieldGuard.GuardBroken`, `MeleeAttacker.SoundCued`, `RangedAttacker.ShotFired(shot, profile)`·`Impacted`, `SpellCaster.CastStarted`·`AreaCast`·`AreaActivated`, `RangedDraw.Started`, `PlayerItemHandler.Equipped`·`PickedUp`, `AreaGate.Opened`·`Closed`.

## 동작 흐름
```
게임플레이 (소리를 모름)                     Audio                       
──────────────────────────────          ─────────────────────────
Health / ShieldGuard 이벤트      ──→   HealthSoundEmitter   ─┐
ActionData SoundEvent → MeleeAttacker.SoundCued ──┐           │
RangedAttacker / SpellCaster 이벤트 ──→ AttackSoundEmitter ←─┘ ├─→ SoundPlayer.Play(SoundData, 위치)
BreakablePartManager.PartBroken  ──→   PartBreakSoundEmitter │      → 풀의 다음 AudioSource를 위치로 옮겨 재생
RangedDraw / RangedWeapon / ItemHandler → PlayerSoundEmitter │      → MainMixer / SFX
AreaGate / InventoryWindow       ──→   Gate·WindowSoundEmitter┘
BGM: 씬 오브젝트 Music의 AudioSource (Loop, Play On Awake) → MainMixer / BGM
```

### 소리 데이터가 흐르는 길
- **화살**: `AmmoData.fireSound·impactSound` → `ToProfile()` → `ProjectileProfile.FireSound·ImpactSound` → `RangedAttacker` 이벤트. 활과 석궁이 쓰는 화살이 달라서 화살이 곧 무기 소리를 나눈다.
- **마법**: `SpellData.castSound·impactSound` (투사체·범위 공통). 시전음은 **시전 시작** 시점(`SpellCaster.NotifyCastStarted`)
  - 조준이 필요한 범위 마법(플레이어 Staff)은 `PlayerSpellTargetState.Enter`(우클릭 조준 시작), 그 외는 시전 모션 시작(`PlayerCastState`·`EnemyCastState.Enter`)
  - 그래서 `ProjectileSpellData.ToProfile`의 발사음은 비움(겹침 방지)
- **휘두르기**: `ActionData`의 `SoundEvent` (타격 구간 1~2프레임 앞). 적 근접 무기도 같은 1H 콤보 에셋을 써서 함께 적용된다.

### 겹침 처리
| 상황 | 결과 |
|---|---|
| 화살이 적에게 명중 | 꽂히는 소리(`Arrow_Impact`) + 피격음(`Hit_Skeleton`) 둘 다 |
| 마법이 적에게 명중 | 명중음만 (`HealthSoundEmitter`는 마법 피해를 건너뜀) |
| 양손검으로 가드를 깸 | 가드 파괴음만. `GuardBroken`(방어 판정)이 `Damaged`보다 먼저 오므로 같은 프레임 번호면 피격음 생략 |
| 부위 파괴 | 파괴음(0.6) + 피격음(0.3), 파괴음이 커서 묻히지 않음 |

## 데이터 파라미터
| 대상 | 필드 | 기본값 | 의미 |
|---|---|---|---|
| SoundData | clips | — | 여러 개면 무작위 (반복감 감소) |
| SoundData | volume | 1 | 볼륨 (선형, 체감은 절반으로 줄여도 조금만 작아짐) |
| SoundData | minPitch / maxPitch | 0.95 / 1.05 | 재생마다 무작위. 신호음은 같은 값으로 고정 |
| SoundData | spatialBlend | 1 | 0 = 2D(UI·게이트), 1 = 3D(전투) |
| SoundPlayer | poolSize | 16 | 동시 소리 수 |
| SoundPlayer | output | MainMixer/SFX | 출력 믹서 그룹 |
| SoundPlayer | minDistance / maxDistance | 2 / 30 | Linear 감쇠: 이 거리까지 최대 / 이 거리에서 0 |
| 각 Emitter | soundPlayer | 자동 탐색 | 비우면 씬에서 1회 `FindFirstObjectByType` (적 프리팹용) |

### MainMixer
| 그룹 | 볼륨 | 용도 |
|---|---|---|
| Master | 0 dB | |
| SFX | 0 dB | 효과음 전체 (전체가 크면 여기서 -6~-8 dB) |
| BGM | -13 dB | BGM이 효과음 뒤에 깔리게 |

### 소리 목록 (`Data/Audio`)
| SoundData | 클립 | 볼륨 | 피치 | 연결 |
|---|---|---|---|---|
| Swing_Light | drawKnife ×3 | 0.3 | 1.15~1.25 | 1H·맨손 액션 SoundEvent |
| Swing_Heavy | drawKnife ×3 | 0.4 | 0.7~0.8 | 2H 액션 SoundEvent |
| Hit_Skeleton | impactPunch_heavy ×5 | 0.3 | 1~1.1 | 적 hitSound |
| Hit_Heavy | impactPunch_heavy ×5 | 0.2 | 0.85~0.95 | 적 heavyHitSound (양손검) |
| Hit_Player | impactSoft_medium ×5 | 0.4 | 0.9~1 | 플레이어 hitSound |
| Block | impactPlate_medium ×5 | 0.2 | 0.95~1.05 | 플레이어·적 blockSound |
| GuardBreak | impactGlass_heavy_001 | 0.45 | 0.7~0.8 | 플레이어·적 guardBreakSound |
| Death_Skeleton | impactWood_heavy, impactPlank_medium | 0.5 | 0.95~1.05 | 적 deathSound |
| PartBreak | impactPlate_heavy ×5 | 0.6 | 0.95~1.05 | 보스 breakSound (가장 큼) |
| Fire_Bow | drawKnife1 | 0.35 | 1.2~1.3 | ArrowBow fireSound |
| Fire_CrossBow | metalLatch | 0.35 | 0.95~1.05 | ArrowCrossBow fireSound |
| Arrow_Impact | impactWood_light ×5 | 0.4 | 0.95~1.05 | 두 화살 impactSound |
| Cast_Wand | laserSmall ×5 | 0.07 | 0.6~0.7 | 완드 마법 castSound (플레이어·적) |
| Impact_Wand | impactGlass_light ×5 | 0.35 | 1.2~1.3 | 완드 마법 impactSound |
| Cast_Staff | forceField ×5 | 0.06 | 0.7~0.8 | 범위 마법 castSound |
| Impact_Staff | impactBell_heavy ×5 | 0.3 | 0.6~0.7 | 범위 마법 impactSound |
| Draw_Bow | handleSmallLeather ×2 | 0.3 | 0.8~0.9 | 플레이어 drawSound |
| Reload_CrossBow | metalClick | 0.3 | 0.95~1.05 | 플레이어 reloadSound (장전 완료) |
| Equip / PickUp | beltHandle ×2 / cloth ×4 | 0.3 | 0.95~1.05 | 플레이어 equip·pickUpSound |
| Gate_Open / Gate_Close | forceField_003 | 0.1 / 0.08 | 1.1 / 0.8 고정 | 게이트 (2D) |
| UI_Open / UI_Close | bookOpen / bookClose | 0.3 / 0.15 | — | 인벤토리 창 (2D) |

## 에디터 설정
1. `Assets/Project/Audio/MainMixer`: Master 아래 SFX·BGM 그룹, BGM -13 dB
2. 씬 빈 오브젝트 `SoundPlayer` + `SoundPlayer` → Output = SFX
3. 씬 빈 오브젝트 `Music` + AudioSource: Clip `CHIPTUNE_Loop_The_Bards_Tale`, Output = BGM, Play On Awake·Loop ✔, Spatial Blend 0, Priority 0
4. BGM 임포트: Load Type Streaming, Vorbis Quality 70, Preload Audio Data 끔 (효과음은 기본값)
5. `Create → ProjectFantasy → Sound → Sound Data` → `Data/Audio`에 소리별 에셋
6. Emitter 배치
   | 대상 | 컴포넌트 |
   |---|---|
   | 플레이어 | HealthSoundEmitter, AttackSoundEmitter, PlayerSoundEmitter |
   | 적 프리팹 5종 | HealthSoundEmitter, AttackSoundEmitter (+ Warrior_Boss: PartBreakSoundEmitter) |
   | GateField | GateSoundEmitter |
   | 인벤토리 창 | WindowSoundEmitter |
7. 데이터 연결: AmmoData(fire·impact), SpellData(cast·impact, 플레이어·적 마법 4종), 공격 ActionData에 SoundEvent (Action Timeline 창, 노란색)

## 주의사항 / 확장 포인트
- **볼륨 기준**: 플레이어 위치에서 나는 소리(시전·발사·휘두르기)는 카메라 바로 앞이라 감쇠가 거의 없다 → 맞은 지점·멀리서 나는 소리보다 낮게. 2D 소리도 감쇠가 없어 3D의 절반 정도로. Sci-Fi 팩은 원본이 커서 Impact·RPG 팩의 절반 이하.
- **크기 순서**: 부위 파괴 > 처치 > 가드 파괴 > 게이트 > 막기·적 피격·화살 명중 > 내 피격 > 휘두르기·발사 > 줍기·UI > 시전음.
- **한 SoundData 안의 클립은 크기·성격이 비슷해야** 한다. 하나만 다르면 그 클립이 걸릴 때만 튄다.
- **신호음은 고정**(클립 1개, min = max 피치): 게이트·UI·장전 완료·가드 파괴. 자주 반복되는 전투음은 무작위 유지.
- 계열 구분: 내가 때림 = Punch("퍽"), 내가 맞음 = Soft("툭"), 막음 = Plate("텅"), 깨짐 = Glass("쩍").
- 시전 도중 끊겨도 이미 재생된 시전음은 끝까지 난다 (정지 기능 없음). 조준 내내 울리는 루프가 필요하면 SoundPlayer에 핸들·정지 추가.
- 확장: 보스 BGM 교체(`MusicPlayer` 교차 페이드), 일시정지 Snapshot, 설정 볼륨 슬라이더(믹서 파라미터 노출 + Settings 구조), 숙련 레벨업·목표 완료음(`LevelChanged`·`StepCompleted`).
- `using NUnit.Framework`가 자동 완성으로 들어가면 빌드에서 컴파일 오류가 난다. 런타임 스크립트에서는 지운다.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-11-27 | 최초 작성: SoundData·SoundPlayer(풀)·MainMixer, 피격·막기·가드 파괴·사망음, 휘두르기(SoundEvent), 화살·마법 발사·명중·시전음, 부위 파괴음, 활 당기기·장전 완료·장비 교체·줍기·게이트·인벤토리 소리, BGM 루프 |
