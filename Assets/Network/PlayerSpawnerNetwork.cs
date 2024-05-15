using Photon.Pun;
using UnityEngine;

public class PlayerSpawnerNetwork : MonoBehaviour
{
    private void Start()
    {
        var player = PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
        player.name = PhotonNetwork.LocalPlayer.NickName;
    }
}
