using UnityEngine;
using System.Collections;

public class PinPrefab : MonoBehaviour {

    public float height;
    public float x;
    public float y;
    public Transform Transform;
	// Use this for initialization

    void Awake()
    {
        Transform = this.Transform;
    }

	void Start () {
	
	}

    void ChangeColor()
    {
    
    }
	
	// Update is called once per frame
	void Update () {
     //   UnityEngine.Debug.Log(height + "," + transform.position.y);

       // transform.Translate(x, height, y);
	}
}
