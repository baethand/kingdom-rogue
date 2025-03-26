using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public GameObject DoorU;
    public GameObject DoorR;
    public GameObject DoorD;
    public GameObject DoorL;
    public GameObject upWay;

    [SerializeField] private Transform[] placeToSpawnLight;
    [SerializeField] private GameObject[] lightPrefabs;
    [SerializeField] private int countOfLights = 1;

    [SerializeField] private GameObject[] backPrefabs;


    private void Start() {

      foreach (var backPrefab in backPrefabs)
      {
          backPrefab.SetActive(false);
      }

      generateUpWay();

      if (placeToSpawnLight.Length < countOfLights){
        countOfLights = placeToSpawnLight.Length;
      }
      
      updateLight();
      updateBackground();
    }

    public void updateBackground() {
        if (backPrefabs.Length > 0) {
          backPrefabs[Random.Range(0, backPrefabs.Length)].SetActive(true);
      }
    }

    public void updateLight() {
      if (placeToSpawnLight.Length > 0) {
        for (int i = 0; i < countOfLights; i++) {
          
          if (Random.value > 0.5f) 
                {
                    int randomIndex = Random.Range(0, lightPrefabs.Length);
                    Instantiate(lightPrefabs[randomIndex], placeToSpawnLight[i].position, Quaternion.identity);
                }
          
          }
      }
    }

    public void generateUpWay() {
      if (DoorU.activeSelf == false) {
        upWay.SetActive(true);
      }
      else {
        upWay.SetActive(false);
      }
    }
}