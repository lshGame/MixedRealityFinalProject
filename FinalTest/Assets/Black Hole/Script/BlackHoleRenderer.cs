using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BlackHoleRenderer : MonoBehaviour {

	public Shader  shader;

	public float ratio = 0.7f;  	// The ratio of the height to the length of the screen to display properly shader
	public float radius = 0.5f; 	// The radius of the black hole measured in the same units as the other objects in the scene
	public bool EinsteinRadiusCompliance;

	public GameObject BH;  // The object whose position is taken as the position of the black hole
	private Material _material; // Material which is located shader
	protected Material material 
	{
		get 
		{
			if (_material == null) 
			{
				_material = new Material (shader);
				_material.hideFlags = HideFlags.HideAndDontSave;
			}
			return _material;
		} 
	}

	void Start()
	{
		shader = Resources.Load<Shader>("BlackHoleDistortion");
		material.SetInt("_EinsteinR", EinsteinRadiusCompliance == true ? 1 : 0);
	}

	protected virtual void OnDisable() 
	{
		if( _material ) 
		{
			DestroyImmediate( _material );
		}
	}

	void OnRenderImage (RenderTexture source, RenderTexture destination) 
	{
		if (shader && material && BH != null) 
		{
			// Find the position of the black hole in screen coordinates
			Vector2 pos = new Vector2(
			Camera.main.WorldToScreenPoint (BH.transform.position).x / Camera.main.pixelWidth,
			Camera.main.WorldToScreenPoint (BH.transform.position).y / Camera.main.pixelHeight);

			// Install all the required parameters for the shader
			material.SetVector("_Position", new Vector2(pos.x, pos.y));
			material.SetFloat("_Ratio", ratio);
			if (Physics.Raycast(BH.transform.position, Camera.main.transform.position, Mathf.Infinity))
			{
				material.SetFloat("_Rad", 0);
			}
			else
			{
				material.SetFloat("_Rad", radius);
			}
			material.SetFloat("_Distance", Vector3.Distance(BH.transform.position, this.transform.position));
			// And is applied to the resulting image.
			Graphics.Blit(source, destination, material);
		}
	}
}
