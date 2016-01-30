using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine;

/// <summary>
/// Basic script to assign a Player per player in a PUN room.
/// </summary>
/// <remarks>
/// This script is but one possible implementation to have players select a Player in a room.
/// It uses a Custom Property per player to store currently selected Players.
/// When a player joins and someone else didn't pick a Player yet, this script waits.
/// When a Player is selected or a player leaves, this scripts selects a Player if it didn't do that before.
///
/// This could be extended to provide easy access to each player's Player. Alternatively, you could write
/// extension methods for the PhotonPlayer class to access the Custom Property for Players in a seamless way.
/// See TeamExtensions for an example.
/// </remarks>
public class PlayerSync: PunBehaviour
{
    /// <summary>
    /// Defines the available Players per room. There should be at least one Player per available player spot.
    /// </summary>
 //   public Player[] Players = new Player[] { Player.red, Player.blue, Player.yellow, Player.green };

    public int PlayerRange = 4;

    /// <summary>
    /// Property-key for Player Player. the value will be the index of the player's Player in array Players (0...)
    /// </summary>
    public const string PlayerProp = "pc";

    /// <summary>
    /// Player this player selected. Defaults to grey.
    /// </summary>
 //   public Player MyPlayer = Player.grey;

 	public int MyPlayer;

    public bool PlayerPicked { get; set; }


    public override void OnJoinedRoom()
    {
        SelectPlayer();
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        SelectPlayer();
    }

    public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
    {
        // important: SelectPlayer() might cause a call to OnPhotonPlayerPropertiesChanged().
        // to avoid endless recursion (and a crash), we skip calling SelectPlayer() if this player changed props.
        // we could also check which props changed and skip all changes, aside from Player-selection.
        PhotonPlayer player = playerAndUpdatedProps[0] as PhotonPlayer;
        if (player != null && player.isLocal)
        {
            return;
        }

        SelectPlayer();
    }

    public override void OnLeftRoom()
    {
        // Players are select per room.
        Reset();
    }

    /// <summary>
    /// Resets the Player locally. In this class and the PhotonNetwork.player instance.
    /// </summary>
    public void Reset()
    {
        PlayerPicked = false;

        // Players are select per room. to reset, we have to clean the locally cached property in PhotonPlayer, too
        Hashtable PlayerProp = new Hashtable();
        PlayerProp.Add(PlayerProp, null);
        PhotonNetwork.player.SetCustomProperties(PlayerProp);
    }

    /// <summary>
    /// Attempts to select a Player out of the existing, not-yet-taken ones.
    /// </summary>
    /// <remarks>
    /// Available Players are defined in Players.
    /// Players are taken, if their Players index is in a player's Custom Property with the key PlayerProp.
    ///
    /// </remarks>
    public void SelectPlayer()
    {
        if (PlayerPicked)
        {
            return;
        }

        HashSet<int> takenPlayers = new HashSet<int>();

        // check which Players the OTHERS picked. we pick one of the remaining Players.
        foreach (PhotonPlayer player in PhotonNetwork.otherPlayers)
        {
            if (player.customProperties.ContainsKey(PlayerProp))
            {
                int picked = (int)player.customProperties[PlayerProp];
                Debug.Log("Taken Player index: " + picked);
                takenPlayers.Add(picked);
            }
            else
            {
                // a player joined earlier but didn't set a Player yet. as that player has a lower ID, it should select a Player before we do.
                // we will wait to avoid clashes when 2 players join soon after another. we don't want a Player picked twice!
                if (player.ID < PhotonNetwork.player.ID)
                {
                    Debug.Log("Can't select a Player yet. This player has to pick one first: " + player);
                    return;
                }
            }
        }

        //Debug.Log("Taken Players: " + takenPlayers.Count);

        if (takenPlayers.Count == this.PlayerRange)
        {
            Debug.LogWarning("No Player available! All picked. Players length should match MaxPlayers of the room.");
            return;
        }

        // go through the list of available Players and check each if it's taken or not
        // pick the first Player that's not taken
        for (int index = 0; index < this.PlayerRange; index++)
        {
            if (!takenPlayers.Contains(index))
            {
                this.MyPlayer = index++;

                // this stores the picked Player in the server and makes it known to the others (network sync)
                Hashtable PlayerProp = new Hashtable();
                PlayerProp.Add(PlayerProp, index);
                PhotonNetwork.player.SetCustomProperties(PlayerProp); // this goes to the server asap.

                Debug.Log("Selected my Player: " + this.MyPlayer);
                PlayerPicked = true;
                break; // one Player selected. break this loop.
            }
        }
    }
}



		//Set the player character by their ID
//		gameObject.transform.GetChild(0).transform.GetChild(playerID).gameObject.SetActive(true);
