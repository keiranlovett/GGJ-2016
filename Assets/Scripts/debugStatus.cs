using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class debugStatus : Photon.MonoBehaviour {

    public Text PhotonStatus;

    public Text PhotonInfo;


    void Update()
    {
        if (PhotonStatus != null)
        {
            PhotonStatus.text = "Connection State: <color=#549BFF>" + PhotonNetwork.connectionStateDetailed.ToString() + "</color>";
        }
        if (PhotonInfo != null)
        {
            PhotonInfo.text = "<color=orange>" + PhotonNetwork.countOfPlayersInRooms + "</color> Players in Rooms   " +
           "<color=orange>" + PhotonNetwork.countOfPlayersOnMaster + "</color> Players in Lobby   " +
            "<color=orange>" + PhotonNetwork.countOfPlayers + "</color> Players in Server   " +
            "<color=orange>" + PhotonNetwork.countOfRooms + "</color> Room are Create";
        }
    }
}
