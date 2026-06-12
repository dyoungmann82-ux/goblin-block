using System.Collections.Generic;
using UnityEngine;

// GOBLIN BLOCK — escalating wave spawner. Put on an empty GameObject.
// Assign the zombie prefab and spawn points at the intersections/alleys.
public class WaveSpawner : MonoBehaviour
{
    public GameObject goblinPrefab;
    public Transform[] spawnPoints;      // place at the four corners + alley mouths
    public float minSpawnDistance = 24f; // prefer spawns away from the player
    public Transform player;

    [Header("Pacing")]
    public int baseCount = 3;
    public int perWave = 2;
    public float intermission = 3.5f;

    static int alive;
    int wave;
    float spawnTimer, restTimer;
    int toSpawn;

    void Start()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        NextWave();
    }

    void Update()
    {
        if (toSpawn > 0)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnOne();
                toSpawn--;
                spawnTimer = Mathf.Max(0.5f, 1.4f - wave * 0.08f);
            }
        }
        else if (alive <= 0)
        {
            restTimer -= Time.deltaTime;
            if (restTimer <= 0f) NextWave();
        }
    }

    void NextWave()
    {
        wave++;
        toSpawn = baseCount + wave * perWave;
        spawnTimer = 0.5f;
        restTimer = intermission;
        Debug.Log($"WAVE {wave} — {toSpawn} goblins");
    }

    void SpawnOne()
    {
        Transform best = PickSpawn();
        var go = Instantiate(goblinPrefab, best.position, best.rotation);

        // scale difficulty with the wave
        var ai = go.GetComponent<GoblinAI>();
        if (ai)
        {
            ai.maxHealth += wave * 4f;
            ai.kind = Random.value < 0.55f ? GoblinAI.Kind.Shambler : GoblinAI.Kind.Runner;
            ai.attackDamage += wave;
        }
        alive++;
    }

    Transform PickSpawn()
    {
        List<Transform> far = new();
        foreach (var s in spawnPoints)
            if (player == null || Vector3.Distance(s.position, player.position) > minSpawnDistance)
                far.Add(s);
        var pool = far.Count > 0 ? far : new List<Transform>(spawnPoints);
        return pool[Random.Range(0, pool.Count)];
    }

    public static void NotifyDeath(GoblinAI g) { alive = Mathf.Max(0, alive - 1); }
}
