using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FijitAddons;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HumanPlayer : MonoBehaviour, PlayerInterface {

	bool skip=false;
	bool drew =false;
	[SerializeField] private ScrollRect hand;
	
	
	string name;
	List<Card> handList = new List<Card> ();
	private GameNetworkHandler _gameNet;
	private List<CardData> playerCards = new List<CardData>();
	private int _test;
	private bool _cardsDealt;

	public HumanPlayer(string name) { //initalizes
		this.name = name;
	}

	public bool skipStatus { //returns if the player should be skipped
		get{return skip; }
		set{ skip = value; }
	}

	public bool playedWild { get; set; }

	private bool _funoCannotBeEnabled;

	public void turn(bool fromUpdate = false) { //does the turn
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

		int playableCards = thisPlayer.playerCardIndices.Count;
		
		foreach (int playerCardIndex in thisPlayer.playerCardIndices)
		{
			// temp = handList[i].loadCard(GameObject.Find("Control").GetComponent<Control>().playerHand.transform);
			var card = _gameNet.cardInfo.mainDeck[playerCardIndex];
			Card cardObject = new Card(card.cardNumber, card.color, card.cardPrefab);
			var currentCard = cardObject.loadCard(playerHand.transform);
			currentCard.GetComponent<PlayCardData>().MakeChild();
			currentCard.GetComponent<PlayCardData>().UpdateCard();
			CardDealAnimation(currentCard, control, i, fromUpdate);
			
			var playCardData = currentCard.GetComponent<PlayCardData>();
			playCardData.cardIndex = playerCardIndex;
			playCardData.handIndex = i;
			playCardData.cardObject = currentCard;
			control.hand.enabled = true;
			
			SetListeners(currentCard);
			var topCardInDiscardDeck = _gameNet.cardInfo.mainDeck[_gameNet.gameData.discardedCardIndices.Last()];

			Player thisLocalPlayer = null;
			
			foreach (var player in _gameNet.gameData.players)
			{
				if (string.Equals(player.playerName, PhotonNetwork.LocalPlayer.NickName))
					thisLocalPlayer = player;
			}

			if (thisLocalPlayer != null && (thisLocalPlayer.playerCardIndices.Count > 1 ||
			                                thisLocalPlayer.playerCardIndices.Count == 0)) _funoCannotBeEnabled = false;

			if (!_funoCannotBeEnabled)
			{
				if (thisLocalPlayer != null && thisLocalPlayer.playerCardIndices.Count == 1)
				{
					_gameNet.funOButton.SetActive(true);
					_funoCannotBeEnabled = true;
				}
				else
				{
					_gameNet.funOButton.SetActive(false);
				}
			}

			// foreach (var player in _gameNet.gameData.players)
			// {
			// 	if (string.Equals(player.playerName, PhotonNetwork.LocalPlayer.NickName))
			// 	{
			// 		if (!string.Equals(player.playerName, _gameNet.gameData.currentTurn))
			// 		{
			// 			if (_gameNet.funOButton.activeInHierarchy)
			// 			{
			// 				_gameNet.funOButton.SetActive(false);
			// 				_gameNet.FunoButtonPress();
			// 			}
			// 		}
			// 	}
			// }
			

			if ((card.cardNumber == topCardInDiscardDeck.cardNumber ||
			    string.Equals(card.color, topCardInDiscardDeck.color) || card.cardNumber >= 13 ||
			    string.Equals(card.color, FindObjectOfType<Control>().wildColor)) && string.Equals(
				    _gameNet.gameData.currentTurn, PhotonNetwork.LocalPlayer.NickName))
			{
				//setListeners(playerCardIndex, temp, i);
				currentCard.GetComponent<PlayCardData>().SetDarkOverlay(false, false);
				currentCard.GetComponent<Button>().interactable = true;
				var rawImageCurrentCard = currentCard.GetComponent<RawImage>().color;
				rawImageCurrentCard.a = 0f;
				currentCard.GetComponent<RawImage>().color = rawImageCurrentCard;
				//currentCard.GetComponent<PlayCardData>().MakeChild();
				if (!playedWild)
					currentCard.GetComponent<PlayCardData>().MoveUp();
				
				//control.hand.enabled = false;
				//localPosition.y = -127f;
				//currentCard.transform.localPosition = localPosition;
				//control.hand.enabled = true;
			}	
			else
			{
				currentCard.GetComponent<Button>().interactable = false;
				if (!_cardsDealt)
					currentCard.GetComponent<PlayCardData>().SetDarkOverlay(true, true);
				else
					currentCard.GetComponent<PlayCardData>().SetDarkOverlay(true, false);
				playableCards--;
			}
			i++;
		}

		if (control.drew)
		{
			control.drew = false;
			var card = _gameNet.cardInfo.mainDeck[control.lastDrawnCard];
			var topCard = _gameNet.cardInfo.mainDeck[_gameNet.gameData.discardedCardIndices.Last()];
			if ((card.cardNumber != topCard.cardNumber && !string.Equals(card.color, topCard.color) &&
			     card.cardNumber < 13 && !string.Equals(card.color, FindObjectOfType<Control>().wildColor)))
			{
				control.players[0].NextPlayersTurn(_gameNet, control);
			}
			else
			{
				if (playableCards != 1)
					control.players[0].NextPlayersTurn(_gameNet, control);
			}
		}

		playedWild = false;

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

	private void CardDealAnimation(GameObject currentCard, Control control, int i, bool fromUpdate = false)
	{
		int cardsDestroyed = 0;
		if (_cardsDealt) return;
		Canvas.ForceUpdateCanvases();
		var tempPlayerCard = Instantiate(currentCard, control.tempHand, true);
		tempPlayerCard.transform.position = control.deckLocation.position;
		print("TEST TEMP 1");
		tempPlayerCard.transform.DOLocalMove(currentCard.transform.localPosition, .1f * i).OnComplete(() =>
		{
			print("TEST TEMP 2");
			Destroy(tempPlayerCard);
			currentCard.GetComponent<RawImage>().enabled = true;
			for (int j = 0; j < currentCard.transform.childCount - 1; j++)
			{
				currentCard.transform.GetChild(j).gameObject.SetActive(true);
			}
			print("TEST TEMP 3");
			print("TEST TEMP 4");
			cardsDestroyed++;
			print(cardsDestroyed);
			if (cardsDestroyed >= 7)
			{
				for (int j = control.tempHand.childCount - 1; j > 0; j--)
				{
					Destroy(control.tempHand.GetChild(i).gameObject);
				}
			}
			_cardsDealt = true;
		});
		currentCard.GetComponent<RawImage>().enabled = false;
		for (int j = 0; j < currentCard.transform.childCount - 1; j++)
		{
			currentCard.transform.GetChild(j).gameObject.SetActive(false);
		}
	}

	public void SetListeners(GameObject temp) { //sets all listeners on the cards
		temp.GetComponent<Button>().onClick.AddListener(() => { PlayTurn(temp.GetComponent<PlayCardData>());});
	}

	public void PlayTurn(PlayCardData playCardData)
	{
		Bridge.GetInstance().VibrateBridge(Bridge.Haptics.rigid);
		foreach (var player in _gameNet.gameData.players)
		{
			if (player.playerCardIndices.Count == 1)
				_gameNet.DisableCatchButtons();
			
			if (string.Equals(player.playerName, PhotonNetwork.LocalPlayer.NickName))
			{
				var control = FindObjectOfType<Control>();
				if (_gameNet.gameData.currentTurn != PhotonNetwork.LocalPlayer.NickName)
				{
					control.myTurn = false;
					_gameNet.thisPlayerTurnIndicator.SetActive(false);
					return;
				}

				if (control.myTurn)
				{
					playedWild = _gameNet.cardInfo.mainDeck[playCardData.cardIndex].cardNumber >= 13;
					if (!playedWild)
						player.playerCardIndices.RemoveAt(playCardData.handIndex);
					
					control.myTurn = false;
					_gameNet.thisPlayerTurnIndicator.SetActive(false);
					if (player.playerCardIndices.Count == 0)
						_gameNet.GameEnd();
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
		if (gameNet.gameData.reversed)
		{
			if (!skipTurn)
			{
				currentTurnIndex = BackCurrentIndex(gameNet, currentTurnIndex, 1);
				while (gameNet.gameData.players[currentTurnIndex].won)
					currentTurnIndex = BackCurrentIndex(gameNet, currentTurnIndex, 1);
			}
			else
			{
				currentTurnIndex = BackCurrentIndex(gameNet, currentTurnIndex, 1);
				while (gameNet.gameData.players[currentTurnIndex].won)
					currentTurnIndex = BackCurrentIndex(gameNet, currentTurnIndex, 1);
				
				currentTurnIndex = BackCurrentIndex(gameNet, currentTurnIndex, 1);
				while (gameNet.gameData.players[currentTurnIndex].won)
					currentTurnIndex = BackCurrentIndex(gameNet, currentTurnIndex, 1);
			}
		}
		else
		{
			if (!skipTurn)
			{
				currentTurnIndex = ForwardCurrentIndex(gameNet, currentTurnIndex, 1);
				while (gameNet.gameData.players[currentTurnIndex].won)
					currentTurnIndex = ForwardCurrentIndex(gameNet, currentTurnIndex, 1);
			}
			else
			{
				currentTurnIndex = ForwardCurrentIndex(gameNet, currentTurnIndex, 1);
				while (gameNet.gameData.players[currentTurnIndex].won)
					currentTurnIndex = ForwardCurrentIndex(gameNet, currentTurnIndex, 1);
				
				currentTurnIndex = ForwardCurrentIndex(gameNet, currentTurnIndex, 1);
				while (gameNet.gameData.players[currentTurnIndex].won)
					currentTurnIndex = ForwardCurrentIndex(gameNet, currentTurnIndex, 1);
			}
		}

		gameNet.gameData.currentTurn = gameNet.gameData.players[currentTurnIndex].playerName;
		turn();
		PhotonNetwork.CurrentRoom.SetCustomProperties(gameNet.GetJSONGameData());
	}

	private static int ForwardCurrentIndex(GameNetworkHandler gameNet, int currentTurnIndex, int amount)
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

	private static int BackCurrentIndex(GameNetworkHandler gameNet, int currentTurnIndex, int amount)
	{
		if (currentTurnIndex > 0)
		{
			currentTurnIndex -= amount;
			gameNet.gameData.currentTurnIndex -= amount;
		}
		else
		{
			currentTurnIndex = PhotonNetwork.PlayerList.Length - amount;
			gameNet.gameData.currentTurnIndex = PhotonNetwork.PlayerList.Length - amount;
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
			//cont.playerHand.transform.GetChild (i).GetChild(0).GetChild(4).gameObject.SetActive (false);
		}
		if (drew) {
			cont.GetComponent<Control> ().enabled = true;
			cont.recieveText (string.Format ("{0} drew a card", name));
			cont.deckGO.GetComponent<Button> ().onClick.RemoveAllListeners ();
		}
		else {	
			int specNumb = _gameNet.cardInfo.mainDeck[cardIndex].cardNumber;	
			if (playedWild) {
				var topDiscardedCard = cont.updateDiscPile(cardIndex, playedCard.transform.position.x, playedCard.transform.position.y);
				// if (handIndex > handList.Count)
				// {
				// 	handList.RemoveAt(handIndex);
				// }
				
				//_gameNet.timer.StopTimer();

				if (specNumb == 13)
				{
					GameObject tempPlayedCard = topDiscardedCard;
					cont.StartWild(name, cardIndex, specNumb, tempPlayedCard, cont, handIndex);
				}

				if (specNumb == 14)
				{
					GameObject tempPlayedCard = topDiscardedCard;
					cont.StartWild(name, cardIndex, specNumb, tempPlayedCard, cont, handIndex);
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


					int playerIndex = _gameNet.gameData.currentTurnIndex;
					if (_gameNet.gameData.reversed)
					{
						if (playerIndex > 0)
							playerIndex--;
						else
							playerIndex = PhotonNetwork.PlayerList.Length - 1;
					}
					else
					{
						if (playerIndex < PhotonNetwork.PlayerList.Length - 1)
							playerIndex++;
						else
							playerIndex = 0;
					}

					while (_gameNet.gameData.players[playerIndex].won)
					{
						if (_gameNet.gameData.reversed)
						{
							if (playerIndex > 0)
								playerIndex--;
							else
								playerIndex = PhotonNetwork.PlayerList.Length - 1;
						} 
						else
						{
							if (playerIndex < PhotonNetwork.PlayerList.Length - 1)
								playerIndex++;
							else
								playerIndex = 0;
						}
					}

					FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "skip",
						_gameNet.cardInfo.mainDeck[cardIndex].color, cardIndex);
					
				 	cont.recieveText("skip", affectedPlayer: _gameNet.gameData.players[playerIndex].playerName);
					cont.updateDiscPile(cardIndex, playedCard.transform.position.x, playedCard.transform.position.y);
					NextPlayersTurn(_gameNet, cont, true);
				}
				else if (specNumb == 11) {
					// _gameNet.gameData.players.Reverse();
					
					if (!_gameNet.gameData.reversed)
						cont.reverseAnimation.Play("reverse", -1, 0f);
					else
						cont.reverseAnimation.Play("reverse back", -1, 0f);
					cont.recieveText (string.Format ("{0} played a {1} reverse", name, _gameNet.cardInfo.mainDeck [cardIndex].color));
					FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "reverse",
						_gameNet.cardInfo.mainDeck[cardIndex].color, cardIndex);
					_gameNet.gameData.reversed = !_gameNet.gameData.reversed;
					cont.updateDiscPile(cardIndex, playedCard.transform.position.x, playedCard.transform.position.y);
					if (_gameNet.activePlayers > 2)
						NextPlayersTurn(_gameNet, cont, reverse: true);
				}
				else if (specNumb == 12) {
					//cont.specialCardPlay (this, 12);

					int playerIndex = _gameNet.gameData.currentTurnIndex;

					if (playerIndex < PhotonNetwork.PlayerList.Length - 1)
						playerIndex++;
					else
						playerIndex = 0;

					while (_gameNet.gameData.players[playerIndex].won)
					{
						if (playerIndex < PhotonNetwork.PlayerList.Length - 1)
							playerIndex++;
						else
							playerIndex = 0;
					}
					
					cont.recieveText(
						string.Format("{0} played a {1} draw 2", name, _gameNet.cardInfo.mainDeck[cardIndex].color),
						affectedPlayer: _gameNet.gameData.players[playerIndex].playerName);
					
					for (int j = 0; j < 2; j++)
					{
						if (_gameNet.gameData.cardIndices.Count < 2) {
							cont.resetDeck ();
						}
						_gameNet.gameData.players[playerIndex].playerCardIndices
							.Add(_gameNet.gameData.cardIndices[0]);
						_gameNet.gameData.cardIndices.RemoveAt(0);

						FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "draw",
							_gameNet.cardInfo.mainDeck[cardIndex].color, cardIndex);
						cont.updateDiscPile(cardIndex, playedCard.transform.position.x, playedCard.transform.position.y);
					}

					NextPlayersTurn(_gameNet, cont, true);
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
