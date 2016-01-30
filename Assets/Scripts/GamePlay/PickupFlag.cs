using UnityEngine;
using System.Collections;

/// <summary>
/// This class defines the specific pickup behavior for the flag
/// It also handles flag drops, returns and captures
/// </summary>
public class PickupFlag : PickupBase
{
	/// <summary>
	/// How long should the flag remain dropped in the field before being returned automatically
	/// </summary>
	public float ReturnTime;
	public ActorController m_CarryingActorController;

	float m_ReturnTimer;
	Vector3 m_HomePosition;
	
	bool withBanana=default (bool);
	int tempScore;
	int scoreMultiplier;
	
	void Start()
	{
		tempScore=0;
		scoreMultiplier=6;	//player gains 6 points by holding banana for 1 frame
	}

	void Awake()
	{
		m_HomePosition = transform.position;
	}

	void LateUpdate()
	{
		HandleFlagDrop();
		UpdatePosition();
		UpdateReturnTimer();
	}

	/// <summary>
	/// This handles the automatic return of a flag after a set amount of time
	/// </summary>
	void UpdateReturnTimer()
	{
		if( m_ReturnTimer == -1f )
		{
			return;
		}

		m_ReturnTimer -= Time.deltaTime;

		if( m_ReturnTimer <= 0f )
		{
			m_ReturnTimer = -1f;
			ReturnFlag();
		}
	}

	/// <summary>
	/// This updates the position of the flag when it is carried by a player
	/// The flag will be always behind the carrying player
	/// </summary>
	void UpdatePosition()
	{
		if( m_CarryingActorController == null )
		{
			return;
		}

	//	transform.position = m_CarryingActorController.transform.position;
	}

	/// <summary>
	/// If the carrying player died, send the drop flag event to all players
	/// </summary>
	void HandleFlagDrop()
	{
		if( m_CarryingActorController == null )
		{
			return;
		}

		if( m_CarryingActorController.ragdollController.m_currOffBalance >= m_CarryingActorController.ragdollController.m_balance)
		{
			DropFlag();
		}
	}

	/// <summary>
	/// Determines whether this instance is at the home base
	/// </summary>
	/// <returns></returns>
	public bool IsHome()
	{
		return transform.position == m_HomePosition;
	}

	void DropFlag()
	{
		Debug.Log("Event Sent: Drop Flag");
		if( PhotonNetwork.offlineMode == true )
		{
			OnDrop( transform.position );
		}
		else
		{
			if( PhotonNetwork.isMasterClient == true )
			{
				PhotonView.RPC( "OnDrop", PhotonTargets.AllBuffered, new object[] { transform.position } );
			}
		}
	}

	void ReturnFlag()
	{
		Debug.Log("Event Sent: Return Flag");

		if( PhotonNetwork.offlineMode == true )
		{
			OnReturn();
		}
		else
		{
			if( PhotonNetwork.isMasterClient == true )
			{
				PhotonView.RPC( "OnReturn", PhotonTargets.AllBuffered );
			}
		}
	}

	void CaptureFlag()
	{

		Debug.Log("Event Sent: Capture Flag");

		if( PhotonNetwork.offlineMode == true )
		{
			OnCapture();
		}
		else
		{
			if( PhotonNetwork.isMasterClient == true )
			{
				PhotonView.RPC( "OnCapture", PhotonTargets.AllBuffered );
			}
		}
	}

	[PunRPC]
	void OnDrop( Vector3 position )
	{
		Debug.Log("Event Received: Drop Flag");

		m_CarryingActorController = null;
		transform.position = position;
		m_ReturnTimer = ReturnTime;
		
		withBanana=false;
		IncreaseEnemyScore();
		tempScore=0;
		
	}

	[PunRPC]
	void OnCapture()
	{
		Debug.Log("Event Received: Capture Flag");

		m_CarryingActorController = null;
		transform.position = m_HomePosition;

		//Only the master client increases the score and sends the update to everyone else, to make sure the team only gets 1 point
		if( PhotonNetwork.isMasterClient == true )
		{
			withBanana=true;
			IncreaseEnemyScore();
		}
	}

	[PunRPC]
	void OnReturn()
	{
		Debug.Log("Event Received: Return Flag");

		transform.position = m_HomePosition;
	}

	void IncreaseEnemyScore()
	{
		GamemodeCaptureTheFlag ctfMode = GamemodeManager.CurrentGamemode as GamemodeCaptureTheFlag;
	//	TODO: MICHELLE CODE HERE, probably maybe?!
		while(withBanana==true)
		{
			tempScore++;
			m_CarryingActorController.score+=tempScore*scoreMultiplier;	
			Debug.Log("Event: Scored");
		}
	}

	public override bool CanBePickedUpBy( ActorController actor )
	{

		//If another player is already carrying the flag, no one else can grab it
		if( m_CarryingActorController != null )
		{
			return false;
		}

		return true;
	}

	public override void OnPickup( ActorController actor )
	{
		m_CarryingActorController = actor;

	}
}
