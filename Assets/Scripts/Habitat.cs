using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Habitat : MonoBehaviour
{
    public List<ZombieDuck> ducks = new List<ZombieDuck>();
    public List<Vegetable> vegetables = new List<Vegetable>();
    public Action restartHabitat;
    public GameObject food;
    public int foodCount;
    public float localPosMinX;
    public float localPosMinY;
    public float localPosMaxX;
    public float localPosMaxY;
    private void Awake()
    {
        restartHabitat += RestartHabitat;

        for (int i = 0; i < foodCount;  i++)
        {
            var duckFood = Instantiate(food);
            duckFood.transform.parent = transform;
            vegetables.Add(duckFood.GetComponent<Vegetable>());
            vegetables[i].gameObject.transform.localPosition = new Vector3(Random.Range(localPosMinX, localPosMaxX),
                Random.Range(localPosMinY, localPosMaxY), 0);
        }
    }
    private void RestartHabitat()
    {
        for (int i = 0; i < foodCount; i++)
        {
            vegetables[i].gameObject.transform.localPosition = new Vector3(Random.Range(localPosMinX, localPosMaxX),
                Random.Range(localPosMinY, localPosMaxY), 0);
            vegetables[i].gameObject.SetActive(true);       
        }
        
        foreach (var d in ducks)
        {
            d.gameObject.SetActive(true);
            d.EndEpisode();
        }
        
    }
}
