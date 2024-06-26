﻿using System.Collections;
using System.Collections.Generic;
using FijitAddons;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviourPunCallbacks
{
	[SerializeField] private string playerName;
	[SerializeField] private string roomName;

	public Text vers;
	public GameObject[] cards;
	float timer=0;
	public GameObject[] scroll;
	List<GameObject> scroll2 = new List<GameObject>();
	public GameObject regCardPrefab;
	public GameObject skipCardPrefab;
	public GameObject revrsCardPrefab;
	public GameObject drawCardPrefab;
	public GameObject wildCardPrefab;
	public GameObject setupCan;
	public GameObject[] toggles = new GameObject[5];
	public bool testing;

	public void SetNetworkData()
	{
		if (testing)
			playerName = "PLAYER" + Random.Range(0, 20000);
		else
		{
			playerName = Bridge.GetInstance().thisPlayerInfo.data.multiplayer.username;
			roomName = Bridge.GetInstance().thisPlayerInfo.data.multiplayer.lobbyId;
		}
	}
	
	void Start () { //start function
		vers.text = "Version: " + Application.version;
		if (testing)
			playerName = "PLAYER" + Random.Range(0, 20000);
		else
			playerName = Bridge.GetInstance().thisPlayerInfo.data.multiplayer.username;

		foreach (GameObject x in cards) { //sets a random color and number for the cards
			string randColor = returnRandColor (Random.Range(0,4));
			int randNumb = Random.Range (0, 10);
			foreach (Transform childs in x.transform) 
				childs.GetComponent<Text>().text = randNumb.ToString();
			x.GetComponent<RawImage> ().texture = Resources.Load (randColor + "Card") as Texture2D;	
			x.transform.GetChild (1).GetComponent<Text> ().color = returnColor (randColor);
		}

		int pos = 400; //below takes care of the moving cards to the right of the screen
		for (int i = 0; i < 6; i++) {
			Card myCard=null;
			int randNumb = Random.Range (0, 15);
			if (randNumb < 10)
				myCard = new Card (randNumb, returnRandColor (Random.Range (0, 4)), regCardPrefab);
			else {
				switch (randNumb) {
				case 10:
					myCard = new Card (randNumb, returnRandColor (Random.Range (0, 4)), skipCardPrefab);
					break;
				case 11:
					myCard = new Card (randNumb, returnRandColor (Random.Range (0, 4)), revrsCardPrefab);
					break;
				case 12:
					myCard = new Card (randNumb, returnRandColor (Random.Range (0, 4)), drawCardPrefab);
					break;
				case 13:
					myCard = new Card (randNumb, "Black", wildCardPrefab);
					break;
				case 14:
					myCard = new Card (randNumb, "Black", wildCardPrefab);
					break;
				}
			}
			GameObject temp = myCard.loadCard (720, pos, GameObject.Find("Canvas").transform);
			scroll2.Add (temp);
			pos -= 300;
		}
	}
	string returnRandColor (int rand) { //gives a random color
		switch(rand) {
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
	Color returnColor(string what) { //gives the color that matches the string
		switch (what) {
		case "Green":
			return new Color32 (0x55, 0xaa, 0x55,255);
		case "Blue":
			return new Color32 (0x55, 0x55, 0xfd,255);
		case "Red":
			return new Color32 (0xff, 0x55, 0x55,255);
		case "Yellow":
			return new Color32 (0xff, 0xaa, 0x00,255);
		}
		return new Color (0, 0, 0);
	}
	public void exit() { //exits the app
		Application.Quit ();
	}
	public void setup(bool openClose) { //starts the setup canvas screen
		
		PhotonNetwork.LocalPlayer.NickName = playerName;
		CreateRoom(); //Create room
	}
	public void TryCreateRoom()
	{
		CreateRoom();
	}

	private void CreateRoom()
	{
		if (!testing)
			roomName = Bridge.GetInstance().thisPlayerInfo.data.multiplayer.lobbyId;
		PhotonNetwork.JoinRoom(roomName);
	}

	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.BroadcastPropsChangeToAll = true;
		PhotonNetwork.CreateRoom(roomName, roomOptions);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		PhotonNetwork.JoinRoom(roomName); //join if there is already a room
	}

	public override void OnJoinedRoom()
	{
		SceneManager.LoadScene(1);
	}

	public void play() { //finds the toggle that is on to decide how many ai players there will be
		for (int i = 0; i < 5; i++) {
			if (toggles [i].GetComponent<Toggle> ().isOn) {
				Control.numbOfAI = i + 1;
				break;
			}				
		}
		UnityEngine.SceneManagement.SceneManager.LoadScene ("Main"); //then it loads the play screen
	}
	void Update() { //this takes care of moving all of the cards in the scroll. It resets them back to the top as well
		for(int i=0;i<6;i++) {
			float curPos = scroll2[i].transform.localPosition.y;
			if (curPos < (Screen.height-300)*-1) {
				Destroy (scroll2[i]);
				Card myCard=null;
				int randNumb = Random.Range (0, 15);
				if (randNumb < 10)
					myCard = new Card (randNumb, returnRandColor (Random.Range (0, 4)), regCardPrefab);
				else {
					switch (randNumb) {
					case 10:
						myCard = new Card (randNumb, returnRandColor (Random.Range (0, 4)), skipCardPrefab);
						break;
					case 11:
						myCard = new Card (randNumb, returnRandColor (Random.Range (0, 4)), revrsCardPrefab);
						break;
					case 12:
						myCard = new Card (randNumb, returnRandColor (Random.Range (0, 4)), drawCardPrefab);
						break;
					case 13:
						myCard = new Card (randNumb, "Black", wildCardPrefab);
						break;
					case 14:
						myCard = new Card (randNumb, "Black", wildCardPrefab);
						break;
					}
				}
				GameObject temp = myCard.loadCard (720, (int)(curPos+1800), GameObject.Find("Canvas").transform);
				scroll2[i]=temp;
			}
			else 
				scroll2[i].transform.localPosition = new Vector2 (scroll2[i].transform.localPosition.x, curPos - 100 * Time.deltaTime);
		}
	}
}
