using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class HumanPlayer : MonoBehaviour, PlayerInterface {

	bool skip=false;
	bool drew =false;
	bool playedWild;
	string name;
	List<Card> handList = new List<Card> ();

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
		int i = 0;
		foreach (Card x in handList) { //foreach card in hand
			
			GameObject temp = null;
			if (GameObject.Find ("Control").GetComponent<Control> ().playerHand.transform.childCount > i) //is the card already there or does it need to be loaded
				temp = GameObject.Find ("Control").GetComponent<Control> ().playerHand.transform.GetChild (i).gameObject;			
			else 
				temp = x.loadCard (GameObject.Find ("Control").GetComponent<Control> ().playerHand.transform);

			
			if (handList [i].getNumb() ==  Control.discard [0].getNumb() || string.Equals(handList[i].getColor(),
				    Control.discard[0].getColor()) || handList [i].getNumb () >= 13) { //if the cards can be played
				setListeners (i, temp);
				temp.transform.GetChild (3).gameObject.SetActive (false);
			}
			else {
				temp.transform.GetChild (3).gameObject.SetActive (true); //otherwise black them out
			}
			i++;
		}
	}
	public void setListeners(int where,GameObject temp) { //sets all listeners on the cards
		temp.GetComponent<Button>().onClick.AddListener(() => { PlayTurn(@where, temp); });
	}

	private void PlayTurn(int @where, GameObject temp)
	{
		var control = FindObjectOfType<Control>();
		if (control.myTurn)
		{
			control.myTurn = false;
			playedWild = handList[@where].getNumb() >= 13;
			var gameNet = FindObjectOfType<GameNetworkHandler>();

			temp.GetComponent<Button>().onClick.RemoveAllListeners();
			Destroy(temp);
			turnEnd(@where);
			NextPlayersTurn(gameNet, control);
		}
	}

	public void NextPlayersTurn(GameNetworkHandler gameNet, Control control)
	{
		var currentTurnIndex = gameNet.gameData.currentTurnIndex;
		if (currentTurnIndex < PhotonNetwork.PlayerList.Length - 1)
		{
			currentTurnIndex++;
			gameNet.gameData.currentTurnIndex++;
		}
		else
		{
			currentTurnIndex = 0;
			gameNet.gameData.currentTurnIndex = 0;
		}

		gameNet.gameData.currentTurn = gameNet.gameData.players[currentTurnIndex].playerName;
		control.players[0].turn();
		PhotonNetwork.CurrentRoom.SetCustomProperties(gameNet.GetJSONGameData());
	}

	public void addCards(Card other) { //recieves cards to add to the hand
		handList.Add (other);
	}
	public void recieveDrawOnTurn() { //if the player decides to draw
		handList[handList.Count-1].loadCard (GameObject.Find ("Control").GetComponent<Control> ().playerHand.transform);
		drew = true;
		turnEnd (-1);
	}
	public void turnEnd(int where) { //ends the player's turn
		Control cont = GameObject.Find("Control").GetComponent<Control>();

		cont.playerHand.GetComponent<RectTransform> ().localPosition = new Vector2 (0, 0);

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
			int specNumb = handList [where].getNumb ();	
			if (playedWild) {
				cont.updateDiscPile (handList [where]);
				handList.RemoveAt (where);	
				cont.startWild (name);
				if (specNumb == 14)
					cont.specialCardPlay (this, 14);
			}
			else {
				if (specNumb < 10) {
					cont.recieveText (string.Format ("{0} played a {1} {2}", name, handList [where].getColor (), handList [where].getNumb ()));
					cont.enabled = true;
					FindObjectOfType<PhotonView>().RPC("UpdateDiscardRegular", RpcTarget.Others, handList[where].getNumb(),
						handList[where].getColor());
				}
				else if (specNumb == 10) {
					cont.specialCardPlay (this, 10);
					cont.recieveText (string.Format ("{0} played a {1} skip", name, handList [where].getColor ()));
					FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "skip",
						handList[where].getColor());
				}
				else if (specNumb == 11) {
					cont.specialCardPlay (this, 11);
					cont.recieveText (string.Format ("{0} played a {1} reverse", name, handList [where].getColor ()));
					FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "reverse",
						handList[where].getColor());
				}
				else if (specNumb == 12) {
					cont.specialCardPlay (this, 12);
					cont.recieveText (string.Format ("{0} played a {1} draw 2", name, handList [where].getColor ()));
					FindObjectOfType<PhotonView>().RPC("UpdateDiscardSpecial", RpcTarget.Others, specNumb, "draw",
						handList[where].getColor());
				}
				print(where);
				cont.updateDiscPile (handList [where]);
				handList.RemoveAt (where);	
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
