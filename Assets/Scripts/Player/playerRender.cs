using UnityEngine;
using System.Collections;

public class playerRender : MonoBehaviour {

	public int playerID;

	public Transform ActorMesh;

	public PlayerSync PlayerSyncScript;

	void Awake () {
		playerID = PlayerSyncScript.MyPlayer;

		foreach (Transform child in ActorMesh) {
			child.gameObject.SetActive(false);
		}
		ActorMesh.GetChild(playerID++).gameObject.SetActive(true);
	}


}
