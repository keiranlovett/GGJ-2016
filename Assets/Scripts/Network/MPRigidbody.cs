/////////////////////////////////////////////////////////////////////////////////
//
//	vp_MPRigidbody.cs
//	© VisionPunk. All Rights Reserved.
//	https://twitter.com/VisionPunk
//	http://www.visionpunk.com
//
//	description:	put this script on a rigidbody gameobject to make it sync
//					authoritatively over the network in multiplayer.
//
//					unity physics is non-deterministic, meaning if you run the
//					exact same case on different machines you will end up with
//					slightly different object positions. this is undesireable
//					in multiplayer since positions will start to deviate over time.
//
//					this script will restrict physics calculations and moving platform
//					logic to occur on the master client only. on all other clients
//					the object will be remote-controlled by the master. rigidbodies
//					will come to rest in the exact same place on all machines.
//
//					NOTES:
//					1) this rigidbody can only be moved by explosions, projectiles
//						and custom master-side scripting. if you want the player to
//						be able to push it around (or stand on it to make it tilt etc.)
//						then instead use a 'vp_MPPushableRigidbody'
//					2) if you want the player to be able to ride the platform,
//						don't forget to put it in the 'MovableObject' layer (28),
//						otherwise the player will typically slide off (which might
//						ofcourse also be a desired behavior sometimes)
//					3) though the rigidbody always comes to rest in the exact same
//						place on all machines, due to network latency its state
//						will differ very slightly on the machines while in motion.
//						it is always possible for a bullet to hit a rigidbody on
//						one machine while missing on another. in these rare cases,
//						what happens on the master client is always what counts!
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody))]

public class MPRigidbody : Photon.MonoBehaviour
{

	protected Vector3 m_LastPosition = Vector3.zero;
	protected Quaternion m_LastRotation = Quaternion.identity;

	protected Rigidbody m_Rigidbody = null;
	protected Rigidbody Rigidbody
	{
		get
		{
			m_Rigidbody = GetComponent<Rigidbody>();
			return m_Rigidbody;
		}
	}

	// NOTE: for use by derived classes
	protected Collider m_Collider = null;
	protected Collider Collider
	{
		get
		{
			if ((m_Collider == null) && (Rigidbody != null))
				m_Collider = Rigidbody.GetComponent<Collider>();
			return m_Collider;
		}
	}

	protected Transform m_Transform = null;
	protected Transform Transform
	{
		get
		{
			if (m_Transform == null)
				m_Transform = transform;
			return m_Transform;
		}
	}

	// list of every vp_MPRigidbody in the scene
	public static List<MPRigidbody> Instances = new List<MPRigidbody>();


	/// <summary>
	///
	/// </summary>
	void Awake()
	{
		// add every vp_MPRigidbody to this list so we can refresh master
		// control whether it's enabled / active or not
		Instances.Add(this);
	}


	/// <summary>
	///
	/// </summary>
	protected virtual void Start()
	{

		// set up the photonview to observe this monobehaviour
		photonView.observed = this;
		photonView.synchronization = ViewSynchronization.UnreliableOnChange;

	}


	/// <summary>
	///
	/// </summary>
	protected virtual void FixedUpdate()
	{

		if (photonView.isMine)
			return;

		// NOTE: the below must all happen in FixedUpdate or non-master clients will
		// be knocked off platforms

		// smooth out movement by performing a plain lerp of the last incoming position and rotation
		Transform.position = Vector3.Lerp(Transform.position, m_LastPosition, Time.deltaTime * 15.0f);
		Transform.rotation = Quaternion.Lerp(Transform.rotation, m_LastRotation, Time.deltaTime * 15.0f);
	}


	/// <summary>
	///
	/// </summary>
	protected virtual void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{

		if (stream.isWriting)
		{
			stream.SendNext((Vector3)Transform.position);
			stream.SendNext((Quaternion)Transform.rotation);
		}
		else
		{
			m_LastPosition = (Vector3)stream.ReceiveNext();
			m_LastRotation = (Quaternion)stream.ReceiveNext();
		}

	}


	/// <summary>
	/// refreshes master control whenever a master client handover occurs
	/// </summary>
	protected virtual void OnPhotonPlayerDisconnected(PhotonPlayer player)
	{
		RefreshMasterControl();
	}



	/// <summary>
	/// refreshes master control every time you join a room
	/// </summary>
	protected virtual void OnJoinedRoom()
	{
		RefreshMasterControl();
	}


	/// <summary>
	/// enables rigidbody physics on the master client and disables
	/// it on all other machines
	/// </summary>
	protected virtual void RefreshMasterControl()
	{

		Rigidbody.isKinematic = !PhotonNetwork.isMasterClient;

	}


	/// <summary>
	/// call this to make the master take over all vp_MPRigidbodies in the
	/// scene, regardless of whether they are enabled / active or not
	/// </summary>
	public static void RefreshMasterControlAll()
	{

		foreach (MPRigidbody r in Instances)
		{
			r.RefreshMasterControl();
		}

	}

}







