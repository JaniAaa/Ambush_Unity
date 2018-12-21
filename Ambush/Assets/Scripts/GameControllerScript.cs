using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameControllerScript : MonoBehaviour {
	private static GameControllerScript gameController;

	public int enemyCount;
	public GameObject[] enemyList;
	public GameObject startButton;
	public bool setupPhase;
	public int friendCount;
	public GameObject[] friendList;
	public int escapedCount;
	//private int startingFriendCount;
	private int startingEnemyCount;

	public GameObject gameOverPanel;
	public GameObject nextLevelButton;
	public Text goText;
	public GameObject spawnPanel;
	public Text pointsLeftText;
	private int pointsLeft;
	public int maxPoints;
	public Text descriptionText;
	public GameObject pausePanel;
	public Slider volumeSlider;

	private bool spawned;
	private GameObject objToSpawn;
	public GameObject MG;
	public GameObject Cannon;
	public GameObject Mortar;
	private int pointDeduction;

	public GameObject unitOptionPanel;
	public Text unitNameText;
	public Text coordinateText;
	public GameObject selectedUnit;
	private int selectedUnitType;
	private bool deleteSpawned;
	LineRenderer lineRenderer;
	private bool checkingLOS;

	public bool ambushRevealed;
	public GameObject openFireButton;
	public GameObject overrideTargetedEnemy;

	public int pointsLvl1;
	public int pointsLvl2;
	string activeScene;


	void Start () {
		//if (enemyList == null)
		//enemyList = GameObject.FindGameObjectsWithTag("Enemy");  // actual way of finding enemies, DOES NOT WORK FOR SOME REASON

		var allObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; // alternate way of finding enemies
		var i = 0;
		foreach(GameObject obj in allObjects) {	// counting the enemies
			if(obj.tag == "Enemy") {
				enemyCount++;
			}
		}
		i = 0;
		enemyList = new GameObject[enemyCount];
		foreach(GameObject obj in allObjects) {	// listing the enemies
			if(obj.tag == "Enemy") {
				//Debug.Log ("Added " + obj.name);
				enemyList [i] = obj;
				i++;
				obj.SetActive (false);
			}
		}
		startingEnemyCount = enemyCount;

		Time.timeScale = 0;
		setupPhase = true;

		objToSpawn = null;
		pointDeduction = 0;
		pointsLeft = maxPoints;
		pointsLeftText.text = pointsLeft + "/" + maxPoints;
		descriptionText.text = "";

		gameOverPanel.SetActive (false);
		nextLevelButton.SetActive (false);
		spawnPanel.SetActive (true);
		pausePanel.SetActive (false);
		unitOptionPanel.SetActive (false);

		deleteSpawned = false;
		checkingLOS = false;
		ambushRevealed = false;

		activeScene = SceneManager.GetActiveScene ().ToString ();
	}

	void Update () { 
		if(setupPhase){
			if (Input.GetButtonDown ("Fire1") && !EventSystem.current.IsPointerOverGameObject () && objToSpawn != null && pointsLeft >= pointDeduction) { // spawning function in setup phase
				var mousePos = Input.mousePosition;
				Vector3 worldPos;
				Ray ray = Camera.main.ScreenPointToRay (mousePos);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit, 1000f)) {
					worldPos = hit.point;
				} else {
					worldPos = Camera.main.ScreenToWorldPoint (mousePos);
				}
				Instantiate (objToSpawn, worldPos + new Vector3 (0f, 0.7f, 0f), Quaternion.identity);
				DeductPoints (pointDeduction);
				pointsLeftText.text = pointsLeft + "/" + maxPoints;
				objToSpawn = null;

				if (deleteSpawned) {
					Destroy (selectedUnit);
					deleteSpawned = false;
				}
			}
		}
		if (Input.GetButtonDown ("Cancel")) {
			PauseGame ();  // opens pause menu
		}

		if (lineRenderer != null) {  // drawing line of sight
			if (checkingLOS) {
				var mousePos = Input.mousePosition;
				Vector3 worldPos;
				Ray ray = Camera.main.ScreenPointToRay (mousePos);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit, 1000f)) {
					worldPos = hit.point;
				} else {
					worldPos = Camera.main.ScreenToWorldPoint (mousePos);
				}

				//if (Physics.Raycast (selectedUnit.transform.position, worldPos, out hit))  // line stops at collision, doesn't work properly
					//worldPos = hit.point;

				lineRenderer.SetPosition (0, selectedUnit.transform.position);
				lineRenderer.SetPosition (1, worldPos);

				if (Input.GetButtonDown ("Cancel"))
					checkingLOS = false;
			}
			if (!checkingLOS) {
				lineRenderer.SetPosition (0, new Vector3 (0f, 0f, 0f));
				lineRenderer.SetPosition (1, new Vector3 (0f, 0f, 0f));
			}
		}

		if (!setupPhase) {  // three different game over cases
			if (enemyCount == 0 && escapedCount == 0) {
				goText.text = "Flawless victory!";
				nextLevelButton.SetActive (true);
				gameOverPanel.SetActive (true);
				if (activeScene == "Level1")
					PlayerPrefs.SetInt ("pointsLvl1", 5);
				if (activeScene == "Level2")
					PlayerPrefs.SetInt ("pointsLvl2", 5);
			}
			if (enemyCount == 0 && escapedCount > 0) {
				goText.text = "Victory... but " + escapedCount + " enemies escaped";
				nextLevelButton.SetActive (true);
				gameOverPanel.SetActive (true);
				if (activeScene == "Level1" && PlayerPrefs.GetInt ("pointsLvl1") < 5 - escapedCount)
					PlayerPrefs.SetInt ("pointsLvl1", 5 - escapedCount);
				if (activeScene == "Level2" && PlayerPrefs.GetInt ("pointsLvl2") < 5 - escapedCount)
					PlayerPrefs.SetInt ("pointsLvl2", 5 - escapedCount);
			}
			if (friendCount == 0 || escapedCount == startingEnemyCount) {
				goText.text = "You lose";
				gameOverPanel.SetActive (true);
			}
		}
	}

	public void BeginGame()  // for the start button; ends setup phase
	{
		if (pointsLeft == maxPoints)
			descriptionText.text = "Don't forget to spawn some units!";
		else {
			var allObjects = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			int i = 0;
			foreach(GameObject obj in allObjects) {	// counting the player units
				if(obj.tag == "Player") {
					friendCount++;
				}
			}
			i = 0;
			friendList = new GameObject[friendCount];
			foreach(GameObject obj in allObjects) {	// listing the player units
				if(obj.tag == "Player") {
					friendList [i] = obj;
					i++;
				}
			}
			//startingFriendCount = friendCount;

			Time.timeScale = 1;
			setupPhase = false;
			startButton.SetActive (false);
			spawnPanel.SetActive (false);
			openFireButton.SetActive (true);

			foreach (GameObject obj in enemyList) {	// activating the enemies
				obj.SetActive (true);
			}
		}
	}

	public void BackToMenu(){
		SceneManager.LoadScene("MainMenu");
	}

	public void RestartLevel(){
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void NextLevel(){
		Scene scene = SceneManager.GetActiveScene();
		int nextSceneInd = scene.buildIndex + 1;
		//nextSceneInd = nextSceneInd++;
		if(nextSceneInd <= SceneManager.sceneCountInBuildSettings)
			SceneManager.LoadScene(nextSceneInd);
		else
			SceneManager.LoadScene("MainMenu");
	}

	public void ChangeVolume(){
		AudioListener.volume = volumeSlider.value;
	}

	public void SpawnMG(){
		objToSpawn = MG;
		pointDeduction = 50;  // point deduction is executed in the spawn function in Update()
	}
	public void SpawnCannon(){
		objToSpawn = Cannon;
		pointDeduction = 80;

	}
	public void SpawnMortar(){
		objToSpawn = Mortar;
		pointDeduction = 100;
	}

	public void DeductPoints(int points){
		pointsLeft -= points;
	}
	public void IncreasePoints(int points){
		pointsLeft += points;
	}

	public void OnPointerEnterMG(){
		descriptionText.text = "Small profile, high rate of fire. Effective against unarmoured vehicles. Will not attack tanks.";
	}

	public void OnPointerEnterCannon(){
		descriptionText.text = "Effective against all unit types, but fires slowly. Attacks tanks first.";
	}

	public void OnPointerEnterMortar(){
		descriptionText.text = "Inaccurate but powerful. Effective against unarmoured vehicles. Fires indirectly without line of sight, shells land after a delay.";
	}

	public void OnPointerExit(){
		descriptionText.text = "";
	}

	  // instantiating function for other scripts to use
	public static GameControllerScript Instance() {
		if (!gameController) {
			gameController = (GameControllerScript)FindObjectOfType(typeof(GameControllerScript));
		}
		return gameController;
	}

	public void PauseGame(){
		if (pausePanel.activeInHierarchy) {
			pausePanel.SetActive (false);
			Time.timeScale = 1;
		}
		else{
			pausePanel.SetActive (true);
			Time.timeScale = 0;
		}
	}

	public void UnitSelected(GameObject selected, int unitType){  // when player clicks on one of their units
		selectedUnit = selected;
		selectedUnitType = unitType;
		unitOptionPanel.SetActive (true);
		unitNameText.text = selected.name;
		coordinateText.text = "Coordinates: x: " + (int)selected.transform.position.x + "; y: " + (int)selected.transform.position.y + "; z: " + (int)selected.transform.position.z;

		lineRenderer = selected.GetComponent<LineRenderer>();
	}

	public void CloseUnitMenu(){
		unitOptionPanel.SetActive (false);
		selectedUnit = null;
		checkingLOS = false;
	}

	public void CheckLoS(){
		if(checkingLOS)
			checkingLOS = false;
		else
			checkingLOS = true;
	}

	public void MoveUnit(){
		objToSpawn = selectedUnit;
		pointDeduction = 0;
		deleteSpawned = true;
	}

	public void DeleteUnit(){
		switch (selectedUnitType) {
		case 1:
			IncreasePoints(50);
			break;
		case 2:
			IncreasePoints(80);
			break;
		case 3:
			IncreasePoints(100);
			break;
		default:
			break;
		}
		Destroy (selectedUnit);
		unitOptionPanel.SetActive (false);
		selectedUnit = null;
	}

	public void OpenFire(){
		ambushRevealed = true;
		openFireButton.SetActive (false);
	}

	public void AttackOrder(GameObject enemy){
		overrideTargetedEnemy = enemy;
		Debug.Log ("Attack order on " + enemy.name);
	}
}

