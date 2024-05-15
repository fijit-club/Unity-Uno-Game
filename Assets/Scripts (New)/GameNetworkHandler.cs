using System.Collections;
using Newtonsoft.Json;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameNetworkHandler : MonoBehaviourPunCallbacks
{
    public int maxPlayers = 2;
    public GameData gameData;
    public CardInfo cardInfo;

    [SerializeField] private Control control;
    
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        print(PhotonNetwork.CurrentRoom.CustomProperties);
        
        GameData tempGameData = JsonConvert.DeserializeObject<GameData>((string) PhotonNetwork.CurrentRoom.CustomProperties["GAME"]);
        gameData = tempGameData;
        
        control.AssignCardsOnClients();
    }

    public void StartGame()
    {
        control.AssignCards();
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            AddMasterPlayer();
        else
            SendRPCToMaster();
    }

    private void SendRPCToMaster()
    {
        FindObjectOfType<PhotonView>().RPC("RegisterPlayer", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.NickName);
    }

    private void AddMasterPlayer()
    {
        Player player = new Player();
        player.playerName = PhotonNetwork.LocalPlayer.NickName;
        gameData.players.Add(player);
        PhotonNetwork.CurrentRoom.SetCustomProperties(GetJSONGameData());
    }

    public Hashtable GetJSONGameData()
    {
        Hashtable hash = new Hashtable();
        hash.Add("GAME", JsonConvert.SerializeObject(gameData));
        return hash;
    }
}
