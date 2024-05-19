using System.Collections;
using Newtonsoft.Json;
using Photon.Pun;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameNetworkHandler : MonoBehaviourPunCallbacks
{
    public int maxPlayers;
    public GameData gameData;
    public CardInfo cardInfo;

    [SerializeField] private Control control;
    [SerializeField] private GameObject waitingUI;
    
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        print(PhotonNetwork.CurrentRoom.CustomProperties);
        
        GameData tempGameData = JsonConvert.DeserializeObject<GameData>((string) PhotonNetwork.CurrentRoom.CustomProperties["GAME"]);
        gameData = tempGameData;
        
        control.AssignCardsOnClients();
        UpdateTurns();
    }

    private void UpdateTurns()
    {
        if (gameData.currentTurn == PhotonNetwork.LocalPlayer.NickName)
            control.myTurn = true;
    }

    public void StartGame()
    {
        control.AssignCards();
        waitingUI.SetActive(false);
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            AddMasterPlayer();
        else
            SendRPCToMaster();
    }

    public void DeactivateWaitingUI()
    {
        waitingUI.SetActive(false);
    }
    
    private void SendRPCToMaster()
    {
        FindObjectOfType<PhotonView>().RPC("RegisterPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
        if (PhotonNetwork.PlayerList.Length >= maxPlayers)
            waitingUI.SetActive(false);
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
