using UnityEngine;
using System.Collections;

public class waterSize : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		int i = myVis.openRadius;
		Vector3 waterScale =transform.localScale;
		waterScale.x = 1.65f * i;
		waterScale.z = waterScale.x;
		transform.localScale = waterScale;
	}
}
