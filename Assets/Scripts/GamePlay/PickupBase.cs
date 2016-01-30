using UnityEngine;
using System.Collections;
namespace TeamUtility.IO.Examples
{
/// <summary>
/// All pickups (flag, health) inherit from this class
/// It defines the basic pickup functionality between the pickup and the actor and passes it on
/// </summary>
public abstract class PickupBase : MonoBehaviour
{
	/// Determines whether this instance [can be picked up by the specified actor.
	/// THis will be defined by the specific pickup script
	public abstract bool CanBePickedUpBy( ActorController actor );

	/// Called when a actor successfully picked up the object
	public abstract void OnPickup( ActorController actor );

	PhotonView m_PhotonView;
	protected PhotonView PhotonView
	{
		get
		{
			if( m_PhotonView == null )
			{
				m_PhotonView = PhotonView.Get( this );
			}

			return m_PhotonView;
		}
	}

	void OnTriggerEnter( Collider collider )
	{

		ActorController actor = collider.gameObject.GetComponent<ActorController>();
		//We check if the actor can pickup this object two times, once here
		//and once after the event propagated through the network, to ensure that
		//possible lag doesn't interfere with the verification
		if( CanBePickedUpBy( actor ) == true )
		{
			PickupObject( actor );
		}

	}

	void PickupObject( ActorController actor )
	{
		//As with all RPC methods, we branch between online and offline mode here
		if( PhotonNetwork.offlineMode == true )
		{
			OnPickup( actor );
		}
		else
		{
			//We use PhotonTargets.AllBufferedViaServer here to avoid two actors picking up the
			//same object before one of the pickup events has reached the server
			//Check out Part 1 Lesson 4 http://youtu.be/Wn9P4d1KwoQ for more detailed explanations
			PhotonView.RPC(
					"OnPickup"
				, PhotonTargets.AllBufferedViaServer
				, new object[] { actor.PhotonView.viewID }
			);
		}
	}

	[PunRPC]
	protected void OnPickup( int viewId )
	{
		PhotonView view = PhotonView.Find( viewId );

		if( view != null )
		{
			ActorController actor = view.GetComponent<ActorController>();

			//This is the second time we check if the pickup can be collected by the actor
			//In online mode this happens after the event has been received from a remote actor
			if( CanBePickedUpBy( actor ) == true )
			{
				OnPickup( actor );
			}
		}
	}
}
}