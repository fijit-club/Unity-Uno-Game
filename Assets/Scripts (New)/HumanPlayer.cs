using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class HumanPlayer : MonoBehaviour, PlayerInterface {

	bool skip=false;
	bool drew =false;
	bool playedWild;

	
	string name;
	List<Card> handList = new List<Card> ();
	private GameNetworkHandler _gameNet;
	private List<CardData> playerCards = new List<CardData>();
	private int _test;

	public HumanPlayer(string name) { //initalizes
		this.name = name;
	}

	public bool skipStatus { //returns if the player should be skipped
		get{return skip; }
		set{ skip = value; }
	}

	public void turn() { //does the turn
		playedWild = false;
		drew = false;
		//int i = 0;
		_gameNet = FindObjectOfType<GameNetworkHandler>();
		
		Player thisPlayer = null;

		foreach (var player in _gameNet.gameData.players)
		{
			if (player.playerName == PhotonNetwork.LocalPlayer.NickName)
			{
				thisPlayer = player;
			}
		}

		// GameObject temp = null;
		// foreach (Card x in handList)
		// {
		// 	if (GameObject.Find("Control").GetComponent<Control>().playerHand.transform.childCount >
		// 	    i) //is the card already there or does it need to be loaded
		// 		temp = GameObject.Find("Control").GetComponent<Control>().playerHand.transform.GetChild(i).gameObject;
		// 	else
		// 		temp = x.loadCard(GameObject.Find("Control").GetComponent<Control>().playerHand.transform);
		// 	i++;
		// 	foreach (int playerCardIndex in thisPlayer.playerCardIndices)
		// 	{
		// 		var card = _gameNet.cardInfo.mainDeck[playerCardIndex];
		// 		var topCardInDiscardDeck = _gameNet.cardInfo.mainDeck[_gameNet.gameData.discardedCardIndices[0]];
		// 		print(card.cardNumber);
		// 		print(topCardInDiscardDeck.cardNumber);
		// 		if (card.cardNumber == topCardInDiscardDeck.cardNumber ||
		// 		    string.Equals(card.color, topCardInDiscardDeck.color) || card.cardNumber >= 13)
		// 		{
		// 			setListeners(card.cardNumber, temp);
		// 			temp.transform.GetChild (3).gameObject.SetActive (false);
		// 		}
		// 		else
		// 		{
		// 			temp.transform.GetChild (3).gameObject.SetActive (true);
		// 		}
		// 	
		// 	}
		// }

		GameObject temp = null;
		int i = 0;
		// foreach (var handCard in handList)
		// {
		// 	if (GameObject.Find("Control").GetComponent<Control>().playerHand.transform.childCount >
		// 	    i) //is the card already there or does it need to be loaded
		// 		temp = GameObject.Find("Control").GetComponent<Control>().playerHand.transform.GetChild(i).gameObject;
		// 	else
		// 		temp = handCard.loadCard(GameObject.Find("Control").GetComponent<Control>().playerHand.transform);
		// 	i++;
		// }
		
		
		foreach (int playerCardIndex in thisPlayer.playerCardIndices)
		{
			if (GameObject.Find("Control").GetComponent<Control>().playerHand.transform.childCount >
			    i) //is the card already there or does it need to be loaded
				temp = GameObject.Find("Control").GetComponent<Control>().playerHand.transform.GetChild(i).gameObject;
			else
				temp = handList[i].loadCard(GameObject.Find("Control").GetComponent<Control>().playerHand.transform);
			
			var card = _gameNet.cardInfo.mainDeck[playerCardIndex];
			var topCardInDiscardDeck = _gameNet.cardInfo.mainDeck[_gameNet.gameData.discardedCardIndices.Last()];

			//print(card.cardNumber + " " + (card.cardNumber == topCardInDiscardDeck.cardNumber));
			//print(card.cardNumber + " " + string.Equals(card.color, topCardInDiscardDeck.color));
			if (card.cardNumber == topCardInDiscardDeck.cardNumber ||
			    string.Equals(card.color, topCardInDiscardDeck.color) || card.cardNumber >= 13 || string.Equals(card.color, FindObjectOfType<Control>().wildColor))
			{
				print("CAN BE PRESSED: " + i);
				setListeners(playerCardIndex, temp, i);
				temp.transform.GetChild (3).gameObject.SetActive (false);
			}
			else
			{
				temp.transform.GetChild (3).gameObject.SetActive (true);
			}
			i++;
		}

		// foreach (Card x in handList) { //foreach card in hand
		// 	
		// 	GameObject temp = null;
		// 	if (GameObject.Find ("Control").GetComponent<Control> ().playerHand.transform.childCount > i) //is the card already there or does it need to be loaded
		// 		temp = GameObject.Find ("Control").GetComponent<Control> ().playerHand.transform.GetChild (i).gameObject;			
		// 	else 
		// 		temp = x.loadCard (GameObject.Find ("Control").GetComponent<Control> ().playerHand.transform);
		//
		// 	
		// 	if (handList[i].getNumb() ==  Control.discard.Last().getNumb() || string.Equals(handList[i].getColor(),
		// 		    Control.discard.Last().getColor()) || handList [i].getNumb () >= 13) { //if the cards can be played
		// 		setListeners (i, temp);
		// 		temp.transform.GetChild (3).gameObject.SetActive (false);
		// 	}
		// 	else {
		// 		temp.transform.GetChild (3).gameObject.SetActive (true); //otherwise black them out
		// 	}
		// 	i++;
		// }
	}
	public void setListeners(int cardIndex,GameObject temp, int handIndex) { //sets all listeners on the cards
		temp.GetComponent<Button>().onClick.AddListener(() => { PlayTurn(cardIndex, temp, handIndex); });
	}

	private void PlayTurn(int cardIndex, GameObject temp, int handIndex)
	{
		var control = FindObjectOfType<Control>();
		if (control.myTurn)
		{
			var gameNet = FindObjectOfType<GameNetworkHandler>();
			control.myTurn = false;
			playedWild = _gameNet.cardInfo.mainDeck[cardIndex].cardNumber >= 13;

			foreach (var player in gameNet.gameData.players)
			{
				if (string.Equals(player.playerName, PhotonNetwork.LocalPlayer.NickName))
				{
					player.playerCardIndices.RemoveAt(handIndex);
				}
			}
			
			temp.GetComponent<Button>().onClick.RemoveAllListeners();
			Destroy(temp);
			turnEnd(cardIndex, handIndex);
		}
	}

	public void NextPlayersTurn(GameNetworkHandler gameNet, Control control, bool skip = false)
	{
		var currentTurnIndex = gameNet.gameData.currentTurnIndex;
		if (!skip)
			currentTurnIndex = UpdateCurrentIndex(gameNet, currentTurnIndex, 1);
		else
			currentTurnIndex = UpdateCurrentIndex(gameNet, currentTurnIndex, 2);

		gameNet.gameData.currentTurn = gameNet.gameData.players[currentTurnIndex].playerName;
		print(gameNet.gameData.discardedCardIndices.Last());
		turn();
		PhotonNetwork.CurrentRoom.SetCustomProperties(gameNet.GetJSONGameData());
	}

	private static int UpdateCurrentIndex(GameNetworkHandler gameNet, int currentTurnIndex, int amount)
	{
		if (currentTurnIndex < PhotonNetwork.PlayerList.Length - amount)
		{
			currentTurnIndex += amount;
			gameNet.gameData.currentTurnIndex += amount;
		}
		else
		{
			currentTurnIndex = 0;
			gameNet.gameData.currentTurnIndex = 0;
		}

		return currentTurnIndex;
	}

	public void addCards(Card other) { //recieves cards to add to the hand
		handList.Add (other);
	}
	public void recieveDrawOnTurn() { //if the player decides to draw
		handList[handList.Count-1].loadCard (GameObject.Find ("Control").GetComponent<Control> ().playerHand.transform);
		drew = true;
		turnEnd (-1, -1);
	}

	public void turnEnd(int cardIndex, int handIndex) { //ends the player's turn
		Control cont = GameObject.Find("Control").GetComponent<Control>();

		cont.playerHand.GetComponent<RectTransform>().localPosition = new Vector2(0, 0);

		for(int i=cont.playerHand.transform.childCount-1;i>=0;i--) {
			cont.playerHand.transform.GetChild(i).GetComponent<Button>().onClick.RemoveAllListeners();
			cont.playerHand.transform.GetChild (i).GetChild (3).gameObject.SetActive (false);
		}
		if (drew) {
			cont.GetComponent<Control> ().enabled = true;
			cont.recieveText (string.Format ("{0} drew a card", name));
			cont.deckGO.GetComponent<Button> ().onClick.RemoveAllListeners ();
		}
		else {	
			int specNumb = _gameNet.cardInfo.mainDeck[cardIndex].cardNumber;	
			if (playedWild) {
				cont.updateDiscPile (cardIndex);
				handList.RemoveAt (handIndex);	
				cont.startWild (name, cardIndex, specNumb);
				// if (specNumb == 14)
				// 	cont.specialCardPlay (this, 14);
			}
			else {
				if (specNumb < 10)
				{
					cont.recieveText(string.Format("{0} played a {1} {2}", name,
						_gameNet.cardInfo.mainDeck[cardIndex].color, _gameNet.cardInfo.mainDeck[cardIndex].cardNumber));
					cont.enabled = true;

					FindObjectOfType<PhotonView>().RPC("UpdateDiscardRegular", RpcTarget.Others, cardIndex);
				}
				else if (specNumb == 10)
				{
					NextPlayersTurn(_gameNet, cont, true);
					cont.recieveText (string.Format ("{0} played a {1} skip", name, _gameNet.cardInfo.mainDeck[cardIndex].color));
					FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "skip",
						_gameNet.cardInfo.mainDeck[cardIndex].color, cardIndex);
				}
				else if (specNumb == 11) {
					cont.specialCardPlay (this, 11);
					cont.recieveText (string.Format ("{0} played a {1} reverse", name, handList [cardIndex].getColor ()));
					FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "reverse",
						_gameNet.cardInfo.mainDeck[cardIndex].color, cardIndex);
				}
				else if (specNumb == 12) {
					cont.specialCardPlay (this, 12);
					cont.recieveText (string.Format ("{0} played a {1} draw 2", name, handList [cardIndex].getColor ()));
					FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "draw",
						_gameNet.cardInfo.mainDeck[cardIndex].color, cardIndex);
				}
				cont.updateDiscPile (cardIndex);
				handList.RemoveAt(handIndex);
				
				NextPlayersTurn(_gameNet, cont);
			}
		}
	}
	public bool Equals(PlayerInterface other) { //equals function based on name
		return other.getName ().Equals (name);
	}
	public string getName() { //returns the name
		return name;
	}
	public int getCardsLeft() { //gets how many cards are left in the hand
		return handList.Count;
	}
}
