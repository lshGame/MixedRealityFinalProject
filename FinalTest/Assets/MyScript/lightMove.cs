using UnityEngine;
using System.Collections;

public class lightMove : MonoBehaviour {
	//public AudioListener AL;
	public AudioSource AS;
	public float moveRange = 1f;
	public int i = 0;
	public GameObject light;
	public int reactSpeed=10;
	float[] spectrum = new float[1024];
	public Light dirLight;

	public static float K=10;
	public float shrinkSpeedDC = 0.001f;
	// Use this for initialization
	void Start () {
		
		light = GameObject.FindGameObjectWithTag("light");
	}
	public class keyPress
	{
		public string key;
		public bool keyDown;
		public float impulseInterval;
		public float impulseLast;
		public float intensity;
		public keyPress(string keyInput){
			key=keyInput;
			keyDown=false;
			impulseInterval=0;
			impulseLast=0;
			intensity=1;
				
		}
		public float keyInterval(){
			if (Input.GetKeyDown (key)&&!keyDown) {
				if (impulseLast == 0)
					impulseLast = Time.time;
				else {
					impulseInterval=Time.time - impulseLast;
					impulseLast = Time.time;
				}
				keyDown = true;
			}
			if (Input.GetKeyUp (key) && keyDown)
				keyDown = false;
			return impulseInterval;
		}

	}
	// Update is called once per frame
	public keyPress keyHeight=new keyPress("q");
	public keyPress keyShrink=new keyPress("w");
	public float shrinkLimit = 1f;
	public float shrinkSpeed=0;
	public Material sky;
	public bool isEnabled=true;
	public float RotationPerSecond = 0;
	void Update () {
		if (isEnabled) {
			float intervalHeightCtrl = keyHeight.keyInterval ();
			float intervalShrinkCtrl = keyShrink.keyInterval ();

			float distanceMove = (intervalHeightCtrl > 0) ? 1 / intervalHeightCtrl : 0;
			//if(Input.GetKeyDown ("w")&&!keyDown)
			//float shrinkCoefficient = (intervalShrinkCtrl>0)?(shrinkSpeedDC+intervalShrinkCtrl):shrinkSpeedDC;
			//float shrinkCoefficient =(Mathf.Abs(keyHeight.impulseLast-keyShrink.impulseLast)<=0.1f)?shrinkSpeedDC:shrinkSpeedDC+intervalShrinkCtrl;
			//Debug.Log (Mathf.Abs(keyHeight.impulseLast-keyShrink.impulseLast));
			Vector3 previousPosition = -transform.localPosition;
			if (Input.GetKeyUp ("q")) {
				previousPosition.z += Mathf.Lerp (0, distanceMove, Time.deltaTime * reactSpeed);
			} else if (previousPosition.z >= 1) {
				shrinkLimit = (Mathf.Abs (keyHeight.impulseLast - keyShrink.impulseLast) <= 0.05f) ? (previousPosition.z + shrinkSpeed / 10) : 1f;
				shrinkSpeed = K * (previousPosition.z - shrinkLimit) * Time.deltaTime;
				previousPosition.z -= shrinkSpeed;
				//Debug.Log (shrinkLimit);
			}

			transform.localPosition = -previousPosition;
			if (previousPosition.z > 10) {
				RenderSettings.skybox = sky;

				previousPosition.z = 20;
				isEnabled = false;
				AS.Play ();
				dirLight.GetComponent<Light> ().enabled = true;
				//GameObject MainCam=GameObject.Find("Main Camera");
				//MainCam.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
			}
		} else {
			if(RenderSettings.ambientIntensity < 1)
				RenderSettings.ambientIntensity += 0.1f*Time.deltaTime;
			RotationPerSecond += 2 * Time.deltaTime;
			sky.SetFloat("_Rotation", RotationPerSecond);

		}
		//int avrSpectrum = 0;
		//i = (i >= 1024) ? 0 : (i + 1);
		/*AS.GetSpectrumData(spectrum,0,FFTWindow.Hamming);
		Vector3 previousPosition =transform.localPosition;
		previousPosition.y =Mathf.Lerp ( previousPosition.y, spectrum[i] * microphoneHeight, Time.deltaTime * reactSpeed);
		previousPosition.y = 5 + 10 * previousPosition.y;
		transform.localPosition = previousPosition;
*/
	}
}
