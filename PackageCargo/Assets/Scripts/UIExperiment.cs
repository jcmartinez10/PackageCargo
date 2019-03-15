using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/**
* This class manages the UI of the experiments
*/
public class UIExperiment : MonoBehaviour
{

    //----------------------------------
    //PUBLIC VARIABLES
    //----------------------------------
    //The items of the list
    ArrayList items;
    //The prefab
    public GameObject UIExperimentPrefab;

    //PackageManager
    public PackageManager packageManager;
    //----------------------------------
    //PRIVATE VARIABLES
    //----------------------------------

    //----------------------------------
    //METHODS
    //----------------------------------
    // Use this for initialization
    void Start ()
    {
        items = new ArrayList();
	}

    public void putExperiments(string[] ids, string[] names, string[] descriptions, float[] times, float[] utilizations, float[] m1s, float[] m2s, float[] m3s, float[] m4s,int[] idContenedors)
    {
        deleteExperimentsFromUI();
        for(int i=0;i<ids.Length;i++)
        {
            string id = ids[i];
            int result =-1;
            if (int.TryParse(id,out result))
            {
                GameObject a = GameObject.Instantiate(UIExperimentPrefab, this.transform);
                UIMiniExp item = a.GetComponent<UIMiniExp>();
                item.setValues(int.Parse(id), a,names[i],descriptions[i],times[i],utilizations[i],m1s[i],m2s[i],m3s[i],m4s[i],idContenedors[i]);
                items.Add(item);
            }
        }

    }
    public void putExperimentsPackages(string[] ids, string[] names, string[] descriptions, int[] quantitys,int[] containerIDs)
    {
        deleteExperimentsFromUI();
        for (int i = 0; i < ids.Length; i++)
        {
            string id = ids[i];
            int result = -1;
            if (int.TryParse(id, out result))
            {
                GameObject a = GameObject.Instantiate(UIExperimentPrefab, this.transform);
                UIMiniExp item = a.GetComponent<UIMiniExp>();
                item.setValuesPack(int.Parse(id), a, names[i], descriptions[i], quantitys[i],containerIDs[i]);
                items.Add(item);
            }
        }

    }
    public void deleteExperimentsFromUI()
    {
        foreach(UIMiniExp item in items)
        {
            GameObject.Destroy(item.getGObject());            
        }
        items.Clear();
    }

    public void getChepForExperiment(int id,int containerID)
    {
        packageManager.readExperimentInput(id,containerID,false);
    }
    
    public void getTempForExperiment(int experimentID,int containerID)
    {
        packageManager.readExperimentResult(experimentID,containerID);
    }
}
