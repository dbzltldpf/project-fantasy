using UnityEngine;

namespace ProjectFantasy.Enemy
{
    // 리쉬: 대상을 버리고 전투를 시작한 지점으로 달려가 그때 보던 방향으로 돌아선 뒤 체력 회복 후 배회
    public sealed class EnemyReturnState : EnemyStateBase
    {
        // 원래 방향과 이 각도(도) 이내면 돌아서기 완료
        private const float FacingTolerance = 1f;
        private bool hasArrived;
        public EnemyReturnState(EnemyController controller) : base(controller) { }

        public override void Enter()
        {
            hasArrived = false;
            Controller.Health.SetInvincible(true);
            Perception.ClearTarget();
            Navigator.SetInstantSpeed(true);
            Navigator.SnapRotation(Controller.ReturnPosition - Controller.transform.position);
            Navigator.MoveTo(Controller.ReturnPosition, Data.ReturnSpeed);
        }

        public override void Tick(float deltaTime)
        {
            if (hasArrived)
            {
                TurnBack(deltaTime);
                return;
            }

            UpdateLocomotion(deltaTime);
            if (!Navigator.IsNear(Controller.ReturnPosition)) return;

            Navigator.Warp(Controller.ReturnPosition, Controller.transform.rotation); // 허용 오차만큼 밀리지 않게 정확한 위치로 보정
            hasArrived = true;
        }
        // 도착 후 전투 시작 때 보던 방향으로 돌아선 뒤 배회
        private void TurnBack(float deltaTime)
        {
            Navigator.FaceTowards(Controller.ReturnRotation * Vector3.forward, Data.TurnSpeed, deltaTime);
            EnemyAnimator.PlayLocomotion(0f);
            if (Quaternion.Angle(Controller.transform.rotation, Controller.ReturnRotation) > FacingTolerance) return;

            Controller.ResetCombat();
            Controller.ChangeState(Controller.PatrolState);
        }
        public override void Exit()
        {
            Controller.Health.SetInvincible(false);
            Navigator.SetInstantSpeed(false);
        }
    }
}
