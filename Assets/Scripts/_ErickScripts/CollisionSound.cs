using UnityEngine;
using System.Collections;

public class CollisionSound : MonoBehaviour {

    AudioSource myAudio;
    public bool spawnMultiple; //plays a new sound even if a current one is already running if true.

	// Use this for initialization
	void Start () {
        myAudio = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnCollisionEnter()
    {
        if (myAudio.isPlaying && spawnMultiple) myAudio.Play();
        if (!myAudio.isPlaying) myAudio.Play();
    }

}
