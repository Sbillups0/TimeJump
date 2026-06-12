using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    private class EnemySnapshot
    {
        public GameObject obj;
        public Vector3 position;
        public Quaternion rotation;
    }

    private List<EnemySnapshot> snapshots = new List<EnemySnapshot>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            snapshots.Add(new EnemySnapshot
            {
                obj = enemy,
                position = enemy.transform.position,
                rotation = enemy.transform.rotation
            });
        }
    }

    public void RespawnAllEnemies()
    {
        foreach (EnemySnapshot snap in snapshots)
        {
            if (snap.obj == null) continue;

            snap.obj.SetActive(true);
            snap.obj.transform.position = snap.position;
            snap.obj.transform.rotation = snap.rotation;

            ArcherEnemy archer = snap.obj.GetComponent<ArcherEnemy>();
            if (archer != null)
                archer.ResetState();

            BirdEnemy bird = snap.obj.GetComponent<BirdEnemy>();
            if (bird != null)
                bird.ResetState();
        }
    }
}
