using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
	public int speed;
	private int origSpeed;
	private int fastSpeed;
	private float yaw = 0.0f;
	private float pitch = 0.0f;

	public AudioSource bGAudio;


	void Start () {
		yaw = transform.eulerAngles.x;
		pitch = transform.eulerAngles.y;
		origSpeed = speed;
		fastSpeed = speed * 3;
	}
		
	void Update () {
		if(Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			transform.Translate(new Vector3(speed * Time.unscaledDeltaTime,0,0));
		}
		if(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			transform.Translate(new Vector3(-speed * Time.unscaledDeltaTime,0,0));
		}
		if(Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
		{
			float currentHeight = transform.position.y;
			//transform.Translate(new Vector3(0,0,-speed * Time.unscaledDeltaTime), Space.World);
			transform.Translate(new Vector3(0,0,-speed * Time.unscaledDeltaTime));
			if(transform.position.y != currentHeight)
				transform.position = new Vector3(transform.position.x,currentHeight,transform.position.z);
		}
		if(Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			float currentHeight = transform.position.y;
			//transform.Translate(new Vector3(0,0,speed * Time.unscaledDeltaTime), Space.World);
			transform.Translate(new Vector3(0,0,speed * Time.unscaledDeltaTime));
			if(transform.position.y != currentHeight)
				transform.position = new Vector3(transform.position.x,currentHeight,transform.position.z);
		}
		if (Input.GetKey (KeyCode.LeftShift))
			speed = fastSpeed;
		else
			speed = origSpeed;

		if(Input.GetKey(KeyCode.F))
		{
			transform.Translate(new Vector3(0,-speed * Time.unscaledDeltaTime,0));
		}
		if(Input.GetKey(KeyCode.R))
		{
			transform.Translate(new Vector3(0,speed * Time.unscaledDeltaTime,0));
		}

		if (Input.GetMouseButton(1))
		{
			yaw -= Input.GetAxis("Mouse Y");
			pitch += Input.GetAxis("Mouse X");

			transform.eulerAngles = new Vector3(yaw, pitch, 0.0f);
		}
	}
}
