using UnityEngine;
using System.Collections;

public class TerrainCtrl : MonoBehaviour {

	public Terrain mainTerrain; //main Terain
	public int resolutionX; //  heightmap resolution X (width)
	public int resolutionY; //  heightmap resolution Y (height)
	float[,] heights; // height array of the terrain (2 dimensional array[,])
	// Use this for initialization
	void Start () {
		resolutionX = mainTerrain.terrainData.heightmapWidth; // initializes the terrain map width
		resolutionY = mainTerrain.terrainData.heightmapHeight; // initializes the terrain map height
		heights = mainTerrain.terrainData.GetHeights (0,0,resolutionX,resolutionY);
		Debug.Log (resolutionX + " " + resolutionY);
		Debug.Log(mainTerrain.terrainData.size.x+" "+mainTerrain.terrainData.size.z);
	}
	
	// Update is called once per frame
	void Update () {
		//converts mouse to a 3d point
		RaycastHit hit; // stores the data of the raycast
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);//position of the ray(converts the position of the mouse to a real world position
		Debug.DrawRay(ray.origin,ray.direction);
		//runs the raycast
		if (Physics.Raycast (ray, out hit)){  //output data into the hit variable
			Debug.Log(hit.point);
			if (Input.GetMouseButtonDown (0)) { // if mouse button 1 pressed
				ModifyTerrain (hit.point,0.001f,20); //raise the terrain at the ray hit point, height, diameter
			}
			//projector.position = new Vector3(hit.point.x,projector.position.y,hit.point.z);
		}

	}
	void ModifyTerrain(Vector3 position, float amount, int diameter){

		int terrainPosX = (int)((position.x / mainTerrain.terrainData.size.x) * resolutionX);// converts the current X position to the Terrain X position
		int terrainPosY = (int)((position.z / mainTerrain.terrainData.size.z) * resolutionY);// converts the current Y position to the Terrain Y position
		//obsolete code
		//		float curHeight = heights [terrainPosY, terrainPosX]; // gets the current height of the terrain
		//		curHeight += amount; //changes the current height by the amount to change
		//		heights [terrainPosY, terrainPosX] = curHeight; // sets the changed heights back to the current height

		float[,] heightChange = new float[diameter,diameter];//changes only one of the heights, not all of them

		int radius = (int)(diameter / 2);// circle diameter(diameter divided by 2)
		for(int x = 0; x < diameter; x++){
			for(int y = 0; y < diameter; y++){
				int x2 = x - radius;// X position relative to the circle
				int y2 = y - radius;// Y position relative to the circle

				if (terrainPosY + y2 <0 || terrainPosY + y2 >= resolutionY || terrainPosX + x2 <0 || terrainPosX + x2 >= resolutionX)
					continue;



				float distance = Mathf.Sqrt((x2*x2)+(y2*y2));

				if (distance > radius)
					heightChange [y, x]=heights [terrainPosY + y2, terrainPosX + x2];
				else{
					heightChange [y, x] = heights [terrainPosY + y2, terrainPosX + x2]+ (amount - (amount* (distance / radius)));// only affects the change we are making
					heights [terrainPosY + y2, terrainPosX + x2] = heightChange [y, x]; // sets the heights to update properly
				}



			}
		}

		//FIXME does not work when array is larger than current terrain
		mainTerrain.terrainData.SetHeights (terrainPosX - radius, terrainPosY - radius, heightChange); // updates the terrain data
	}
}
