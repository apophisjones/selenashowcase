using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float _speed = 15;
    public int _damage;
    public Vector3 _direction;
    public int _projectileType;
    //Projectile Type is what projectile it is (no fucking duh)
    //0 = Declamator Basic; 1 = Wisey Sphere; more TBD

    private void Start()
    {
        StartCoroutine(SphereProjectileTimeout());
    }

    public void SetMovement(Vector3 hitPoint, int damage, int type)
    {
        _direction = (hitPoint - transform.position).normalized;
        _damage = damage;
        _projectileType = type;
        if (_projectileType == 0)
        {
            _speed = 45;
        }
        else if (_projectileType == 1)
        {
            _speed = 15;
        }
    }
    void FixedUpdate()
    {
        transform.Translate(_direction * (_speed * Time.fixedDeltaTime), Space.Self);
    }
    
   private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && _projectileType == 0)
        {
            other.GetComponent<Miniath>().TakeDamage(_damage, 0);
        }
        else if (other.CompareTag("Enemy") && _projectileType == 1)
        {
            other.GetComponent<Miniath>().TakeDamage(_damage, 3);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            return;
        }
        else if (other.CompareTag("PlayerVision"))
        {
            return;
        }
        else if (other.CompareTag("AttackHitbox"))
        {
            return;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator SphereProjectileTimeout()
    {
        yield return new WaitForSeconds(6f);
        Destroy(gameObject);
        yield break;
    }

}
