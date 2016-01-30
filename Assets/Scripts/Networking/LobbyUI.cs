using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour {
	private List<GameObject> CacheRoomList = new List<GameObject>();

	[Header("Lobby Windows")]
	public GameObject LobbyHeader = null;

	public GameObject PlayerNameWindow = null;
	public GameObject JoinWindow = null;
	public GameObject HostWindow = null;
	public GameObject OptionsWindow = null;
	public GameObject WaitingWindow = null;

	[Header("Join Room")]
	public InputField PlayerName;
	public float UpdateListEach = 5f;
	public Transform RoomPanel = null;
	public GameObject GORoomInfo;

	[Header("Host Room")]
	public InputField RoomNameInput = null;
	public Text TimeText;
	public Text MaxPlayersText;
	public Text MapNameText = null;
	public Image MapPreviewImage = null;
	public Text GameModeNameText = null;
	public Transform GamePanel = null;

	[Header("Room Settings")]
	public Gamemode m_SelectedMode = Gamemode.CaptureTheFlag;
	public string m_SelectedMap = "Greenlands";
	public int m_SelectedTime = 2;
	public int m_SelectedPlayers = 2;


	/// This enum represents the different ways the room list can be sorted
	enum SortRoomList
	{
		None,
		NameAsc,
		NameDesc,
		PlayersAsc,
		PlayersDesc,
		ModeAsc,
		ModeDesc,
		MapAsc,
		MapDesc,
	}
	SortRoomList m_SortRoomList = SortRoomList.None;


	//Temp Lobby Settings for Setup UI
	int m_PlayerCount = 0;
	int m_TimeCount = 0;
	int m_MapCount = 0;
	int m_ModeCount = 0;

	//The finalised list of all gametypes.
	public List<MapQueueEntry> m_MapQueue = new List<MapQueueEntry>();

	void AddMapToQueue(string map, Gamemode mode)
	{
		MapQueueEntry newEntry = new MapQueueEntry
		{
			Name = map,
			Mode = mode,
		};

		m_MapQueue.Add( newEntry );
		UpdateGamesList();
	}


	/// <summary>
	/// used to change the pannels in lobby scene
	/// </summary>
	/// <param name="id"></param>
	public void ChangeWindow(int id)
	{
		switch (id)
		{

		case 0 ://PlayerName
			LobbyHeader.SetActive (false);
			PlayerNameWindow.SetActive(true);
			JoinWindow.SetActive(false);
			HostWindow.SetActive(false);
			OptionsWindow.SetActive(false);
			WaitingWindow.SetActive(false);
			break;
		case 1://Join Room
			PlayerNameWindow.SetActive(false);
			JoinWindow.SetActive(true);
			HostWindow.SetActive(false);
			OptionsWindow.SetActive(false);
			WaitingWindow.SetActive(false);
			break;
		case 2:// Host Room
			PlayerNameWindow.SetActive(false);
			JoinWindow.SetActive(false);
			HostWindow.SetActive(true);
			OptionsWindow.SetActive(false);
			WaitingWindow.SetActive(false);
			break;
		case 3://Options
			PlayerNameWindow.SetActive(false);
			JoinWindow.SetActive(false);
			HostWindow.SetActive(false);
			OptionsWindow.SetActive(true);
			WaitingWindow.SetActive(false);
			break;
		case 4:// Waiting or Loading
			PlayerNameWindow.SetActive(false);
			JoinWindow.SetActive(false);
			HostWindow.SetActive(false);
			OptionsWindow.SetActive(false);
			WaitingWindow.SetActive(true);
			break;

		}
	}

	void Start()
	{

		//makes sure that the header is not displayed unless you have a username
		LobbyHeader.SetActive (false);
		ChangeWindow(0);

		//Start Update List Loop
		InvokeRepeating("UpdateRoomList", 1, UpdateListEach);
		InvokeRepeating("UpdateGamesList", 1, UpdateListEach);

		if( PlayerPrefs.HasKey("LoginUsername") == true ) {
		//	ChatHandler.Instance.ChatUsername = PlayerPrefs.GetString( "LoginUsername" );
		}
		else {
		//	ChatHandler.Instance.ChatUsername = "Pilot " + System.Environment.TickCount % 1000;
		}
		PlayerName.text = "hello";

		if (RoomNameInput != null) { RoomNameInput.text = PlayerName.text + "'s Room (" + Random.Range(0, 9999) + ")"; }
	}


/// <summary>
	/// Sort the room info list with the selected sorting type
	/// </summary>
	void SortRoomInfoList( ref List<RoomInfo> list, SortRoomList sortType )
	{
		switch( sortType )
		{
		case SortRoomList.NameAsc:
			list.Sort( delegate( RoomInfo room1, RoomInfo room2 )
			{
				return room1.name.CompareTo( room2.name );
			} );
			break;
		case SortRoomList.NameDesc:
			list.Sort( delegate( RoomInfo room1, RoomInfo room2 )
			{
				return room2.name.CompareTo( room1.name );
			} );
			break;

		case SortRoomList.PlayersAsc:
			list.Sort( delegate( RoomInfo room1, RoomInfo room2 )
			{
				if( room1.playerCount == room2.playerCount )
				{
					return room1.maxPlayers.CompareTo( room2.maxPlayers );
				}

				return room1.playerCount.CompareTo( room2.playerCount );
			} );
			break;
		case SortRoomList.PlayersDesc:
			list.Sort( delegate( RoomInfo room1, RoomInfo room2 )
			{
				if( room1.playerCount == room2.playerCount )
				{
					return room2.maxPlayers.CompareTo( room1.maxPlayers );
				}

				return room2.playerCount.CompareTo( room1.playerCount );
			} );
			break;

		case SortRoomList.MapAsc:
			list.Sort( delegate( RoomInfo room1, RoomInfo room2 )
			{
				string map1 = (string)room1.customProperties[ RoomProperty.Map ];
				string map2 = (string)room2.customProperties[ RoomProperty.Map ];

				return map1.CompareTo( map2 );
			} );
			break;
		case SortRoomList.MapDesc:
			list.Sort( delegate( RoomInfo room1, RoomInfo room2 )
			{
				string map1 = (string)room1.customProperties[ RoomProperty.Map ];
				string map2 = (string)room2.customProperties[ RoomProperty.Map ];

				return map2.CompareTo( map1 );
			} );
			break;

		case SortRoomList.ModeAsc:
			list.Sort( delegate( RoomInfo room1, RoomInfo room2 )
			{
				int mode1 = (int)room1.customProperties[ RoomProperty.Mode ];
				int mode2 = (int)room2.customProperties[ RoomProperty.Mode ];

				return mode1.CompareTo( mode2 );
			} );
			break;
		case SortRoomList.ModeDesc:
			list.Sort( delegate( RoomInfo room1, RoomInfo room2 )
			{
				int mode1 = (int)room1.customProperties[ RoomProperty.Mode ];
				int mode2 = (int)room2.customProperties[ RoomProperty.Mode ];

				return mode2.CompareTo( mode1 );
			} );
			break;
		}
	}


	string GetGamemodeShortform( Gamemode mode )
	{
		switch( mode )
		{
		case Gamemode.Deathmatch:
			return "DM";
		case Gamemode.CaptureTheFlag:
			return "CTF";
		case Gamemode.TeamDeathmatch:
			return "TDM";
		}

		return "N/A";
	}

	/// <summary>
	/// called to update the join list
	/// </summary>
	public void UpdateGamesList()
	{
		//Removed old list
		if (CacheRoomList.Count > 0)
		{
			foreach (GameObject g in CacheRoomList)
			{
				Destroy(g);
			}
			CacheRoomList.Clear();
		}
		//Update List
		for( int i = 0; i < m_MapQueue.Count; ++i ) {
			MapQueueEntry entry = m_MapQueue[ i ];
			GameObject r = Instantiate(GORoomInfo) as GameObject;
//			r.GetComponent<bs_RoomInfo>().GetInfo( entry.Name + " [" + GetGamemodeShortform( entry.Mode ) + "]");
			r.transform.SetParent(GamePanel, false);
			CacheRoomList.Add(r);

			//m_MapQueue.RemoveAt( i );
		}
	}



	/// <summary>
	/// called to update the join list
	/// </summary>
	public void UpdateRoomList()
	{
/*		//Removed old list
		if (CacheRoomList.Count > 0)
		{
			foreach (GameObject g in CacheRoomList)
			{
				Destroy(g);
			}
			CacheRoomList.Clear();
		}
*/
		List<RoomInfo> roomInfoList = new List<RoomInfo>();
		roomInfoList = new List<RoomInfo>( PhotonNetwork.GetRoomList() );
		//Sort the list first
		SortRoomInfoList( ref roomInfoList, m_SortRoomList );


		for (int i = 0; i < roomInfoList.Count; i++)
		{

			RoomListEntry(roomInfoList[i]);

	//		CacheRoomList.Add(r);
	//
		}

	}

	public void RoomListEntry(RoomInfo roomInfo){
		GameObject r = Instantiate(GORoomInfo) as GameObject;
		r.GetComponent<bs_RoomInfo>().GetInfo(roomInfo);
		r.transform.SetParent(RoomPanel, false);
	}

	/// <summary>
	/// when this gets disabled
	/// </summary>
	void OnDisable()
	{
		StopAllCoroutines();
	}


	public void QuitLobby()
	{

//		ChatHandler.Instance.Client.Disconnect();
		MultiplayerConnector.Instance.Disconnect();
		//Application.LoadLevel( "" );
		Application.Quit();

		//if in editor stop it from playing
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#endif
	}

	/// <summary>
	/// changes username and sets it as your PhotonNetwork.player.name
	/// </summary>
	/// <param name="n"></param>
	public void UpdatePlayerName(InputField n)
	{
		if (System.String.IsNullOrEmpty(n.text))
		{
			Debug.Log("Player Name can not be empty! Please enter a player name now.");
		}
		else
		{
			PhotonNetwork.player.name = n.text;
//			ChatHandler.Instance.ChatUsername = n.text;

//			PlayerPrefs.SetString( "LoginUsername", ChatHandler.Instance.ChatUsername );
//			ChatHandler.Instance.Connect();
			MultiplayerConnector.Instance.Connect();

			LobbyHeader.SetActive (true);
			ChangeWindow(1);
		}
	}

	/// <summary>
	/// Chages how long the round will be
	/// </summary>
	/// <param name="b"></param>
	public void ChangeTimer(bool timerBool)
	{
		if (timerBool)
		{
			if (m_TimeCount < ServerOptions.RoomTime.Length)
			{
				m_TimeCount++;
				if (m_TimeCount > (ServerOptions.RoomTime.Length - 1))
				{
					m_TimeCount = 0;

				}

			}
		}
		else
		{
			if (m_TimeCount < ServerOptions.RoomTime.Length)
			{
				m_TimeCount--;
				if (m_TimeCount < 0)
				{
					m_TimeCount = ServerOptions.RoomTime.Length - 1;

				}
			}
		}

		m_SelectedTime = ServerOptions.RoomTime[m_TimeCount];
		TimeText.text = m_SelectedTime.ToString();

	}
	/// <summary>
	/// Chages max players for the round will be
	/// </summary>
	/// <param name="b"></param>
	public void ChangeMaxPlayer(bool playerBool)
	{
		if (playerBool)
		{
			if (m_PlayerCount < ServerOptions.AvaliablePlayers.Length)
			{
				m_PlayerCount++;
				if (m_PlayerCount > (ServerOptions.AvaliablePlayers.Length - 1))
				{
					m_PlayerCount = 0;

				}

			}
		}
		else
		{
			if (m_PlayerCount < ServerOptions.AvaliablePlayers.Length)
			{
				m_PlayerCount--;
				if (m_PlayerCount < 0)
				{
					m_PlayerCount = ServerOptions.AvaliablePlayers.Length - 1;

				}
			}
		}


		m_SelectedPlayers = ServerOptions.AvaliablePlayers[m_PlayerCount];
		MaxPlayersText.text = "Players: "+  m_SelectedPlayers.ToString();
	}
	/// <summary>
	/// Chages what map will be played for the round will be
	/// </summary>
	/// <param name="b"></param>
	public void ChangeMap(bool mapBool)
	{
		if (mapBool)
		{
			if (m_MapCount < ServerOptions.AvailableMaps.Length)
			{
				m_MapCount++;
				if (m_MapCount > (ServerOptions.AvailableMaps.Length - 1))
				{
					m_MapCount = 0;
				}
			}
		}
		else
		{
			if (m_MapCount < ServerOptions.AvailableMaps.Length)
			{
				m_MapCount--;
				if (m_MapCount < 0)
				{
					m_MapCount = ServerOptions.AvailableMaps.Length - 1;
				}
			}
		}
	//	MapPreviewImage.sprite = Lobby.MPScene[m_SelectedMap].PreviewImage;
		m_SelectedMap = ServerOptions.AvailableMaps[m_MapCount];

		MapNameText.text = m_SelectedMap;
	}


	/// <summary>
	/// Chages what map will be played for the round will be
	/// </summary>
	/// <param name="b"></param>
	public void ChangeGametype(bool gametypeBool)
	{
		if (gametypeBool)
		{
			if (m_ModeCount < ServerOptions.AvailableModes.Length)
			{
				m_ModeCount++;
				if (m_ModeCount > (ServerOptions.AvailableModes.Length - 1))
				{
					m_ModeCount = 0;
				}
			}
		}
		else
		{
			if (m_ModeCount < ServerOptions.AvailableModes.Length)
			{
				m_ModeCount--;
				if (m_ModeCount < 0)
				{
					m_ModeCount = ServerOptions.AvailableModes.Length - 1;
				}
			}
		}
		m_SelectedMode = ServerOptions.AvailableModes[m_ModeCount];
		MapNameText.text = m_SelectedMode.ToString();
	}

	public void AddGame() {
		AddMapToQueue( m_SelectedMap, m_SelectedMode );
	}

	public void CreateRoom(InputField i)
	{
		ChangeWindow(4);
		//Save Room properties for load in room
		ServerOptions.CreateRoom(RoomNameInput.text, MapQueue.ListToString(m_MapQueue), m_SelectedTime, m_SelectedPlayers);
	}

}

