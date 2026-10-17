using UnityEngine;

namespace SkyHopper
{
    /// <summary>마우스 커서 숨기기(게임 조작 중) / 보이기(메뉴 화면)</summary>
    public static class GameCursor
    {
        public static bool IsLocked => Cursor.lockState == CursorLockMode.Locked;

        public static void SetLocked(bool locked)
        {
            // 자동 시연 녹화 중에는 사용자의 마우스를 붙잡지 않음
            if (PlayerInputHandler.DemoMode)
                locked = false;

            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
