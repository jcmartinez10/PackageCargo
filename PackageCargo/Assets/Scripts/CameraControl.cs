using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    //----------------------------------
    //PUBLIC VARIABLES
    //----------------------------------
    //The static point to look at
    public Transform objective;
    //The speed of the rotation
    public float camSpeed;
    //Zoom of the  camera
    public float zoom;
    public bool scrollLock;
    //----------------------------------
    //PRIVATE VARIABLES
    //----------------------------------
    private Camera cam;
    //Current objective
    private Transform currentObjective;
    //----------------------------------
    //METHODS
    //----------------------------------


    // Use this for initialization
    void Start ()
    {
        scrollLock=false;
        cam = GetComponent<Camera>();
        currentObjective = objective;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        try
        {
            //Check if collides with something when click
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                {

                    if (hit.transform.gameObject.name.Contains("Package"))
                    {

                        Package p = hit.transform.gameObject.GetComponentInParent<Package>();

                        p.showPackageInfo();
                        currentObjective = hit.transform;
                    }

                }
            }
            //Movement of the camera
            if (Input.GetMouseButton(1))
            {
                transform.RotateAround(currentObjective.position, Vector3.up, camSpeed * Input.GetAxis("Mouse X") * -1);
                transform.RotateAround(currentObjective.position, Vector3.forward, camSpeed * Input.GetAxis("Mouse Y") * 1);

            }
            transform.LookAt(currentObjective);
            //Zoom
            if (!scrollLock)
            {
                transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * zoom, Space.Self);
            }
        }
        catch(System.Exception e)
        {
            resetCameraObjective();
        }


    }
    /**
    *Reset the camera
    */
    public void resetCameraObjective()
    {
        currentObjective = objective;
    }
}
