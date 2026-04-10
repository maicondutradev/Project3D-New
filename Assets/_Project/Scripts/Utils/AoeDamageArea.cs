using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SphereCollider))]
public class AoeDamageArea : MonoBehaviour
{
    public int minDamage = 5;
    public int maxDamage = 10;
    public float damageInterval = 0.5f;
    public LayerMask enemyLayer;

    private float timer;
    private List<Collider> enemiesInside = new List<Collider>();

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= damageInterval)
        {
            DealDamage();
            timer = 0f;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            if (!enemiesInside.Contains(other))
            {
                enemiesInside.Add(other);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (enemiesInside.Contains(other))
        {
            enemiesInside.Remove(other);
        }
    }

    private void DealDamage()
    {
        for (int i = enemiesInside.Count - 1; i >= 0; i--)
        {
            Collider enemyCol = enemiesInside[i];

            if (enemyCol == null || !enemyCol.gameObject.activeInHierarchy)
            {
                enemiesInside.RemoveAt(i);
                continue;
            }

            int damage = Random.Range(minDamage, maxDamage + 1);

            Enemy enemyScript = enemyCol.GetComponent<Enemy>();

            if (enemyScript != null)
            {
                Debug.Log("Meteoro aplicou " + damage + " de dano no alvo: " + enemyCol.gameObject.name);
                enemyScript.TakeDamage(damage);
            }
        }
    }
}