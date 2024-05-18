using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerConnection : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] stuffToActivate;
    
    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        foreach (var stuff in stuffToActivate)
        {
            stuff.SetActive(true);
        }
    }
}
