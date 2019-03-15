using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMiniExp : MonoBehaviour {

    public int id;
    int containerID;
    public Text[] text;
    public GameObject gObject;

    public void setValues(int idP, GameObject gObj,string name,string description,float time, float utilization, float m1, float m2, float m3, float m4,int idContenedor)
    {
        gObject = gObj;
        //text = gObj.GetComponentInChildren<Text>();
        text[0].text = name+" ,id " + idP;
        text[1].text =description;
        text[2].text = "Time: " + time;
        text[3].text = "Utilization " + utilization + "%" ;
        text[4].text = "M1 "+m1+" ,M2 " +m2+" ,M3 "+m3+" ,M4 "+m4;
        id = idP;
        containerID = idContenedor;
    }
    public void setValuesPack(int idP, GameObject gObj, string name, string description, int quantity,int containerIDP)
    {
        gObject = gObj;
        //text = gObj.GetComponentInChildren<Text>();
        text[0].text = name + " ,id " + idP;
        text[1].text = description;
        text[2].text = "Quantity: " + quantity;
        id = idP;
        containerID = containerIDP;
    }
    public int getId() { return id; }
    public GameObject getGObject() { return gObject; }

    public void connectServer()
    {
        UIExperiment myui= GetComponentInParent<UIExperiment>();
        myui.getTempForExperiment(id,containerID);
    }
    public void connectServerPackages()
    {
        UIExperiment myui = GetComponentInParent<UIExperiment>();
        myui.getChepForExperiment(id,containerID);
    }
}
