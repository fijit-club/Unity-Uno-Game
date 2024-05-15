using Photon.Pun;
using UnityEngine;

public class PlayerSpawnerNetwork : MonoBehaviour
{
    private void Start()
    {
        PhotonNetwork.Instantiate("Player", Vector3.zero, Quaternion.identity);
    }
}
