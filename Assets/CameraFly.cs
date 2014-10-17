using UnityEngine;
using System.Collections;

public class CameraFly : MonoBehaviour {

	public float Speed = 1f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey (KeyCode.A))
			gameObject.transform.position += new Vector3 (-Speed, 0, 0);
		if (Input.GetKey (KeyCode.D))
			gameObject.transform.position += new Vector3 (Speed, 0, 0);
		if (Input.GetKey (KeyCode.W))
			gameObject.transform.position += new Vector3 (0, 0, Speed);
		if (Input.GetKey (KeyCode.S))
			gameObject.transform.position += new Vector3 (0, 0, -Speed);

	}


}
