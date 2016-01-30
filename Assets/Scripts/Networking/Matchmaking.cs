using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MatchmakingType
{
	Random,
	RoomProperties
}

public class Matchmaking : MonoBehaviour
{
	public MatchmakingType SelectedMatchmakingType = MatchmakingType.RoomProperties;

	Dictionary<string, bool> m_MapSelection = new Dictionary<string, bool>();
	Dictionary<Gamemode, bool> m_ModeSelection = new Dictionary<Gamemode, bool>();

	bool m_IsMatchmakingStarted = false;
	int m_JoinAttempt = 0;

	List<MapQueueEntry> m_MatchmakingMapQueue = new List<MapQueueEntry>();

	void Awake()
	{
		for( int i = 0; i < ServerOptions.AvailableMaps.Length; ++i )
		{
			m_MapSelection.Add( ServerOptions.AvailableMaps[ i ], true );
		}

		for( int i = 0; i < ServerOptions.AvailableModes.Length; ++i )
		{
			m_ModeSelection.Add( ServerOptions.AvailableModes[ i ], true );
		}
	}


	public bool IsMapSelected( string map )
	{
		if( m_MapSelection.ContainsKey( map ) == false )
		{
			return false;
		}

		return m_MapSelection[ map ];
	}

	public bool IsModeSelected( Gamemode mode )
	{
		if( m_ModeSelection.ContainsKey( mode ) == false )
		{
			return false;
		}

		return m_ModeSelection[ mode ];
	}

	public void SetMapSelection( string map, bool value )
	{
		if( m_MapSelection.ContainsKey( map ) == false )
		{
			return;
		}

		m_MapSelection[ map ] = value;
	}

	public void SetModeSelection( Gamemode mode, bool value )
	{
		if( m_ModeSelection.ContainsKey( mode ) == false )
		{
			return;
		}

		m_ModeSelection[ mode ] = value;
	}

	public bool IsMatchmakingStarted()
	{
		return m_IsMatchmakingStarted;
	}

	public string GetMatchmakingInfo()
	{
		switch( SelectedMatchmakingType )
		{
		case MatchmakingType.Random:
			return "";
		case MatchmakingType.RoomProperties:
			return "Join attempt " + m_JoinAttempt + "/" + m_MatchmakingMapQueue.Count;
		}
		return "";
	}

	public void CancelMatchmaking()
	{
		m_IsMatchmakingStarted = false;
		StopAllCoroutines();
	}

	void OnPhotonRandomJoinFailed()
	{
		if( SelectedMatchmakingType == MatchmakingType.Random )
		{
			CreateRandomMatchmakingServer();
		}
		else if( SelectedMatchmakingType == MatchmakingType.RoomProperties )
		{
			Invoke( "MakeRoomPropertiesMatchmakingJoinAttempt", 1f );
		}
	}

	public void StartMatchmaking()
	{
		if( IsMatchmakingStarted() == true )
		{
			return;
		}

		m_IsMatchmakingStarted = true;
		m_JoinAttempt = 0;
		m_MatchmakingMapQueue = CreateRoomPropertiesMapQueue();

		switch( SelectedMatchmakingType )
		{
		case MatchmakingType.Random:
			DoRandomMatchmaking();
			break;
		case MatchmakingType.RoomProperties:
			MakeRoomPropertiesMatchmakingJoinAttempt();
			break;
		}
	}

	#region Random Matchmaking
	void DoRandomMatchmaking()
	{
		PhotonNetwork.JoinRandomRoom();
	}

	void CreateRandomMatchmakingServer()
	{
		string serverName = "Keiran's Server";
		string mapQueueString = "City#0~Greenlands#1~City#2~Greenlands#0~City#1~Greenlands#2";

		ServerOptions.CreateRoom( serverName, mapQueueString );
	}
	#endregion

	#region Room Properties Matchmaking
	void MakeRoomPropertiesMatchmakingJoinAttempt()
	{
		if( m_JoinAttempt < m_MatchmakingMapQueue.Count )
		{
			MapQueueEntry searchForMap = m_MatchmakingMapQueue[ m_JoinAttempt ];

			ExitGames.Client.Photon.Hashtable expectedProperties = new ExitGames.Client.Photon.Hashtable();
			expectedProperties.Add( RoomProperty.Map, searchForMap.Name );
			expectedProperties.Add( RoomProperty.Mode, searchForMap.Mode );

			PhotonNetwork.JoinRandomRoom( expectedProperties, 0 );

			m_JoinAttempt++;
		}
		else
		{
			CreateRoomPropertiesMatchmakingServer();
		}
	}

	/// <summary>
	/// This methods takes the users map and mode selections and builds
	/// a list which each possible map/mode pair, this can then be used
	/// to start matchmaking searches for the different combinations
	/// </summary>
	/// <returns></returns>
	List<MapQueueEntry> CreateRoomPropertiesMapQueue()
	{
		List<MapQueueEntry> mapQueue = new List<MapQueueEntry>();

		//Iterate over all available maps
		foreach( KeyValuePair<string, bool> mapPair in m_MapSelection )
		{
			//and iterate over all available gamemodes
			foreach( KeyValuePair<Gamemode, bool> modePair in m_ModeSelection )
			{
				//If both the map and the mode are selected to be included in matchmaking
				if( mapPair.Value == true &&
					modePair.Value == true )
				{
					//add this pair to the list
					mapQueue.Add( new MapQueueEntry
					{
						Name = mapPair.Key,
						Mode = modePair.Key,
					} );
				}
			}
		}

		return mapQueue;
	}

	/// <summary>
	/// If no suitable room was found through room properties matchmaking, we just create a new one
	/// </summary>
	void CreateRoomPropertiesMatchmakingServer()
	{
		string serverName = "Keirans's Server";

		ServerOptions.CreateRoom( serverName, MapQueue.ListToString( m_MatchmakingMapQueue ) );
	}
	#endregion
}