using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using TMPro;
using Random = UnityEngine.Random;


public class Player : MonoBehaviour
{
    //    ----Base----
    private InteractionsHUDManager _interactionsHudManager;
    private HUD _HUD;
    public MeshRenderer _playerMesh;
    [SerializeField] private Transform _playerEyes;
    [SerializeField] public bool _inCombat = false;
    [SerializeField] public bool _inRanged = false;
    private Camera _mainCamera;
    private CinemachineFreeLook _mainMotor;
    private CinemachineFreeLook _aimMotor;
    private Interactions _interactions;
    private Animator _animator; 
    private AnimationState _animationState;

    //    ----Speed---- 
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _sprintSpeed = 9f;
    private float _walkSpeed = 2f;
    private float _battleSpeed = 6f;
    private float _airSpeed = 10f;
    
    //    ----Inputs----
    public bool _playerCanMove = true;
    private float _verticalInput;
    private float _horizontalInput;
    private bool _jumpPressed = false;
    private bool _dashPressed = false;
    private bool _sprintHeld = false;

    //    ----Movement----
    private Rigidbody _playerRigidbody;
    public Vector3 _direction;
    private Vector3 _verticalDir;
    private float _smoothTurnTime = 0.1f;
    private float _smoothTurnVelocity;
    private float _gravityAccel = -12f;
    private float _jumpForce = 4000f;
    private float _dragCoef = 0.3f;
    [SerializeField] private bool _playerGrounded;
    private CapsuleCollider _playerCollider;
   
    //    ----Dashing----
    public float _dashForce;
    private float _dashSpeed = 8000f;
    private float _airDashSpeed = 5000f;
    [SerializeField] public bool _dashReady = true;
    
    //    ----Combat----
    public int _weapon = 0; // 0 = Wisey, 1 = Declamator;
    private Wisey _wisey;
    private Declamator _declamator;
    [SerializeField] private float _inputTimeout = 0;
 }

    
    private enum AnimationState
    {
        Idle,
        Walking,
        Sprinting,
        Running,
        Airborne,
    }

    void Awake()
    {
        _HUD = GameObject.Find("HUD").GetComponent<HUD>();
        if (_HUD == null)
        {
            Debug.LogError("HUD is null");
        }
        _playerRigidbody = gameObject.GetComponent<Rigidbody>();
        _mainCamera = FindObjectOfType<MainCamera>().GetComponent<Camera>();
        _wisey = transform.Find("Body/Wisey").GetComponent<Wisey>();
        _declamator = transform.Find("Body/Declamator").GetComponent<Declamator>();
        _mainMotor = GameObject.Find("Main_Motor").GetComponent<CinemachineFreeLook>();
        _aimMotor = GameObject.Find("Aim_Motor").GetComponent<CinemachineFreeLook>();
        _interactionsHudManager = GameObject.Find("Interactions_HUD").GetComponent<InteractionsHUDManager>();
        if (_interactionsHudManager == null)
        {
            Debug.LogError("Interactions HUD Manager is null");
        }

    }

    private void Start()
    {
        _playerCollider = GetComponent<CapsuleCollider>();
        _animator = GetComponent<Animator>();
        _interactionsHudManager.gameObject.SetActive(false);
        
    }

    private void Update()
    {
        DetectInput();
        GroundCheck();
        Combat();
        SwitchRanged();
        SwitchCombat();
        CameraPriority();
        StatsTracking();
        StatsPlayerTest();
        AnimationStates();
        Aim();
    }

    private void FixedUpdate()
    {
        Movement();
        Dash();
    }

    private void AnimationStates()
    {
        switch (_animationState)
        {
            case AnimationState.Walking:
                _animator.SetBool("isMoving", true);
                _animator.SetBool("isRunning", false);
                _animator.SetBool("isSprinting", false);
                _animator.SetBool("isFalling", false);
                break;
            case AnimationState.Sprinting:
                _animator.SetBool("isMoving", true);
                _animator.SetBool("isRunning", false);
                _animator.SetBool("isSprinting", true);
                _animator.SetBool("isFalling", false);
                break;
            case AnimationState.Airborne:
                _animator.SetBool("isMoving", false);
                _animator.SetBool("isRunning", false);
                _animator.SetBool("isFalling", true);
                _animator.SetBool("isSprinting", false);
                break;
            case AnimationState.Idle:
                _animator.SetBool("isMoving", false);
                _animator.SetBool("isRunning", false);
                _animator.SetBool("isSprinting", false);
                _animator.SetBool("isFalling", false);
                break;
            case AnimationState.Running:
                _animator.SetBool("isMoving", true);
                _animator.SetBool("isRunning", true);
                _animator.SetBool("isSprinting", false);
                _animator.SetBool("isFalling", false);
                break;
            default:
                _animator.SetBool("isMoving", false);
                _animator.SetBool("isRunning", false);
                _animator.SetBool("isSprinting", false);
                _animator.SetBool("isFalling", false);
                break;
        }
    }

          
    void DetectInput()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");
        if (Input.GetButtonDown("Jump") && _playerGrounded)
        {
            _jumpPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && _dashReady && _inCombat)
        {
            _dashPressed = true;
        }
        
        if (Input.GetKey(KeyCode.LeftShift) && !_inCombat)
        {
            _sprintHeld = true;
        }
        else
        {
            _sprintHeld = false;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            SwitchWeapon();
        }
    }

    void GroundCheck()
    {
        RaycastHit groundhit = new RaycastHit();
        var groundDetection = Physics.Raycast(transform.position, Vector3.down, out groundhit, 1.5f);
        _playerGrounded = groundDetection;
        if (_playerGrounded)
        {
            _dashForce = _dashSpeed;
        }
        else if (!_playerGrounded)
        {
            _dashForce = _airDashSpeed;
            _animationState = AnimationState.Airborne;
        }
    }
    void Movement()
    {
        if (!_playerCanMove) return;
        _direction = new Vector3(_horizontalInput, 0f, _verticalInput).normalized;
        if (_direction.magnitude <= 0.1f && _playerGrounded)
        {
            _animationState = AnimationState.Idle;
        }
        else if (_direction.magnitude >= 0.1f)
        {
            if (!_inRanged)
            {
                float facingAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg +
                                    _mainCamera.transform.eulerAngles.y;
                float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, facingAngle,
                                                          ref _smoothTurnVelocity,
                                                          _smoothTurnTime);
                _direction = Quaternion.Euler(0f, facingAngle, 0f) * Vector3.forward;
                transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
            }
            else if (_inRanged)
            {
                float facingAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg +
                                    _mainCamera.transform.eulerAngles.y;
                _direction = Quaternion.Euler(0f, facingAngle, 0f) * Vector3.forward;
                transform.rotation = Quaternion.Euler(0f, _mainCamera.transform.eulerAngles.y, 0f);
            }

            var velocity = _playerRigidbody.velocity;

            if (_playerGrounded)
            {
                _playerRigidbody.MovePosition(transform.position + _direction * (_moveSpeed * Time.fixedDeltaTime));
                if (!_sprintHeld && !_inCombat)
                {
                    _animationState = AnimationState.Walking;
                }
                else if (_inCombat)
                {
                    _animationState = AnimationState.Running;
                }
            }
            else if (!_playerGrounded)
            {
                _playerRigidbody.AddForce((_direction) * (_airSpeed * Time.fixedDeltaTime),
                                          ForceMode.VelocityChange);
                velocity.x *= 1.00f - _dragCoef;
                velocity.z *= 1.00f - _dragCoef;
            }
        }
        else if (_inRanged)
        {
            transform.rotation = Quaternion.Euler(0f, _mainCamera.transform.eulerAngles.y, 0f);
        }

        if (_playerGrounded && _jumpPressed)
        {
            _jumpPressed = false;
            _animator.Play("Base.Jump");
            _playerRigidbody.AddForce(Vector3.up * (_jumpForce * Time.fixedDeltaTime), ForceMode.Impulse);
            if (_sprintHeld)
            {
                
                _playerRigidbody.AddForce(_direction * (_jumpForce * Time.fixedDeltaTime), ForceMode.Impulse);
            }
            else
            {
                _playerRigidbody.AddForce(_direction * (_jumpForce / 2 * Time.fixedDeltaTime), ForceMode.Impulse);
            }
        }

        if (!_playerGrounded && Input.GetKey(KeyCode.C))
        {
            _playerRigidbody.AddForce(Vector3.down * (70 * Time.fixedDeltaTime), ForceMode.VelocityChange);
        }

        if (!_inCombat && _sprintHeld)
        {
            _animationState = AnimationState.Sprinting;
            _moveSpeed = _sprintSpeed;
        }
        else if (!_inCombat && !_sprintHeld)
        {
            _moveSpeed = _walkSpeed;
        }
        else if (_inCombat)
        {
            _moveSpeed = _battleSpeed;
        }
    }

    void Dash()
    {
        if (_inCombat && _dashReady && _dashPressed)
        {
            _dashPressed = false;
            _playerRigidbody.AddForce(_direction * (_dashForce * Time.fixedDeltaTime), ForceMode.Impulse);
            _animator.Play("Base.Dash");
            StartCoroutine(DashCooldown());
        }
    }

    IEnumerator DashCooldown()
    {
        _dashReady = false;
        _dragCoef = 0.99f;
        yield return new WaitForSeconds(1f);
        _dragCoef = 0.3f; 
        yield return new WaitForSeconds(0.5f);
        _dashReady = true; 
        yield break;
    }
    public void InteractionsHUD()
    {
        if (_HUD.gameObject.activeInHierarchy)
        {
            _HUD.gameObject.SetActive(false);
        }
        if (_interactionsHudManager.gameObject.activeInHierarchy == false)
        {
            _interactionsHudManager.gameObject.SetActive(true);
        }
    }
    
    public void GameHUD()
    {
        if (_interactionsHudManager.gameObject.activeInHierarchy)
        {
            _interactionsHudManager.gameObject.SetActive(false);
        }

        if (_HUD.gameObject.activeInHierarchy == false)
        {
            _HUD.gameObject.SetActive(true);
        }
    }

    public void HUDRaiseOpacity()
    {
        _interactionsHudManager.RaiseOpacity();
    }

    public void HUDLowerOpacity()
    {
        _interactionsHudManager.LowerOpacity();
    }

    void SwitchWeapon()
    {
        if (_weapon == 0)
        {
            _weapon = 1;
            _wisey.gameObject.SetActive(false);
            _declamator.gameObject.SetActive(true);
        }
        else if (_weapon == 1)
        {
            _weapon = 0;
            _declamator.gameObject.SetActive(false);
            _wisey.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Wrong Weapon!");
        }
    }

    void SwitchCombat()
    {
        //THIS IS A VERY TEMPORARY METHOD THAT WILL BE CHANGED
        //IF YOU SEE THE METHOD "SWITCH COMBAT" IN LATER STAGES
        //DO SOMETHING
        
        if (Input.GetKeyDown(KeyCode.J))
        {
            _inCombat = !_inCombat;
        }

        if (_inCombat)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void Aim()
    {
        if (!_inCombat) return;
        Miniath targetMiniath;
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        Ray mouseRay = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit mouseRayHit = new RaycastHit();
        if (Physics.Raycast(mouseRay, out mouseRayHit, 500, layerMask))
        {
            //TODO: Rewrite this using sending messages, this is ass 
            if (mouseRayHit.transform.CompareTag("Miniath"))
            {
                targetMiniath = mouseRayHit.transform.GetComponent<Miniath>();
                if (Physics.Raycast(mouseRay, out mouseRayHit, 500, layerMask))
                {
                    targetMiniath.BeingAimedAt();
                }
            }
            else if (mouseRayHit.transform.CompareTag("Singer"))
            {
                //placeholder
            }
        }
    }

    void Combat()
    {
        if (_weapon == 0)
        {
            if (_inCombat && Input.GetMouseButtonDown(0))
            {
                _wisey.BasicAttack();
                _inRanged = true;
            }

            if (_inCombat && Input.GetMouseButtonDown(1))
            {
                _wisey.UseSkill();
                _inRanged = true;
            }
        }
        else if (_weapon == 1)
        {
            if (_inCombat && Input.GetMouseButton(0))
            {
                _declamator.BasicAttack();
                _inRanged = true;
            }

            if (_inCombat && Input.GetMouseButtonDown(1))
            {
                _declamator.UseSkill();
                _inRanged = true;
            }
        }
    }

    void CameraPriority()
    {
        int activeMotor = 0; //0 = main, 1 = aim
        if (_inRanged)
        {
            _aimMotor.Priority = 10;
            _mainMotor.Priority = 5;
            activeMotor = 1;
        }
        else if (!_inRanged)
        {
            _aimMotor.Priority = 5;
            _mainMotor.Priority = 10;
            activeMotor = 0;
        }
        AlignMotors(activeMotor);
    }
    
    void AlignMotors(int activeMotor)
    {
        if (activeMotor == 0)
        {
            _aimMotor.m_XAxis = _mainMotor.m_XAxis;
            _aimMotor.m_YAxis = _mainMotor.m_YAxis;
        }
        else if (activeMotor == 1)
        {
            _mainMotor.m_XAxis = _aimMotor.m_XAxis;
            _mainMotor.m_YAxis = _aimMotor.m_YAxis;
        }
    }
    void SwitchRanged()
    {
        if (_inRanged && _jumpPressed || _dashPressed)
        {
            _inRanged = false;
        }
    }