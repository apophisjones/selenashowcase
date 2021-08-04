using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wisey : MonoBehaviour
{
    private Player _player;
    private Vector3 _initialPosition;
    private Miniath _hitEnemy;
    private int _basicDamage = 10;
    private int _smartSkillDamage = 10;
    private int _talkSkillDamage = 10;
    private int _specialSkillDamage = 30;
    [SerializeField] private int _lastAttack; // 0 = basic, 1 = smart, 2 = talkative, 3 = special;
    [SerializeField] private int _activeSkill = 0; // 0 = skill inactive; 1 = smart, 2 = talkative, 3 = special;
    private MeshCollider _loudHitbox;
    private BoxCollider _basicHitbox;
    private Camera _mainCamera;
    [SerializeField] private GameObject _sphereProjectile;
    private LineRenderer _lineRenderer;

    void Start()
    {
        _mainCamera = GameObject.FindObjectOfType<MainCamera>().GetComponent<Camera>();
        _initialPosition = transform.localPosition;
        _player = GameObject.FindObjectOfType<Player>();
        _loudHitbox = transform.Find("Loud_Hitbox").GetComponent<MeshCollider>();
        _basicHitbox = gameObject.GetComponent<BoxCollider>();
        _basicHitbox.enabled = false;
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
    }

    void Update()
    {
        SwitchSkill();
    }

    public void BasicAttack()
    {
        _lastAttack = 0;
        _basicHitbox.enabled = true;
        transform.localPosition = new Vector3(-0.5f, 0.3f, 1.5f);
        StartCoroutine(Swing());
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
                WittyLaser();
                break;
            case 2:
                LoudAndFurious();
                break;
            case 3:
                _lastAttack = 3;
                MindblowingSphere();
                break;
        }
    }
    void WittyLaser()
    {
        _lineRenderer.enabled = true;
        StartCoroutine(BeamDisable());
        //sound placeholder
        _lineRenderer.SetPosition(0, transform.position + new Vector3(0, 0, 0.2f));
        
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        Ray mouseRay = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit mouseRayHit = new RaycastHit();
        if (Physics.Raycast(mouseRay, out mouseRayHit, 500, layerMask))
        {
           _lineRenderer.SetPosition(1, mouseRayHit.point);
            Debug.Log(mouseRayHit.transform.name);
            if (mouseRayHit.transform.CompareTag("Enemy"))
            {
                mouseRayHit.transform.GetComponent<Miniath>().TakeDamage(_smartSkillDamage, 1);
            }
        }
        else
        {
            _lineRenderer.SetPosition(1, _mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 200f)));
        }
    }

    IEnumerator BeamDisable()
    {
        yield return new WaitForSeconds(0.25f);
        _lineRenderer.enabled = false;
    }
    void LoudAndFurious()
    {
        _lastAttack = 2;
        _loudHitbox.enabled = true;
        StartCoroutine(LoudTiming());
    }

    IEnumerator LoudTiming()
    {
        yield return new WaitForSeconds(0.1f);
        _loudHitbox.enabled = false;
        yield break;
    }

    void MindblowingSphere()
    {
        //This skill uses projectile type 1
        int layerMask = 1 << 8;
        layerMask = ~layerMask;
        Ray mouseRay = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit mouseRayHit = new RaycastHit();
        if (Physics.Raycast(mouseRay, out mouseRayHit, 500, layerMask))
        {
            Instantiate(_sphereProjectile, gameObject.transform.position, Quaternion.identity).
                GetComponent<Projectile>().SetMovement(mouseRayHit.point, _specialSkillDamage, 1);
        }
        else
        {
            Instantiate(_sphereProjectile, gameObject.transform.position, Quaternion.identity).
                GetComponent<Projectile>().SetMovement(mouseRay.GetPoint(100), _specialSkillDamage, 1);
        }
    }
    
    IEnumerator Swing()
    {
        yield return  new WaitForSeconds(0.6f);
        _basicHitbox.enabled = false;
        transform.localPosition = _initialPosition;
        yield break;
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
    
    private void OnTriggerEnter(Collider other)
    {
        int damage = _basicDamage;
        int type = 0; // 0 = basic, 1 = smart, 2 = talkative, 3 = special
        if (other.CompareTag("Enemy"))
        {
            switch (_lastAttack)
            {
                case 0:
                    damage = _basicDamage;
                    type = 0;
                    break;
                case 1:
                    damage = _smartSkillDamage;
                    type = 1;
                    break;
                case 2:
                    damage = _talkSkillDamage;
                    type = 2;
                    break;
                case 3:
                    damage = _specialSkillDamage;
                    type = 3;
                    break;
            }
            _hitEnemy = other.GetComponent<Miniath>(); //other enemy types TBD
            _hitEnemy.TakeDamage(damage, type);
        }
    }
}
