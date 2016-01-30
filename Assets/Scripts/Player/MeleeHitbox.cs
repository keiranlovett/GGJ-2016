using UnityEngine;
using System.Collections;
namespace TeamUtility.IO.Examples
{
public class MeleeHitbox : MonoBehaviour
{
	public ActorController playerCombat;
	public float activeTime = 1;

	private float _timer = 0;

	void Update()
	{
		if(_timer >= activeTime)
		{
			_timer = 0;
			gameObject.SetActive(false);
		}
		else
		{
			_timer += Time.deltaTime;
		}
	}

	void OnTriggerEnter(Collider other)
	{
		//Set the damage from the value of the attack
		float damageValue = playerCombat.WeaponDamage;

		//Find the target root and cancel if its us
		GameObject target =  other.transform.root.gameObject;
		Debug.Log(target); //Show me the target for debug reasons

		if(target == this.transform.root.gameObject) {
			return;
		}

	//	Vector3 forceVec = -target.GetComponent<Rigidbody>().velocity.normalized * damageValue*100;
    // 	target.GetComponent<Rigidbody>().AddForce(forceVec);

		//get the root GO controller to apply damage
		if(other.transform.root.transform.Find("ActorController")) {
			ActorController enemyStats = other.transform.root.transform.Find("ActorController").gameObject.GetComponent<ActorController>();
			if(enemyStats) {
				enemyStats.TakeDamage(damageValue);
			}
			Debug.Log("Applying Force to: " + enemyStats.ragdollController.m_hips);
			enemyStats.ragdollController.m_hips.AddForce(transform.forward * damageValue * 100);
			enemyStats.ragdollController.m_currOffBalance = damageValue*2;


		} else {
			//Apply some dramatic Hollywoo force
			target.GetComponent<Rigidbody>().AddForce(transform.forward * damageValue * 100);
		}
	}
}
}