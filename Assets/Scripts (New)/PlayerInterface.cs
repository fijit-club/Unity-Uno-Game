using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface PlayerInterface { //the interface the is inherited by the player objs
	void turn(bool fromUpdate = false);
	
	bool skipStatus {
		get;
		set;
	}
	void addCards(Card other);
	string getName();
	bool Equals(PlayerInterface other);
	int getCardsLeft();
	void NextPlayersTurn(GameNetworkHandler gameNet, Control control, bool skipTurn = false, bool reverse = false);

	public void PlayTurn(PlayCardData playCardData);
}
