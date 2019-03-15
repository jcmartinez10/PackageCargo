using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*
 *This class controls the UI element of containers to resize the container
 */
public class UIContainer : MonoBehaviour
{
    //----------------------------------
    //PUBLIC VARIABLES
    //----------------------------------
    //This UI Container Size
    public Vector3 containerSize;
    //The title of this container
    public string title;
    //UI text for title and size
    public Text title_txt;
    public Text size_txt;
    //Is this a custom size?
    public bool isCustomSize;
    //Canvas panel for custom UI
    public GameObject customUICanvas;
    //The text of the  height, width, lenght
    public InputField lenght_Input;
    public InputField height_Input;
    public InputField width_Input;
    //Toggles for container
    public Toggle pallet_toggle;
    public Toggle container_toggle;
    //Game objects for container and pallet
    public GameObject pallet_GO;
    public MeshRenderer palletMesh;
    public GameObject container_GO;
    public MeshRenderer[] containerMesh;
    //Is it the manager?
    public bool isTheManager;
    public GameObject uiPrefab;
   
    //----------------------------------
    //PRIVATE VARIABLES
    //----------------------------------
    //The pacakge Manager
    PackageManager packageManager;
    int containerId;
    ArrayList containerList;
    
    //----------------------------------
    //METHODS
    //----------------------------------

    // Use this for initialization
    void Start ()
    {
        packageManager = PackageManager.instance;
        if (isTheManager)
        {
            containerList = new ArrayList();
            packageManager.setContainerUIMaster(this);
        }
        else
        {
            title_txt.text = title;
            size_txt.text = "(" + containerSize.x + "m ," + containerSize.y + "m ," + containerSize.z + "m )";
            
            if (isCustomSize)
            {
                pallet_toggle.onValueChanged.AddListener((value) =>
                {
                    palletListener(value);
                });
                container_toggle.onValueChanged.AddListener((value) =>
                {
                    containerListener(value);
                });
            }
        }
    }

    /**
      * Listener for pallet toggle. Swaps pallet mesh on and off
      */
    public void palletListener(bool value)
    {
        palletMesh.enabled=value;
    }
    /**
      * Listener for container toggle. Swaps continer wall mesh on and off
      */
    public void containerListener(bool value)
    {
        foreach(MeshRenderer mesh in containerMesh)
        {
            mesh.enabled = value;
        }

    }
    /*
     *Function Called when this UI element is Pressed
     */
    public void buttonPressed()
    {
        if (isCustomSize)
        {
           
            customUICanvas.SetActive(true);
        }
            
        packageManager.resizeContainer(containerSize,containerId);
    }

    /**
      *Close custom canvas
      */
      public void closeCustomCanvas()
    {
        if (isCustomSize)
            customUICanvas.SetActive(false);
    }
    /**
     *Saves custom canvas values
     */
    public void saveCustomCanvasValues()
    {
        if (isCustomSize)
        {
            containerSize.x = float.Parse(lenght_Input.text);
            containerSize.y = float.Parse(height_Input.text);
            containerSize.z = float.Parse(width_Input.text);
            size_txt.text = "(" + containerSize.x + "m ," + containerSize.y + "m ," + containerSize.z + "m )";
            packageManager.resizeContainer(containerSize,containerId);
        }
    }

    /**
      * Set custom size for the container
      */
      public void setCustomValues(Vector3 newSize,string newTitle,int containerIdP)
    {
        containerId = containerIdP;
        containerSize = newSize;
        title = newTitle;
        title_txt.text = title;
        size_txt.text = "(" + containerSize.x + "m ," + containerSize.y + "m ," + containerSize.z + "m )";
    }

    /**
    *Creates the containers on UI
    */
    public void createContainersOnUI(Vector3[] sizes,string[] titles,int[] containerIds)
    {
        deleteContainersFromUI();
        for (int i = 0; i < sizes.Length; i++)
        {
            GameObject theNewGuy= GameObject.Instantiate(uiPrefab, this.transform);
            containerList.Add(theNewGuy);
            theNewGuy.GetComponent<UIContainer>().setCustomValues(sizes[i], titles[i],containerIds[i]);
        }
    }

    public void deleteContainersFromUI()
    {
        foreach (GameObject item in containerList)
        {
            GameObject.Destroy(item);
        }
        containerList.Clear();
    }
}
