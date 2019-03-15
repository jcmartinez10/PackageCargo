using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
/**
  *This class reads the original txt files produced by the original program
  *It also tells the package manager to create the packages
  */
public class reportManager : MonoBehaviour
{
    //----------------------------------
    //PUBLIC VARIABLES
    //----------------------------------
    //The file path to the txt
    //example .\Assets\Chep\chep.txt
    public string filePath;
    //----------------------------------
    //PRIVATE VARIABLES
    //----------------------------------
    //File, reader and text for reading purposes
    private FileInfo theSourceFile = null;
    private StreamReader reader = null;
    private string text = " "; // assigned to allow first line to be read below
    //Atributes of the txt
    private int numberOfPackageTypes;
    private Vector3 containerSize;
    //----------------------------------
    //METHODS
    //----------------------------------



    void Start()
    {
        //ReadTxt();
        
    }
    /**
      *Reads the txt file and loads the info
      */
    void ReadTxt()
    {
        //Initialize the reading
        theSourceFile = new FileInfo(filePath);
        reader = theSourceFile.OpenText();

        //First Line
        text = reader.ReadLine();
        string[] split = text.Split('\t');
        numberOfPackageTypes = int.Parse(split[0]);
        containerSize = new Vector3(int.Parse(split[2]), int.Parse(split[3]), int.Parse(split[4]));
        Debug.Log("Total pcs = " + split[0] + " # Destinations = " + split[1]);        
        Debug.Log("Container size = " + split[2] + "mm " + split[3] + "mm " + split[4] + "mm");
        //Second Line
        text = reader.ReadLine();
        split = text.Split('\t');
        Debug.Log("Total Volume = " + split[0] + " Used Volume = " + split[1]);
        //Read the packages
        
        while (text != null)
        {
            text = reader.ReadLine();
            split = text.Split('\t');
            //Is it over?
            if (split[8] == "0")
                return;
            //Place Package
            Debug.Log(text);
            
        }
    }
}
