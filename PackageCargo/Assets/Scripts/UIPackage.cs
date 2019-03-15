using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/**
 * This class manages the UI of each package
 */
public class UIPackage : MonoBehaviour
{
    //----------------------------------
    //PUBLIC VARIABLES
    //----------------------------------
    //The Text of the Title
    public Text title;
    //The text of the  height, width, lenght
    public InputField lenght_Input;
    public InputField height_Input;
    public InputField width_Input;
    //text for the id
    public Text id;
    //Text for weight
    public InputField weight_Input;
    //Text for pcs
    public InputField pcs_Input;
    //Image of the color
    public Image colorImage;
    //side up
    public Toggle togglex;
    public Toggle toggley;
    public Toggle togglez;
    //forces
    public InputField[] force_input ;
    //client and group
    public InputField client_input;
    public InputField group_input;
    //----------------------------------
    //PRIVATE VARIABLES
    //----------------------------------

    //----------------------------------
    //METHODS
    //----------------------------------
    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}
    /**
     * This method updates the values in the UI
     */ 
    public void updateValues(Color packageColorP, int packageIdP, Vector3 packageSizeP, int quantityP, float weightP,bool isEmpty,Vector3 maxForce,Vector3 verticalPos,int clientP,int groupP)
    {
        if(!isEmpty)
        {
            lenght_Input.text = packageSizeP.z + "";
            height_Input.text = packageSizeP.y + "";
            width_Input.text = packageSizeP.x + "";
            pcs_Input.text = quantityP + "";
            weight_Input.text = weightP + "";
            force_input[0].text = maxForce.x + "";
            force_input[1].text = maxForce.y + "";
            force_input[2].text = maxForce.z + "";
            togglex.isOn = verticalPos.x == 0 ? false : true;
            toggley.isOn = verticalPos.y == 0 ? false : true;
            togglez.isOn = verticalPos.z == 0 ? false : true;
            client_input.text = clientP+"";
            group_input.text= groupP+"";
        }
        title.text = "Package " + packageIdP;
        id.text = packageIdP + "";        
        colorImage.color = packageColorP;
    }

   /**
    *This method eliminates the current package type 
    */
    public void deletePackage()
    {
        
        PackageManager.instance.deletePackageType(int.Parse(id.text));
        
    }
    /**
    *Updates the package information from the input
    */
    public void updatePackage()
    {
        try
        {
            Color packageColorP = colorImage.color;
            int packageIdP = int.Parse(id.text);
            Vector3 packageSizeP = new Vector3();
            packageSizeP.x = float.Parse(width_Input.text);
            packageSizeP.y = float.Parse(height_Input.text);
            packageSizeP.z = float.Parse(lenght_Input.text);
            Vector3 verticalP = new Vector3();
            verticalP.x = togglex.isOn?1:0;
            verticalP.y = toggley.isOn ? 1 : 0;
            verticalP.z = togglez.isOn ? 1 : 0;
            int clientP = int.Parse(client_input.text);
            int groupP = int.Parse(group_input.text);
            int quantityP = int.Parse(pcs_Input.text );
            float weightP = float.Parse(weight_Input.text);
            Vector3 maxForce = new Vector3();
            maxForce.x = float.Parse(force_input[0].text);
            maxForce.y = float.Parse(force_input[1].text);
            maxForce.z = float.Parse(force_input[2].text);
            PackageManager.instance.updatePackageValues(packageColorP, packageIdP, packageSizeP, quantityP, weightP,verticalP,clientP,groupP,maxForce);
        }
        catch (System.Exception)
        {
            Debug.Log("Información erronea");
            PackageManager.instance.switchErrorUI(true);
        }        
        
    }

    /**
      *Disables the panel
      */
      public void disablePanel()
    {
        this.gameObject.SetActive(false);
        Camera.main.gameObject.GetComponent<CameraControl>().resetCameraObjective();
    }
}
