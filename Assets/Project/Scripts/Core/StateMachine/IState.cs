namespace ProjectFantasy.Core
{
    // 상태 머신이 구동하는 단일 상태
    public interface IState
    {
        void Enter();
        void Tick(float deltaTime);
        void Exit();
    }
}
