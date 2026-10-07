namespace ProjectFantasy.Core
{
    // 처치 보상(경험치)을 받는 쪽 (처치에 기여한 공격자의 게임오브젝트에 부착)
    public interface IKillRewardReceiver
    {
        void ReceiveKillReward(int experience);
    }
}
