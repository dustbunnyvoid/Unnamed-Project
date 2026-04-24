using UnityEngine;
using System.Collections.Generic;

public class RoomTrigger : MonoBehaviour
{
    [Header("Enemies")]
    public List<GameObject> enemiesToSpawn;
    public List<Transform> spawnPoints;

    [Header("Door")]
    public GameObject door;

    bool _activated;
    int _remainingEnemies;

    void OnTriggerEnter(Collider other)
    {
        if (_activated || !other.CompareTag("Player")) return;
        _activated = true;

        if (door != null) door.SetActive(true);
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        _remainingEnemies = enemiesToSpawn.Count;

        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            Transform spawnPoint = (spawnPoints != null && i < spawnPoints.Count)
                ? spawnPoints[i]
                : transform;

            var go = Instantiate(enemiesToSpawn[i], spawnPoint.position, spawnPoint.rotation);
            var health = go.GetComponent<HealthComponent>();
            if (health != null)
                health.OnDeath += OnEnemyDied;
            else
                _remainingEnemies--;
        }

        if (_remainingEnemies <= 0)
            OpenDoor();
    }

    void OnEnemyDied()
    {
        _remainingEnemies--;
        if (_remainingEnemies <= 0)
            OpenDoor();
    }

    void OpenDoor()
    {
        if (door != null) door.SetActive(false);
    }
}
