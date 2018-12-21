using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUnitController : MonoBehaviour {
	public static GameControllerScript gameController;
	private GameObject goal;
	private UnityEngine.AI.NavMeshAgent agent;
	private GameObject turret;
	private GameObject barrel;
	private GameObject enemyTarget;
	private GameObject[] enemyTargetList;
	private GameObject current;
	private GameObject closest;
	private GameObject mainCam;
	private bool gunReady;
	public bool isTank;
	public bool isBike;
	private bool targetJustSighted;
	public float startHealth;
	public float health;
	public bool alive;
	public int damage;
	public float reloadTime;
	public Vector3 dyingV;
	private float ownSpeed;

	public Image healthBar;
	private Transform healthUI;
	public GameObject wreck;
	//private GameObject wreckClone;
	AudioSource audioSource;
	AudioSource turretAudioSource;


	void Start () {
		gameController = GameControllerScript.Instance ();
		agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
		goal = GameObject.Find ("Goal");
		agent.destination = goal.transform.position;
		health = startHealth;
		ownSpeed = agent.speed;
		agent.speed = 5;  // makes all enemies move slowly before ambush is spotted
		mainCam = GameObject.Find ("Main Camera");

		enemyTargetList = new GameObject[gameController.friendCount];
		for (int i = 0; i < gameController.friendList.Length; i++) {
			enemyTargetList [i] = gameController.friendList [i];
			//Debug.Log ("In list: " + enemyTargetList [i].name);
		}

		if (isTank) {
			turret = GameObject.Find (gameObject.name + "/Turret");
			barrel = GameObject.Find (gameObject.name + "/Turret/Barrel");

			closest = null;

			enemyTarget = FindTarget ();	// finding closest target
			gunReady = true;
		}

		healthUI = gameObject.transform.Find ("HealthCanvas");
		healthUI.transform.LookAt (mainCam.transform);
		audioSource = GetComponent<AudioSource>();
		if (isTank)
			turretAudioSource = turret.GetComponent<AudioSource>();
	}

	void FixedUpdate () { 
		if (alive && !gameController.ambushRevealed) {  // checking distance to friend units before ambush is revealed
			enemyTarget = FindTarget ();
			var heading = (enemyTarget.transform.position) - transform.position;
			var distance = heading.magnitude;
			RaycastHit hit;
			//if (isTank)
				//Debug.Log (distance);
			if (Physics.Raycast(transform.position, heading, out hit) && distance < 80)  // distance too short(?)
				gameController.OpenFire ();  // when enemies get too close to friends, ambush is revealed
		}

		if (gameController.ambushRevealed)
			agent.speed = ownSpeed;  // full speed restored
		
		if (isTank && alive && gameController.ambushRevealed){  // targeting player units when ambush has been spotted
			RaycastHit hit;

			if (enemyTarget != null){
				var heading = (enemyTarget.transform.position) - barrel.transform.position;
				var distance = heading.magnitude;

				if (Physics.Raycast(barrel.transform.position, heading, out hit))
				{
					if(hit.collider.tag == "Player"){
						Debug.DrawRay(barrel.transform.position, heading * distance, Color.red);  // transform.TransformDirection(Vector3.forward)  // + new Vector3(0f, 0f, 0.5f)
						//Debug.Log(hit.distance);
						//targetSighted = true;
						if(gunReady){
							StartCoroutine(Shoot(hit.distance));
						}
					}
					else{
						//Debug.DrawRay(barrel.transform.position, heading * hit.distance, Color.white); 		// to enemyTarget, not hitting
					}
					//turret.transform.LookAt (enemyTarget.transform);
					Vector3 targetPositionHor = new Vector3( enemyTarget.transform.position.x, this.transform.position.y, enemyTarget.transform.position.z ) ;
					turret.transform.LookAt (targetPositionHor);
					//Vector3 targetPositionVer = new Vector3( barrel.transform.position.x, barrel.transform.position.y, barrel.transform.position.z + turret.transform.position.x ) ;  // NG
					//barrel.transform.LookAt (targetPositionVer);
				}

				enemyTarget = FindTarget ();
			}
		}

		var goalDist = goal.transform.position - transform.position;
		var goalDistM = goalDist.magnitude;
		//Debug.Log (gameObject.name + " remainingDistance " + goalDistM);
		if (goalDistM < 15) {  																	// escape mechanic
			Destroy(gameObject);
			//Debug.Log (gameObject.name + " escaped!");
			gameController.enemyCount--;
			gameController.escapedCount++;
		}

		healthUI.transform.LookAt (mainCam.transform);
	}


	GameObject FindTarget() {
		float distance = Mathf.Infinity;
		Vector3 position = transform.position;
		bool enemyAlive = false;
		foreach (GameObject target in enemyTargetList)		//		finding closest enemy
		{
			if (target.GetComponent<PlayerUnitController> ().alive){  		//  CHECK TYPE
				Vector3 diff = target.transform.position - position;
				float curDistance = diff.sqrMagnitude;
				if (curDistance < distance)
				{
					closest = target;
					distance = curDistance;
					enemyAlive = true;
				}
			}
		}

		if (current != closest) {
			current = closest;
			targetJustSighted = true;
			//Debug.Log (gameObject.name + " new target: " + closest.name);
		}

		if (!closest.GetComponent<PlayerUnitController> ().alive)
			enemyAlive = false;
		if (!enemyAlive)
			closest = null;

		return closest;
	}

	IEnumerator Shoot(float distance) {
		gunReady = false;
		//Debug.Log("Tank aiming!");
		if (targetJustSighted)
			yield return new WaitForSeconds (2f);
		ShootExplode ();
		//Debug.Log("Tank just shot!");
		if (CheckForHit (distance)) {
			enemyTarget.GetComponent<PlayerUnitController> ().Hit (damage);
		}
		yield return new WaitForSeconds (4f);
		//Debug.Log("Tank reloaded!");
		gunReady = true;
		targetJustSighted = false;
	}

	bool CheckForHit(float distance) {  								// MISSING hitChanceMod
		float hitChance = 0f;
		hitChance = (120f - distance) * 0.5f;
		Debug.Log (gameObject.name + " vs " + enemyTarget.name + " distance, hitChance " + distance + ", " + hitChance);

		float rand = Random.value;
		rand = rand * 100;  // rand = 0...100
		if (rand <= hitChance) {
			Debug.Log (gameObject.name + " vs " + enemyTarget.name + " rand " + rand + ", hit");
			return true;
		}
		else
			Debug.Log (gameObject.name + " vs " + enemyTarget.name + " rand " + rand + ", miss");

		return false;
	}

	void ShootExplode() {
		var exp = turret.GetComponent<ParticleSystem> ();
		exp.Play ();
		turretAudioSource.Play ();
	}

	public void Hit(int dmg) {
		health -= dmg;
		healthBar.fillAmount = health / startHealth;
		//if (health > 1)
		//Debug.Log (gameObject.name + " health: " + health + ", healthBar.fillAmount: " + healthBar.fillAmount);
		if (health < 1 && alive){
			Explode ();
			//Debug.Log (gameObject.name + " dead");
		}
	}
	void Explode() {  // upon death
		var exp = GetComponent<ParticleSystem> ();
		exp.Play ();
		alive = false;
		dyingV = agent.velocity;  // for wreck
		agent.velocity = new Vector3(agent.velocity.x * 0.5f, agent.velocity.y * 0.5f, agent.velocity.z * 0.5f);  // halves speed upon death
		agent.ResetPath ();

		if (wreck != null) {
			wreck = (GameObject)Instantiate (wreck, transform.position, transform.rotation);
			//wreck.GetComponent<Rigidbody> ().AddForce (dyingV, ForceMode.VelocityChange);  // doesn't work
			//wreck.GetComponent<CarWreck>().speed = GetComponent<CarForwards>().agent.speed;
			Destroy (gameObject);
		}

		gameController.enemyCount--;
	}

	void OnMouseOver()
	{
		if(Input.GetButtonDown ("Fire2")) {
			gameController.AttackOrder (gameObject);
		}
	}
}