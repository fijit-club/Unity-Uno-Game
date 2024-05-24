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
        if (string.Equals(text, "draw"))
            _gameNet.DrawAnimationOther();
    }

    [PunRPC]
    private void UpdateDiscardRegular(int cardNumber)
    {
        var otherCardLocation = GameObject.Find("Opponent Card Location").transform;
        _control.updateDiscPile(cardNumber, otherCardLocation.position.x, otherCardLocation.position.y);
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
        else if (string.Equals(cardName, "draw4"))
        {
            card = new Card(cardNumber, cardColor, _control.wildCardPrefab);
            _control.wildColor = cardColor;
            // foreach (var player in _gameNet.gameData.players)
            // {
            //     if (string.Equals(player.playerName, _gameNet.gameData.currentTurn))
            //     {
            //         print(player.playerName);
            //         _gameNet.draw4 = true;
            //     }
            // }
        }
        else
            card = null;
        var otherCardLocation = GameObject.Find("Opponent Card Location").transform;
        _control.updateDiscPile(cardIndex, otherCardLocation.position.x, otherCardLocation.position.y);
    }
}
