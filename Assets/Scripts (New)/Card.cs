using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class Card : MonoBehaviour {

	/*
	* 1-9 are regular
	* 10 is skip
	* 11 is reverse
	* 12 is draw 2
	* 13 is wild
	* 14 is wild draw 4
	*/

	int number;
	string color;
	GameObject cardObj;

	public Card (int numb, string color, GameObject obj) { //defines the object
		number = numb;
		this.color = color;
		cardObj = obj;
	}
	public GameObject loadCard(int x, int y, Transform parent) { //when ran, it tells where to load the card on the screen
		GameObject temp = loadCard (parent);
		temp.transform.localPosition = new Vector2 (x, y+540);
		return temp;
	}
	public GameObject loadCard(Transform parent) { //does all the setup for loading. Used if card doesn't need a specific position		
		GameObject temp = Instantiate (cardObj);
		temp.name = color + number;
		if (number < 10) {
			foreach (Transform childs in temp.transform) {
				if (childs.name.Equals ("Cover"))
					break;
				childs.GetComponent<TMP_Text> ().text = number.ToString ();
			}
			temp.transform.GetChild (1).GetComponent<TMP_Text> ().color = returnColor (color);
		}
		else if (number == 10 || number == 11) {
			temp.transform.GetChild (1).GetComponent<RawImage> ().color = returnColor (color);
		}
		else if (number == 12)
		{
			temp.transform.GetChild(1).GetComponent<RawImage>().texture = Resources.Load(color + "Draw") as Texture2D;
		}
		else if (number == 13) {
			temp.transform.GetChild (0).GetComponent<TMP_Text> ().text = "";
			temp.transform.GetChild(1).GetChild(0).gameObject.SetActive(true);
			temp.transform.GetChild(1).GetChild(1).gameObject.SetActive(false);
			temp.transform.GetChild (2).GetComponent<TMP_Text> ().text = "";
		}
		else if (number == 14)
		{
			temp.transform.GetChild(1).GetChild(0).gameObject.SetActive(false);
			temp.transform.GetChild(1).GetChild(1).gameObject.SetActive(true);
		}

		temp.GetComponent<RawImage> ().texture = Resources.Load (color + "Card") as Texture2D;
		temp.transform.SetParent (parent);
		temp.transform.localScale = new Vector3 (1, 1, 1);
		return temp;
	}
	Color returnColor(string what) { //returns a color based on the color string
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
	public int getNumb() { //accessor for getting the number
		return number;
	}
	public string getColor() { //accessor for getting the color
		return color;
	}
	public bool Equals(Card other) { //overides the original Equals so that color or number must be equal
		return other.getNumb () == number || other.getColor ().Equals (color);
	}
	public void changeColor(string newColor) { //mutator that changes the color of a wild card to make the color noticable
		color = newColor;
	}
}
