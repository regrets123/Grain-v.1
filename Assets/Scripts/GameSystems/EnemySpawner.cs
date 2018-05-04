using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//By Andreas Nilsson

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    List<GameObject> enemiesToSpawn;
    [SerializeField]
    List<Transform> spawnPositions;
    [SerializeField]
    float respawnTime;

    //When the player character enters the collider area one or several enemies is spawned in specific locations
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Spawner();

            GetComponent<Collider>().enabled = false;

            StartCoroutine("Respawn");
        }
    }

    //Spawns enemy/enemies
    void Spawner()
    {
        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            Instantiate(enemiesToSpawn[i], spawnPositions[i].position, spawnPositions[i].rotation);
        }
    }

    //Turns of the Collider upon activation and sets it back on after a respawn timer
    private IEnumerator Respawn()
    {
        GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(respawnTime);

        GetComponent<Collider>().enabled = true;
    }
}
