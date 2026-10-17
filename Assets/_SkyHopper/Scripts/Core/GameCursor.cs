using UnityEngine;

namespace SkyHopper
{
    /// <summary>마우스 커서 숨기기(게임 조작 중) / 보이기(메뉴 화면)</summary>
    public static class GameCursor
    {
        public static bool IsLocked => Cursor.lockState == CursorLockMode.Locked;

        public static void SetLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
