using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
public class CalculatorUser : MonoBehaviour {



    // Other platforms load plugins dynamically, so pass the name
    // of the plugin's dynamic library.
    [DllImport("MathLibrary")]


    private static extern float Multiply(float a, float b);

    // Use this for initialization
    void Start ()
    {
        Debug.Log(Multiply(5, 2));
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
