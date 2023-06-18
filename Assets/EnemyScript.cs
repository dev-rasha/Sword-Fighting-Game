using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    public Transform target;

    NavMeshAgent navAgent;

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();    
    }


    void Update()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= navAgent.stoppingDistance)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookDir = Quaternion.LookRotation (new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp (transform.rotation, lookDir, Time.deltaTime * 10f);
        }

        navAgent.SetDestination (target.position);
    }
}
