namespace ProjectFantasy.Combat
{
    // ActionData 재생: 프레임 시계를 진행하며 이벤트 구간 진입·유지·종료 호출 (재생 중 GC 없음)
    public sealed class ActionPlayer
    {
        private const int InitialEventCapacity = 8;
        private const float BeforeStartFrame = -1f;

        private readonly IActionContext context;
        private bool[] activeEvents = new bool[InitialEventCapacity];
        private float elapsedTime;

        public ActionData Current { get; private set; }
        public bool IsPlaying => Current != null;
        public float CurrentFrame => Current != null ? elapsedTime * Current.FrameRate : 0f;
        public bool IsFinished => Current != null && CurrentFrame >= Current.LengthFrames;

        public ActionPlayer(IActionContext context)
        {
            this.context = context;
        }

        public void Play(ActionData action)
        {
            Stop();

            Current = action;
            elapsedTime = 0f;
            if (activeEvents.Length < action.EventCount) activeEvents = new bool[action.EventCount];

            Evaluate(BeforeStartFrame, CurrentFrame);
        }

        // deltaTime은 히트스톱 등으로 멈춘 시간을 제외한 값
        public void Tick(float deltaTime)
        {
            if (Current == null) return;

            float previousFrame = CurrentFrame;
            elapsedTime += deltaTime * Current.PlaybackSpeed;
            Evaluate(previousFrame, CurrentFrame);
        }

        // 활성 이벤트를 모두 종료 (상태 이탈·다음 액션 시작 시)
        public void Stop()
        {
            ActionData action = Current;
            if (action == null) return;

            Current = null;
            for (int i = 0; i < action.EventCount; i++)
            {
                if (!activeEvents[i]) continue;

                activeEvents[i] = false;
                action.GetEvent(i).OnEnd(context);
            }
        }

        private void Evaluate(float previousFrame, float frame)
        {
            ActionData action = Current;

            for (int i = 0; i < action.EventCount; i++)
            {
                ActionEvent actionEvent = action.GetEvent(i);
                if (actionEvent == null) continue;

                bool isInside = actionEvent.Contains(frame);

                if (activeEvents[i])
                {
                    if (isInside)
                    {
                        actionEvent.OnTick(context);
                    }
                    else
                    {
                        activeEvents[i] = false;
                        actionEvent.OnEnd(context);
                    }
                }
                else if (isInside)
                {
                    activeEvents[i] = true;
                    actionEvent.OnBegin(context);
                    actionEvent.OnTick(context);
                }
                else if (previousFrame < actionEvent.StartFrame && frame >= actionEvent.EndFrame)
                {
                    // 프레임 드랍으로 구간 전체를 건너뛰어도 1회 실행
                    actionEvent.OnBegin(context);
                    actionEvent.OnTick(context);
                    actionEvent.OnEnd(context);
                }

                // 콜백에서 상태 전이로 재생이 바뀌면 중단
                if (Current != action) return;
            }
        }
    }
}
