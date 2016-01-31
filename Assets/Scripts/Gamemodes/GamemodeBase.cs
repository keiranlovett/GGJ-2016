using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// This is the base class for all game modes.
/// </summary>

namespace TeamUtility.IO.Examples
{
public abstract class GamemodeBase : MonoBehaviour
{
	public abstract Gamemode GetGamemodeType();

	public abstract void OnSetup();
	public abstract void OnTearDown();

	public abstract bool IsRoundFinished();

	public float score = 100;

	public float TotalRoundTime = 1 * 60;

	float m_LastRealTime;
	public GameObject particle;


	public GameObject[] Actors;

	public Text[] ScoreBoard;

	void Update()
	{
		if( GamemodeManager.CurrentGamemode != this )
		{
			return;
		}

		updateScore();


		//Since we are setting Time.timeScale = 0 when a round is finished, we cannot use
		//Time.deltaTime to calculate how much time has passed since the last frame. So we
		//store the real time here to be able to calculate the real deltaTime ourselves
		m_LastRealTime = Time.realtimeSinceStartup;

		if(m_LastRealTime >= TotalRoundTime) {
			Debug.Log("IT'S THE END");
			StartCoroutine (EndGame());
		}
	}

public void updateScore() {

	    Actors = GameObject.FindGameObjectsWithTag("Player");


	for( int i = 0; i < Actors.Length; ++i )
		{

			Debug.Log(Actors[ i ]);

			//	Actors[ i ].OnSetup();

float ActorScore = Actors[i].gameObject.transform.GetChild(1).gameObject.GetComponent<ActorController>().score;
    	ScoreBoard[i].text = ActorScore.ToString();

    	if(ActorScore > score){
    		StartCoroutine (EndGame());
    	}


		}





}

	public IEnumerator EndGame(){
		 particle.GetComponent<ParticleSystem>().enableEmission = true;

		yield return new WaitForSeconds(10f);
		//LoadNextMap();
	}

	/// <summary>
	/// This method is storing PhotonNetwork.time in the rooms custom properties
	/// Since PhotonNetwork.time is synchronized between all players, we can use this to
	/// share time information between all players
	/// </summary>
	protected void SetRoundStartTime()
	{
		ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable();
		newProperties.Add( RoomProperty.StartTime, PhotonNetwork.time );

		PhotonNetwork.room.SetCustomProperties( newProperties );
	}


	void Awake()
	{
		PhotonNetwork.automaticallySyncScene = true;
	}

	/// <summary>
	/// Gets all the necessary data that is needed to load the next map and stores
	/// the information in the rooms custom properties
	/// </summary>
	protected void LoadNextMap()
	{
		//MapQueue has a quick access function that retrieves the next map from the custom properties
		MapQueueEntry map = MapQueue.GetNextMap();

		//We also have to calculate what the index of the next map is, so everybody knows where
		//we are in the map queue
		int currentMapIndex = (int)PhotonNetwork.room.customProperties[ RoomProperty.MapIndex ];

		//The map queue simply loops when all maps have been played
		int nextMapIndex = ( currentMapIndex + 1 ) % MapQueue.GetCurrentMapQueueLength();

		//Store all the information in a photon hashtable that are needed for the next map
		ExitGames.Client.Photon.Hashtable newProperties = new ExitGames.Client.Photon.Hashtable();
		newProperties.Add( RoomProperty.BlueScore, 0 );
		newProperties.Add( RoomProperty.RedScore, 0 );
		newProperties.Add( RoomProperty.MapIndex, nextMapIndex );
		newProperties.Add( RoomProperty.Map, map.Name );
		newProperties.Add( RoomProperty.Mode, (int)map.Mode );

		//And store this information in the rooms custom properties
		PhotonNetwork.room.SetCustomProperties( newProperties );

		//Then load the next map. PhotonNetwork.LoadLevel automatically sends the LoadLevel event
		//to all other clients so that everybody joins the next map. This only has to be called on
		//the master client
		PhotonNetwork.LoadLevel( map.Name );
	}
}
}
