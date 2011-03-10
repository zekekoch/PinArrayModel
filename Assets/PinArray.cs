using System;
using System.Runtime.InteropServices;
using System.Collections;
//using System.Windows.Media.Imaging;
using System.Threading;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class PinArray : MonoBehaviour {

    public Transform PinPrefab;
	int gridX = 64;
	int  gridY = 48;
	float spacing = 4.5f;
    int[,] depthmap = new int[48, 64];
    int[,] colormap = new int[48, 64];
    Transform[,] pins = new Transform[480, 640];
    int fileCount = 0;

    Thread thread;
    Mutex mainLoop;

	void Awake()
	{
		mainLoop = new Mutex(true);
        thread = new Thread(ThreadWorker);
	}
	
	// Use this for initialization
	void Start () {
		// Instantiates a prefab in a grid
		UnityEngine.Debug.Log("hello");
        UnityEngine.Debug.Log("loaded depth Array");

        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                //float h = (float)depthmap[y*10, x*10]/depthScale;
                Transform t = (Transform) Instantiate(PinPrefab, new Vector3(x * spacing, 0, y * spacing), Quaternion.identity);
                pins[x, y] = t;
            }
		}

        thread.Start();
	}



    void ThreadWorker()
    {
        try
        {
            _ThreadWorker();
        }
        catch (Exception e)
        {
            if (!(e is ThreadAbortException))
                UnityEngine.Debug.LogError("unexpected death: " + e.ToString());
        }

    }

    void _ThreadWorker()
    {
        while (true)
        {
            //Thread.Sleep(250);

            // wait until it's safe to work with gameobjects
            mainLoop.WaitOne();
            // work with gameobjects here
            try
            {
                LoadArray("depthMap", depthmap);
                LoadArray("colorMap", colormap);
            }
            catch
            {
            }
            // signal that we're done
            mainLoop.ReleaseMutex();
        }
    }

    void PrintArray(int[,] a)
    {
        for (int x = 0; x < 480; x++)
        {
            StringWriter s = new StringWriter();
            for (int y = 0; y < 640; y++)
            {
                s.Write(a[x, y] + ",");
            }
            UnityEngine.Debug.Log(s);
        }
    }

    void LoadArray(string source, int [,] dest)
    {
        if (fileCount < 240)
            fileCount++;
        else
            fileCount = 0;
        
        string fileName = "\\Depth\\" + source + fileCount + ".csv";
        StreamReader sr = new StreamReader(File.OpenRead(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + fileName));
        string file = sr.ReadToEnd();
        string[] rows = file.Split('\n');
        for (int y = 0; y < 64; y++)
        {
            string[] columns = rows[y].Split(',');
            for (int x = 0; x < 48; x++)
            {
                int number = 0;
                bool result = Int32.TryParse(columns[x], out number) ;

                if (result)
                    dest[x, y] = number;
                else
                    dest[x, y] = 0;
            }
        }
        sr.Close();
    }

    void OnApplicationQuit()
	{
        thread.Abort();
	}
		
	// Update is called once per frame
	void Update () 
	{
        mainLoop.ReleaseMutex();
        mainLoop.WaitOne();
        
        GameObject cam = GameObject.Find("Main Camera");
	    if (cam != null)
	    {
		    Transform t = cam.transform;
		
		    //if (t.eulerAngles.y < 180 && t.eulerAngles.y > 0)
			    t.RotateAround(new Vector3(0,0,0), Vector3.forward, 20* Input.GetAxis("Vertical"));
        }
        transform.RotateAround(new Vector3(0, 0, 0), Vector3.up, 20 * Input.GetAxis("Horizontal"));
	
        // update the grid from the images
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                float h = depthmap[y, x];
                Transform t = (Transform)pins[x,y];
                float height = t.position.y;
                //UnityEngine.Debug.Log(height +":"+ h);
                
                t.position = new Vector3(t.position.x,  h, t.position.z);

                byte[] bytes = BitConverter.GetBytes(colormap[y,x]);
                Color color = new Color(bytes[2]/256F,bytes[1]/256F,bytes[0]/256F,bytes[3]/256F);
                t.Find("Head").renderer.material.color = color;
                //UnityEngine.Debug.Log(bytes +":" + color);
                

            }
        }
        //UnityEngine.Debug.Log("moved");
//				Process(colorImage, depthImage, pinImage);

	}
}
