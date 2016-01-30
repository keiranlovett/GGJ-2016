using UnityEngine;
using System.Collections;

public class ServerOptions
{

	public static int[] RoomTime = new int [] {
		2,4,6,8,10
	};

	public static int[] AvaliablePlayers = new int [] {
		2,4,6,8,10
	};

	public static string[] AvailableMaps = new string[]
	{
		"Greenlands",
		"City"
	};

	public static Gamemode[] AvailableModes = new Gamemode[]
	{
		Gamemode.CaptureTheFlag,
		Gamemode.Deathmatch,
		Gamemode.TeamDeathmatch
	};

	public static void CreateRoom( string name, string mapQueueString, int time = 5, int players = 2 )
	{
		MapQueueEntry firstMap = MapQueue.GetSingleEntryInMapQueue( mapQueueString, 0 );

		RoomOptions roomOptions = new RoomOptions();
		roomOptions.maxPlayers = (byte)players;

		roomOptions.customRoomProperties = new ExitGames.Client.Photon.Hashtable();
		roomOptions.customRoomProperties.Add( RoomProperty.MapQueue, mapQueueString );
		roomOptions.customRoomProperties.Add( RoomProperty.MapIndex, 0 );
		roomOptions.customRoomProperties.Add( RoomProperty.RedScore, 0 );
		roomOptions.customRoomProperties.Add( RoomProperty.BlueScore, 0 );
		roomOptions.customRoomProperties.Add( RoomProperty.Map, firstMap.Name );
		roomOptions.customRoomProperties.Add( RoomProperty.Mode, (int)firstMap.Mode );
		roomOptions.customRoomProperties.Add( RoomProperty.Time, time );

		roomOptions.customRoomPropertiesForLobby = new string[] {
			RoomProperty.Map,
			RoomProperty.Mode,
			RoomProperty.Time,
		};

		PhotonNetwork.JoinOrCreateRoom( name, roomOptions, MultiplayerConnector.Lobby );
	}
}
