using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    //Navigation agent component
    private NavMeshAgent agent;
    [Header("Navigation Settings")]
    public float speedRun = 20f;
    public float speedSearch = 8f;
    public float speedWander = 10f;

    [Header("Vision Settings")]
    public float viewDistance = 10f; // How far the enemy can see
    public float instantViewDistance = 10f; // How far the enemy can see
    [Range(0, 360)]
    public float viewAngle = 90f; // The enemy's field of view in degrees
    public LayerMask playerLayer; // Set this to the Player layer
    public LayerMask obstacleLayer; // Set this to Walls, Obstacles, etc.

    [Header("Search Mode Setting")]
    private float rotationSpeed = 0f;
    private float rotationDirection = 0f;
    private float rotationChangeTime = 0f;

    [Header("Wander Settings")]
    public float wanderRadius = 10f; // How far enemy can move randomly
    public float waitTimeMin = 1f; // Minimum wait time
    public float waitTimeMax = 3f; // Maximum wait time

    //search for player for 5 sec after lose player's sight
    private bool isSearching = false;
    private float searchTimer = 0f;


    private Transform player;
    bool previousCanSeePlayer;
    bool currentCanSeePlayer;

    void Start()
    {
        //Get self components
        agent = GetComponent<NavMeshAgent>();

        //Get other objects
        player = GameObject.FindGameObjectWithTag("Player").transform; // Make sure your player has the "Player" tag

    }

    void Update()
    {
        // Chase player detector
        // Chase if player is in sight
        currentCanSeePlayer = CanSeePlayer(); // Update the current state

        if (currentCanSeePlayer)
        {
            MoveTowardPlayer(speedRun); 
            isSearching = false;
            searchTimer = 0f;
        }
        else
        {
            if (!isSearching)
            {
                isSearching = true;
                searchTimer = 5f; // Start countdown
            }

            if (searchTimer > 0f)
            {
                searchTimer -= Time.deltaTime;
                MoveTowardPlayer(speedSearch); // Slow search movement, still updating
                RandomRotate();
            }
            else
            {
                isSearching = false;
                RandomRotate();
                StartCoroutine(Wander()); // Start wander behavior
            }
        }

        previousCanSeePlayer = currentCanSeePlayer; // Store previous state
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("bullet"))
        {
            Die();
        }
    }

    //Check if player is in range and not blocked by obstacles
    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if the player is within view distance
        if (distanceToPlayer > viewDistance) return false;
        else if (distanceToPlayer < instantViewDistance) return true;

        // Check if the player is within the enemy's field of view
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > viewAngle / 2) return false;

        // Check if there are obstacles blocking the enemy's view
        if (Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer)) return false;

        return true;
    }

    //Draw see range in editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 fovLine1 = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewDistance;
        Vector3 fovLine2 = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);
    }

    private void MoveTowardPlayer(float speed)
    {
        agent.speed = speed;
        agent.SetDestination(player.position);
    }

    void RandomRotate()
    {
        // If it's time to change direction
        if (Time.time > rotationChangeTime)
        {
            rotationSpeed = Random.Range(0f, 100f); // Random speed (0 = no rotation)
            rotationDirection = Random.Range(-1f, 1f); // -1 (left), 0 (no rotate), 1 (right)
            rotationChangeTime = Time.time + Random.Range(1f, 3f); // Change every 1 to 3 seconds
        }

        // Apply rotation
        transform.Rotate(Vector3.up * rotationSpeed * rotationDirection * Time.deltaTime);
    }

    IEnumerator Wander()
    {
        while (true) // Infinite loop for continuous wandering
        {
            Vector3 newDestination = GetRandomNavMeshPosition();
            agent.SetDestination(newDestination);

            // Wait until the enemy reaches the position
            while (agent.pathPending || agent.remainingDistance > agent.stoppingDistance)
            {
                yield return null;
            }

            // Wait for a random amount of time before moving again
            float waitTime = Random.Range(waitTimeMin, waitTimeMax);
            yield return new WaitForSeconds(waitTime);
        }
    }

    Vector3 GetRandomNavMeshPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return transform.position; // Fallback to current position if no valid point found
    }

    void Die()
    {

        Destroy(gameObject); 
    }
}
