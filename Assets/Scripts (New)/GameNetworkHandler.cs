using System;
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
    public bool gameStarted;
    
    [SerializeField] private Control control;
    [SerializeField] private GameObject waitingUI;
    [SerializeField] private GameObject[] otherPlayers;
    [SerializeField] private GameObject otherCardPrefab;
    [SerializeField] private Animator drawOther;
    
    private bool _assignedPlayerLocation;

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        print(PhotonNetwork.CurrentRoom.CustomProperties);

        GameData tempGameData =
            JsonConvert.DeserializeObject<GameData>((string) PhotonNetwork.CurrentRoom.CustomProperties["GAME"]);
        gameData = tempGameData;

        control.AssignCardsOnClients();
        UpdateTurns();
        UpdateOtherPlayerSet(false);
    }

    private void UpdateTurns()
    {
        if (control.assignedCards)
            control.players[0].turn(true);
        if (gameData.currentTurn == PhotonNetwork.LocalPlayer.NickName)
            control.myTurn = true;
    }

    public void StartGame()
    {
        control.AssignCards();
        waitingUI.SetActive(false);
    }

    private int _assigned;
    
    public void UpdateOtherPlayerSet(bool assignLocation)
    {
        ResetCards();
    }

    private void ResetCards()
    {
        for (int j = 0; j < otherPlayers.Length; j++)
        {
            for (int i = otherPlayers[j].transform.childCount - 1; i >= 0; i--)
            {
                Destroy(otherPlayers[j].transform.GetChild(i).gameObject);
            }
        }

        int n = 0;
        foreach (var player in gameData.players)
        {
            if (string.Equals(player.playerName, PhotonNetwork.LocalPlayer.NickName)) continue;
            for (int i = 0; i < player.playerCardIndices.Count; i++)
            {
                Instantiate(otherCardPrefab, otherPlayers[n].transform);
            }

            n++;
        }
    }

    public void DrawAnimationOther()
    {
        drawOther.Play("draw other", -1, 0f);
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
