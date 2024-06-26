﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Photon.Pun;
using TMPro;
using Random = UnityEngine.Random;

public class Control : MonoBehaviour
{
	public bool myTurn;
	
	[SerializeField] private GameNetworkHandler gameNetworkHandler;

	public List<PlayerInterface> players = new List<PlayerInterface>();
	//public List<int> deck = new List<int>();
	//public List<int> discard = new List<int>();
	public GameObject playerHand;
	public GameObject contentHolder;
	public Text dialogueText;
	public string wildColor;

	public static GameObject discardPileObj;

	public GameObject regCardPrefab;
	public GameObject skipCardPrefab;
	public GameObject reverseCardPrefab;
	public GameObject drawCardPrefab;
	public GameObject wildCardPrefab;

	public GameObject[] colors = new GameObject[4];
	string[] colorsMatch = {"Yellow","Green","Blue","Red"};
	public GameObject[] aiPlayers = new GameObject[5];
	public GameObject colorText;
	public GameObject deckGO;
	public GameObject pauseCan;
	public GameObject endCan;
	bool enabledStat=false;

	int where=0;
	float timer=0;
	bool reverse=false;
	private bool _assignedCards;

	public static int numbOfAI;
	[SerializeField] private TMP_Text currentTurnPlayerText;
	
	void Start () { //this does all the setup. Makes the human and ai players. sets the deck and gets the game ready

		players.Add(new HumanPlayer(PhotonNetwork.LocalPlayer.NickName));
		// for (int i = 0; i < numbOfAI; i++)
		// {
		// 	players.Add(new AiPlayer("AI " + (i + 1)));
		// }

		// for (int i = 0; i < players.Count - 1; i++) {
		// 	aiPlayers [i].SetActive (true);
		// 	aiPlayers [i].transform.Find ("Name").GetComponent<Text> ().text = players [i + 1].getName ();
		// }

		int cardsPlaced = -1;

		for (int i = 0; i < 15; i++) { //setups the deck by making cards
			for (int j = 0; j < 8; j++) {
				switch (i) {
					case 10:
					{
						var card = new Card(i, returnColorName(j % 4), skipCardPrefab);
						CardData cardData = new CardData();
						cardData.cardPrefab = skipCardPrefab;
						cardData.color = card.getColor();
						cardData.cardNumber = card.getNumb();
						cardData.skip = true;
						gameNetworkHandler.cardInfo.mainDeck.Add(cardData);
						cardsPlaced++;
						gameNetworkHandler.gameData.cardIndices.Add(cardsPlaced);
					}
						break;
					case 11:
					{
						var card = new Card(i, returnColorName(j % 4), reverseCardPrefab);
						CardData cardData = new CardData();
						cardData.cardPrefab = reverseCardPrefab;
						cardData.color = card.getColor();
						cardData.cardNumber = card.getNumb();
						cardData.reverse = true;
						gameNetworkHandler.cardInfo.mainDeck.Add(cardData);
						cardsPlaced++;
						gameNetworkHandler.gameData.cardIndices.Add(cardsPlaced);
					}
						break;
					case 12:
					{
						var card = new Card(i, returnColorName(j % 4), drawCardPrefab);
						CardData cardData = new CardData();
						cardData.cardPrefab = drawCardPrefab;
						cardData.color = card.getColor();
						cardData.cardNumber = card.getNumb();
						cardData.draw = true;

						gameNetworkHandler.cardInfo.mainDeck.Add(cardData);
						cardsPlaced++;
						gameNetworkHandler.gameData.cardIndices.Add(cardsPlaced);
					}
						break;
					case 13:
					{
						var card = new Card(i, "Black", wildCardPrefab);
						CardData cardData = new CardData();
						cardData.color = card.getColor();
						cardData.cardPrefab = wildCardPrefab;
						cardData.cardNumber = card.getNumb();
						cardData.wild = true;
						gameNetworkHandler.cardInfo.mainDeck.Add(cardData);
						cardsPlaced++;
						gameNetworkHandler.gameData.cardIndices.Add(cardsPlaced);
					}
						break;
					case 14:
					{
						var card = new Card(i, "Black", wildCardPrefab);
						CardData cardData = new CardData();
						cardData.color = card.getColor();
						cardData.cardPrefab = wildCardPrefab;
						cardData.cardNumber = card.getNumb();
						cardData.wild = true;
						gameNetworkHandler.cardInfo.mainDeck.Add(cardData);
						cardsPlaced++;
						gameNetworkHandler.gameData.cardIndices.Add(cardsPlaced);

					}
						break;
					default:
					{
						var card = new Card(i, returnColorName(j % 4), regCardPrefab);
						CardData cardData = new CardData();
						cardData.cardPrefab = regCardPrefab;
						cardData.color = card.getColor();
						cardData.cardNumber = card.getNumb();
						gameNetworkHandler.cardInfo.mainDeck.Add(cardData);
						cardsPlaced++;
						gameNetworkHandler.gameData.cardIndices.Add(cardsPlaced);
					}
						break;
				}

				if ((i == 0 || i>=13) && j >= 3)
					break;
			}
		}
		//shuffle ();
		
		if (!PhotonNetwork.IsMasterClient) return;
		gameNetworkHandler.gameData.cardIndices.Shuffle();
	}

	public void AssignCards()
	{
		if (!PhotonNetwork.IsMasterClient) return;
		gameNetworkHandler.gameData.currentTurn = PhotonNetwork.LocalPlayer.NickName;
		myTurn = true;
		_assignedCards = true;
		Card first = null;
		if (gameNetworkHandler.cardInfo.mainDeck[gameNetworkHandler.gameData.cardIndices[0]].cardNumber < 10)
		{
			first = new Card(
				gameNetworkHandler.cardInfo.mainDeck[gameNetworkHandler.gameData.cardIndices[0]].cardNumber,
				gameNetworkHandler.cardInfo.mainDeck[gameNetworkHandler.gameData.cardIndices[0]].color, regCardPrefab);
			
			gameNetworkHandler.gameData.discardedCardIndices.Add(gameNetworkHandler.gameData.cardIndices[0]);
			gameNetworkHandler.gameData.cardIndices.RemoveAt(0);
		}
		else
		{
			while (gameNetworkHandler.cardInfo.mainDeck[gameNetworkHandler.gameData.cardIndices[0]].cardNumber >= 10)
			{
				gameNetworkHandler.gameData.cardIndices.Add(gameNetworkHandler.gameData.cardIndices[0]);
				gameNetworkHandler.gameData.cardIndices.RemoveAt(0);

			}
			
			gameNetworkHandler.gameData.discardedCardIndices.Add(gameNetworkHandler.gameData.cardIndices[0]);
			var topCard = gameNetworkHandler.cardInfo.mainDeck[gameNetworkHandler.gameData.discardedCardIndices[0]];
			first = new Card(topCard.cardNumber, topCard.color, topCard.cardPrefab);
		}

		//gameNetworkHandler.gameData.discardedCardIndices.Add(gameNetworkHandler.gameData.cardIndices[0]);
		discardPileObj = first.loadCard(0, 0, GameObject.Find("Main").transform);
		gameNetworkHandler.gameData.cardIndices.RemoveAt(0);
		
		for (int i = 0; i < 7; i++)
		{
			gameNetworkHandler.gameData.players[0].playerCardIndices.Add(gameNetworkHandler.gameData.cardIndices[0]);
			//var cardData = gameNetworkHandler.cardInfo.mainDeck[gameNetworkHandler.gameData.cardIndices[0]];
			//Card playerCard = new Card(cardData.cardNumber,
			//	cardData.color, cardData.cardPrefab);
			//players[0].addCards(playerCard);
			
			gameNetworkHandler.gameData.cardIndices.RemoveAt(0);
		}

		for (int i = 1; i < gameNetworkHandler.gameData.players.Count; i++)
		{
			var player = gameNetworkHandler.gameData.players[i];
			for (int j = 0; j < 7; j++)
			{
				player.playerCardIndices.Add(gameNetworkHandler.gameData.cardIndices[0]);
				gameNetworkHandler.gameData.cardIndices.RemoveAt(0);
			}
		}

		players [0].turn ();
		PhotonNetwork.CurrentRoom.SetCustomProperties(gameNetworkHandler.GetJSONGameData());
	}

	public void AssignCardsOnClients()
	{
		currentTurnPlayerText.text = gameNetworkHandler.gameData.currentTurn + "'s turn.";

		if (_assignedCards || PhotonNetwork.IsMasterClient) return;

		print(gameNetworkHandler.cardInfo.mainDeck.Count);
		print(gameNetworkHandler.gameData.discardedCardIndices.Count);
		if (gameNetworkHandler.gameData.discardedCardIndices.Count > 0)
		{
			var cardData = gameNetworkHandler.cardInfo.mainDeck[gameNetworkHandler.gameData.discardedCardIndices[0]];
			Card first = new Card(cardData.cardNumber, cardData.color, cardData.cardPrefab);
			first.loadCard(0, 0, GameObject.Find("Main").transform);

			for (int i = 1; i < gameNetworkHandler.gameData.players.Count; i++)
			{
				var player = gameNetworkHandler.gameData.players[i];
				if (string.Equals(player.playerName, PhotonNetwork.LocalPlayer.NickName))
				{
					foreach (int playerCardIndex in player.playerCardIndices)
					{
						_assignedCards = true;

						// var cardDataPlayer = gameNetworkHandler.cardInfo.mainDeck[playerCardIndex];
						// Card card = new Card(cardDataPlayer.cardNumber, cardDataPlayer.color,
						// 	cardDataPlayer.cardPrefab);
						//players[0].addCards(card);
					}
				}
			}
		}

		players[0].turn();
	}

	string returnColorName (int numb) { //returns a color based on a number, used in setup
		switch(numb) {
		case 0: 
			return "Green";
		case 1:
			return "Blue";
		case 2: 
			return "Red";
		case 3: 
			return "Yellow";
		}
		return "";
	}
	
	public void recieveText(string text, bool sendToOthers = true) { //updates the dialogue box
		dialogueText.text += text + "\n";
		contentHolder.GetComponent<RectTransform> ().localPosition = new Vector2 (0, contentHolder.GetComponent<RectTransform> ().sizeDelta.y);
		if (sendToOthers)
			FindObjectOfType<PhotonView>().RPC("SendGameLog", RpcTarget.Others, text);
	}

	public void updateDiscPile(int cardIndex) { //this changes the last card played. Top of the discard pile
		gameNetworkHandler.gameData.discardedCardIndices.Add (cardIndex);
		Destroy(discardPileObj);

		var cardData = gameNetworkHandler.cardInfo.mainDeck[cardIndex];
		Card card = new Card(cardData.cardNumber, cardData.color, cardData.cardPrefab);
		
		discardPileObj=card.loadCard (0, 0, GameObject.Find ("Main").transform);
		players[0].turn();
		//discardPileObj.transform.SetSiblingIndex(9);
	}
	public bool updateCardsLeft() { //this updates the number below each ai, so the player knows how many cards they have left
		for (int i = 0; i < players.Count - 1; i++) {
			int temp = players [i + 1].getCardsLeft ();
			aiPlayers [i].transform.Find ("CardsLeft").GetComponent<Text> ().text = temp.ToString();
		}
		foreach (PlayerInterface i in players) {
			if (i.getCardsLeft()==0) {
				//this.enabled = false;
				//recieveText (string.Format ("{0} won!", i.getName()));
				//endCan.SetActive (true);
				//endCan.transform.Find ("WinnerTxt").gameObject.GetComponent<Text> ().text = string.Format ("{0} Won!", i.getName ());
				return true;
			}
		}
		return false;
	}
	
	public void startWild(string name, int cardIndex, int specNumb) { //this starts the color chooser for the player to choose a color after playing a  wild
		for (int i = 0; i < 4; i++) {
			colors [i].SetActive (true);
			addWildListeners (i, name, cardIndex, specNumb);
		}
		colorText.SetActive (true);
	}
	public void addWildListeners(int i, string name, int cardIndex, int specNumb) { //this is ran from the start wild. It sets each color option as a button and sets the onclick events
		colors [i].GetComponent<Button> ().onClick.AddListener (() => {
			var cardData = gameNetworkHandler.cardInfo.mainDeck[gameNetworkHandler.gameData.discardedCardIndices.Last()];
			wildColor = colorsMatch[i];
		
			Destroy(discardPileObj);
			Card card = new Card(cardData.cardNumber, cardData.color, cardData.cardPrefab);
			discardPileObj=card.loadCard (0, 0, GameObject.Find ("Main").transform);
			 
			foreach (GameObject x in colors) {
				x.SetActive (false);
				x.GetComponent<Button>().onClick.RemoveAllListeners();
			}
			colorText.SetActive (false);
			this.enabled=true;
			if (specNumb == 13)
			{
				FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "color",
					wildColor, cardIndex);
				recieveText(string.Format("{0} played a wild, Color: {1}",PhotonNetwork.LocalPlayer.NickName,colorsMatch[i]));
			}
			else if (specNumb == 14)
			{
				FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "draw4",
					wildColor, cardIndex);

				int playerIndex = gameNetworkHandler.gameData.currentTurnIndex;

				if (playerIndex < PhotonNetwork.PlayerList.Length - 1)
					playerIndex++;
				else
					playerIndex = 0;

				for (int j = 0; j < 4; j++)
				{
					gameNetworkHandler.gameData.players[playerIndex].playerCardIndices
						.Add(gameNetworkHandler.gameData.cardIndices[0]);
					gameNetworkHandler.gameData.cardIndices.RemoveAt(0);
				}

				updateDiscPile(cardIndex);
				
				recieveText(string.Format("{0} played a wild draw 4, Color: {1}",PhotonNetwork.LocalPlayer.NickName,colorsMatch[i]));
			}

			foreach (var player in gameNetworkHandler.gameData.players)
			{
				print(player.playerCardIndices.Count);
			}
			players[0].NextPlayersTurn(gameNetworkHandler, this);
			foreach (var player in gameNetworkHandler.gameData.players)
			{
				print(player.playerCardIndices.Count);
			}
			FindObjectOfType<Control>().players[0].turn();
			foreach (var player in gameNetworkHandler.gameData.players)
			{
				print(player.playerCardIndices.Count);
			}

		});
	}

	public void PlayerDraw()
	{
		if (myTurn)
		{
			myTurn = false;
			recieveText ($"{PhotonNetwork.LocalPlayer.NickName} drew a card");
			players[0].NextPlayersTurn(gameNetworkHandler, GetComponent<Control>());
			draw(1);
		}
	}
	
	public void draw(int amount) { //gives cards to the players. Players can ask to draw or draw will actrivate from special cards
		if (gameNetworkHandler.gameData.cardIndices.Count < amount) {
			resetDeck ();
		}
		for (int i = 0; i < amount; i++) {
			// var cardData = gameNetworkHandler.cardInfo.mainDeck[gameNetworkHandler.gameData.cardIndices[0]];
			// Card card = new Card(cardData.cardNumber, cardData.color, cardData.cardPrefab);
			//players[0].addCards (card);
			foreach (var player in gameNetworkHandler.gameData.players)
			{
				if (string.Equals(player.playerName, PhotonNetwork.LocalPlayer.NickName))
				{
					player.playerCardIndices.Add(gameNetworkHandler.gameData.cardIndices[0]);
				}
			}
			gameNetworkHandler.gameData.cardIndices.RemoveAt (0);
		}
		players[0].turn();
		PhotonNetwork.CurrentRoom.SetCustomProperties(gameNetworkHandler.GetJSONGameData());
	}
	public void resetDeck() { //this resets the deck when all of the cards run out
		// print ("reseting");
		// foreach (int x in discard) {
		// 	var cardData = gameNetworkHandler.cardInfo.mainDeck[x];
		// 	if (cardData.cardNumber == 13 || cardData.cardNumber == 14) {
		// 		x.changeColor ("Black");
		// 	}
		// 	deck.Add (x);
		//}
		// shuffle ();
		// Card last = discard [discard.Count - 1];
		// discard.Clear ();
		// discard.Add (last);
	}
	public void specialCardPlay(PlayerInterface player, int cardNumb) { //takes care of all special cards played
		int who = players.FindIndex (e=>e.Equals(player)) + (reverse?-1:1);
		if (who >= players.Count)
			who = 0;
		else if (who < 0)
			who = players.Count - 1;
		
		switch (cardNumb) {
			case 10:				
				players [who].skipStatus = true;
				break;
			case 11:
				reverse = !reverse;
				int difference = 0;
				if (reverse) {
					difference = who - 2;
					if (difference >= 0)
						where = difference;
					else {
						difference = Mathf.Abs (difference);
						where = players.Count - difference;
					}
				}
				else {
					difference = who + 2;
					if (difference > players.Count - 1)
						where = difference - players.Count;
					else
						where = difference;
				}
				break;
			case 12:
				draw (2);
				break;
			case 14:
				draw (4);
				break;
		}
		if(cardNumb!=14)
			this.enabled = true;
	}
	public void pause(bool turnOnOff) { //turns the pause canvas on/off
		if (turnOnOff) {
			pauseCan.SetActive (true);
			enabledStat = this.enabled;
			this.enabled = false;
		}
		else {
			pauseCan.SetActive (false);
			this.enabled = enabledStat;
		}
	}
	public void returnHome() { //loads the home screen
		UnityEngine.SceneManagement.SceneManager.LoadScene ("Start");
	}
	public void exit() { //quits the app
		Application.Quit ();
	}
	public void playAgain() { //resets everything after a game has been played
		this.enabled = false;
		reverse = false;
		players.Clear ();
		dialogueText.text = "";
		contentHolder.GetComponent<RectTransform> ().localPosition = new Vector2 (0, 0);
		endCan.SetActive (false);
		for (int i = playerHand.transform.childCount - 1; i >= 0; i--) {
			Destroy (playerHand.transform.GetChild (i).gameObject);
		}
		Destroy(discardPileObj);
		where = 0;
		//Start ();
		this.enabled = true;
	}
}
