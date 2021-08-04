using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Declamator : MonoBehaviour
{
    private Player _player;
    private Vector3 _initialPosition;
    private Miniath _hitEnemy;
    private int _basicDamage = 2;
    private int _smartSkillDamage = 10;
    private int _talkSkillDamage = 10;
    private int _specialSkillDamage = 30;
    [SerializeField] private int _lastAttack; // 0 = basic, 1 = smart, 2 = talkative, 3 = special;
    [SerializeField] private int _activeSkill = 0; // 0 = skill inactive; 1 = smart, 2 = talkative, 3 = special;
    private Camera _mainCamera;
    [SerializeField] private GameObject _chattyProjectile;
    [SerializeField] private int _firingOrder = 0;
    private Vector3 _bulletStaggerBarrel;
    private Vector3 _bulletStaggerTarget;
    private float _firerate = 0.15f;
    private float _nextShotTime = 0f;

    void Start()
    {
        _mainCamera = GameObject.FindObjectOfType<MainCamera>().GetComponent<Camera>();
        _initialPosition = transform.localPosition;
        _player = GameObject.FindObjectOfType<Player>();
    }

    
    void Update()
    {
        SwitchSkill();
    }

    public void BasicAttack()
    {
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        Ray mouseRay = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit mouseRayHit = new RaycastHit();
        if (Time.time > _nextShotTime)
        {
            if (Physics.Raycast(mouseRay, out mouseRayHit, 500, layerMask))
            {
                _nextShotTime = Time.time + _firerate;
                switch (_firingOrder)
                {
                    case 0:
                        _bulletStaggerBarrel = new Vector3(transform.position.x + 0.1f, transform.position.y, transform.position.z);
                        _bulletStaggerTarget = new Vector3(mouseRay.GetPoint(100).x + 0.1f, mouseRay.GetPoint(100).y,
                                                           mouseRay.GetPoint(100).z);
                        _firingOrder++;
                        break;
                    case 1:
                        _bulletStaggerBarrel = new Vector3(transform.position.x - 0.1f, transform.position.y, transform.position.z);
                        _bulletStaggerTarget = new Vector3(mouseRay.GetPoint(100).x - 0.1f, mouseRay.GetPoint(100).y,
                                                           mouseRay.GetPoint(100).z);
                        _firingOrder--;
                        break;
                }
                Instantiate(_chattyProjectile, _bulletStaggerBarrel, Quaternion.identity).GetComponent<Projectile>()
                    .SetMovement(_bulletStaggerTarget, _basicDamage, 0);
            }
            else
            {
                _nextShotTime = Time.time + _firerate;
                Instantiate(_chattyProjectile, _bulletStaggerBarrel, Quaternion.identity).GetComponent<Projectile>()
                    .SetMovement(_bulletStaggerTarget, _basicDamage, 0);
                switch (_firingOrder)
                {
                    case 0:
                        _bulletStaggerBarrel = new Vector3(transform.position.x + 0.1f, transform.position.y, transform.position.z);
                        _bulletStaggerTarget = new Vector3(mouseRay.GetPoint(100).x + 0.1f, mouseRay.GetPoint(100).y,
                                                           mouseRay.GetPoint(100).z);
                        _firingOrder++;
                        break;
                    case 1:
                        _bulletStaggerBarrel = new Vector3(transform.position.x - 0.1f, transform.position.y, transform.position.z);
                        _bulletStaggerTarget = new Vector3(mouseRay.GetPoint(100).x - 0.1f, mouseRay.GetPoint(100).y,
                                                           mouseRay.GetPoint(100).z);
                        _firingOrder--;
                        break;
                }
            }
        }
    }
    
    public void UseSkill()
    {
        switch (_activeSkill)
        {
            case 0:
                Debug.Log("No skill active");
                break;
            case 1:
                _lastAttack = 1;
                OvermindWaves();
                break;
            case 2:
                SilentShield();
                break;
            case 3:
                SweetspeechNet();
                _lastAttack = 3;
                break;
        }
    }

    void OvermindWaves()
    {
        
    }

    void SilentShield()
    {
        
    }

    void SweetspeechNet()
    {
        
    }
    void SwitchSkill()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _activeSkill = 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _activeSkill = 2; 
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _activeSkill = 3;
        }
        else
        {
            return;
        }
    }
}
