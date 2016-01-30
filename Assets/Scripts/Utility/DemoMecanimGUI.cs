using Photon;
using UnityEngine;
using System.Collections;

public class DemoMecanimGUI : PunBehaviour
{
    #region Properties

    #endregion


    #region Members

    private PhotonAnimatorView m_AnimatorView;  // local animatorView. set when we create our character in CreatePlayerObject()
    private Animator m_RemoteAnimator;          // to display the synchronized values on the right side in the GUI. A third player will simply be ignored (until the second player leaves)

    private float m_SlideIn = 0f;
    private float m_FoundPlayerSlideIn = 0f;
    private bool m_IsOpen = false;

    #endregion



    public void Update()
    {
        FindRemoteAnimator();

        m_SlideIn = Mathf.Lerp( m_SlideIn, m_IsOpen ? 1f : 0f, Time.deltaTime * 9f );
        m_FoundPlayerSlideIn = Mathf.Lerp( m_FoundPlayerSlideIn, m_AnimatorView == null ? 0f : 1f, Time.deltaTime * 5f );
    }

    /// <summary>Finds the Animator component of a remote client on a GameObject tagged as Player and sets m_RemoteAnimator.</summary>
    public void FindRemoteAnimator()
    {
        if( m_RemoteAnimator != null )
        {
            return;
        }

        // the prefab has to be tagged as Player
        GameObject[] gos = GameObject.FindGameObjectsWithTag( "Player" );
        for( int i = 0; i < gos.Length; ++i )
        {
            PhotonView view = gos[ i ].GetComponent<PhotonView>();
            if( view != null && view.isMine == false )
            {
                m_RemoteAnimator = gos[ i ].GetComponent<Animator>();
            }
        }
    }


    #region Photon

    public override void OnJoinedRoom()
    {
        CreatePlayerObject();
    }

    private void CreatePlayerObject()
    {
        Vector3 position = new Vector3( -2, 0, 0 );
        position.x += Random.Range( -3f, 3f );
        position.z += Random.Range( -4f, 4f );

        GameObject newPlayerObject = PhotonNetwork.Instantiate( "Actor", position, Quaternion.identity, 0 );

        PhotonView pv = PhotonView.Get(newPlayerObject);
        if (pv.isMine) {
            newPlayerObject.name = "Actor (Local)";
            Camera.main.gameObject.GetComponent<SmoothFollow>().target = newPlayerObject.transform.Find("ActorController");
        } else {
            newPlayerObject.name = "Actor (Network)";
        }

        m_AnimatorView = newPlayerObject.GetComponent<PhotonAnimatorView>();
    }

    #endregion
}
