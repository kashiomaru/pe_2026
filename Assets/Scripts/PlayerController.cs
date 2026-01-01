using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;
    public float rotationSpeed = 10.0f;
    public float gravity = -9.81f;

    [Header("References")]
    public Animator animator;
    public Transform cameraTransform; // カメラのTransformを割り当てる
    public InputActionAsset inputActions; // Input Actionsファイルを割り当てる

    private CharacterController _characterController;
    private Vector3 _velocity;
    private float _currentSpeed;
    
    // Input System用の変数
    private InputActionMap _playerActionMap;
    private InputAction _moveAction;
    private InputAction _sprintAction;
    private Vector2 _moveInput;

    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        
        // カメラが未設定ならメインカメラを自動取得
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Input Systemの初期化
        if (inputActions != null)
        {
            _playerActionMap = inputActions.FindActionMap("Player");
            if (_playerActionMap != null)
            {
                _moveAction = _playerActionMap.FindAction("Move");
                _sprintAction = _playerActionMap.FindAction("Sprint");
                
                // アクションを有効化
                _moveAction.Enable();
                _sprintAction.Enable();
            }
        }
    }

    void OnEnable()
    {
        // アクションを有効化
        if (_moveAction != null) _moveAction.Enable();
        if (_sprintAction != null) _sprintAction.Enable();
    }

    void OnDisable()
    {
        // アクションを無効化
        if (_moveAction != null) _moveAction.Disable();
        if (_sprintAction != null) _sprintAction.Disable();
    }

    void Update()
    {
        Move();
        ApplyGravity();
    }

    void Move()
    {
        // 入力取得 (新しいInput System)
        if (_moveAction != null)
        {
            _moveInput = _moveAction.ReadValue<Vector2>();
        }
        else
        {
            _moveInput = Vector2.zero;
        }
        
        // 入力があるかどうか
        Vector3 direction = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;
        bool hasInput = direction.magnitude >= 0.1f;

        // シフトキー（Shift）判定：押していると歩く、押していないと走る
        bool isWalking = _sprintAction != null && _sprintAction.IsPressed();
        float targetSpeed = hasInput ? (isWalking ? walkSpeed : runSpeed) : 0f;

        // 移動処理
        if (hasInput)
        {
            // カメラの向きを基準に移動方向を計算
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            
            // キャラクターの向きを滑らかに補間
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _currentSpeed, 1.0f / rotationSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // 移動ベクトルを作成
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _characterController.Move(moveDir.normalized * targetSpeed * Time.deltaTime);
        }

        // アニメーション制御 (Blend TreeのSpeedパラメータ)
        // 0=Idle, 0.5=Walk, 1.0=Run になるように正規化して渡す
        float animValue = 0f;
        if (hasInput) animValue = isWalking ? 0.5f : 1.0f;
        
        // DampTimeを使って数値の急変を防ぐ（アニメーションがカクつかないように）
        animator.SetFloat("Speed", animValue, 0.1f, Time.deltaTime);
    }

    void ApplyGravity()
    {
        // 接地しているなら重力リセット（少し押し付ける）
        if (_characterController.isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
        }

        _velocity.y += gravity * Time.deltaTime;
        _characterController.Move(_velocity * Time.deltaTime);
    }
}