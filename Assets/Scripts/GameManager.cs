using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<City> Cities { get; } = new List<City>();
    public List<IronDome> Domes { get; } = new List<IronDome>();
    public List<EnemyMissile> ActiveMissiles { get; } = new List<EnemyMissile>();

    public int Score { get; private set; }
    public int TotalHits { get; private set; }
    public bool IsGameOver { get; private set; }

    float nextSpawnTime;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ScheduleNextSpawn();
    }

    void Update()
    {
        if (IsGameOver) return;

        ActiveMissiles.RemoveAll(m => m == null);

        if (Time.time >= nextSpawnTime)
        {
            SpawnMissile();
            ScheduleNextSpawn();
        }
    }

    void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + Random.Range(GameConfig.MinSpawnInterval, GameConfig.MaxSpawnInterval);
    }

    void SpawnMissile()
    {
        if (Cities.Count == 0) return;

        City target = Cities[Random.Range(0, Cities.Count)];
        float edgeX = Random.Range(-GameConfig.MapHalfWidth * 0.8f, GameConfig.MapHalfWidth * 0.8f);
        Vector3 start = new Vector3(edgeX, GameConfig.SpawnHeight, GameConfig.SpawnEdgeZ);

        var go = new GameObject("EnemyMissile");
        var missile = go.AddComponent<EnemyMissile>();
        missile.Init(start, target);
        ActiveMissiles.Add(missile);
    }

    public void OnCityHit()
    {
        TotalHits++;
        if (TotalHits >= GameConfig.MaxCityHits)
            IsGameOver = true;
    }

    public void OnMissileIntercepted()
    {
        Score += 10;
    }

    public void RegisterCity(City c) => Cities.Add(c);
    public void RegisterDome(IronDome d) => Domes.Add(d);

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
