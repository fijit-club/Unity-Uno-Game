﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Photon.Pun;

public class Control : MonoBehaviour
{
	[SerializeField] private GameNetworkHandler gameNetworkHandler;

	List<PlayerInterface> players = new List<PlayerInterface>();
	public List<Card> deck = new List<Card>();
	public static List<Card> discard = new List<Card>();
	public GameObject playerHand;
	public GameObject contentHolder;
	public Text dialogueText;

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

	void Start () { //this does all the setup. Makes the human and ai players. sets the deck and gets the game ready

		discard.Clear ();
		deck.Clear ();

		players.Add(new HumanPlayer("You"));
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
						deck.Add (card);
						CardData cardData = new CardData();
						cardData.color = card.getColor();
						cardData.cardNumber = card.getNumb();
						cardData.skip = true;
						if (PhotonNetwork.IsMasterClient)
						{
							gameNetworkHandler.cardInfo.mainDeck.Add(cardData);
							cardsPlaced++;
							gameNetworkHandler.gameData.cardIndices.Add(cardsPlaced);
						}
					}
						break;
					case 11:
					{
						var card = new Card(i, returnColorName(j % 4), reverseCardPrefab);
						deck.Add(card);
						CardData cardData = new CardData();
						cardData.color = card.getColor();
						cardData.cardNumber = card.getNumb();
						cardData.reverse = true;
						if (PhotonNetwork.IsMasterClient)
						{
							gameNetworkHandler.cardInfo.mainDeck.Add(cardData);
							cardsPlaced++;
							gameNetworkHandler.gameData.cardIndices.Add(cardsPlaced);
						}
					}
						break;
					case 12:
					{
						var card = new Card(i, returnColorName(j % 4), drawCardPrefab);
						deck.Add(card);
						CardData cardData = new CardData();
						cardData.color = card.getColor();
						cardData.cardNumber = card.getNumb();
						cardData.draw = true;

						if (PhotonNetwork.IsMasterClient)
						{
							gameNetworkHandler.cardInfo.mainDeck.Add(cardData);
							cardsPlaced++;
							gameNetworkHandler.gameData.cardIndices.Add(cardsPlaced);
						}
					}
						break;
					case 13:
					{
						var card = new Card(i, "Black", wildCardPrefab);
						deck.Add (card);
						CardData cardData = new CardData();
						cardData.color = card.getColor();
						cardData.cardNumber = card.getNumb();
						cardData.wild = true;
						if (PhotonNetwork.IsMasterClient)
						{
							gameNetworkHandler.cardInfo.mainDeck.Add(cardData);
							cardsPlaced++;
							gameNetworkHandler.gameData.cardIndices.Add(cardsPlaced);
						}
					}
						break;
					case 14:
					{
						var card = new Card(i, "Black", wildCardPrefab);
						deck.Add (card);
						CardData cardData = new CardData();
						cardData.color = card.getColor();
						cardData.cardNumber = card.getNumb();
						cardData.wild = true;
						if (PhotonNetwork.IsMasterClient)
						{
							gameNetworkHandler.cardInfo.mainDeck.Add(cardData);
							cardsPlaced++;
							gameNetworkHandler.gameData.cardIndices.Add(cardsPlaced);
						}

					}
						break;
					default:
					{
						var card = new Card(i, returnColorName(j % 4), regCardPrefab);
						deck.Add(card);
						CardData cardData = new CardData();
						cardData.color = card.getColor();
						cardData.cardNumber = card.getNumb();
						if (PhotonNetwork.IsMasterClient)
						{
							gameNetworkHandler.cardInfo.mainDeck.Add(cardData);
							cardsPlaced++;
							gameNetworkHandler.gameData.cardIndices.Add(cardsPlaced);
						}
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
		_assignedCards = true;
		Card first = null;
		if (deck[gameNetworkHandler.gameData.cardIndices[0]].getNumb() < 10)
		{
			first = deck[gameNetworkHandler.gameData.cardIndices[0]];
		}
		else
		{
			while (deck[gameNetworkHandler.gameData.cardIndices[0]].getNumb() >= 10)
			{
				deck.Add(deck[gameNetworkHandler.gameData.cardIndices[0]]);
				gameNetworkHandler.gameData.cardIndices.RemoveAt(0);

			}

			first = deck[gameNetworkHandler.gameData.cardIndices[0]];
		}

		discard.Add(first);
		discardPileObj = first.loadCard(0, 0, GameObject.Find("Main").transform);
		gameNetworkHandler.gameData.cardIndices.RemoveAt(0);
		
		for (int i = 0; i < 7; i++)
		{
			gameNetworkHandler.gameData.players[0].playerCardIndices.Add(gameNetworkHandler.gameData.cardIndices[0]);
			players[0].addCards(deck[gameNetworkHandler.gameData.cardIndices[0]]);
			
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
		if (_assignedCards) return;
		
		discard.Add(deck[0]);
		
		for (int i = 1; i < gameNetworkHandler.gameData.players.Count; i++)
		{
			var player = gameNetworkHandler.gameData.players[i];
			if (string.Equals(player.playerName, PhotonNetwork.LocalPlayer.NickName))
			{
				foreach (int playerCardIndex in player.playerCardIndices)
				{
					_assignedCards = true;
					players[0].addCards(deck[playerCardIndex]);
				}
			}
		}
		players [0].turn ();
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
	
	
	void shuffle() { //shuffles the deck by changing cards around
		for (int i = 0; i < deck.Count; i++) {
			Card temp = deck.ElementAt (i);
			int posSwitch = Random.Range (0, deck.Count);
			deck [i] = deck [posSwitch];
			deck [posSwitch] = temp;
		}
	}
	public void recieveText(string text) { //updates the dialogue box
		dialogueText.text += text + "\n";
		contentHolder.GetComponent<RectTransform> ().localPosition = new Vector2 (0, contentHolder.GetComponent<RectTransform> ().sizeDelta.y);
	}
	public void updateDiscPile(Card card) { //this changes the last card played. Top of the discard pile
		discard.Add (card);
		Destroy(discardPileObj);
		discardPileObj=card.loadCard (0, 0, GameObject.Find ("Main").transform);
		discardPileObj.transform.SetSiblingIndex(9);
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

	void Update () 
	{ //this runs the players turns
		// bool win = updateCardsLeft ();
		// if (win)
		// 	return;
		if (players [where] is HumanPlayer) {
			// if (players [where].skipStatus) {
			// 	players [where].skipStatus = false;
			// 	//where += reverse ? -1 : 1;
			// 	if (where >= players.Count)
			// 		where = 0;
			// 	else if (where < 0)
			// 		where = players.Count - 1;
			// 	return;
			// }
			// PlayerInterface temp = players [where];
			// deckGO.GetComponent<Button> ().onClick.RemoveAllListeners ();
			// deckGO.GetComponent<Button> ().onClick.AddListener (() => {
			// 	draw (1, temp);
			// 	((HumanPlayer)temp).recieveDrawOnTurn();
			// });
			//where+=reverse?-1:1;
			// this.enabled = false;
			// players [where+(reverse?1:-1)].turn ();
		}
		// else if (players [where] != null) {
		// 	if (players [where].skipStatus) {
		// 		players [where].skipStatus = false;
		// 		where += reverse ? -1 : 1;
		// 		if (where >= players.Count)
		// 			where = 0;
		// 		else if (where < 0)
		// 			where = players.Count - 1;
		// 		return;
		// 	}
		// 	timer += Time.deltaTime;
		// 	if (timer < 2.2)
		// 		return;
		// 	this.enabled = false;
		// 	timer = 0;
		// 	where+=reverse?-1:1;
		// 	players [where+(reverse?1:-1)].turn ();
		// }
		// else
		// 	where += reverse ? -1 : 1;
		//
		// if (where >= players.Count)
		// 	where = 0;
		// else if (where < 0)
		// 	where = players.Count - 1;
			
	}
	public void startWild(string name) { //this starts the color chooser for the player to choose a color after playing a  wild
		for (int i = 0; i < 4; i++) {
			colors [i].SetActive (true);
			addWildListeners (i, name);
		}
		colorText.SetActive (true);
	}
	public void addWildListeners(int i, string name) { //this is ran from the start wild. It sets each color option as a button and sets the onclick events
		colors [i].GetComponent<Button> ().onClick.AddListener (() => {
			discard[discard.Count-1].changeColor(colorsMatch[i]);
			recieveText(string.Format("{0} played a wild, Color: {1}",name,colorsMatch[i]));

			Destroy(discardPileObj);
			discardPileObj=discard[discard.Count-1].loadCard (0, 0, GameObject.Find ("Main").transform);
			discardPileObj.transform.SetSiblingIndex(9);
			 
			foreach (GameObject x in colors) {
				x.SetActive (false);
				x.GetComponent<Button>().onClick.RemoveAllListeners();
			}
			colorText.SetActive (false);
			this.enabled=true;
		});
	}
	public void draw(int amount, PlayerInterface who) { //gives cards to the players. Players can ask to draw or draw will actrivate from special cards
		if (deck.Count < amount) {
			resetDeck ();
		}
		for (int i = 0; i < amount; i++) {
			who.addCards (deck [0]);
			deck.RemoveAt (0);
		}
	}
	public void resetDeck() { //this resets the deck when all of the cards run out
		print ("reseting");
		foreach (Card x in discard) {
			if (x.getNumb () == 13 || x.getNumb () == 14) {
				x.changeColor ("Black");
			}
			deck.Add (x);
		}
		shuffle ();
		Card last = discard [discard.Count - 1];
		discard.Clear ();
		discard.Add (last);
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
				draw (2, players [who]);
				break;
			case 14:
				draw (4, players [who]);
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
		Start ();
		this.enabled = true;
	}
}
