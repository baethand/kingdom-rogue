using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Doors")]
    public GameObject DoorU;
    public GameObject DoorR;
    public GameObject DoorD;
    public GameObject DoorL;
    public GameObject upWay;

    [Header("Light Settings")]
    [SerializeField] private Transform[] placeToSpawnLight;
    [SerializeField] private GameObject[] lightPrefabs;
    [SerializeField] private int countOfLights = 1;

    [Header("Background Settings")]
    [SerializeField] private GameObject[] backPrefabs;

    [Header("Enemy Spawn Settings")]
    [SerializeField] private Transform[] enemySpawnPoints;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int maxEnemiesPerRoom = 2;
    [SerializeField] private int minEnemiesPerRoom = 0;
    [SerializeField] [Range(0f, 1f)] private float enemySpawnChance = 0.7f;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Start() {
        InitializeRoom();
    }

    public void InitializeRoom() {
        foreach (var backPrefab in backPrefabs) {
            backPrefab.SetActive(false);
        }

        generateUpWay();
        UpdateLight();
        UpdateBackground();
        SpawnEnemies(); // Добавляем спавн врагов
    }

    public void UpdateBackground() {
        if (backPrefabs.Length > 0) {
            backPrefabs[Random.Range(0, backPrefabs.Length)].SetActive(true);
        }
    }

    public void UpdateLight() {
        if (placeToSpawnLight.Length > 0) {
            countOfLights = Mathf.Min(countOfLights, placeToSpawnLight.Length);
            
            for (int i = 0; i < countOfLights; i++) {
                if (Random.value > 0.5f) {
                    int randomIndex = Random.Range(0, lightPrefabs.Length);
                    Instantiate(lightPrefabs[randomIndex], placeToSpawnLight[i].position, Quaternion.identity);
                }
            }
        }
    }

    public void generateUpWay() {
        upWay.SetActive(!DoorU.activeSelf);
    }

    public void SpawnEnemies() {
        if (enemySpawnPoints.Length == 0 || enemyPrefabs.Length == 0) {
            return;
        }

        int enemiesToSpawn = Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom);

        List<Transform> shuffledSpawnPoints = new List<Transform>(enemySpawnPoints);
        ShuffleList(shuffledSpawnPoints);
        
        if (Random.value < enemySpawnChance)
            for (int i = 0; i < Mathf.Min(enemiesToSpawn, shuffledSpawnPoints.Count); i++) {
                GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                
                GameObject enemy = Instantiate(
                    enemyPrefab, 
                    shuffledSpawnPoints[i].position, 
                    Quaternion.identity, 
                    transform
                );
                enemy.transform.localScale = new Vector3(4f, 4f, 4f);
                
                spawnedEnemies.Add(enemy);
            }
    }

    // Метод для очистки комнаты (можно вызывать при переходе между комнатами)
    public void ClearRoom() {
        foreach (var enemy in spawnedEnemies) {
            if (enemy != null) {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }

    // Вспомогательный метод для перемешивания списка
    private void ShuffleList<T>(List<T> list) {
        for (int i = 0; i < list.Count; i++) {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}