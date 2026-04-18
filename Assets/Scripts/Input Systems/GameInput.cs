using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    public Vector2 Move { get; private set; }  // latest move/aim vector
    public event Action AttackPressed;
    public event Action JumpPressed;
    public event Action JumpReleased;
    public event Action PushPressed;
    public event Action PushReleased;    
    public event Action PausePressed;

    private PlayerControls controls;
    private bool isHooked;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        // No longer using InputManager architecture

        controls.Player.Enable();
        HookEvents();
    }

    private void OnDisable()
    {
        controls.Player.Disable();
        UnhookEvents();
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            controls.Dispose();
            Instance = null;
        }
    }

    private void HookEvents()
    {
        if (isHooked) return;
        isHooked = true;

        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled  += OnMove;

        controls.Player.Attack.performed += OnAttack;
        controls.Player.Pause.performed += OnPause;

        controls.Player.Jump.performed   += OnJump;
        controls.Player.Jump.canceled    += OnJumpCanceled;
        
        controls.Player.Push.performed   += OnPush;
        controls.Player.Push.canceled    += OnPushCanceled;
    }

    private void UnhookEvents()
    {
        if (!isHooked) return;
        isHooked = false;

        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled  -= OnMove;

        controls.Player.Attack.performed -= OnAttack;
        controls.Player.Pause.performed -= OnPause;

        controls.Player.Jump.performed   -= OnJump;
        controls.Player.Jump.canceled    -= OnJumpCanceled;

        
        controls.Player.Push.performed   -= OnPush;
        controls.Player.Push.canceled    -= OnPushCanceled;
    }

    private void OnMove(InputAction.CallbackContext ctx) => Move = ctx.ReadValue<Vector2>();
    private void OnAttack(InputAction.CallbackContext ctx) => AttackPressed?.Invoke();
    private void OnPause(InputAction.CallbackContext ctx) => PausePressed?.Invoke();
    private void OnJump(InputAction.CallbackContext ctx) => JumpPressed?.Invoke();
    private void OnJumpCanceled(InputAction.CallbackContext ctx) => JumpReleased?.Invoke();
    private void OnPush(InputAction.CallbackContext ctx) => PushPressed?.Invoke();
    private void OnPushCanceled(InputAction.CallbackContext ctx) => PushReleased?.Invoke();

}