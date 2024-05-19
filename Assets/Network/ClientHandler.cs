using System;
using Photon.Pun;
using UnityEngine;

public class ClientHandler : MonoBehaviour
{
    private GameNetworkHandler _gameNet;
    private Control _control;
    
    private void Awake()
    {
        _gameNet = FindObjectOfType<GameNetworkHandler>();
        _control = FindObjectOfType<Control>();
    }

    [PunRPC]
    private void RegisterPlayer(string playerName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Player player = new Player();
            player.playerName = playerName;
            _gameNet.gameData.players.Add(player);

            PhotonNetwork.CurrentRoom.SetCustomProperties(_gameNet.GetJSONGameData());
        }

        if (PhotonNetwork.PlayerList.Length >= _gameNet.maxPlayers)
        {
            if (PhotonNetwork.IsMasterClient)
                _gameNet.StartGame();
            else
                _gameNet.DeactivateWaitingUI();
        }
    }

    [PunRPC]
    private void SendGameLog(string text)
    {
        _control.recieveText(text, false);
    }

    [PunRPC]
    private void UpdateDiscardRegular(int cardNumber)
    {
        _control.updateDiscPile(cardNumber);
    }

    [PunRPC]
    private void UpdateDiscardSpecial(int cardNumber, string cardName, string cardColor, int cardIndex)
    {
        Card card;
        if (string.Equals(cardName, "reverse"))
            card = new Card(cardNumber, cardColor, _control.reverseCardPrefab);
        else if (string.Equals(cardName, "skip"))
            card = new Card(cardNumber, cardColor, _control.skipCardPrefab);
        else if (string.Equals(cardName, "draw"))
            card = new Card(cardNumber, cardColor, _control.drawCardPrefab);
        else if (string.Equals(cardName, "color"))
        {
            card = new Card(cardNumber, cardColor, _control.wildCardPrefab);
            _control.wildColor = cardColor;
        }
        else
            card = null;
        _control.updateDiscPile(cardIndex);
    }
}
