using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Habitat : MonoBehaviour
{
    public List<ZombieDuck> ducks = new List<ZombieDuck>();
    public List<Vegetable> vegetables = new List<Vegetable>();
    public Action restartHabitat;
    private void Awake()
    {
        restartHabitat += RestartHabitat;
    }
    private void RestartHabitat()
    {
        foreach (var d in ducks)
        {
            d.gameObject.SetActive(true);
            d.EndEpisode();
        }
        foreach (var v in vegetables)
        {
            v.gameObject.SetActive(true);
        }
    }
}
