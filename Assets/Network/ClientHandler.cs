using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ClientHandler : MonoBehaviour
{
    private GameNetworkHandler _gameNet;
    private Control _control;
    
    private void Awake()
    {
        _gameNet = FindObjectOfType<GameNetworkHandler>();
        _control = FindObjectOfType<Control>();
    }

    public bool caughtCards;

    [PunRPC]
    private void RegisterPlayer(string playerName, string avatar)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Player player = new Player();
            player.playerName = playerName;
            player.avatar = avatar;
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
    private void DisableAllCatchButtons()
    {
        _gameNet.DisableCatchButtonsNoRPC();
    }

    [PunRPC]
    private void SendGameLog(string text, string affectedPlayer)
    {
        _control.recieveText(text, false);
        if (string.Equals(text, "draw") || text.Contains("drew") || text.Contains("draw"))
        {
            _gameNet.DrawAnimationOther(affectedPlayer);
            
        }
        if (string.Equals(affectedPlayer, PhotonNetwork.LocalPlayer.NickName))
        {
            if (text.Contains("skip"))
                _control.skipAnimation.Play("popup", -1, 0f);
        }

        if (text.Contains("reverse"))
        {
            if (!_gameNet.gameData.reversed)
                _control.reverseAnimation.Play("reverse", -1, 0f);
            else
                _control.reverseAnimation.Play("reverse back", -1, 0f);
        }
    }

    [PunRPC]
    private void UpdateDiscardRegular(int cardNumber)
    {
        _control.wildColor = null;
        var otherCardLocation = GameObject.Find("Opponent Card Location").transform;
        _control.updateDiscPile(cardNumber, otherCardLocation.position.x, otherCardLocation.position.y);
    }

    [PunRPC]
    private void UpdateDiscardSpecial(int cardNumber, string cardName, string cardColor, int cardIndex)
    {
        _control.wildColor = null;
        Card card;
        if (string.Equals(cardName, "reverse"))
            card = new Card(cardNumber, cardColor, _control.reverseCardPrefab);
        else if (string.Equals(cardName, "skip"))
            card = new Card(cardNumber, cardColor, _control.skipCardPrefab);
        else if (string.Equals(cardName, "draw"))
            card = new Card(cardNumber, cardColor, _control.drawCardPrefab);
        else if (string.Equals(cardName, "color"))
        {
            _control.wildColor = cardColor;
        }
        else if (string.Equals(cardName, "draw4"))
        {
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
        var topCard = _control.updateDiscPile(cardIndex, otherCardLocation.position.x, otherCardLocation.position.y);
        if (_control.wildColor != null)
        {
            print(_control.wildColor);
            if (_control.wildColor == "Red")
            {
                topCard.GetComponent<RawImage>().texture = _control.colorTextures[0];
                print("changed card to red");
            }
            else if (_control.wildColor == "Green")
            {
                topCard.GetComponent<RawImage>().texture = _control.colorTextures[1];
                print(_control.wildColor);
            }
            else if (_control.wildColor == "Blue")
            {
                topCard.GetComponent<RawImage>().texture = _control.colorTextures[2];
                print(_control.wildColor);
            }
            else if (_control.wildColor == "Yellow")
            {
                topCard.GetComponent<RawImage>().texture = _control.colorTextures[3];
                print(_control.wildColor);
            }
        }
    }

    [PunRPC]
    private void SayFuno(int playerIndex)
    {
        _gameNet.otherPlayersHandler[playerIndex].saidFUNO = true;
        _gameNet.UpdateCatchButtonPlayers(playerIndex);
    }

    [PunRPC]
    private void Catch(int playerIndex)
    {
        _gameNet.UpdateCatchButtonPlayers(playerIndex);
        for (int i = 0; i < _gameNet.gameData.players.Count; i++)
        {
            if (i == playerIndex && string.Equals(_gameNet.gameData.players[i].playerName,
                    PhotonNetwork.LocalPlayer.NickName))
            {
                if (!caughtCards)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        _gameNet.gameData.players[playerIndex].playerCardIndices.Add(_gameNet.gameData.cardIndices[0]);
                        _gameNet.gameData.cardIndices.RemoveAt(0);
                    }

                    caughtCards = true;
                }

                PhotonNetwork.CurrentRoom.SetCustomProperties(_gameNet.GetJSONGameData());
            }
        }
    }
}
