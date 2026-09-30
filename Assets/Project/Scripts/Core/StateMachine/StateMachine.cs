using System;

namespace ProjectFantasy.Core
{
    // 상태 전이만 담당하는 제네릭 상태 머신 (플레이어/적 공용)
    public sealed class StateMachine<TState> where TState : class, IState
    {
        public TState CurrentState { get; private set; }

        public event Action<TState, TState> StateChanged;

        public void Initialize(TState startState)
        {
            CurrentState = startState ?? throw new ArgumentNullException(nameof(startState));
            CurrentState.Enter();
        }

        // 같은 상태로의 전이는 재진입(Exit → Enter)으로 처리
        public void ChangeState(TState nextState)
        {
            if (nextState == null) return;

            TState previousState = CurrentState;
            previousState?.Exit();
            CurrentState = nextState;
            CurrentState.Enter();
            StateChanged?.Invoke(previousState, nextState);
        }

        public void Tick(float deltaTime) => CurrentState?.Tick(deltaTime);
    }
}
