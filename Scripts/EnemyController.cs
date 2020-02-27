using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{

    public float lookRadius = 10f;
    Transform target;
    NavMeshAgent agent;
    public float speed = 0.3f;

    public Transform[] waypoints;
    public int cur = 0;


    // Start is called before the first frame update
    void Start()
    {
        target = PlayerManager.instance.player.transform;
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(target.position, transform.position);
        if(distance <= lookRadius)
        {
            agent.SetDestination(target.position);
        }
        else if (transform.position != waypoints[cur].position)
        {
            agent.SetDestination(waypoints[cur].position);
        }

        if (Input.GetKey(KeyCode.R))
            SceneManager.LoadScene("Menu");

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Path")
        {
            cur++;
            if (cur >= waypoints.Length) cur = 0;
        }
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }
}
