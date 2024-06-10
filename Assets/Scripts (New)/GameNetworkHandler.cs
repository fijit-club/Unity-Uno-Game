using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    public int activePlayers;
    public GameData gameData;
    public CardInfo cardInfo;
    public int scoreForLeaderboard;
    public GameObject thisPlayerTurnIndicator;
    public bool pressedFunoButton;
    public OtherPlayer[] otherPlayersHandler;
    public bool won;
    public Animator userProfileAnim;
    public TimerHandler timer;
    public GameObject[] otherPlayersNamesHandler;
    
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
    [SerializeField] private GameObject[] otherCatchButtons;
    [SerializeField] private Transform movingDeck;
    [SerializeField] private Transform movingDeckPosition;
    
    private bool _assignedPlayerLocation;
    private int _imagesDownloaded;
    private bool _downloadedAllImages;
    private bool _scoreSent;
    private bool _catch;
    private bool _gameStarted;
    private bool _dealtCards;
    
    private void Awake()
    {
        maxPlayers = Bridge.GetInstance().thisPlayerInfo.data.multiplayer.lobbySize;
    }

    public void DisableCatchButtons()
    {
        foreach (var catchButton in otherCatchButtons)
            catchButton.SetActive(false);
        
        for (int i = 0; i < gameData.players.Count; i++)
            FindObjectOfType<PhotonView>().RPC("DisableAllCatchButtons", RpcTarget.Others);
        
        funOButton.SetActive(false);
    }

    public void DisableCatchButtonsNoRPC()
    {
        foreach (var catchButton in otherCatchButtons)
            catchButton.SetActive(false);
        
        funOButton.SetActive(false);
    }
    
    public void FunoButtonPress()
    {
        pressedFunoButton = true;

        int thisPlayerIndex = 0;

        for (int i = 0; i < gameData.players.Count; i++)
        {
            var player = gameData.players[i];
            if (string.Equals(player.playerName, PhotonNetwork.LocalPlayer.NickName))
                thisPlayerIndex = i;
        }
        
        FindObjectOfType<PhotonView>().RPC("SayFuno", RpcTarget.Others, thisPlayerIndex);
        funOButton.SetActive(false);
    }
    
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
            if (string.Equals(player.playerName, PhotonNetwork.LocalPlayer.NickName))
            {
                player.won = true;
                won = true;
            }
        }

        if (playersWon > 0)
            scoreForLeaderboard = 4000 - playersWon;
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

        var lastTurn = gameData.currentTurnIndex;
        GameData tempGameData =
            JsonConvert.DeserializeObject<GameData>((string) PhotonNetwork.CurrentRoom.CustomProperties["GAME"]);
        gameData = tempGameData;
        if (lastTurn != gameData.currentTurnIndex)
        {
            FindObjectOfType<ClientHandler>().caughtCards = false;
        }

        control.AssignCardsOnClients();
        UpdateTurns();
        UpdateOtherPlayerSet(false);

        SendScore();
        
        CheckActivePlayers();
    }

    private void CheckActivePlayers()
    {
        activePlayers = 0;
        foreach (var player in gameData.players)
        {
            if (!player.won)
                activePlayers++;
        }
    }
    
    private void UpdateTurns()
    {
        if (control.assignedCards)
            control.players[0].turn(true);
        if (gameData.currentTurn == PhotonNetwork.LocalPlayer.NickName)
        {
            control.myTurn = true;
            thisPlayerTurnIndicator.SetActive(true);
            userProfileAnim.Play("indicate turn", -1, 0f);
        }
        else
        {
            thisPlayerTurnIndicator.SetActive(false);
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

    public void UpdateCatchButtonPlayers(int playerIndex)
    {
        for (int i = 0; i < gameData.players.Count; i++)
        {
            if (string.Equals(gameData.players[i].playerName, PhotonNetwork.LocalPlayer.NickName))
            {
                UpdateCatchButton(playerIndex, i);
            }
        }
    }
    
    private void UpdateCatchButton(int playerIndex, int playerScreen)
    {
        if (maxPlayers == 2)
            otherCatchButtons[1].SetActive(false);
        else if (maxPlayers == 3)
        {
            CatchButtonSetupFor3Players(playerIndex, playerScreen);
        }
        else if (maxPlayers == 4)
        {
            CatchButtonSetupFor4Players(playerIndex, playerScreen);
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

    private void CatchButtonSetupFor4Players(int playerIndex, int playerScreen)
    {
        if (playerScreen == 0)
        {
            if (playerIndex == 1)
                otherCatchButtons[0].SetActive(false);
            else if (playerIndex == 2)
                otherCatchButtons[1].SetActive(false);
            else if (playerIndex == 3)
                otherCatchButtons[2].SetActive(false);
        }
        else if (playerScreen == 1)
        {
            if (playerIndex == 0)
                otherCatchButtons[2].SetActive(false);
            else if (playerIndex == 2)
                otherCatchButtons[0].SetActive(false);
            else if (playerIndex == 3)
                otherCatchButtons[1].SetActive(false);
        }
        else if (playerScreen == 2)
        {
            if (playerIndex == 0)
                otherCatchButtons[1].SetActive(false);
            else if (playerIndex == 1)
                otherCatchButtons[2].SetActive(false);
            else if (playerIndex == 3)
                otherCatchButtons[0].SetActive(false);
        }
        else if (playerScreen == 3)
        {
            if (playerIndex == 0)
                otherCatchButtons[0].SetActive(false);
            else if (playerIndex == 1)
                otherCatchButtons[1].SetActive(false);
            else if (playerIndex == 2)
                otherCatchButtons[2].SetActive(false);
        }
    }

    private void CatchButtonSetupFor3Players(int playerIndex, int playerScreen)
    {
        if (playerScreen == 0)
        {
            if (playerIndex == 1)
                otherCatchButtons[0].SetActive(false);
            else if (playerIndex == 2)
                otherCatchButtons[2].SetActive(false);
        }
        else if (playerScreen == 1)
        {
            if (playerIndex == 0)
                otherCatchButtons[2].SetActive(false);
            else if (playerIndex == 2)
                otherCatchButtons[0].SetActive(false);
        }
        else if (playerScreen == 2)
        {
            if (playerIndex == 0)
                otherCatchButtons[0].SetActive(false);
            else if (playerIndex == 1)
                otherCatchButtons[2].SetActive(false);
        }
    }

    private List<int> playersWith1Left = new List<int>();

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

        otherPlayers[place].transform.parent.parent.GetComponent<OtherPlayerHandler>().playerName = gameData.players[playerIndex].playerName;
        
        if (gameData.players[playerIndex].playerCardIndices.Count == 0)
            crowns[place].SetActive(true);
        else
            crowns[place].SetActive(false);

        _catch = false;

        if (gameData.players[playerIndex].playerCardIndices.Count > 1 && playersWith1Left.Contains(playerIndex))
        {
            otherCatchButtons[place].SetActive(false);
            playersWith1Left.Remove(playerIndex);
        }
        
        if (gameData.players[playerIndex].playerCardIndices.Count == 0)
            otherCatchButtons[place].SetActive(false);
        
        
        if (gameData.players[playerIndex].playerCardIndices.Count == 1)
        {
            if (!playersWith1Left.Contains(playerIndex) && !won)
            {
                otherCatchButtons[place].SetActive(true);
                playersWith1Left.Add(playerIndex);
            }
            otherCatchButtons[place].GetComponent<Button>().onClick.AddListener(() =>
            {
                if (!_catch)
                {
                    otherCatchButtons[place].SetActive(false);
                    CatchButton(playerIndex);
                    _catch = true;
                }
            });
        }

        if (playerIndex == gameData.currentTurnIndex)
            otherTurnIndicators[place].SetActive(true);
        else
            otherTurnIndicators[place].SetActive(false);
    }

    public void CatchButton(int playerIndex)
    {
        FindObjectOfType<PhotonView>().RPC("Catch", RpcTarget.Others, playerIndex);
    }
    
    public void DrawAnimationOther(string playerName)
    {
        Transform playerPlace = null;
        print(playerName);

        foreach (var otherPlayer in otherPlayersNamesHandler)
        {
            if (string.Equals(otherPlayer.GetComponent<OtherPlayerHandler>().playerName, playerName))
                playerPlace = otherPlayer.transform;
        }

        movingDeck.position = movingDeckPosition.position;
        if (playerPlace != null)
        {
            print(playerPlace.GetComponent<OtherPlayerHandler>().playerName);
            print(playerPlace.gameObject.name);
            movingDeck.DOMove(playerPlace.GetComponent<OtherPlayerHandler>().location.position, .1f).OnComplete(
                () =>
                {
                    movingDeck.position = movingDeckPosition.position;
                });}
        //drawOther.Play("draw other", -1, 0f);
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Player player = new Player();
            player.playerName = targetPlayer.NickName;
            player.avatar = Bridge.GetInstance().thisPlayerInfo.data.multiplayer.avatar;

            List<string> playerNameList = new List<string>();

            foreach (var playerData in gameData.players)
            {
                playerNameList.Add(playerData.playerName);
            }

            if (!playerNameList.Contains(targetPlayer.NickName))
                gameData.players.Add(player);

            PhotonNetwork.CurrentRoom.SetCustomProperties(GetJSONGameData());
        }
        
        if (gameData.players.Count >= maxPlayers)
        {
            if (PhotonNetwork.IsMasterClient && !_dealtCards)
            {
                _dealtCards = true;
                StartGame();
            }
            else
                DeactivateWaitingUI();
        }
    }

    private void UpdatePlayerProps()
    {
        Player player = new Player();
        player.playerName = PhotonNetwork.LocalPlayer.NickName;
        player.avatar = Bridge.GetInstance().thisPlayerInfo.data.multiplayer.avatar;
        //gameData.players.Add(player);

        Hashtable hash = new Hashtable();
        string playerJSON = JsonConvert.SerializeObject(player);
        hash.Add("GAME", playerJSON);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            AddMasterPlayer();
        else
            UpdatePlayerProps();

        StartCoroutine(DownloadImage(Bridge.GetInstance().thisPlayerInfo.data.multiplayer.avatar,
            thisPlayerProfileImage, true));
        thisPlayerName.text = PhotonNetwork.LocalPlayer.NickName;
    }

    private IEnumerator DelayRPC()
    {
        yield return new WaitForSeconds(2f);
        SendRPCToMaster();
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
