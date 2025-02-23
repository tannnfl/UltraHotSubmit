using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private bool hasTarget = false;

    public void SetTarget(Vector3 targetPoint, float bulletSpeed)
    {
        // Calculate direction once and keep moving in that direction forever
        direction = (targetPoint - transform.position).normalized;
        speed = bulletSpeed;
        hasTarget = true;
    }

    void Update()
    {
        if (hasTarget)
        {
            //Move the bullet continuously in the same direction
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("collided with:" + collision.gameObject.name);
        //print(collision.gameObject.name);
        if(collision.gameObject.CompareTag("Player") && collision.gameObject.CompareTag("bullet")) Destroy(gameObject);
    }
}
