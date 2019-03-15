using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * This class models one package
 */
public class Package : MonoBehaviour
{
    //----------------------------------
    //PUBLIC VARIABLES
    //----------------------------------
	public bool broken;
    public Vector3 myAccel;
    public int pileID;
    //----------------------------------
    //PRIVATE VARIABLES
    //----------------------------------
    private Vector3 lastVel;
    private float deltaVel;
    //----------------------------------
    //METHODS
    //----------------------------------


    /*
     * This method sets the values of a package
     * @param size the Size of the package in meters
     * @param position the position of the package in world space
     * @param packageId the id to be desplayed on the package box
     * @param packageColorP  the color of the package 
     */
    public void setPackageValues(Vector3 size,Vector3 position,int packageId,Color packageColor,float weight)
    {
        lastVel = Vector3.zero;
		//Unbroken initial state
		broken = false;
		//Set the position in space
		transform.parent.position = position;
		//Set the dimensions of the package
		transform.parent.localScale = size;
		//Set the color of the package
		GetComponent<Renderer>().material.color = packageColor;
		//Set the letters of the package
		TextMesh[] tms = GetComponentsInChildren<TextMesh>();
		//Set the mass on the rigidbody
		GetComponent<Rigidbody>().mass = weight;

        //Set the txt in the id
        foreach (TextMesh tm in tms)
        {
            tm.text = packageId+"";
        }
    }

    /**
    *Shows the package Information on clicked
    */
    public void showPackageInfo()
    {
        PackageManager.instance.showPackageInfo(int.Parse(GetComponentInChildren<TextMesh>().text));
    }

    void FixedUpdate()
    {
        Vector3 vel = GetComponent<Rigidbody>().velocity;
        myAccel = (vel - lastVel) / Time.fixedDeltaTime;
        deltaVel = (vel - lastVel).magnitude;
        lastVel = vel;
    }

    /**
    *Returns the global velocity of the package
    */
    public Vector3 comparePackageVelocity()
	{
		return GetComponent<Rigidbody>().velocity;
	}


    public Vector3 GetAccel()
    {
        return myAccel;
    }

    public float GetDeltaVel()
    {
        return deltaVel;
    }

    /**
    *Changes the material of the package to reflect itś state in the DBC
    */
    public void breakPackage()
	{
		broken = true;
		GetComponent<Renderer> ().material.color = Color.black;
	}

	/**
    *Changes the material of the package to reflect itś state in the DBC
    */
	public bool isBroken()
	{
		return broken;
	}
	public void setPileID(int newID)
	{
		pileID=newID;
	}

	public int getPileID()
	{
		return pileID;
	}


}

