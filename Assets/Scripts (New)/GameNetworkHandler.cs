using Photon.Pun;
using UnityEngine;

public class GameNetworkHandler : MonoBehaviour
{
    public GameData gameData;
    public CardInfo cardInfo;

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Player player = new Player();
            player.playerName = PhotonNetwork.LocalPlayer.NickName;
            gameData.players.Add(player);
        }
    }
}
