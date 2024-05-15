using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Player
{
    public string playerName;
    public string avatar;
    public int score = 0;
    public List<int> cards = new List<int>();
}

[Serializable]
public class CardData
{
    public int cardNumber;
    public string color;
    public bool wild;
    public bool reverse;
    public bool drawCard;
}

[Serializable]
public class GameData
{
    public List<Player> players = new List<Player>();
}
