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
    public int scoreForLeaderboard;
    public GameObject thisPlayerTurnIndicator;

    [SerializeField] private Control control;
    [SerializeField] private GameObject waitingUI;
    [SerializeField] private GameObject[] otherPlayers;
    [SerializeField] private Image[] otherPlayersProfileImages;
    [SerializeField] private TMP_Text[] otherPlayersUsernameTexts;
    [SerializeField] private Image thisPlayerProfileImage;
    [SerializeField] private TMP_Text thisPlayerName;
    [SerializeField] private GameObject[] crowns;
    [SerializeField] public GameObject funOButton;
    [SerializeField] private GameObject otherCardPrefab;
    [SerializeField] private Animator drawOther;
    [SerializeField] private GameObject thisPlayerCrown;
    [SerializeField] private GameObject[] otherTurnIndicators;

    private bool _assignedPlayerLocation;
    private int _imagesDownloaded;
    private bool _downloadedAllImages;
    private bool _scoreSent;

    public void GameEnd()
    {
        for (int i = control.playerHand.transform.childCount - 1; i > 0; i--)
            Destroy(control.playerHand.transform.GetChild(i).gameObject);

        int playersWon = 0;
        thisPlayerCrown.SetActive(true);
        
        foreach (var player in gameData.players)
        {
            if (player.won)
                playersWon++;
        }

        if (playersWon > 0)
            scoreForLeaderboard = 4000 / playersWon;
        else
            scoreForLeaderboard = 4000;
    }

    public void SendScore()
    {
        int playersWon = 0;
        
        foreach (var player in gameData.players)
        {
            if (player.won)
                playersWon++;
        }

        if (playersWon == maxPlayers - 1 && !_scoreSent)
        {
            _scoreSent = true;
            Bridge.GetInstance().SendScore(scoreForLeaderboard);
        }
    }
    
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

        SendScore();
    }

    private void UpdateTurns()
    {
        if (control.assignedCards)
            control.players[0].turn(true);
        if (gameData.currentTurn == PhotonNetwork.LocalPlayer.NickName)
        {
            control.myTurn = true;
            thisPlayerTurnIndicator.SetActive(true);
        }
    }

    public void StartGame()
    {
        control.AssignCards();
        waitingUI.SetActive(false);
    }

    private int _assigned;
    
    public void UpdateOtherPlayerSet(bool assignLocation)
    {
        if (gameData.players.Count == maxPlayers)
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

        for (int j = 0; j < gameData.players.Count; j++)
        {
            if (maxPlayers == 2)
                SetupFor2Players(j);
            
            if (maxPlayers == 3)
                SetupFor3Players(j);

            if (maxPlayers == 4)
                SetupFor4Players(j);
        }
    }

    private void SetupFor2Players(int playerScreen)
    {
        if (string.Equals(gameData.players[playerScreen].playerName, PhotonNetwork.LocalPlayer.NickName))
        {
            if (playerScreen == 0)
                SetupPlayer(1, 1);
            else if (playerScreen == 1)
                SetupPlayer(0, 1);
        }
    }
    
    private void SetupFor3Players(int playerScreen)
    {
        if (string.Equals(gameData.players[playerScreen].playerName, PhotonNetwork.LocalPlayer.NickName))
        {
            if (playerScreen == 0)
            {
                SetupPlayer(1, 0);
                SetupPlayer(2, 2);
            }
            else if (playerScreen == 1)
            {
                SetupPlayer(0, 2);
                SetupPlayer(2, 0);
            }
            else if (playerScreen == 2)
            {
                SetupPlayer(0, 0);
                SetupPlayer(1, 2);
            }
        }
    }

    private void SetupFor4Players(int playerScreen)
    {
        if (string.Equals(gameData.players[playerScreen].playerName, PhotonNetwork.LocalPlayer.NickName))
        {
            if (playerScreen == 0)
            {
                SetupPlayer(1, 0);
                SetupPlayer(2, 1);
                SetupPlayer(3, 2);
            }
            else if (playerScreen == 1)
            {
                SetupPlayer(0, 2);
                SetupPlayer(2, 0);
                SetupPlayer(3, 1);
            }
            else if (playerScreen == 2)
            {
                SetupPlayer(0, 1);
                SetupPlayer(1, 2);
                SetupPlayer(3, 0);
            }
            else if (playerScreen == 3)
            {
                SetupPlayer(0, 0);
                SetupPlayer(1, 1);
                SetupPlayer(2, 2);
            }
        }
    }

    private void SetupPlayer(int playerIndex, int place)
    {
        otherPlayersUsernameTexts[place].text = gameData.players[playerIndex].playerName;
        if (!_downloadedAllImages)
        {
            otherPlayersProfileImages[place].transform.parent.parent.gameObject.SetActive(true);
            StartCoroutine(DownloadImage(gameData.players[playerIndex].avatar, otherPlayersProfileImages[place]));
        }
        for (int i = 0; i < gameData.players[playerIndex].playerCardIndices.Count; i++)
            Instantiate(otherCardPrefab, otherPlayers[place].transform);
        if (gameData.players[playerIndex].playerCardIndices.Count == 0)
            crowns[place].SetActive(true);
        else
            crowns[place].SetActive(false);

        if (playerIndex == gameData.currentTurnIndex)
            otherTurnIndicators[place].SetActive(true);
        else
            otherTurnIndicators[place].SetActive(false);
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
