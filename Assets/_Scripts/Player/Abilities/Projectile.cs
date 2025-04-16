using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed;
    public float damage;
    public float range;
    public LayerMask enemyLayer;

    private Vector2 startPos;
    private Vector2 direction;

    private void Start()
    {
        startPos = transform.position;
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle); 
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
        Debug.DrawRay(transform.position, direction * 0.5f, Color.red, 0.1f); 

        

        if (Vector2.Distance(startPos, transform.position) >= range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Projectile] Hit object: {other.name} on layer {LayerMask.LayerToName(other.gameObject.layer)}");

        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamageFromAbilities(damage);
            }
            Destroy(gameObject);
        }
    }
}
