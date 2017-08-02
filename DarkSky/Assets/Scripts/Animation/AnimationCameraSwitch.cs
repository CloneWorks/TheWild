using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCameraSwitch : MonoBehaviour {

	public Camera mainCamera;
	public Camera animationCamera; 
	public Animator anim;

	public TextMesh DailyMessage;

	private GameObject player;
	private GameManager gm;
	private WorldClock clock;

	private bool bAnimationStarted;

	// Use this for initialization
	void Start () {
		gm = FindObjectOfType<GameManager>();
		player = GameObject.Find ("Player");
		clock = FindObjectOfType<WorldClock>();

		bAnimationStarted = false;
		//StartCoroutine(PlayAnimation(5));
	}

	void Update () {
		// Check time and trigger animation
		if (((clock.CurrentTime) > (clock.fullDay - 5)) && clock.fullDay != 0) 
		{
			
			if (!bAnimationStarted) 
			{
				StartCoroutine(PlayAnimation(10));
			}
		}
	}

	void PlayerEnabled(bool enabled)
	{
		//mainCamera.GetComponent<ThirdPersonOrbitCam> ().enabled = enabled;
		player.GetComponent<BasicBehaviour> ().enabled = enabled;
		player.GetComponent<PlayerWeapon> ().enabled = enabled;
		player.GetComponent<MoveBehaviour> ().enabled = enabled;
		player.GetComponent<PlayerSounds> ().enabled = enabled;
		player.GetComponent<AudioSource> ().enabled = enabled;
		player.GetComponent<Rigidbody> ().isKinematic = !enabled;
	}

	IEnumerator PlayAnimation(int iAnimLength)
	{
		mainCamera.enabled = false;
		animationCamera.enabled = true;
		animationCamera.GetComponentInChildren<MeshRenderer> ().enabled = true;
		bAnimationStarted = true;
		PlayerEnabled (false);
		DailyMessage.text = "Day " + gm.iTotalDays;

		anim.Play ("DailyCamera");

		yield return new WaitForSeconds (iAnimLength);
		PlayerEnabled (true);
		bAnimationStarted = false;
		mainCamera.enabled = true;
		animationCamera.enabled = false;
		animationCamera.GetComponentInChildren<MeshRenderer> ().enabled = false;
		gm.iTotalDays += 1;
	}
}
