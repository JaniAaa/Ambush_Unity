using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManagerScript : MonoBehaviour {
	public GameObject menu;
	public GameObject options;
	public GameObject levels;
	public Dropdown dropdown;
	private bool fullscreenOn;
	public Toggle toggle;
	public Slider volumeSlider;
	private int pointsLvl1;
	private int pointsLvl2;
	public Text pointsLvl1Txt;
	//public Text pointsLvl2Txt;

	//private static GameControllerScript gameController;


	public void Start(){
		levels.SetActive (false);
		options.SetActive (false);
		menu.SetActive (true);
		fullscreenOn = toggle.isOn;

		//gameController = GameControllerScript.Instance ();
		/*
		if (PlayerPrefs.GetInt ("pointsLvl1") == null)
			PlayerPrefs.SetInt("pointsLvl1", 0);

		if (PlayerPrefs.GetInt ("pointsLvl2") == null)
			PlayerPrefs.SetInt("pointsLvl2", 0);*/

		pointsLvl1Txt.text = "Score: " + PlayerPrefs.GetInt ("pointsLvl1").ToString () + "/5";
		//pointsLvl2Txt.text = "Score: " + PlayerPrefs.GetInt ("pointsLvl2").ToString () + "/5";
	}

	public void OpenOptions(){
		menu.SetActive (false);
		options.SetActive (true);
	}

	public void GoBack(){
		levels.SetActive (false);
		options.SetActive (false);
		menu.SetActive (true);
	}

	public void StartGame(){
		menu.SetActive (false);
		levels.SetActive (true);
	}

	public void QuitGame(){
		Application.Quit();
	}

	public void Lvl0Start(){
		SceneManager.LoadScene("TestingScene");
	}

	public void Lvl1Start(){
		SceneManager.LoadScene("Level1");
	}

	public void ResolutionChange(){
		if (dropdown.value == 0)
			Screen.SetResolution(1920, 1080, fullscreenOn);
		if (dropdown.value == 1)
			Screen.SetResolution(1600, 900, fullscreenOn);
		if (dropdown.value == 2)
			Screen.SetResolution(1280, 720, fullscreenOn);
		if (dropdown.value == 3)
			Screen.SetResolution(960, 600, fullscreenOn);
	}

	public void FullscreenToggle(){
		fullscreenOn = toggle.isOn;  // takes effect after changing resolution
	}

	public void ChangeVolume(){
		AudioListener.volume = volumeSlider.value;
	}

}
