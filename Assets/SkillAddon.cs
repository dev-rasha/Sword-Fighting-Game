using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAddon : MonoBehaviour
{
    public int damage;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnCollisionEnter(Collision collision)
    {
        // check if you hit an enemy
        if(collision.gameObject.GetComponent<EnemyAi>() != null)
        {
            EnemyAi actor = collision.gameObject.GetComponent<EnemyAi>();

            actor.TakeDamage(damage);
            rb.isKinematic = true;
            Destroy(gameObject, 0.5f);
        }
    }
}
