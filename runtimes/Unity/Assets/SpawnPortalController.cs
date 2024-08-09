using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Aby
{
    public class SpawnPortalController : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private PatrolPath patrolPath;
        [SerializeField] private float spawnInterval = 20f;
        [SerializeField] private float defaultMaxSpeed = 1.25f;
        [SerializeField] private bool isGatewayOpen = true;

        /// <summary>
        /// TODO
        /// </summary>
        [SerializeField] private int maxEntities = 1;

        private int spawnCount;
        private float timer;

        /// <summary>
        /// TODO
        /// </summary>
        void Start()
        {
            timer = spawnInterval;
            spawnCount = 0;
        }

        /// <summary>
        /// TODO
        /// </summary>
        void Update()
        {
            if (isGatewayOpen)
            {
                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    SpawnEntity();
                    timer = spawnInterval;
                }
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        void SpawnEntity()
        {
            if (enemyPrefab != null)
            {
                if (spawnCount < maxEntities)
                {

                    var instance = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
                    if (instance == null)
                    {
                        Debug.LogWarning("Failed to instantiate Enemy prefab!");
                    }

                    var enemy = instance.GetComponent<EnemyController>();
                    if (enemy == null)
                    {
                        var instanceID = instance.GetInstanceID().ToString();
                        Debug.LogWarningFormat("Failed to find Enemy component for prefab instance {0}", instanceID);
                    }

                    instance.layer = LayerMask.NameToLayer("Enemies");
                    enemy.control.maxSpeed = defaultMaxSpeed;
                    enemy.path = patrolPath;
                    enemy.tag = "Enemy";
                    spawnCount++;
                }
            }
        }
    }
}
