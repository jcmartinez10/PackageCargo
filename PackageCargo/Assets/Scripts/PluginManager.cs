using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PluginManager : MonoBehaviour
{
    //DLL DECLARATION
    [DllImport("Multidrop")]

    //Dll method declaration
    private static extern void main(int argc, char[] argv);
    // Use this for initialization
    void Start ()
    {
        Debug.Log("Dll init");
        main(7, new char[7]);
        Debug.Log("Sucess");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
