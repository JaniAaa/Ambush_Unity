using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnitController : MonoBehaviour {
	public static GameControllerScript gameController;
	private GameObject barrel;
	private GameObject enemyTarget;
	private GameObject[] enemyTargetList;
	private GameObject closest;
	private bool targetJustSighted;
	//private IEnumerator coroutine;
	private bool gunReady;

	public int health;
	public bool alive;
	public int damage;
	public float reloadTime;
	public bool isMG;
	public bool isMortar;
	private GameObject mortarShell;
	//private GameObject MortarShellClone;
	private float reloadRandom;
	private bool targetingOverride;

	MeshRenderer meshRenderer;
	Behaviour halo;
	AudioSource audioSource;
	AudioSource mortarShellAudio;


	void Start () {													// before opening fire accuracy should be 100
		barrel = GameObject.Find(gameObject.name + "/Barrel");
		gameController = GameControllerScript.Instance ();
		enemyTargetList = new GameObject[gameController.enemyCount];
		closest = null;
		for (int i = 0; i < gameController.enemyList.Length; i++) {
			enemyTargetList [i] = gameController.enemyList [i];
			//Debug.Log ("In list: " + enemyTargetList [i].name);
		}

		enemyTarget = FindTarget ();	// finding closest target
		gunReady = true;
		mortarShell = GameObject.Find ("MortarShell");

		halo = (Behaviour)GetComponent("Halo");
		audioSource = GetComponent<AudioSource>();
		mortarShellAudio = mortarShell.GetComponent<AudioSource>();

		targetingOverride = false;
	}

	void FixedUpdate () {
		if(alive){
			if (gameController.overrideTargetedEnemy != null) {
				enemyTarget = gameController.overrideTargetedEnemy;
				targetingOverride = true;
			}

			RaycastHit hit;

			if (enemyTarget != null) {
				var heading = enemyTarget.transform.position - transform.position;
				var distance = heading.magnitude;
				//var direction = heading / distance;
				//int layerMask = 1 << 9;
				//layerMask = ~layerMask;

				if (Physics.Raycast (barrel.transform.position, heading, out hit)) {  // raycast at enemy target to see if can shoot at it
					if (hit.collider.tag == "Enemy" && !isMortar) {
						Debug.DrawRay (barrel.transform.position, heading * distance, Color.red);  // transform.TransformDirection(Vector3.forward)  // + new Vector3(0f, 0f, 0.5f)
						if (gunReady && gameController.ambushRevealed) {
							StartCoroutine (Shoot (hit.distance));
						}
					} else {
						Debug.DrawRay (barrel.transform.position, heading * hit.distance, Color.white); 		// to enemyTarget, not hitting
					}
					if (isMortar) {
						Debug.DrawRay (barrel.transform.position, heading * distance, Color.red);
						if (gunReady && gameController.ambushRevealed) {
							StartCoroutine (ShootIndirect (hit.distance));
						}
					}
					transform.LookAt (enemyTarget.transform);
				}

				// checks for closer targets each frame unless player has designated a target
				if (!targetingOverride) {
					if (isMG)
						enemyTarget = FindTargetMG ();
					else if (isMortar)
						enemyTarget = FindTarget ();
					else
						enemyTarget = FindTargetCannon ();
				}
			} 
			else if (enemyTarget == null) { // if player designated target dies
				targetingOverride = false;
			}
		}
	}

	GameObject FindTarget() {		//		finding closest enemy
		GameObject current = closest;
		float distance = Mathf.Infinity;
		Vector3 position = transform.position;
		foreach (GameObject target in enemyTargetList) {
			if (target != null) {
				if (target.GetComponent<EnemyUnitController> ().alive) {
					Vector3 diff = target.transform.position - position;
					float curDistance = diff.sqrMagnitude;
					if (curDistance < distance) {
						closest = target;
						distance = curDistance;
					}
				}
			}
		}
		if (current != closest) {
			targetJustSighted = true;
			//Debug.Log (gameObject.name + " new target: " + closest.name);
		}
		if (!closest.GetComponent<EnemyUnitController> ().alive){
			//enemyAlive = false;
			closest = null;
		}
		
		return closest;
	}

	GameObject FindTargetMG() {
		GameObject current = closest;
		float distance = Mathf.Infinity;
		Vector3 position = transform.position;
		foreach (GameObject target in enemyTargetList) {
			if (target != null) {
				if (target.GetComponent<EnemyUnitController> ().alive && target.GetComponent<EnemyUnitController> ().isBike) {  // first targeting bikes
					Vector3 diff = target.transform.position - position;
					float curDistance = diff.sqrMagnitude;
					if (curDistance < distance) {
						closest = target;
						distance = curDistance;
					}
				}
				if (closest == null) {  // if no bikes were found
					if (target.GetComponent<EnemyUnitController> ().alive && !target.GetComponent<EnemyUnitController> ().isTank) {  // excluding tanks
						Vector3 diff = target.transform.position - position;
						float curDistance = diff.sqrMagnitude;
						if (curDistance < distance) {
							closest = target;
							distance = curDistance;
						}
					}
				}
			}
		}
		if (current != closest) {
			targetJustSighted = true;
			//Debug.Log (gameObject.name + " new target: " + closest.name);
		}
		if (!closest.GetComponent<EnemyUnitController> ().alive){
			//enemyAlive = false;
			closest = null;
		}
		return closest;
	}

	GameObject FindTargetCannon() {
		GameObject current = closest;
		float distance = Mathf.Infinity;
		Vector3 position = transform.position;
		foreach (GameObject target in enemyTargetList) {
			if (target != null) {
				if (target.GetComponent<EnemyUnitController> ().alive && target.GetComponent<EnemyUnitController> ().isTank) {  // first targeting tanks
					Vector3 diff = target.transform.position - position;
					float curDistance = diff.sqrMagnitude;
					if (curDistance < distance) {
						closest = target;
						distance = curDistance;
					}
				}
				if (closest == null) {  // if no tanks were found
					if (target.GetComponent<EnemyUnitController> ().alive) {
						Vector3 diff = target.transform.position - position;
						float curDistance = diff.sqrMagnitude;
						if (curDistance < distance) {
							closest = target;
							distance = curDistance;
						}
					}
				}
			}
		}
		if (current != closest) {
			targetJustSighted = true;
			//Debug.Log (gameObject.name + " new target: " + closest.name);
		}
		if (!closest.GetComponent<EnemyUnitController> ().alive){
			//enemyAlive = false;
			closest = null;
		}
		return closest;
	}

	IEnumerator Shoot(float distance) {
		if (!isMG) { // slightly randomised reload time
			float rando = Random.value;
			reloadRandom = rando * 2 - 1;
			reloadTime += reloadRandom / 2;
		}
		gunReady = false; // function can't be called each frame
		if (targetJustSighted)
			yield return new WaitForSeconds (reloadTime);
		ShootEffect ();
		if (CheckForHit (distance)) {
			enemyTarget.GetComponent<EnemyUnitController> ().Hit (damage);
			//Debug.Log (gameObject.name + " shot hit!");
		}
		yield return new WaitForSeconds (reloadTime);
		gunReady = true;
		targetJustSighted = false;
		if (isMG)
			audioSource.Stop (); // an awkward way of preventing looping
	}
	IEnumerator ShootIndirect(float distance) {  // for mortars
		gunReady = false;
		float rando = Random.value;
		reloadRandom = rando * 2 - 1;
		reloadTime += reloadRandom / 2;

		if (distance < 200) {  					// max firing distance
			if (targetJustSighted)
				yield return new WaitForSeconds (reloadTime / 2); // extra sighting time
			
			if (enemyTarget != null) {   // enemy could have escaped or died
				Debug.Log (gameObject.name + " shot at " + enemyTarget.name);
				ShootEffect ();
				Vector3 hitPosition = enemyTarget.transform.position;
				float rand = Random.value;
				rand = rand * 20 - 10;
				hitPosition.x = hitPosition.x + rand;  // grenade's landing point slightly randomised
				rand = Random.value;
				rand = rand * 20 - 10;
				hitPosition.z = hitPosition.z + rand;
				yield return new WaitForSeconds (2); // flight time
				mortarShell.transform.position = hitPosition;
				var exp = mortarShell.GetComponent<ParticleSystem> ();
				exp.Play ();
				mortarShellAudio.Play ();

				CheckSplashDmg ();

				targetJustSighted = false;
				yield return new WaitForSeconds (reloadTime / 2);
				gunReady = true;
			}
		}
	}
	void CheckSplashDmg(){	// check splash damage from mortar explosion
		foreach (GameObject target in enemyTargetList) {
			if (target != null) {
				Vector3 diff = target.transform.position - mortarShell.transform.position;
				if (diff.sqrMagnitude < 20) {
					if(target.GetComponent<EnemyUnitController> ().isTank)
						target.GetComponent<EnemyUnitController> ().Hit (damage / 2);
					else
						target.GetComponent<EnemyUnitController> ().Hit (damage);
					//Debug.Log ("Mortar shell landed at: " + MortarShell.transform.position.x + " / " + MortarShell.transform.position.z);
					Debug.Log (target.name + " took splash damage!");
				}
			}
		}
	}

	bool CheckForHit(float distance) {  // checks how likely a shot is to hit
		float hitChance = 0f;
		float hitChanceMod = 0f;
		if (enemyTarget.GetComponent<EnemyUnitController> ().isTank)
			hitChanceMod = 0.8f;
		else if (enemyTarget.GetComponent<EnemyUnitController> ().isBike)
			hitChanceMod = 0.5f;
		else
			hitChanceMod = 1f;
		hitChance = (110 - (distance * 0.1f)) * hitChanceMod;  // 1000m = 10%, 100m = 100%, 0m = 110% for car
		//Debug.Log (gameObject.name + " vs " + enemyTarget.name + " distance, hitChance " + distance + ", " + hitChance);

		float rand = Random.value;
		rand = rand * 100;  // rand = 0...100
		if (rand <= hitChance) {
			//Debug.Log (gameObject.name + " vs " + enemyTarget.name + " rand " + rand + ", hit");
			return true;
		}
		//else
			//Debug.Log (gameObject.name + " vs " + enemyTarget.name + " rand " + rand + ", miss");
			
		
		return false;
	}

	void ShootEffect() {
		var exp = GetComponent<ParticleSystem> ();
		exp.Play ();
		audioSource.Play ();
	}

	public void Hit(int dmg) {  // called when hit by enemy
		health -= dmg;
		//if (health > 1)
			//Debug.Log ("Health: " + health);
		if (health < 1) {
			Explode ();
			Debug.Log (gameObject.name + " dead");
		}
	}
	void Explode() {
		//var exp = GetComponent<ParticleSystem> ();
		//exp.Play ();

		alive = false;
		//Destroy(gameObject);
		gameController.friendCount--;

		//replace with wreck
	}

	void OnMouseOver()
	{
		if(Input.GetButtonDown ("Fire1")) {
			//Debug.Log ("selected");
			int type = 0;
			if (isMG)
				type = 1;
			else if (isMortar)
				type = 3;
			else
				type = 2;
			gameController.UnitSelected (gameObject, type);
		}

		halo.enabled = true;
	}

	void OnMouseExit()
	{
		halo.enabled = false;
	}

	public void OverrideTarget(GameObject enemy){
		if (gameObject == gameController.selectedUnit)
			enemyTarget = enemy;
	}
}
