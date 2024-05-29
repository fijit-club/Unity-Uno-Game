using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Player
{
    public string playerName;
    public string avatar;
    public int score = 0;
    public List<int> playerCardIndices = new List<int>();
    public bool won = false;
}

[Serializable]
public class CardInfo
{
    public List<CardData> mainDeck = new List<CardData>();
}

[Serializable]
public class GameData
{
    public List<Player> players = new List<Player>();
    public List<int> cardIndices = new List<int>();
    public string currentTurn;
    public int currentTurnIndex;
    public List<int> discardedCardIndices;
    public bool reversed;
}
