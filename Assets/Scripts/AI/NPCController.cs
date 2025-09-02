using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class NPCController : MonoBehaviour
{
    [Header("Patrol Settings")]
    public List<Transform> patrolPoints = new List<Transform>();
    public float waitTime = 2f;
    public float detectionRange = 5f;
    public float fieldOfView = 60f;
    
    [Header("Detection")]
    public LayerMask playerLayer;
    public Transform playerTarget;
    
    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool hasDetectedPlayer = false;
    
    public System.Action<bool> OnPlayerDetected;
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (patrolPoints.Count > 0)
        {
            agent.SetDestination(patrolPoints[0].position);
        }
        
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTarget = player.transform;
        }
    }
    
    private void Update()
    {
        if (GameManager.Instance.currentState != GameState.Playing) return;
        
        Patrol();
        DetectPlayer();
    }
    
    private void Patrol()
    {
        if (patrolPoints.Count == 0) return;
        
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = waitTime;
            }
            else
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0f)
                {
                    isWaiting = false;
                    currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                }
            }
        }
    }
    
    private void DetectPlayer()
    {
        if (playerTarget == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        
        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            
            if (angle < fieldOfView / 2f)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRange, playerLayer))
                {
                    if (hit.transform == playerTarget)
                    {
                        if (!hasDetectedPlayer)
                        {
                            hasDetectedPlayer = true;
                            OnPlayerDetected?.Invoke(true);
                            GameManager.Instance.AddSuspicion(20f);
                        }
                    }
                }
            }
        }
        else if (hasDetectedPlayer)
        {
            hasDetectedPlayer = false;
            OnPlayerDetected?.Invoke(false);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Gizmos detection
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Gizmos field of view
        Vector3 leftBoundary = Quaternion.Euler(0, -fieldOfView / 2f, 0) * transform.forward * detectionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, fieldOfView / 2f, 0) * transform.forward * detectionRange;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
}
