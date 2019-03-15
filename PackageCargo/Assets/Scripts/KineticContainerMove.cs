using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KineticContainerMove : MonoBehaviour {

	//The container's rigidbody 
	private Rigidbody rb;
	public GameObject manager;
	private float g=9.81f;
	private float lastVel=0f;
	private Vector3 dir;
	private bool testing = false;
	// Use this for initialization
	void Start () {
		dir = Vector3.zero;
		lastVel = 0f;
		rb = GetComponent<Rigidbody> ();
		g = 1062 * 9.81f;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown ("y")) {
			StartTest();
		}
		rb.AddForce (dir);

		//rb.AddForce (dir, ForceMode.Acceleration);
	}

	void FixedUpdate () {
		float vel = rb.velocity.magnitude;
		float accel = (vel - lastVel)/(9.81f*Time.deltaTime);
		lastVel=vel;
		float[] containerMove = new float[3];
		containerMove[0]=rb.velocity.x;
		containerMove[1]=rb.velocity.y;
		containerMove[2]=rb.velocity.z;
		if (testing) {
		manager.GetComponent<PackageManager> ().CheckDamage ();
		}
		//print (accel);
	}

	public void StartTest (){
		StartCoroutine("TestOne");
	}

	IEnumerator TestOne() {
		rb.position = Vector3.zero;
        manager.GetComponent<PackageManager>().loadPackages();
        testing = true;
		g = (1000f+manager.GetComponent<PackageManager>().GetMass())* 9.81f;
        manager.GetComponent<PackageManager> ().GetStartingYPositions ();
		dir=0.5f*g*transform.forward;
		Debug.Log ("Forward accel");
		yield return new WaitForSeconds(2f);
		dir=-1f*g*transform.forward;
		Debug.Log ("Brake accel");
		yield return new WaitForSeconds(2f);
		dir=0.5f*g*transform.right;
		Debug.Log ("Right accel");
		yield return new WaitForSeconds(2f);
		dir=-0.5f*g*transform.right;
		Debug.Log ("Left accel");
		yield return new WaitForSeconds(2f);
		manager.GetComponent<PackageManager> ().GetFinalYPositions();
		dir= Vector3.zero;
		rb.velocity = Vector3.zero;
		testing = false;
		yield return null;

	}
}
