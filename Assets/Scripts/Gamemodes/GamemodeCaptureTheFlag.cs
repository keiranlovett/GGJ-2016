using UnityEngine;
using System.Collections;

/// <summary>
/// This class checks the winning conditions and handles round restart
/// </summary>

namespace TeamUtility.IO.Examples
{
public class GamemodeCaptureTheFlag : GamemodeBase
{
	/// <summary>
	/// How long is one match?
	/// </summary>
    private PhotonAnimatorView m_AnimatorView;  // local animatorView. set when we create our character in CreatePlayerObject()

    public Transform[] spawnPoints;


	public override void OnSetup()
	{
		Debug.Log("START ROUND");
		if( PhotonNetwork.isMasterClient == true )
		{
			SetRoundStartTime();
		}

        CreatePlayerObject();
	}

	 private void CreatePlayerObject()
    {
       Vector3 position = spawnPoints[PhotonNetwork.playerList.Length-1].transform.position;
        GameObject newPlayerObject = PhotonNetwork.Instantiate( "Actor", position, Quaternion.identity, 0 );

        PhotonView pv = PhotonView.Get(newPlayerObject);
        if (pv.isMine) {
            newPlayerObject.name = "Actor (Local)";
            Camera.main.gameObject.GetComponent<SmoothFollow>().target = newPlayerObject.transform.Find("ActorController");
        }

        m_AnimatorView = newPlayerObject.GetComponent<PhotonAnimatorView>();
    }

	public override void OnTearDown()
	{
		Destroy( this.gameObject );
	}

	public override Gamemode GetGamemodeType()
	{
		return Gamemode.CaptureTheFlag;
	}



	public override bool IsRoundFinished()
	{
		return false;
	}


}
}
