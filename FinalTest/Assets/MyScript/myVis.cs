using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class myVis : MonoBehaviour {
	public Transform CellsParent;
	public GameObject prefab;
	public static int openRadius;
	public int Radius;
	public List<Cell> hexagons;
	public AudioSource myAudio;
	public float changSpeed=10;
	public float changScale=50;
	public int lastHexCount = 0;
	//public GameObject parentObj;
	public int innerRadius;
	public float infoLimit=100;
	public Vector3 tiltAngle;
	public Vector3 rotAngle;
	public bool platformEnabled=false;
	private Vector3 zeroVec = new Vector3 (0, 0, 0);
	public Vector3 prevLocationEuler;
	// Use this for initialization
	void MyDestroyHex(int lastRadius,int newRadius){
		foreach (Transform child in CellsParent) {
			int dg=child.gameObject.GetComponent<Hexagon> ().degree;
			if (dg >= lastRadius && dg <= newRadius) {
				Destroy (child.gameObject);
			}
		}
	}
	void MyClearHex(int lastRadius,int newRadius){
		foreach (Transform child in CellsParent) {
			int dg=child.gameObject.GetComponent<Hexagon> ().degree;
			if (dg >= lastRadius && dg <= newRadius) {
				hexagons.Remove (child.gameObject.GetComponent<Cell> ());
			}
		}
	}
	void MyGenerateHex(int lastRadius,int newRadius){
		if (newRadius>0) {
			for (int i = lastRadius; i <=newRadius; i++) {
				for (int j = -i + 1; j < i; j++) {
					GameObject hexagon = Instantiate (prefab);
					GameObject hexagon2 = Instantiate (prefab);

					var hexSize = hexagon.GetComponent<Cell> ().GetCellDimensions ();
					int jj = Mathf.Abs (j);
					hexagon.transform.position = coordTrans (new Vector2 (j, (i - 1 - (jj * 0.5f))), hexSize.x, hexSize.y);
					hexagon2.transform.position = coordTrans (new Vector2 (j, -(i - 1 - (jj * 0.5f))), hexSize.x, hexSize.y);
					hexagon.GetComponent<Hexagon> ().OffsetCoord = new Vector2 (j, (i - 1 - (jj * 0.5f)));
					hexagon2.GetComponent<Hexagon> ().OffsetCoord = new Vector2 (j, -(i - 1 - (jj * 0.5f)));
						
					hexagon.GetComponent<Hexagon> ().degree = i;
					hexagon2.GetComponent<Hexagon> ().degree = i;
					hexagons.Add (hexagon.GetComponent<Cell> ());
					hexagon.transform.parent = CellsParent;
					hexagons.Add (hexagon2.GetComponent<Cell> ());
					hexagon2.transform.parent = CellsParent;

					if (jj == i - 1 && i > 2) {
						//Debug.Log (i % 2);
						int modi = i % 2;
						//Debug.Log (modi);
						if (modi != 0) {
							GameObject hexagon5 = Instantiate (prefab);
							hexagon5.transform.position = coordTrans (new Vector2 (j, 0), hexSize.x, hexSize.y);
							hexagon5.GetComponent<Hexagon> ().OffsetCoord = new Vector2 (j, 0);
							hexagons.Add (hexagon5.GetComponent<Cell> ());
							hexagon5.transform.parent = CellsParent;
							hexagon5.GetComponent<Hexagon> ().degree = i;

						}
						if (i > 3) {
							int li = (i - 2 - modi) / 2;
							//Debug.Log ("li" + li);
							for (int m = 0; m < li; m++) {
								GameObject hexagon3 = Instantiate (prefab);
								GameObject hexagon4 = Instantiate (prefab);
								hexagon3.transform.position = coordTrans (new Vector2 (j, m + 0.5f + modi * 0.5f), hexSize.x, hexSize.y);
								hexagon4.transform.position = coordTrans (new Vector2 (j, -(m + 0.5f + modi * 0.5f)), hexSize.x, hexSize.y);
								hexagon3.GetComponent<Hexagon> ().OffsetCoord = new Vector2 (j, m + 0.5f + modi * 0.5f);
								hexagon4.GetComponent<Hexagon> ().OffsetCoord = new Vector2 (j, -(m + 0.5f + modi * 0.5f));
								hexagons.Add (hexagon3.GetComponent<Cell> ());
								hexagon3.transform.parent = CellsParent;
								hexagons.Add (hexagon4.GetComponent<Cell> ());
								hexagon4.transform.parent = CellsParent;
								hexagon3.GetComponent<Hexagon> ().degree = i;
								hexagon4.GetComponent<Hexagon> ().degree = i;
							}
						}
					}



				}
			}
			 
		}
		Quaternion hexRotation = transform.localRotation;
		hexRotation.eulerAngles = tiltAngle;
		transform.localRotation = hexRotation;
	}
	Vector3 coordTrans(Vector2 vec2,float x, float y){
		Vector3 newVec=new Vector3((vec2.x*x*0.75f),(vec2.y*y));
		return newVec;
		
	}
	void Start () {
		/*
		myAudio.clip = Microphone.Start("Built-in Microphone", true, 1000, 44100);
		myAudio.pitch = 0.5f;
		myAudio.Play();
		*/
		int min;
		int max;
		Microphone.GetDeviceCaps ("Steinberg CI1", out min,out max);
		Debug.Log (min + " " + max);
		MyGenerateHex (innerRadius,Radius);

		if (platformEnabled) {
			MyGenerateHex (18, 20);
			MyClearHex (18, 20);
			openRadius = 19;
			MyGenerateHex (5, 5);
			MyGenerateHex (10, 10);

		} 



	}
	public void SetColor(Color color,GameObject hex)
	{
		for (int i = 0; i < hex.transform.childCount; i++)
		{
			var rendererComponent = hex.transform.GetChild(i).GetComponent<Renderer>();
			if (rendererComponent != null)
				rendererComponent.material.color = color;
		}
	}

	// update offset value and calc avr
	float avrOffset(Cell hex ,float offsetRec){
		float offset = 0;
		int cnt = hex.transform.gameObject.GetComponent<Hexagon> ().recCnt;
		hex.transform.gameObject.GetComponent<Hexagon> ().offsetRec = offsetRec;
		//prevSum =  hex.transform.gameObject.GetComponent<Hexagon> ().offset;
		//if (offsetRec >= 0.02f) {
			prevSum = cnt++ * hex.transform.gameObject.GetComponent<Hexagon> ().offset;
			//cnt++;
			offset = (prevSum + (2f+Mathf.Log10(offsetRec))*(2f+Mathf.Log10(offsetRec))) / cnt;
			//offset = (prevSum + offsetRec) / cnt;
		//}else
		//	offset=hex.transform.gameObject.GetComponent<Hexagon> ().offset  ;
		hex.transform.gameObject.GetComponent<Hexagon> ().offset = offset;
		hex.transform.gameObject.GetComponent<Hexagon> ().recCnt = cnt;

		return offset;

	}
	// Update is called once per frame




	private float lerp;
	private Cell[] hex;
	private float degr;
	private float avr ;
	private Vector3 previousScale;
	private float offset;
	private float[] spectrum;
	private float prevSum;
	private int i;
	public bool isWall;
	public Material sky;
	public float RotationPerSecond;
	//public AudioSource AS;
	void Update () {
		//openRadius = Radius;

		/*while Playing*/
		if (platformEnabled) {
			
			myAudio.clip = Microphone.Start ("Built-in Microphone", true, 100, 44100);
			//if (Microphone.IsRecording("Steinberg CI1"))
			//	Debug.Log ("yes");
			myAudio.pitch = 0.5f;
			myAudio.Play ();
			//myAudio.mute = true;
		}
		if (myAudio.isPlaying) {
			offset = 0;
			spectrum = myAudio.GetComponent<AudioSource> ().GetSpectrumData (1024, 0, FFTWindow.Hamming);
			hex = hexagons.ToArray ();
			for (i = 0; i < hex.Length; i++) {
				previousScale = hex [i].transform.localScale;
				lerp = Mathf.Lerp (previousScale.z, spectrum [i+1] * changScale, Time.deltaTime * changSpeed);
				//Debug.Log (lerp);
				if (lerp >= 0.0000001) {
				//if (true) {
					degr = hex [i].transform.gameObject.GetComponent<Hexagon> ().degree;
					avr = avrOffset (hex [i], lerp);
					offset = (platformEnabled) ? (avr *50f / (Mathf.Sqrt (degr))) : (5f * avr);
					offset = (degr == 0) ? offset + 1 : offset;
					//Debug.Log (offset+"  "+i);
					if (degr != 10 && degr != 5)
						previousScale.z = offset + lerp;
					else
						previousScale.z = 1.2f * offset + lerp;
					if (!float.IsNaN (previousScale.z))
						hex [i].transform.localScale = previousScale; 
				}
				//hex.transform.gameObject.GetComponent<Hexagon> ().record.Add (lerp);
				//equalizer
				/*
				if (lerp >= 2f) {
					SetColor (Color.cyan, hex.transform.gameObject);
				} else if (lerp >= 1.5f && lerp < 2f) {
					SetColor (Color.magenta, hex.transform.gameObject);
				}else{
					SetColor (Color.white, hex.transform.gameObject);
				}*/
				//}
			}
		}
			//int prevDeg = 1;
			//int deg= Mathf.RoundToInt(50*Mathf.Lerp (prevDeg, spectrum[i] * changScale, Time.deltaTime * changSpeed));
			//Debug.Log (deg);
			/*if (deg > Radius) {
				MyGenerateHex(deg);
				Radius = deg;
			}*/
				//scanner
			/*
			foreach (Transform child in CellsParent) {
				if (child.gameObject.GetComponent<Hexagon> ().degree == deg ) {
					SetColor (Color.cyan, child.gameObject);
				} else {
					SetColor (Color.white, child.gameObject);
				}
				//scanner
			}*/

	//	}

		/*while Playing*/

		if (Input.GetKeyUp ("a")&&Radius<100) {
			MyGenerateHex (innerRadius,++Radius);

		}
		if (Input.GetKeyUp ("s")&&Radius>1) {
			MyGenerateHex (innerRadius,--Radius);

		}
		//rotate sky
		if (RotationPerSecond > 0) {
			RotationPerSecond += 2 * Time.deltaTime;
			sky.SetFloat ("_Rotation", RotationPerSecond);
		}
		//rotate sky

		//rotate outer rings

		if (rotAngle != zeroVec) {
			Quaternion prevRotation = transform.localRotation;

			//prevLocationEuler+=rotAngle*Time.deltaTime;
			prevLocationEuler += rotAngle*Time.deltaTime;
			if (prevLocationEuler.x >= 90 || prevLocationEuler.x <= -90) {
				
				//Vector3 tmpVec = prevRotation.eulerAngles;
				prevLocationEuler.x = -prevLocationEuler.x;
				//prevRotation.eulerAngles = tmpVec;

			}
			prevRotation.eulerAngles = prevLocationEuler;
			transform.localRotation = prevRotation;
		}
		//rotate outer rings

	}

}
