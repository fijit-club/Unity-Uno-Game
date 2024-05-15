using Photon.Pun;
using UnityEngine;

public class ClientHandler : MonoBehaviour
{
    [PunRPC]
    private void RegisterPlayer(string playerName)
    {
        Player player = new Player();
        player.playerName = playerName;
        var gameNetworkHandler = FindObjectOfType<GameNetworkHandler>();
        gameNetworkHandler.gameData.players.Add(player);
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(gameNetworkHandler.GetJSONGameData());

        if (PhotonNetwork.PlayerList.Length >= gameNetworkHandler.maxPlayers)
        {
            gameNetworkHandler.StartGame();
        }
    }
}
