using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    //private Enemy_Master enemyMaster; 
    private Transform myTransform;
    public Transform head;
    public LayerMask playerLayer;
    public LayerMask sightLayer;
    private float checkRate;
    private float nextCheck;
    private float detectRadius = 80f;
    private RaycastHit hit;

    void Start()
    {
        if (head == null)
        {
            head = myTransform; 
        }

        checkRate = Random.Range(.08f, 1.2f);

    }
   
    // Update is called once per frame
    void Update()
    {
        CarryOutDetection();
    }

    void CarryOutDetection()
    {
        if(Time.time > nextCheck)
        {
            nextCheck = Time.time + checkRate;

            Collider[] colliders = Physics.OverlapSphere(myTransform.position, detectRadius, playerLayer);

            if(colliders.Length > 0)
            {
                foreach(Collider potentialTargetCollider in colliders)
                {
                    if(potentialTargetCollider.tag == "Player")
                    {
                        if(CanTargetBeSeen(potentialTargetCollider.transform))
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    public bool CanTargetBeSeen(Transform potentialTarget)
    {
        if (Physics.Linecast(head.position, potentialTarget.position, out hit, sightLayer))
        {
            if (hit.transform == potentialTarget)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else return false;
    }

    void DisableThis()
    {
        this.enabled = false;
    }
}
