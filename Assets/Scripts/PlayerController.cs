using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI; // UI操作に必要

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2.0f;
    public float runSpeed = 5.0f;
    public float rotationSpeed = 10.0f;
    public float gravity = -9.81f;

    [Header("Battle System (PE Style)")]
    public float chargeTime = 3.0f; // ゲージが溜まるまでの秒数
    public Slider atbSlider;        // UIのスライダーをアタッチ
    public GameObject gunObject;    // 手に持っている銃（表示/非表示用）

    [Header("References")]
    public Animator animator;
    public Transform cameraTransform; // カメラのTransformを割り当てる
    public InputActionAsset inputActions; // Input Actionsファイルを割り当てる
    public GameObject rangeDome;

    private CharacterController _characterController;
    private Vector3 _velocity;
    private float _currentSpeed;
    
    // Input System用の変数
    private InputActionMap _playerActionMap;
    private InputAction _moveAction;
    private InputAction _sprintAction;
    private Vector2 _moveInput;
    
    // 状態管理
    private float _currentCharge = 0f;
    private bool _isBattleReady = false; // 攻撃可能状態か
    private bool _isAiming = false;      // 構えているか

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
        
        // 最初は銃を隠しておく（お好みで）
        if (gunObject != null) gunObject.SetActive(false);
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
        // 1. 構え中（Aiming）の処理
        if (_isAiming)
        {
            HandleAiming();
        }
        else
        {
            // 2. 通常移動＆ゲージ溜め
            HandleMovement();
            UpdateATB();
        }
        
        // 重力は常に適用
        ApplyGravity();
    }

    void UpdateATB()
    {
        // 構えていない時だけゲージが溜まる
        if (_currentCharge < chargeTime)
        {
            _currentCharge += Time.deltaTime;
            _isBattleReady = false;
        }
        else
        {
            _currentCharge = chargeTime;
            _isBattleReady = true;
        }

        // スライダーに反映
        if (atbSlider != null)
        {
            atbSlider.value = _currentCharge / chargeTime;
        }

        // ゲージMAX ＆ マウス左クリックで「構えモード」へ移行
        if (_isBattleReady && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            EnterAimMode();
        }
    }

    void EnterAimMode()
    {
        _isAiming = true;
        animator.SetBool("IsAiming", true); // 上半身レイヤーが有効になる
        if (gunObject != null) gunObject.SetActive(true);

        rangeDome.SetActive(true);

        // ★ここで時間が止まる演出を入れるとPEっぽくなります
    }

    void ExitAimMode()
    {
        _isAiming = false;
        animator.SetBool("IsAiming", false);
        if (gunObject != null) gunObject.SetActive(false);

        rangeDome.SetActive(false);
        
        // ゲージをリセット
        _currentCharge = 0f;
    }

    void HandleAiming()
    {
        // ここで敵選択などを行うが、今はクリックで発砲して終了にする
        
        // プレイヤーの向きを敵（今は仮でカメラ前方）に向ける
        if (cameraTransform != null)
        {
            Vector3 lookDirection = -cameraTransform.forward;
            lookDirection.y = 0f;
            if (lookDirection.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        // マウス左クリックで発砲
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            animator.SetTrigger("Fire");
            Debug.Log("Bang!");
            
            // 発砲アニメーションが終わったくらいのタイミングでモード解除（コルーチン推奨だが今は簡易的に）
            Invoke("ExitAimMode", 0.5f);
        }
        
        // キャンセル処理（右クリック）
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            ExitAimMode(); // 撃たずに戻る
        }
    }

    void HandleMovement()
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