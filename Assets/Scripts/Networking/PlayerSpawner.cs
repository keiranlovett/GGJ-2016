using UnityEngine;
using System.Collections;

/// <summary>
/// Creates the synchronized ship objects
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
	void Start()
	{
		//if we are not connected, than we probably pressed play in a level in editor mode.
		//In this case go back to the main menu to connect to the server first
		if( PhotonNetwork.connected == false )
		{
			Application.LoadLevel( "MainMenu" );
			return;
		}
	}

	public void CreateLocalPlayer()
	{
		object[] instantiationData = new object[] {  } ;

		//Notice the differences from PhotonNetwork.Instantiate to Unitys GameObject.Instantiate
		GameObject newShipObject = PhotonNetwork.Instantiate(
			"Ship",
			Vector3.zero,
			Quaternion.identity,
			0,
			instantiationData
		);

//		Transform spawnPoint = GamemodeManager.CurrentGamemode.GetSpawnPoint( team );


//		Ship newShip = newShipObject.GetComponent<Ship>();
	//	newShip.SetTeam( team );
	}

}