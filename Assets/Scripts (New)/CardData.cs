using System;
using UnityEngine;

[Serializable]
public class CardData
{
    public int cardNumber;
    public string color;
    public bool wild;
    public bool reverse;
    public bool draw;
    public bool skip;
    public GameObject cardPrefab;
}