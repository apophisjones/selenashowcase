using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Analytics;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Miniath : MonoBehaviour
{
    // ---- Base ----
    private float _health = 200;
    private float _maxhealth = 200;
    private float _resistance = 7;
    private State _state;
    // ---- Other Objects ----
    private Text _combatLog;
    private Player _player;
    private EnemySpawner _enemySpawner;
    // ---- Movement ----
    private Rigidbody _rigidbody;
    private float _stalkSpeed = 2f;
    private float _canterSpeed = 5f;
    private float _gallopSpeed = 7f;
    private float _attackMoveSpeed = 20f;
    private Vector3 _towardsPlayer;
    private Vector3 _awayFromPlayer;
    private Vector3 _clockwiseThePlayer;
    private Vector3 _counterClockwiseThePlayer;
    private Vector3 _movementDirection;
    private float _moveSpeed;
    public bool _navigatingObstacles = false;
    private bool _canMoveForward = true;
    private bool _runningClockwise;
    private float _smoothTurnTime = 0.1f;
    private float _smoothTurnVelocity;
    private List<ArenaPortal> _portalList;
    private ArenaPortal _closestPortal;

    // ---- Combat ----
    private bool _playerInSightRange, 
        _playerInAttackRange = false;
    private BoxCollider _clawsHitbox;
    private Animator _clawsAnimator;
    private bool _prefertailAttack = false;
    private bool _maneuvering = false;
    private float _clawAttackDamage = 30f;
    private float _tailAttackDamage = 30f;
    private float _oblivionSkillDamage = 60f;
    private float _vengeanceSkillDamage = 50f;
    private bool _jumpReady = false;
    private float _dodgeChance;
    
    // ---- Cooldowns// ---- 
    private bool _clawAttackReady = false;
    private float _clawAttackCooldown = 3f;
    private float _clawAttackTime;
    private bool _oblivionSkillReady = false ;
    private float _oblivionSkillCooldown = 9f;
    private float _oblivionSkillTime;
    private bool _vengeanceSkillReady = false;
    private float _vengeanceSkillCooldown = 24f;
    private float _vengeanceSkillTime;
    private bool _dodgeReady = false;
    private float _dodgeCooldown = 3f;
    private float _dodgeTime;
    private bool _teleportReady = false;
    private float _teleportCooldown = 5f;
    private float _teleportTime;

    // ---- Animation ----
    private int _hashAttack1;
    private int _hashAttack2;
    private int _hashAttack3;

    private enum State
    {
        Fatigue,
        Evade,
        Stalk,
        Approach,
        Attack,
        Stun
    }


    private void Start()
    {
        _clawsHitbox = gameObject.transform.Find("Claws_Collider").GetComponent<BoxCollider>();
        _clawsHitbox.enabled = false;
        _player = GameObject.FindObjectOfType<Player>();
        _combatLog = GameObject.Find("HUD/Combat_Log").GetComponent<Text>();
        _enemySpawner = GameObject.FindObjectOfType<EnemySpawner>();
        _rigidbody = gameObject.GetComponent<Rigidbody>();
        _clawsAnimator = gameObject.transform.Find("Claws_Collider").GetComponent<Animator>();
        GetTeleporters();
        BeginEvading();
    }

    private void GetTeleporters()
    {
        _portalList = FindObjectsOfType<ArenaPortal>().ToList();
    }

    private void Update()
    {
        StateMachine();
        Death();
        Combat();
        PlayerTracking();
        Cooldowns();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    void StateMachine()
    {
        switch (_state)
        {
            case State.Evade:
                _moveSpeed = _canterSpeed;
                _movementDirection = _closestPortal.transform.position - transform.position;
                break;
            case State.Stalk when Vector3.Distance(transform.position, _player.transform.position) > 10f:
                _moveSpeed = _gallopSpeed;
                _movementDirection = _towardsPlayer;
                break;
            case State.Stalk when Vector3.Distance(transform.position, _player.transform.position) < 4f:
                _moveSpeed = _canterSpeed;
                _movementDirection = _awayFromPlayer;
                break;
            case State.Stalk:
                _moveSpeed = _stalkSpeed;
                _movementDirection = Random.Range(0, 1) == 0 ? _clockwiseThePlayer : _counterClockwiseThePlayer;
                break;
        }
    }
    
    private void Cooldowns()
    {
        _clawAttackReady = Time.time > _clawAttackTime;
        _oblivionSkillReady = Time.time > _oblivionSkillTime;
        _vengeanceSkillReady = Time.time > _vengeanceSkillTime;
        _dodgeReady = Time.time > _dodgeTime;
        _teleportReady = Time.time > _teleportTime;
    }

    void Movement()
    {
        _movementDirection = new Vector3(_movementDirection.x, 0, _movementDirection.z).normalized;
        _rigidbody.MovePosition(transform.position + _movementDirection * (_moveSpeed * Time.fixedDeltaTime));
        float facingAngle =
            Mathf.Atan2(_movementDirection.x, _movementDirection.z) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, facingAngle, ref _smoothTurnVelocity,
                                                  _smoothTurnTime);
        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
    }

    void PlayerTracking()
    {
        _towardsPlayer = _player.transform.position - transform.position;
        _awayFromPlayer = transform.position - _player.transform.position;
        _clockwiseThePlayer = Vector3.Cross(_towardsPlayer.normalized, Vector3.up);
        _counterClockwiseThePlayer = Vector3.Cross(_awayFromPlayer.normalized, Vector3.up);
    }

    void BeginEvading()
    {
        _state = State.Evade;
        PickPortal();
        _dodgeTime = Time.time + _dodgeCooldown;
        _dodgeChance = 5f;
    }

    public void BeingAimedAt()
    {
        switch (_state)
        {
            case State.Evade:
            {
                float dodgeRoll = Random.Range(0f, 100f);
                if (!_dodgeReady)
                {
                    return;
                }

                if (_dodgeReady && _dodgeChance >= dodgeRoll)
                {
                    Dodge();
                }
                else if (_dodgeReady && _dodgeChance <= dodgeRoll)
                {
                    _dodgeChance += 0.5f;
                }

                break;
            }
            case State.Stalk:
                if (_dodgeReady)
                {
                    _rigidbody.AddForce(_awayFromPlayer * (1000f * Time.fixedDeltaTime), ForceMode.Impulse);
                }
                _dodgeTime = Time.time + _dodgeCooldown;
                if (_teleportReady)
                {
                    BeginEvading();
                }
                break;
        }
    }

    void Dodge()
    {
        //TODO: Make the pattern of dodging more random and thus interesting or something 
        
        var direction = Random.Range(0, 1) == 0 ? _clockwiseThePlayer : _counterClockwiseThePlayer;
        _rigidbody.AddForce(Vector3.up * (50f * Time.fixedDeltaTime), ForceMode.Impulse);
        _rigidbody.AddForce(direction  * (50f * Time.fixedDeltaTime), ForceMode.Impulse);
        _dodgeTime = Time.time + _dodgeCooldown;
        _dodgeChance = 5f;
    }
    
    private void PickPortal()
    {
        float closestDistance = 10000;
        foreach (ArenaPortal portal in _portalList)
        {
            if (Vector3.Distance(portal.transform.position, transform.position) < closestDistance)
            { 
                closestDistance = Vector3.Distance(portal.transform.position, transform.position); 
                _closestPortal = portal;
                print(_closestPortal);
            }
        } 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal") && _state == State.Evade)
        {
            Teleport(other.GetComponent<ArenaPortal>());
        }
    }

    private void Teleport(ArenaPortal thisPortal)
    {
        ArenaPortal randomPortal;
        do
        {
            randomPortal = _portalList[Random.Range(0, _portalList.Count)];
        } 
        while (randomPortal == thisPortal);
        transform.position = randomPortal.transform.position;
        BeginStalking();
        _teleportReady = false;
        _teleportTime = Time.time + _teleportCooldown;
    }

    private void BeginStalking()
    {
        _state = State.Stalk;
    }

    public void AvoidObstacles(Vector3 obstacleClosestPoint)
    {
        print(obstacleClosestPoint);
        Vector3 awayFromObstacle = Vector3.Cross((obstacleClosestPoint - transform.position).normalized, Vector3.up).normalized;
        _movementDirection = awayFromObstacle.normalized;
    }
    
    void Combat()
    {
        
    }
    void ClawAttack()
    {
        if (_clawAttackReady)
        {
            _maneuvering = true;
            _rigidbody.velocity = Vector3.zero;
            _movementDirection = _towardsPlayer;
            _moveSpeed = _attackMoveSpeed;
            _clawsHitbox.enabled = true;
        }
    }

    void ClawAttackAnimation()
    {
        bool playerInRange = Vector3.Distance(transform.position, _player.transform.position) <= 1.5f;
        
        _clawsAnimator.SetBool("Attack 1", true);
        if (playerInRange) 
        {
            _clawsAnimator.SetBool("Attack 1", false);
        }
        else
        {
            _clawsAnimator.SetBool("Attack 2", true);
            _clawsAnimator.SetBool("Attack 1", false);
        }
    }

    void Oblivion()
    {
        
    }

    void TailWhip()
    {
        
    }

    void ShadowsOfVengeance()
    {
        
    }
    public void TakeDamage(int damage, int type)
    {
        int damageTaken = 0;
        switch (type)
        {
            case 0:
                damageTaken = damage;
                break;
            case 1:
                damageTaken = damage * 3;
                break;
            case 2:
                damageTaken = damage / 2;
                break;
            case 3:
                damageTaken = damage;
                //effects placeholder
                break;
        }
        _health -= damageTaken;
        Log(damageTaken);
    }

    void Log(int damageTaken)
    {
       _combatLog.text += " Enemy damaged for: " + damageTaken + " and is at: " + _health;
       float logCleanTime = Time.time + 2f;
       LogClean(logCleanTime);
    }

    void LogClean(float logCleanTime)
    {
        if (Time.time >= logCleanTime)
        {
            _combatLog.text = "";
        }
    }
    
    void Death()
    {
        if (_health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
