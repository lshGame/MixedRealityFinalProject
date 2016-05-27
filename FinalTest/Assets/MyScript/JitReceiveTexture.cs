// mu (myu) Max-Unity Interoperability Toolkit
// Ivica Ico Bukvic <ico@vt.edu> <http://ico.bukvic.net>
// Ji-Sun Kim <hideaway@vt.edu>
// Keith Wooldridge <kawoold@vt.edu>
// With thanks to Denis Gracanin

// Virginia Tech Department of Music
// DISIS Interactive Sound & Intermedia Studio
// Collaborative for Creative Technologies in the Arts and Design

// Copyright DISIS 2008.
// mu is distributed under the GPL license v3 (http://www.gnu.org/licenses/gpl.html)

using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.InteropServices;

public class JitReceiveTexture : MonoBehaviour {
	
	public int portNo = 32005;
	
	//Texture
	private int wdth = 256;
	private int hght = 256;
	private Texture2D m_Texture;
	private int texPointer;
	private bool validObj;

	//Networking
	private TcpClient incoming_client;
	private NetworkStream netStream;
	private TcpListener server;
	private bool waiting;
	
	//Packet parser
	private bool header;
	private bool dataRead;
	private	long jitPlanecount;
	private	long jitType;
	private	long jitDimcount;
	private	int[] jitDim = new int[32];
	private	long jitDatasize;
	private	int[] jitTexData;
	private byte[] jitData;
	private int numread;
	
	// Use this for initialization
	void Start () {
		if (portNo == 0) portNo = 32005;

		validObj = false;
		waiting = false;
		header = false;
		
		// Create texture that will be updated in the plugin
		m_Texture = new Texture2D (wdth, hght, TextureFormat.ARGB32, false);
		// Make sure that the texture name exists etc.
		m_Texture.Apply();
		// Initialize Texture Pointer
		texPointer = m_Texture.GetInstanceID();

		// Assign texture to the renderer
		if (GetComponent<Renderer>()) {
			GetComponent<Renderer>().material.mainTexture = m_Texture;
			validObj = true;
			server = new TcpListener(IPAddress.Any, portNo);
			server.Start();
		}
		else
		{
			Debug.Log("Game object has no renderer to assign the generated texture to!");
		}
	}
	
	// Link to the UnityTexture plugin and call the UpdateTexture function there.
	[DllImport ("jitTexture")]
	private static extern void UpdateTexture (int texID, int width, int height, byte[] databuf);
	
	// Update is called once per frame
	void Update () {
		
		if (validObj) {
				
			byte[] worker = new byte[4];
			byte[] gap = new byte[128];
			
			dataRead = false;
			
			if (server.Pending()) {
				incoming_client = server.AcceptTcpClient();
				netStream = incoming_client.GetStream();
				header = false;
				waiting = true;
			}
			
			while (waiting && netStream.DataAvailable) {
				if (!header) {
				
					try {
						//HEADER
						numread = 0;
						
						//First 4 bytes is ID
						netStream.Read(worker, 0, worker.Length);
						if(String.Compare(Encoding.ASCII.GetString(worker,0,worker.Length),"JMTX") == 0) {

							//no idea why + 4 bytes for header size
							netStream.Read(gap, 0, 12);
							
							//planecount
							//Only 4 planes are ok
							netStream.Read(worker, 0, worker.Length);
							Array.Reverse(worker);
							jitPlanecount = BitConverter.ToInt32(worker, 0);
							if (jitPlanecount == 4) {
			
								//Type
								//Only char is ok
								netStream.Read(worker, 0, worker.Length);
								Array.Reverse(worker);
								jitType = BitConverter.ToInt32(worker, 0);
								if (jitType == 0) {
									
									//Dimcount
									//Only 2D is ok
									netStream.Read(worker, 0, worker.Length);
									Array.Reverse(worker);
									jitDimcount = BitConverter.ToInt32(worker, 0);
									if (jitDimcount == 2) {
									
										//32 dim array
										for (int i=0; i < 32; i++) {
											netStream.Read(worker, 0, worker.Length);
											Array.Reverse(worker);
											jitDim[i] = BitConverter.ToInt32(worker, 0);
										}
										
										//If dimensions have changed
										if (jitDim[0] != wdth || jitDim[1] != hght) {
											wdth = jitDim[0];
											hght = jitDim[1];
											m_Texture = null;
											m_Texture = new Texture2D (wdth, hght, TextureFormat.ARGB32, false);
											m_Texture.Apply();
											texPointer = m_Texture.GetInstanceID();
											GetComponent<Renderer>().material.mainTexture = m_Texture;
										}
										
										numread = netStream.Read(gap, 0, 128);
										
										//Datasize
										netStream.Read(worker, 0, worker.Length);
										Array.Reverse(worker);
										jitDatasize = BitConverter.ToInt32(worker, 0);
										
										numread = netStream.Read(gap, 0, 8);
										
										jitData = new byte[jitDatasize];		
										numread = 0;
										header = true;
									}
									else Reset();
								}
								else Reset();
							}
							else Reset();							
						}
						else Reset();
					}
					//Any Exception
					catch (Exception e) {
						Reset();
					}
				}
				
				else if (header) {
					//DATA
					try {
						numread += netStream.Read(jitData, numread, jitData.Length - numread);
						System.Threading.Thread.Sleep(1); //is netStream.Read blocking?
						if (numread == jitData.Length) {
							if (!dataRead) {
								UpdateTexture (texPointer, wdth, hght, jitData);
								dataRead = true;
							}
							header = false;
						}
					}
					catch (Exception e) {
						Reset();
					}
				}
			}
		}
	}
	
	void Reset() {
		waiting = false;
		if (netStream != null) netStream.Close();
		if (incoming_client != null) incoming_client.Close();
	}
}
