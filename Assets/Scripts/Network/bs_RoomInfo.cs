using UnityEngine;
using UnityEngine.UI;

public class bs_RoomInfo : MonoBehaviour  {

	public Text RoomNameUI;
	public Text MapNameUI;
	public Text MaxPlayerUI;
	public Text ButtonText;
	public Image StatusImg;
	[Space(7)]
	public Color AvailableColor;
	public Color FullColor;

public RoomInfo globalRoomInfo;

	/// <summary>
	/// Gets all the info from the room (ie. players, map, ect..)
	/// </summary>
	/// <param name="rn"></param>
	/// <param name="mn"></param>
	/// <param name="p"></param>
	/// <param name="mType"></param>
	public void GetInfo(RoomInfo roomInfo)
	{
		globalRoomInfo = roomInfo;

		//Receive the map and mode data from the rooms custom properties
		string map = (string)roomInfo.customProperties[ RoomProperty.Map ];
		Gamemode mode = (Gamemode)( (int)roomInfo.customProperties[ RoomProperty.Mode ] );


		RoomNameUI.text = roomInfo.name.ToString() + "["+ mode.ToString() + "]";

		MapNameUI.text = map.ToString();

		MaxPlayerUI.text = mode.ToString();


		if (roomInfo.playerCount >= roomInfo.maxPlayers)
		{
			ButtonText.text = "Full";
			StatusImg.color = FullColor;
		}
		else
		{
			ButtonText.text = "Join";
			StatusImg.color = AvailableColor;
		}
	}
	/// <summary>
	/// Join Room
	/// </summary>
	public void EnterRoom()
	{
		//if (roomInfo.playerCount < roomInfo.maxPlayers)
	//	{
			PhotonNetwork.JoinRoom( globalRoomInfo.name );
	//	}
	/*	else
		{
			Debug.Log("This Room is Full");
		}*/
	}

}