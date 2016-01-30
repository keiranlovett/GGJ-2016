using UnityEngine;
using System.Collections;

/// <summary>
/// This class defines the specific pickup behavior for the flag
/// It also handles flag drops, returns and captures
/// </summary>
namespace TeamUtility.IO.Examples
{
public class PickupFlag : PickupBase
{
	/// <summary>
	/// How long should the flag remain dropped in the field before being returned automatically
	/// </summary>
	public float ReturnTime;
	public ActorController m_CarryingActorController;
	float m_ReturnTimer;
	Vector3 m_HomePosition;
	public GameObject target;

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


	GameObject GetClosestEnemy (GameObject[] enemies) {
	    GameObject bestTarget = null;
	    float closestDistanceSqr = Mathf.Infinity;
	    Vector3 currentPosition = transform.position;
	    foreach(GameObject potentialTarget in enemies)
	    {
	        Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
	        float dSqrToTarget = directionToTarget.sqrMagnitude;
	        if(dSqrToTarget < closestDistanceSqr)
	        {
	            closestDistanceSqr = dSqrToTarget;
	            bestTarget = potentialTarget;
	        }
	    }
	    return bestTarget;
	}


	void LateUpdate()
	{

		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Player");
		target = GetClosestEnemy(enemies);
		RotateToTarget();
		HandleFlagDrop();
		UpdatePosition();
		UpdateReturnTimer();
	}

	void RotateToTarget(){
		//make character point at target
		Quaternion targetRotation;
		Vector3 targetPos = target.transform.position;
		targetRotation = Quaternion.LookRotation(targetPos - new Vector3(transform.position.x,0,transform.position.z));
		transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y,targetRotation.eulerAngles.y,(20 * Time.deltaTime) * 20);
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

	transform.LookAt(m_CarryingActorController.transform);
   	transform.Translate(5*Vector3.forward*Time.deltaTime);

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

	[PunRPC]
	void OnDrop( Vector3 position )
	{
		Debug.Log("Event Received: Drop Flag");

		m_CarryingActorController.isScoring = false;

		m_CarryingActorController = null;
		transform.position = position;
		m_ReturnTimer = ReturnTime;
	}

	[PunRPC]
	void OnCapture()
	{
		Debug.Log("Event Received: Capture Flag");

		m_CarryingActorController = null;
		transform.position = m_HomePosition;

	}

	[PunRPC]
	void OnReturn()
	{
		Debug.Log("Event Received: Return Flag");

		transform.position = m_HomePosition;
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
		m_CarryingActorController.isScoring = true;

	}
}
}
