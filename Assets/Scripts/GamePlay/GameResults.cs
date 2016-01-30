using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameResults : MonoBehaviour {
	public GameObject[] places;
	public Material[] playerMaterials;


	public void GivePlayerScores( int[] playerPoints ) {
		List<int> points = new List<int>();
		for( int i = 0; i < playerPoints.Length; ++i ) {
			points.Add( playerPoints[i] );
		}

		int place = 0;
		for( int num = 0; num < playerPoints.Length; ++num ) {
			int max = -1;//playerPoints[0];
			int player = 0;
			for( int i = 0; i < points.Count; ++i ) {
				if( points[i] != -1 && points[i] > max ) {
					max = points[i];
					player = i;
				}
			}
			points[player] = -1;

			var texts = places[place].GetComponentsInChildren<Text>();
			texts[1].text = max.ToString() + " points!";
			texts[2].text = "Player " + (player+1).ToString();
			places[place].GetComponent<MeshRenderer>().material = playerMaterials[player];
			place++;
		}

	}

	public void Replay() {
		Application.LoadLevel( "GameScene" );
	}

	public void Setup() {
		Application.LoadLevel( "PlayerSetup" );
	}
}
