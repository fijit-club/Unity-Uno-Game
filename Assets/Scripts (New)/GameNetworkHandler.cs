using System;
using System.Collections;
using FijitAddons;
using Newtonsoft.Json;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
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
    [SerializeField] private Image[] otherPlayersProfileImages;
    [SerializeField] private TMP_Text[] otherPlayersUsernameTexts;
    [SerializeField] private Image thisPlayerProfileImage;
    [SerializeField] private TMP_Text thisPlayerName;
    
    [SerializeField] private GameObject otherCardPrefab;
    [SerializeField] private Animator drawOther;
    
    private bool _assignedPlayerLocation;
    private int _imagesDownloaded;
    private bool _downloadedAllImages;
    
    IEnumerator DownloadImage(string MediaUrl, Image profilePic, bool thisPlayer = false)
    {   
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
        }
        else
        {
            var tex = ((DownloadHandlerTexture) request.downloadHandler).texture;
            Sprite profileSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
            profilePic.sprite = profileSprite;
            if (!thisPlayer)
            {
                _imagesDownloaded++;
                if (_imagesDownloaded == maxPlayers)
                    _downloadedAllImages = true;
            }
        }
    }

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
        if (maxPlayers == 2)
            n = 1;
        for (int j = 0; j < gameData.players.Count; j++)
        {
            var player = gameData.players[j];
            if (string.Equals(player.playerName, PhotonNetwork.LocalPlayer.NickName)) continue;
            otherPlayersUsernameTexts[n].text = player.playerName;
            if (!_downloadedAllImages)
            {
                otherPlayersProfileImages[n].transform.parent.parent.gameObject.SetActive(true);
                StartCoroutine(DownloadImage(player.avatar, otherPlayersProfileImages[n]));
            }
            for (int i = 0; i < player.playerCardIndices.Count; i++)
            {
                Instantiate(otherCardPrefab, otherPlayers[n].transform);
            }

            if (maxPlayers > 2)
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

        StartCoroutine(DownloadImage(Bridge.GetInstance().thisPlayerInfo.data.multiplayer.avatar,
            thisPlayerProfileImage, true));
        print(PhotonNetwork.LocalPlayer.NickName);
        thisPlayerName.text = PhotonNetwork.LocalPlayer.NickName;
    }

    public void DeactivateWaitingUI()
    {
        waitingUI.SetActive(false);
    }
    
    private void SendRPCToMaster()
    {
        FindObjectOfType<PhotonView>().RPC("RegisterPlayer", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName,
            Bridge.GetInstance().thisPlayerInfo.data.multiplayer.avatar);
        if (PhotonNetwork.PlayerList.Length >= maxPlayers)
            waitingUI.SetActive(false);
    }

    private void AddMasterPlayer()
    {
        Player player = new Player();
        player.playerName = PhotonNetwork.LocalPlayer.NickName;
        player.avatar = Bridge.GetInstance().thisPlayerInfo.data.multiplayer.avatar;
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
