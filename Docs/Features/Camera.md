# Camera

## 개요
플레이어를 따라가는 3인칭 오빗 카메라. 마우스/패드로 회전하고, 벽에 막히면 앞으로 당겨진다. 조준 모드에서는 숄더뷰 프로필로 부드럽게 전환된다.

## 구성 스크립트
| 파일 | 책임 |
|---|---|
| [ThirdPersonCamera.cs](../../Assets/Project/Scripts/CameraSystem/ThirdPersonCamera.cs) | 회전 입력, 타깃 추적, 충돌 거리 보정, 커서 잠금 |
| [CameraData.cs](../../Assets/Project/Scripts/CameraSystem/Data/CameraData.cs) | 감도·피치·거리·충돌 튜닝 데이터 (SO) |

## 동작 흐름
`LateUpdate` (플레이어 이동 이후)
1. **회전** – Look 입력으로 yaw/pitch 갱신, pitch는 min~max로 제한
   - 마우스: 프레임 누적 델타 × `mouseSensitivity` (deltaTime 미적용)
   - 패드: 스틱 값 × `gamepadSensitivity` × deltaTime (초당 회전량)
2. **추적** – 타깃 위치 + `targetHeight`를 `SmoothDamp`로 따라감
3. **충돌** – 초점에서 뒤쪽으로 `SphereCast` → 막히면 `hit.distance`(최소 `minDistance`)
   - 가까워질 때는 **즉시**, 멀어질 때는 `distanceRecoverSpeed`로 **부드럽게** 복귀
4. `SetPositionAndRotation`으로 한 번에 적용

### 조준 프로필 블렌드
- `SetAiming(bool)` → 블렌드 값이 `aimBlendSpeed`로 0↔1 이동 (`PlayerAimPresenter`가 호출)
- 기본 `cameraData`와 `aimCameraData` 사이를 보간: 감도, 거리, 초점 높이, **어깨 오프셋**(yaw 기준 오른쪽), **FOV**
- 피치 제한·충돌 설정은 기본 프로필 사용

## 데이터 파라미터
### CameraData
| 필드 | 기본값 | 의미 |
|---|---|---|
| mouseSensitivity | 0.1 | 마우스 감도 (°/픽셀) |
| gamepadSensitivity | 180 | 패드 감도 (°/s) |
| invertY | false | 상하 반전 |
| defaultPitch / minPitch / maxPitch | 15 / -30 / 70 | 피치 각도 |
| distance | 5 | 기본 거리 |
| targetHeight | 1.5 | 초점 높이 |
| followSmoothTime | 0.05 | 추적 스무딩 |
| shoulderOffset | 0 | 좌우 어깨 오프셋 (조준 프로필 0.6 권장) |
| fieldOfView | 60 | 시야각 (조준 프로필 50 권장) |
| collisionLayers | Default Raycast Layers | 충돌 검사 레이어 |
| collisionRadius | 0.25 | SphereCast 반경 |
| minDistance | 0.5 | 최소 거리 |
| distanceRecoverSpeed | 8 | 거리 복귀 속도 |
| lockCursor | true | 활성화 시 커서 잠금 |

## 에디터 설정
- Main Camera에 `ThirdPersonCamera` 추가 → Target(Rogue), CameraData, Look 액션(`Player/Look`) 연결
- 조준용 `AimCameraData`(distance 2.2, targetHeight 1.6, shoulderOffset 0.6, FOV 50, mouseSensitivity 0.06) → `Aim Camera Data` (비우면 조준 전환 없음)
- `Project/Data`에서 Create → ProjectFantasy → Camera → Third Person Camera Data 생성
- ⚠️ `collisionLayers`에서 **Player 레이어 제외** (안 하면 캐릭터 몸에 걸려 카메라가 당겨짐)

## 주의사항 / 확장 포인트
- 다른 대상을 추적하려면 `SetTarget(Transform)` 사용 (컷신, 탈것 등).
- 예정: 주목(락온) 시 타깃 프레이밍.

## 변경 이력
| 날짜 | 내용 |
|---|---|
| 2026-09-30 | 최초 작성 (오빗 회전, SmoothDamp 추적, SphereCast 충돌) |
| 2026-10-02 | 조준 프로필 블렌드(숄더뷰), `shoulderOffset`·`fieldOfView` 추가 |
