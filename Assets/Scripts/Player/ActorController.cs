﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Available teams to choose from
/// </summary>
public enum Team
{
	Red,
	Blue,
	None,
}
namespace TeamUtility.IO.Examples
{
public class ActorController : MonoBehaviour {

 	public AudioClip[] audioClips;

	public static ActorController actor;
	Animator animator;

	public bool isScoring;

	public int score;

	Vector3 m_HomePosition;

	public bool isControlledLocally;

	public GameObject target;
	public GameObject meleeHitbox;	// The hitbox for melee attacks

	public BodyPhysicsController ragdollController;
	float rotationSpeed = 15f;

	//Local Variables
	Vector3 inputVec;
	Vector3 targetDirection;
	Vector3 targetDashDirection;
	bool dead = false;
	bool isMoving;
	bool isStrafing;
	bool isBlocking = false;
	bool isStunned = false;
	bool inBlock;
	bool isInAir;
	bool isStealth;
	bool canChain;
	bool chain1;  //used to select which attack to chain to
	bool chain2;  //used to select which attack to chain to
	bool specialAttack2Bool;

	public float WeaponDamage;

	private RaycastHit hit;
 	private Vector3 dir = Vector3.up;

 	//Networked Variables
 	public bool inputJump;
 	public bool inputAttack0;
 	public bool inputAttack1;
 	public bool inputAttack2;
 	public bool inputAttack3;
  	public bool inputLightHit;
	float z;
	float x;

 	PhotonView m_View;
	public PhotonView PhotonView
	{
		get
		{
			return Helper.GetCachedComponent<PhotonView>( gameObject, ref m_View );
		}
	}

	void Awake()
	{
		m_HomePosition = transform.position;
	}

	void Start() {
		animator = this.GetComponent<Animator>();
		ragdollController = this.transform.root.gameObject.GetComponent<BodyPhysicsController>();

		PhotonView view = this.GetComponent<PhotonView>();
		isControlledLocally = view.isMine;
	}

	void OnCollisionEnter(Collision hit)
	{
		Debug.Log(hit.gameObject.tag);
	    if(hit.gameObject.tag == "Kill")
	    {
	    	Debug.Log("OUT OF BOUNDS");
	    	transform.parent.transform.position = m_HomePosition;
	    }
	}

	void Update(){
		if(isScoring) {
			InvokeRepeating("myScore", 1F, 5F);
		} else {
			CancelInvoke("myScore");
		}

		if (Physics.Raycast(transform.position, -Vector3.up, out hit)) {
			if(hit.distance > 2) {
				Debug.Log(hit.distance + "In Air?: " + isInAir);
				StartCoroutine(COSetInAir(0f, 4f));
				if(!isInAir) {
					ragdollController.m_currOffBalance = ragdollController.m_balance;
				}
			}
		}

		if(ragdollController.currState_ == BodyPhysicsController.State.FALLING) {
			dead = true;
							animator.SetBool("Block", true);

		} else {
			dead = false;
										animator.SetBool("Block", false);

		}

		GameObject[] enemies = GameObject.FindGameObjectsWithTag("Player");
		target = GetClosestEnemy(enemies);

		//Normalise back to 0
		inputVec = new Vector3(0,0,0);


		if(isControlledLocally) {
			//Get input from controls
			inputVec = new Vector3(-(InputManager.GetAxis("Vertical")), 0, InputManager.GetAxis("Horizontal"));
		}

		//If we're not dead, we can control the player
		//Anything locally controlled goes here!
		if(!dead) {
			//Apply inputs to animator
			animator.SetFloat("Input X", inputVec.z);
			animator.SetFloat("Input Z", -(inputVec.x));

			if (inputVec.x > .1 || inputVec.x < -.1 || inputVec.z > .1 || inputVec.z < -.1){  //if there is some input (account for controller deadzone)
				//set that character is moving
				animator.SetBool("Moving", true);
				isMoving = true;
				if (InputManager.GetKey(KeyCode.LeftShift) || InputManager.GetAxisRaw("TargetBlock") > .1){  //if strafing
					isStrafing = true;
					animator.SetBool("Running", false);
				}
				else{
					isStrafing = false;
					animator.SetBool("Running", true);
				}
			}
			else{
				//character is not moving
				animator.SetBool("Moving", false);
				animator.SetBool("Running", false);
				isMoving = false;
			}
		} else {
			inputVec = new Vector3(0,0,0);

			animator.SetFloat("Input X", 0);
			animator.SetFloat("Input Z", -0);
		}

		//JUMP
		if (InputManager.GetButtonDown("Jump") && isControlledLocally || inputJump && !isControlledLocally){
			if(isStrafing){
				animator.SetTrigger("JumpTrigger");
				StartCoroutine(COSetInAir(.3f, .2f));
			}
			if(isMoving){
				animator.SetTrigger("JumpForwardTrigger");
				StartCoroutine(COSetInAir(0f, .2f));
			}else{
		    	animator.SetTrigger("JumpTrigger");
				StartCoroutine(COSetInAir(.3f, .2f));
			}
			if(isControlledLocally) {
				inputJump = true;
			}

			if(inputJump && !isControlledLocally) {
				inputJump = false;
			}
		}

		//ATTACK
		if (InputManager.GetButtonDown("Fire0") && isControlledLocally || inputAttack0 && !isControlledLocally){
			animator.SetTrigger("RangeAttack1Trigger");
			StartCoroutine (COStunPause(1.2f));
			if(isControlledLocally) {
				inputAttack0 = true;
			}

			if(inputAttack0 && !isControlledLocally) {
				inputAttack0 = false;
			}
		}
		if (InputManager.GetButtonDown("Fire1") && isControlledLocally || inputAttack1 && !isControlledLocally){
			if(!canChain){ //used for characters who can chain attacks to chain to 2nd Attack
				animator.SetTrigger("Attack1Trigger");
				StartCoroutine(COChainWindow(.2f, .4f));
				StartCoroutine (COStunPause(.6f));
				chain1 = true;
				chain2 = false;
			}
			else{
				if(chain1){  //if within chain time do ATTACK2
					animator.SetTrigger("Attack2Trigger");
					StopAllCoroutines();

					StartCoroutine(COChainWindow(.1f, 2f));
					chain1 = false;
					chain2 = true;
				}
				else if(chain2){
					StopAllCoroutines();
					animator.SetTrigger("Attack3Trigger");
					chain1 = false;
					chain2 = false;
					StartCoroutine (COStunPause(1.2f));
					canChain = false;
				}
			}
			if(isControlledLocally) {
				inputAttack1 = true;
			}

			if(inputAttack1 && !isControlledLocally) {
				inputAttack1 = false;
			}
		}

		if (InputManager.GetButtonDown("Fire2") && isControlledLocally || inputAttack2 && !isControlledLocally){
			animator.SetTrigger("MoveAttack1Trigger");
			StartCoroutine (COStunPause(.9f));
			if(isControlledLocally) {
				inputAttack2 = true;
			}

			if(inputAttack2 && !isControlledLocally) {
				inputAttack2 = false;
			}
		}
		if (InputManager.GetButtonDown("Fire3") && isControlledLocally || inputAttack3 && !isControlledLocally){
			animator.SetTrigger("SpecialAttack1Trigger");
			StartCoroutine (COStunPause(1.7f));
			if(isControlledLocally) {
				inputAttack3 = true;
			}

			if(inputAttack3 && !isControlledLocally) {
				inputAttack3 = false;
			}
		}
		if(InputManager.GetAxis("DashVertical") > .5 || InputManager.GetAxis("DashVertical") < -.5 || InputManager.GetAxis("DashHorizontal") > .5 || InputManager.GetAxis("DashHorizontal") < -.5){
			StartCoroutine (CODirectionalDash(InputManager.GetAxis("DashVertical"), InputManager.GetAxis("DashHorizontal")));
		}

		if(isControlledLocally) {
		} else {
			return;
		}

		UpdateMovement();  //update character position and facing

	}

	/**
	 * CROSS SCRIPT FUNCTIONS
	**/

	public void ApplyDamage(float damageValue) {
		Debug.Log("ApplyDamage:" + damageValue);
		WeaponDamage = damageValue;
		LookAtTarget(1);
		if(meleeHitbox) {
			meleeHitbox.SetActive(true);
		}
	}

	public void TakeDamage(float damageValue) {
		Debug.Log("TakeDamage:" + damageValue);
	}

	void PlayOneShotRandom(){
		AudioClip audio = audioClips[Random.Range(0,audioClips.Length)];
		GetComponent<AudioSource>().PlayOneShot(audio);
	}

	void PlayOneShot(string clip){
		AudioClip audio = Resources.Load<AudioClip>("Sounds/"+clip);
		GetComponent<AudioSource>().PlayOneShot(audio);
	}


	/**
	 * NETWORK FUNCTIONS
	**/

	void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
	{
		//Multiple components need to synchronize values over the network.
		//The SerializeState methods are made up, but they're useful to keep
		//all the data separated into their respective components
		if( stream.isWriting == true )
		{
			// We own this player: send the others our data
			stream.SendNext(inputJump);
			inputJump = false;

			stream.SendNext(inputAttack0);
			inputAttack0 = false;

			stream.SendNext(inputAttack1);
			inputAttack1 = false;

		}
		else
		{
			inputJump = (bool)stream.ReceiveNext();
			inputAttack0 = (bool)stream.ReceiveNext();
			inputAttack1 = (bool)stream.ReceiveNext();
		}

		//SerializeState( stream, info );
		//OtherScript.SerializeState( stream, info );
	}



	/**
	 * HELPER FUNCTIONS
	**/


	public void myScore(){
		score++;
	}
	public IEnumerator CODirectionalDash(float x, float v){
		//check which way the dash is pressed relative to the character facing
		float angle = Vector3.Angle(targetDashDirection,-transform.forward);
		float sign = Mathf.Sign(Vector3.Dot(transform.up,Vector3.Cross(targetDashDirection,transform.forward)));
		// angle in [-179,180]
		float signed_angle = angle * sign;
		//angle in 0-360
		float angle360 = (signed_angle + 180) % 360;
		//deternime the animation to play based on the angle
		if( angle360 > 315 || angle360 < 45){
			animator.SetBool("DashForwardBool", true);
			yield return null;
			animator.SetBool("DashForwardBool", false);
		}
		if (angle360 > 45 && angle360 < 135){
			animator.SetBool("DashRightBool", true);
			yield return null;
			animator.SetBool("DashRightBool", false);
		}
		if (angle360 > 135 && angle360 < 225){
			animator.SetBool("DashBackwardBool", true);
			yield return null;
			animator.SetBool("DashBackwardBool", false);
		}
		if (angle360 > 225 && angle360 < 315){
			animator.SetBool("DashLeftBool", true);
			yield return null;
			animator.SetBool("DashLeftBool", false);
		}
		yield return null;
	}

	public IEnumerator CODash(string direction){
		animator.SetBool(direction, true);
		yield return null;
		animator.SetBool(direction, false);
	}

	public IEnumerator COSetInAir(float timeToStart, float lenthOfTime){
		yield return new WaitForSeconds(timeToStart);
		isInAir = true;
	//	gameObject.GetComponent<SphereCollider>().center = new Vector3(0,2,0);

		yield return new WaitForSeconds(lenthOfTime);
		isInAir = false;
	//	gameObject.GetComponent<SphereCollider>().center = new Vector3(0,0,0);

	}

	public IEnumerator COStunPause(float pauseTime){
		isStunned = true;
		animator.SetFloat("Input X", 0);
		animator.SetFloat("Input Z", 0);
		animator.SetBool("Moving", false);
		yield return new WaitForSeconds(pauseTime);
		isStunned = false;
	}

	public IEnumerator LookAtTarget(float pauseTime){
		isStrafing = true;
		yield return new WaitForSeconds(pauseTime);
		isStrafing = false;
	}

	public IEnumerator COSetLayerWeight(float time){
		animator.SetLayerWeight(1, 1);
		yield return new WaitForSeconds(time);
		float a = 1;
		for (int i = 0; i < 20; i++){
			a -= .05f;
			animator.SetLayerWeight(1, a);
			yield return new WaitForEndOfFrame();
		}
		animator.SetLayerWeight(1, 0);
	}

	public IEnumerator COChainWindow(float timeToWindow, float chainLength){
		yield return new WaitForSeconds(timeToWindow);
		animator.SetBool("CanChainBool", true);
		canChain = true;
		yield return new WaitForSeconds(chainLength);
		canChain = false;
		animator.SetBool("CanChainBool", false);
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


	void UpdateMovement(){
		Vector3 motion = inputVec;  //get movement input from controls
		//reduce input for diagonal movement
		motion *= (Mathf.Abs(inputVec.x) == 1 && Mathf.Abs(inputVec.z) == 1)?.7f:1;

		if (!isStrafing )
			RotateTowardMovementDirection();  //if not strafing, face character along input direction
		if (isStrafing){  //if strafing, look at the target
			//make character point at target
			RotateToTarget();
		}
		GetCameraRelativeMovement();
	}

	void RotateToTarget(){
		//make character point at target
		Quaternion targetRotation;
		Vector3 targetPos = target.transform.position;
		targetRotation = Quaternion.LookRotation(targetPos - new Vector3(transform.position.x,0,transform.position.z));
		transform.eulerAngles = Vector3.up * Mathf.MoveTowardsAngle(transform.eulerAngles.y,targetRotation.eulerAngles.y,(rotationSpeed * Time.deltaTime) * rotationSpeed);
	}


	void GetCameraRelativeMovement(){  //converts control input vectors into camera facing vectors
		Transform cameraTransform = Camera.main.transform;
		// Forward vector relative to the camera along the x-z plane
		Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
		forward.y = 0;
		forward = forward.normalized;
		// Right vector relative to the camera
		// Always orthogonal to the forward vector
		Vector3 right= new Vector3(forward.z, 0, -forward.x);
		//directional inputs
		float v= InputManager.GetAxisRaw("Vertical");
		float h= InputManager.GetAxisRaw("Horizontal");
		float dv= InputManager.GetAxisRaw("DashVertical");
		float dh= InputManager.GetAxisRaw("DashHorizontal");
		// Target direction relative to the camera
		targetDirection = h * right + v * forward;
		// Target dash direction relative to the camera
		targetDashDirection = dh * right + dv * -forward;
	}

	void RotateTowardMovementDirection(){  //face character along input direction
		if(!dead){  //if character isn't dead
			if (inputVec != Vector3.zero && !isStrafing){  //if we're not strafing
				//take the camera orientated input vector and apply it to our characters facing with smoothing
				transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDirection), Time.deltaTime * rotationSpeed);
			}
		}

	}
}
}