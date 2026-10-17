using UnityEngine;
using UnityEngine.InputSystem;

namespace SkyHopper
{
    /// <summary>
    /// 키보드·마우스·게임패드 입력을 읽어서 다른 스크립트가 쓰기 쉽게 정리해 주는 컴포넌트.
    /// 키 배치는 GAME_DESIGN.md의 "4. 조작" 표와 같습니다.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        InputAction moveAction;
        InputAction mouseLookAction;
        InputAction stickLookAction;
        InputAction jumpAction;
        InputAction dashAction;

        /// <summary>이동 입력 (x = 좌우, y = 앞뒤), 길이는 최대 1</summary>
        public Vector2 Move => Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        /// <summary>이번 프레임 마우스 이동량 (픽셀)</summary>
        public Vector2 MouseLook => mouseLookAction.ReadValue<Vector2>();
        /// <summary>게임패드 오른쪽 스틱 (-1 ~ 1)</summary>
        public Vector2 StickLook => stickLookAction.ReadValue<Vector2>();
        public bool JumpPressed => jumpAction.WasPressedThisFrame();
        public bool JumpHeld => jumpAction.IsPressed();
        public bool DashPressed => dashAction.WasPressedThisFrame();

        void Awake()
        {
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            moveAction.AddBinding("<Gamepad>/leftStick");

            mouseLookAction = new InputAction("MouseLook", InputActionType.Value, "<Mouse>/delta");
            stickLookAction = new InputAction("StickLook", InputActionType.Value, "<Gamepad>/rightStick");

            jumpAction = new InputAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/space");
            jumpAction.AddBinding("<Gamepad>/buttonSouth");

            dashAction = new InputAction("Dash", InputActionType.Button);
            dashAction.AddBinding("<Keyboard>/leftShift");
            dashAction.AddBinding("<Gamepad>/buttonWest");
        }

        void OnEnable()
        {
            moveAction.Enable();
            mouseLookAction.Enable();
            stickLookAction.Enable();
            jumpAction.Enable();
            dashAction.Enable();
        }

        void OnDisable()
        {
            moveAction.Disable();
            mouseLookAction.Disable();
            stickLookAction.Disable();
            jumpAction.Disable();
            dashAction.Disable();
        }

        void OnDestroy()
        {
            moveAction.Dispose();
            mouseLookAction.Dispose();
            stickLookAction.Dispose();
            jumpAction.Dispose();
            dashAction.Dispose();
        }
    }
}
