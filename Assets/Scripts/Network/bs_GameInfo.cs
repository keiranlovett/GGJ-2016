using UnityEngine;
using UnityEngine.UI;

public class bs_GameInfo : MonoBehaviour  {

	public Text RoomNameUI;

	public void GetInfo(string roomInfo)
	{



		RoomNameUI.text = roomInfo;


	}


}