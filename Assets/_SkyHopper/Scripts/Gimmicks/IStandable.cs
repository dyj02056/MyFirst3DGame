namespace SkyHopper
{
    /// <summary>플레이어가 위에 서 있으면 매 프레임 알림을 받는 장치 (떨어지는 발판, 점프대 등)</summary>
    public interface IStandable
    {
        void OnPlayerStand(PlayerController player);
    }
}
