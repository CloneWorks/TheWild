using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCameraSwitch : MonoBehaviour {

	public Camera main;
	public Camera animation; 

	private GameObject player;

	// Use this for initialization
	void Start () {
		player = GameObject.Find ("Player");
		PlayerEnabled (false);
		main.enabled = false;
		animation.enabled = true;
	}

	void PlayerEnabled(bool enabled)
	{
		//for (int i = 0; i < player.transform.childCount; i++)
		//{
		//	player.transform.GetChild (i).gameObject.SetActive(false);
		//}
		MyWait();
		player.GetComponent<BasicBehaviour> ().enabled = enabled;
		player.GetComponent<PlayerWeapon> ().enabled = enabled;
		main.GetComponent<ThirdPersonOrbitCam> ().enabled = enabled;
	}

	IEnumerator MyWait()
	{
		yield return new WaitForSeconds (20);
	}
}
