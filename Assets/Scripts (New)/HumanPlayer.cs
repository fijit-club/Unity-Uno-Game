﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
				thisPlayer = player;
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

		// if (thisPlayer != null)
		// {
		// 	handList.Clear();
		// 	foreach (var handCard in handList)
		// 		Destroy(handCard.gameObject);
		// 	
		// 	foreach (int playerCard in thisPlayer.playerCardIndices)
		// 	{
		// 		var cardData = _gameNet.cardInfo.mainDeck[playerCard];
		// 		Card card = new Card(cardData.cardNumber, cardData.color, cardData.cardPrefab);
		// 		handList.Add(card);
		// 	}
		// }

		var control = GameObject.FindObjectOfType<Control>();
		var playerHand = control.playerHand;

		for (int j = playerHand.transform.childCount - 1; j >= 0; j--)
		{
			Destroy(playerHand.transform.GetChild(j).gameObject);
		}
		print("TURN");
		foreach (int playerCardIndex in thisPlayer.playerCardIndices)
		{
			// temp = handList[i].loadCard(GameObject.Find("Control").GetComponent<Control>().playerHand.transform);
			var card = _gameNet.cardInfo.mainDeck[playerCardIndex];
			Card cardObject = new Card(card.cardNumber, card.color, card.cardPrefab);
			var currentCard = cardObject.loadCard(playerHand.transform);

			var playCardData = currentCard.GetComponent<PlayCardData>();
			playCardData.cardIndex = playerCardIndex;
			playCardData.handIndex = i;
			playCardData.cardObject = currentCard;
			
			SetListeners(currentCard);
			var topCardInDiscardDeck = _gameNet.cardInfo.mainDeck[_gameNet.gameData.discardedCardIndices.Last()];

			//print(card.cardNumber + " " + (card.cardNumber == topCardInDiscardDeck.cardNumber));
			//print(card.cardNumber + " " + string.Equals(card.color, topCardInDiscardDeck.color));
			if (card.cardNumber == topCardInDiscardDeck.cardNumber ||
			    string.Equals(card.color, topCardInDiscardDeck.color) || card.cardNumber >= 13 ||
			    string.Equals(card.color, FindObjectOfType<Control>().wildColor))
			{
				//setListeners(playerCardIndex, temp, i);
				currentCard.transform.GetChild (3).gameObject.SetActive (false);
				currentCard.GetComponent<Button>().interactable = true;
			}
			else
			{
				currentCard.GetComponent<Button>().interactable = false;
				currentCard.transform.GetChild (3).gameObject.SetActive (true);
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
	public void SetListeners(GameObject temp) { //sets all listeners on the cards
		temp.GetComponent<Button>().onClick.AddListener(() => { PlayTurn(temp.GetComponent<PlayCardData>());});
	}

	public void PlayTurn(PlayCardData playCardData)
	{
		foreach (var player in _gameNet.gameData.players)
		{
			if (string.Equals(player.playerName, PhotonNetwork.LocalPlayer.NickName))
			{
				var control = FindObjectOfType<Control>();
				if (_gameNet.gameData.currentTurn != PhotonNetwork.LocalPlayer.NickName)
				{
					control.myTurn = false;
					return;
				}

				if (control.myTurn)
				{
					player.playerCardIndices.RemoveAt(playCardData.handIndex);
					control.myTurn = false;
					playedWild = _gameNet.cardInfo.mainDeck[playCardData.cardIndex].cardNumber >= 13;
					PhotonNetwork.CurrentRoom.SetCustomProperties(_gameNet.GetJSONGameData());
					turnEnd(playCardData.cardIndex, playCardData.handIndex, playCardData.cardObject);
				}
				break;
			}
		}
	}
	
	public void PlayTurn(int cardIndex, GameObject temp, int handIndex)
	{
		var control = FindObjectOfType<Control>();
		if (_gameNet.gameData.currentTurn != PhotonNetwork.LocalPlayer.NickName)
		{
			control.myTurn = false;
			return;
		}
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

			PhotonNetwork.CurrentRoom.SetCustomProperties(gameNet.GetJSONGameData());
			
			temp.GetComponent<Button>().onClick.RemoveAllListeners();
			turnEnd(cardIndex, handIndex, temp);
		}
	}

	public void NextPlayersTurn(GameNetworkHandler gameNet, Control control, bool skipTurn = false, bool reverse = false)
	{
		var currentTurnIndex = gameNet.gameData.currentTurnIndex;
		if (!skipTurn)
			currentTurnIndex = UpdateCurrentIndex(gameNet, currentTurnIndex, 1);
		else if (skipTurn)
		{
			currentTurnIndex = UpdateCurrentIndex(gameNet, currentTurnIndex, 1);
			currentTurnIndex = UpdateCurrentIndex(gameNet, currentTurnIndex, 1);
		}

		print(currentTurnIndex);
		print(gameNet.gameData.players[currentTurnIndex].playerName);
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
		// turnEnd (-1, -1);
	}

	public void turnEnd(int cardIndex, int handIndex, GameObject tempCard) { //ends the player's turn
		Control cont = GameObject.Find("Control").GetComponent<Control>();

		GameObject playedCard = tempCard;

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
				cont.updateDiscPile(cardIndex, playedCard.transform.position.x, playedCard.transform.position.y);
				// if (handIndex > handList.Count)
				// {
				// 	handList.RemoveAt(handIndex);
				// }

				if (specNumb == 13)
				{
					cont.startWild(name, cardIndex, specNumb);
				}

				if (specNumb == 14)
				{
					cont.startWild(name, cardIndex, specNumb);
				}
			}
			else {
				if (specNumb < 10)
				{
					cont.recieveText(string.Format("{0} played a {1} {2}", name,
						_gameNet.cardInfo.mainDeck[cardIndex].color, _gameNet.cardInfo.mainDeck[cardIndex].cardNumber));
					cont.enabled = true;

					FindObjectOfType<PhotonView>().RPC("UpdateDiscardRegular", RpcTarget.Others, cardIndex);
					cont.updateDiscPile(cardIndex, playedCard.transform.position.x, playedCard.transform.position.y);
					NextPlayersTurn(_gameNet, cont);
				}
				else if (specNumb == 10)
				{
					cont.recieveText (string.Format ("{0} played a {1} skip", name, _gameNet.cardInfo.mainDeck[cardIndex].color));
					FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "skip",
						_gameNet.cardInfo.mainDeck[cardIndex].color, cardIndex);
					cont.updateDiscPile(cardIndex, playedCard.transform.position.x, playedCard.transform.position.y);
					NextPlayersTurn(_gameNet, cont, true);
				}
				else if (specNumb == 11) {
					_gameNet.gameData.players.Reverse();
					cont.recieveText (string.Format ("{0} played a {1} reverse", name, _gameNet.cardInfo.mainDeck [cardIndex].color));
					FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "reverse",
						_gameNet.cardInfo.mainDeck[cardIndex].color, cardIndex);
					cont.updateDiscPile(cardIndex, playedCard.transform.position.x, playedCard.transform.position.y);
					NextPlayersTurn(_gameNet, cont, reverse: true);
				}
				else if (specNumb == 12) {
					//cont.specialCardPlay (this, 12);
					cont.recieveText (string.Format ("{0} played a {1} draw 2", name, _gameNet.cardInfo.mainDeck[cardIndex].color));

					int playerIndex = _gameNet.gameData.currentTurnIndex;

					if (playerIndex < PhotonNetwork.PlayerList.Length - 1)
						playerIndex++;
					else
						playerIndex = 0;
					
					for (int j = 0; j < 2; j++)
					{
						_gameNet.gameData.players[playerIndex].playerCardIndices
							.Add(_gameNet.gameData.cardIndices[0]);
						_gameNet.gameData.cardIndices.RemoveAt(0);

						FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "draw",
							_gameNet.cardInfo.mainDeck[cardIndex].color, cardIndex);
						cont.updateDiscPile(cardIndex, playedCard.transform.position.x, playedCard.transform.position.y);
					}
					NextPlayersTurn(_gameNet, cont);
				}

				//handList.RemoveAt(handIndex);

				//NextPlayersTurn(_gameNet, cont);
			}

			Destroy(tempCard);
			// handList[handIndex].transform.DOMove(discardedTop.transform.position, 3f).OnComplete();
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
