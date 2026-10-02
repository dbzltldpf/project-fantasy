namespace ProjectFantasy.Combat
{
    // 액션 이벤트가 소유자(플레이어·적)에게 요청하는 동작
    public interface IActionContext
    {
        void BeginHit(float damageMultiplier, float hitStopDuration);
        void TickHit();
        void EndHit();
        void OpenComboWindow(int transitionFrame);
        void CloseComboWindow();
        void SetMoveSpeed(float speed);
    }
}
