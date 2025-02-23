using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClickToMove : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("Layers")]
    [SerializeField] private LayerMask clickableLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Click Indicator")]
    public GameObject clickEffect;

    [Header("Line Renderer")]
    public LineRenderer lineRenderer;

    float t;
    bool canShoot;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 50;
    private Transform firePoint;
    [SerializeField] private Transform bar;
    [SerializeField] private Transform fill;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private bool isDead;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        firePoint = transform.Find("firePoint").transform;
        SetAlpha(bar.GetComponent<SpriteRenderer>(), 0);
        fill.localScale = new Vector3(0, fill.localScale.y, fill.localScale.z);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if (!isDead)
        {
            //always face mouse
            Ray _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit _hit;
            if (Physics.Raycast(_ray, out _hit))
            {
                // Get the point where the ray hits and face that direction
                Vector3 targetPosition = new Vector3(_hit.point.x, transform.position.y, _hit.point.z);
                transform.LookAt(targetPosition);
            }

            //move
            if (Input.GetMouseButton(0)) // Left Click
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Check if we hit a valid clickable layer
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayer))
                {
                    // Ensure it's not an obstacle
                    if (!Physics.Raycast(ray, Mathf.Infinity, obstacleLayer))
                    {
                        MoveTo(hit.point);
                    }
                }
            }

            //shoot check
            if (t > 0)
            {
                t -= Time.deltaTime;
                canShoot = false;
                //set UI Slider
                fill.localScale = new Vector3(t / 1.5f, fill.localScale.y, fill.localScale.z);
            }
            else
            {
                canShoot = true;
            }
            //shoot
            if (canShoot && Input.GetMouseButton(1)) // Right-click to shoot
            {
                // Raycast to get the clicked position
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Ensure we hit a clickable plane before shooting
                Vector3 targetPoint = new Vector3(ray.GetPoint(100f).x, firePoint.position.y, ray.GetPoint(100f).z);
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayer))
                {
                    // Adjust the target point to stay at the same Y level as the firePoint
                    targetPoint = new Vector3(hit.point.x, firePoint.position.y, hit.point.z);
                }

                // Instantiate a bullet at the firePoint position
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

                // Assign the target position to the bullet
                Bullet bulletScript = bullet.GetComponent<Bullet>();
                if (bulletScript != null)
                {
                    bulletScript.SetTarget(targetPoint, bulletSpeed); // Set target for continuous movement
                }

                // Apply shooting cooldown (optional)
                canShoot = false;
                t = 2f;
            }

            if (canShoot) SetAlpha(bar.GetComponent<SpriteRenderer>(), 0);

            else SetAlpha(bar.GetComponent<SpriteRenderer>(), 1);

            //change time scale based on movementspeed
            Time.timeScale = agent.velocity.magnitude / 20f + 0.1f;
            //animator.SetFloat("Blend", agent.velocity.magnitude / 20f);
        }

        if (isDead) Time.timeScale = 1;

        //die, restart
        //if (animator.GetCurrentAnimatorStateInfo(0).IsName("Restart"))
        //{
        //    print("Animator state is restart");
        //    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //}
            
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            Die();
        }
    }
    public void SetAlpha(SpriteRenderer _sprite, float alpha)
    {
        Color color = _sprite.color; // Get current color
        color.a = Mathf.Clamp01(alpha); // Ensure alpha is between 0 and 1
        _sprite.color = color; // Apply new color with updated alpha
    }
    private void MoveTo(Vector3 target)
    {
        // Draw a Click Indicator
        if (clickEffect != null)
        {
            Instantiate(clickEffect, target, Quaternion.identity);
        }

        // Move with NavMeshAgent
        agent.SetDestination(target);

        // Draw a Line to Target
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, target);
        }
    }

    void Die()
    {
        //restart scene
        //animator.SetTrigger("Die");
        isDead = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }
}