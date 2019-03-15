using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Collections;
using UnityEngine.Networking;
using System.Xml;
using System.Text;
using UnityEditor;

/*
* This Class is in charge of managing all packages on a container
*/
public class PackageManager : MonoBehaviour
{
    
    //**************************************************************************
    //The definiton of a package type
    public class PackageType
    {
              
        //----------------------------------
        //PUBLIC VARIABLES
        //----------------------------------
        //The Size of the package
        //x = width
        // y = height
        //z = lenght
        public Vector3 packageSize;
        //Is possible vertical 0 no 1 yes
        public Vector3 verticalPos;
        //The Color of the Package
        public Color packageColor;
        //The id of the package
        public int packageId;
        //the number of packages
        public int quantity;
        //weight of the package
        public float weight;
        //The ui package controller
        public UIPackage uiPackage;
        //Positions for each package of this group
        public List<Vector3> packagePositions;
        //End position for each package
        public List<Vector3> endPositions;
        //Client and group id
        public int clientId;
        public int groupId;
        //force
        public Vector3 maxForce;
        //----------------------------
        //Methods
        //----------------------------
        public PackageType(Color packageColorP, int packageIdP, Vector3 packageSizeP, int quantityP, float weightP, UIPackage uiPackageP,Vector3 verticalPosP,int clientIdP,int groupIdP,Vector3 maxForceP)
        {
            maxForce = maxForceP;
            clientId= clientIdP;
            groupId = groupIdP;
            verticalPos = verticalPosP;
            packageColor = packageColorP;
            packageSize = packageSizeP;
            packageId = packageIdP;
            quantity = quantityP;
            weight = weightP;
            uiPackage = uiPackageP;
            packagePositions = new List<Vector3>();
            endPositions = new List<Vector3>();
            
        }
        public void updatePType(Color packageColorP, int packageIdP, Vector3 packageSizeP, int quantityP, float weightP, UIPackage uiPackageP,Vector3 verticalposP, int clientIdP, int groupIdP,Vector3 maxForceP)
        {
            maxForce = maxForceP;
            clientId = clientIdP;
            groupId = groupIdP;
            verticalPos =verticalposP;
            packageColor = packageColorP;
            packageSize = packageSizeP;
            packageId = packageIdP;
            quantity = quantityP;
            weight = weightP;
            uiPackage = uiPackageP;           
        }
        public void AddPosition(Vector3 initialPosition,Vector3 endPosition)
        {
            packagePositions.Add(initialPosition);
            endPositions.Add(endPosition);
        }
       
    }
    //**************************************************************************
    //----------------------------------
    //PUBLIC VARIABLES
    //----------------------------------
    //Units to be used
    public enum myUnits
    { m=1,dm=10,cm= 100, mm= 1000  }
    public myUnits dbUnit=myUnits.mm;
    //The file path to the txt
    //example .\Assets\Chep\chep.txt
    public string filePath;
    //The prefab of the gameobject to be placed on the UI
    public GameObject UIPrefab;
    //The list where the element is going to be placed
    public RectTransform UIList;   
    //Singleton instance
    public static PackageManager instance;
    //The prefab of a package
    public GameObject packagePrefab;
    //Experiment id
    public int experimentId;
    //The Color of the Package
    public Color[] packageColors;
    //Error message UI
    public GameObject[] errorMessage;
    //Warning message UI
    public GameObject[] warningMessage;
    //Info Panel
    public UIPackage infoPanel;
    //Size of the container in m
    public Vector3 containerSize;
    //The transform of the container
    public Transform containerTransform;
    //The Game object of the UI for a new container size
    public GameObject UIContainerSizeGO;
    //The list where the containerSize is going to be placed
    public RectTransform UIListVehicles;
    //Time out for server
    public float timeoutTime = 5f;
    //The Ip of the server
    public string server = "localhost";
    //UIExperiment controller
    public UIExperiment[] uiExperiment;
    public GameObject[] experimentUIElement;
    //UI Experiment Name Input
    public InputField[] UIExperimentName;
    //UI Run Algorithm Input
    public InputField[] UIRunAlgorithm;
    //UI CUrrent container
    public Text[] uiCurrentContainer;

    public Text brokenCounter;
	public Text fallenCounter;

	int predictedFB = 0;
	int predictedBB = 0;
	float computationTime=0f;

	public LayerMask mask = -1;
    //----------------------------------
    //PRIVATE VARIABLES
    //----------------------------------
    //Algorithm
    int p6=2;
    int p7 = 10;
    //File, reader and text for reading purposes
    private FileInfo theSourceFile = null;
    private StreamReader reader = null;
    private string text = " "; // assigned to allow first line to be read below
    //last id used
    private int lastId;
    //The package types for each package
    private Dictionary<int,PackageType> pTypes;
    //The packages to be managed
    private List<GameObject> packages;
   
    //Iterations for timeout and flag
    private int tcpIterations = 0;
    private bool isWaiting;
    //SQL Manager
    SQLController sqlController;
    //SQL 
    bool answerReady;
    string answer;
    //uicontainermaster to set new ui items
    UIContainer uiContainerMaster;
    //containerid
    int containerId;
    string containerName ="Custom Container";
    string containerDescription="Custom Container made in Unity";
    //Experiment Name and description
    string experimentName ="New Experiment";
    string experimentDescription="This Experiment was made in Unity";
    //Has the algorithm been executed?
    bool hasAnswer;
    //M1.M1.M3.M4
    float[] ms= new float[4];
    //P Utilization
    float pUtilization;
    //Time of algorithm execution
    float runTime;


//----------------------------------
	//STABILITY VARIABLES
	//----------------------------------
	//The mass of the packages
	private float packageMass;
	//The count of packages
	private int boxCount;
	//The starting delta for the packages, used to obtain M3 
	private float[] yDelta;
	//How far, in meters, must the packages change position to be considered as fallen
	public float fallThreshold = 0.05f;
	//The metric indicating the number of fallen boxes
	private float m3;
	//How fast, in meters per second, can the packages move in respect to the container
	public float velocityDelta = 5f;
	//The acceleration, in meters per second squared, that will cause the package to break
	public float allowableAcceleration = 5f;
	//The metric indicating the number of boxes within the damage boundary curve
	private float nDBC;
	//The adjusted metric indicating the boxes within the damage boundary curve
	private float m4;
	//The counter

    //----------------------------------
    //METHODS
    //----------------------------------
    //Singleton
    void Awake()
    {       
        //Check if instance already exists
        if (instance == null)
            //if not, set instance to this
            instance = this;
        //If instance already exists and it's not this:
        else if (instance != this)
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);

        //Sets this to not be destroyed when reloading scene
        //DontDestroyOnLoad(gameObject);
    }
    // Use this for initialization
    void Start ()
    {
        infoPanel.gameObject.SetActive(false);
        errorMessage[0].SetActive(false);
        lastId = 0;
        pTypes = new Dictionary<int,PackageType>();
        packages = new List<GameObject>();
        resizeContainer(containerSize,-1);
        sqlController = GetComponent<SQLController>();
        hasAnswer = false;
    }
    /**
     * Method that creates a new package type
     * @param packageColorP The color of the package type
     * @param packageIdP    The id of the package type
     * @param packageSizeP The size of the package type width, height, lenght in meters
     * @param quantityP The number of packages of these package type
     * @param weightP The weight of the package type in Kg
     * @param uiPackageP the UI controller of the package
     *@param determines if isEmpty
     */
    public void createNewPackageType(Vector3 packageSizeP,int quantityP, float weightP,bool isEmpty,int idP,Vector3 verticalPosP, int clientIdP, int groupIdP,Vector3 maxforceP)
    {
        Color packageColorP = packageColors[0];
        if (lastId < packageColors.Length)
            packageColorP = packageColors[lastId];
       
           
        //        char packageIdP = ALPHABET[lastId+1];
        Debug.Log("Creating package " + idP);
        lastId++;
        GameObject go = GameObject.Instantiate(UIPrefab);
        go.transform.SetParent(UIList,false);
        UIPackage uiPackageP = go.GetComponent<UIPackage>();
        //Create a new package type then add it to the list
        PackageType p = new PackageType(packageColorP, idP, packageSizeP, quantityP, weightP,uiPackageP, verticalPosP,  clientIdP,  groupIdP,maxforceP);
        uiPackageP.updateValues(packageColorP, idP, packageSizeP, quantityP, weightP, isEmpty,maxforceP,verticalPosP,clientIdP,groupIdP);
        pTypes.Add(idP,p);
        
    }
    /**
    * Method that updates a package type
    * @param packageColorP The color of the package type
    * @param packageIdP    The id of the package type
    * @param packageSizeP The size of the package type width, height, lenght in meters
    * @param quantityP The number of packages of these package type
    * @param weightP The weight of the package type in Kg
    * @param uiPackageP the UI controller of the package
    */
    public void updatePackageValues(Color packageColorP, int packageIdP, Vector3 packageSizeP, int quantityP, float weightP,Vector3 verticalPosP, int clientIdP, int groupIdP,Vector3 maxForceP)
    {      
          
        try
        {
            Debug.Log("Updating package " + packageIdP + " # " + quantityP);
            UIPackage uiPackageP = pTypes[packageIdP].uiPackage;
            pTypes[packageIdP].updatePType(packageColorP, packageIdP, packageSizeP, quantityP, weightP, uiPackageP, verticalPosP,  clientIdP,  groupIdP, maxForceP);
            uiPackageP.updateValues(packageColorP, packageIdP, packageSizeP, quantityP, weightP, false,maxForceP,verticalPosP,clientIdP,groupIdP);
            infoPanel.updateValues(packageColorP, packageIdP, packageSizeP, quantityP, weightP, false,maxForceP, verticalPosP, clientIdP, groupIdP);
            Debug.Log("Updated package " + packageIdP + " # " + quantityP);
            return;            
        }
        catch (Exception e)
        {
           Debug.Log("Error on update package "+e.Message);
        }          
        
    }
    /**
     * This method adds a UI Element and creates an empty package type
     */
    public void createUIPackage()
    {
        int idP = 0;
        while (pTypes.ContainsKey(idP))
        {
            idP++;
        }
        createNewPackageType(new Vector3(), 0, 0, true,idP,new Vector3(),0,0,new Vector3());        
    }
    //Destroys all packages
    void destroyAllPackages()
    {
        foreach (GameObject pack in packages)
        {
            GameObject.Destroy(pack);
        }
        packages.Clear();
    }
    //Load the packages in the space
    public void loadPackages()
    {
        destroyAllPackages();
        boxCount=0;
        if(!hasAnswer)
        {
            switchWarningUI(true);
            return;
        }
        //Load the packages

        foreach (PackageType pType in pTypes.Values)
        {
            for (int i = 0; i < pType.packagePositions.Count; i++)
            {
                GameObject go = GameObject.Instantiate(packagePrefab);
                packages.Add(go);
        
                go.GetComponentInChildren<Package>().setPackageValues(pType.endPositions[i]-pType.packagePositions[i], pType.packagePositions[i], pType.packageId, pType.packageColor,pType.weight);
               
            }
        }
        Debug.Log("Loaded "+packages.Count+" packages");
        boxCount=packages.Count;
    }

    /**
      *Deletes a package type with and ID
      *@id the id of the package
      */
      public void deletePackageType(int id)
    {
        PackageType item;
        pTypes.TryGetValue(id, out item);
        if(pTypes.ContainsKey(id))
        {
            Debug.Log("Deleted " + id);
            GameObject.Destroy( item.uiPackage.gameObject);
            infoPanel.gameObject.SetActive(false);
            pTypes.Remove(id);
            return;    
        }        
    }

    /**
    *Deletes all package types
    */
    void deleteAllPackageTypes()
    {
        int[] keys = pTypes.Keys.ToArray();
        foreach (int id in keys)
        {
            PackageType item;
            pTypes.TryGetValue(id, out item);
            if (pTypes.ContainsKey(id))
            {
                Debug.Log("Deleted " + id);
                GameObject.Destroy(item.uiPackage.gameObject);
                infoPanel.gameObject.SetActive(false);
                pTypes.Remove(id);
                
            }
        }
    }
    /**
    *Enables or disables error UI
    */
    public void switchErrorUI(bool uistate)
    {
        errorMessage[0].SetActive(uistate);
    }
    /**
    *Enables or disables error UI
    */
    public void switchEmptyPackUI(bool uistate)
    {
        errorMessage[1].SetActive(uistate);
    }
    /**
   *Enables or disables warning UI
   */
    public void switchWarningUI(bool uistate)
    {
        warningMessage[0].SetActive(uistate);
    }
    /**
  *Enables or disables warning UI
  */
    public void switchOkAlgorithmUI(bool uistate)
    {
        warningMessage[1].SetActive(uistate);
    }
    /**
 *Enables or disables warning UI
 */
    public void switchOkSaveUI(bool uistate)
    {
        warningMessage[2].SetActive(uistate);
    }
    /**
      *Shows package info on UI
      *@param id of the package
      */
    public void showPackageInfo(int id)
    {
        PackageType item;
        pTypes.TryGetValue(id, out item);
        if (pTypes.ContainsKey(id))
        {
            infoPanel.gameObject.SetActive(true);
            infoPanel.updateValues(item.packageColor, item.packageId, item.packageSize, item.quantity, item.weight, false,item.maxForce,item.verticalPos,item.clientId,item.groupId);
            return;
        }
        
    }

    /**
      *Resizes the container
      @param newSize the new Size of the container
      */
     public void resizeContainer(Vector3 newSize,int containerIdP)
    {
        //UI CUrrent container
        uiCurrentContainer[0].text = "Custom Container "+containerId;
        uiCurrentContainer[1].text = "( " + newSize.x + "m -" + newSize.y + "m -" + newSize.z + "m )";
        containerId = containerIdP;
        containerSize = newSize;
        containerTransform.localScale = containerSize;
        containerTransform.position = Vector3.zero;
    }

    /**
      *Adds a new container size on the UI
      */
      public void addNewContainerSizeUI(Vector3 newSize)
    {
        GameObject go = GameObject.Instantiate(UIContainerSizeGO);
        go.transform.SetParent(UIListVehicles,false);
        UIContainer uiContainerP = go.GetComponent<UIContainer>();
        uiContainerP.setCustomValues(newSize,"Loaded Container",containerId);
    }
    //--------------------------------------------------------------
    //Package load from txt file
    //--------------------------------------------------------------
    /**
     *Reads the txt file and loads the info
     */
    public void ReadTxt()
    {
        hasAnswer = true;
        destroyAllPackages();
        deleteAllPackageTypes();
        //Initialize the reading
        //theSourceFile = new FileInfo(filePath);
        var reader = new StreamReader("Resultados.txt");
        //reader = theSourceFile.OpenText();
        StreamWriter sw = new StreamWriter("Test.txt");
        text = reader.ReadLine();
        sw.WriteLine(text);
        sw.Close();


        //First Line
        string[] split = text.Split('\t');
        int totalPcs = int.Parse(split[0]);
        resizeContainer(new Vector3(float.Parse(split[2]), float.Parse(split[4]), float.Parse(split[3])) /((int) dbUnit), -1);
        addNewContainerSizeUI(new Vector3(float.Parse(split[2]), float.Parse(split[4]), float.Parse(split[3])) / ((int)dbUnit));
        Debug.Log("Total pcs = " + split[0] + " # Destinations = " + split[1]);
        Debug.Log("Container size = " + split[2] + "mm " + split[4] + "mm " + split[3] + "mm");
        //Second Line
        text = reader.ReadLine();
        split = text.Split('\t');
        Debug.Log("Total Volume = " + split[0] + " Used Volume = " + split[1]);
        //Read the packages
        
        while (text != "")
        {
            text = reader.ReadLine();
            Debug.Log(text);
            split = text.Split('\t');
            //Is it over?
            //if (split[8] == "0")
            //if (float.Parse(split[0]) >= containerSize.x)
            //        break;
            //Read Package
            Debug.Log("3:"+split[3]+" 5:"+split[5]+" :4"+ split[4]);
            Vector3 endPosition = new Vector3(float.Parse(split[3]), float.Parse(split[5]), float.Parse(split[4])) / ((int)dbUnit);
            Vector3 packPosition = new Vector3(float.Parse(split[0]), float.Parse(split[2]), float.Parse(split[1])) / ((int)dbUnit);
            Vector3 packSize = endPosition - packPosition;
            int packId = int.Parse(split[6]);
            int packGroup = int.Parse(split[7]);
            int packContainerId = int.Parse(split[8]);
             
            //Check If package type exist 
            if(!pTypes.ContainsKey(packId))
                createNewPackageType(packSize, 0, 1, false, packId,new Vector3(),0,0,new Vector3());
           
            PackageType pType = pTypes[packId];
            pType.AddPosition(packPosition,endPosition);
            updatePackageValues(pType.packageColor, packId, packSize, pType.quantity + 1, pType.weight,new Vector3(),0,0,new Vector3());
            
       }
        reader.Close();
        
    }
    //Recieves the UIContainerMaster
    public void setContainerUIMaster(UIContainer uiContainerM)
    {
        uiContainerMaster = uiContainerM;
    }
    //---------------------------------------
    //RUN ALGORITHM
    //---------------------------------------
    /*public WWW makeSQLQuery(string query)
    {
        Debug.Log(query);
        query = query.Replace(" ", "%20");
        WWW itemsData = new WWW(query);

        return itemsData;
    }*/

    public void runAlgorithm()
    {
        if(pTypes.Count==0)
        {
            enableMenu(4);
            switchEmptyPackUI(true);
            return;
        }
        foreach(PackageType pType in pTypes.Values)
        {
            pType.packagePositions.Clear();
        }
        try { p6 = int.Parse(UIRunAlgorithm[0].text); p7 = int.Parse(UIRunAlgorithm[1].text); }catch(Exception){ }
        ArrayList mymessage = new ArrayList();
        mymessage.Add(p6+"\t"+p7+"\n");
        
        //Number of packages and number of clients
        mymessage.Add( pTypes.Count + "\t"+1+"\n") ;
        //Container Size
        mymessage.Add( containerSize.x* ((int)dbUnit) + "\t"+containerSize.z* ((int)dbUnit) + "\t"+containerSize.y* ((int)dbUnit) + "\n");
        foreach(PackageType ptype in pTypes.Values)
        {
            //pId, Largo, Largo Vertical, Ancho, Ancho vertical, Alto, Alto vertical, Cantidad,Peso, Soporte largo, Soporte Ancho, Soporte Alto, Group Id,Destino
            mymessage.Add(ptype.packageId + "\t" + ptype.packageSize.x* ((int)dbUnit) + "\t" + ptype.verticalPos.x + "\t" + ptype.packageSize.z * ((int)dbUnit) + "\t" + ptype.verticalPos.z + "\t"  + ptype.packageSize.y * ((int)dbUnit) + "\t" + ptype.verticalPos.y + "\t"
                + ptype.quantity + "\t" + ptype.weight +"\t"+ ptype.maxForce.x + "\t" + ptype.maxForce.y + "\t" +ptype.maxForce.z + "\t" + ptype.clientId + "\t" +ptype.groupId + "\n");
        }
        
     

        StreamWriter sw = new StreamWriter("file.txt");
        foreach (string message in mymessage)
        {

            sw.WriteLine(message);
        }
        sw.Close();
        StartCoroutine(UploadLevel());

        StartCoroutine(pruebaResul());
    }


 

IEnumerator UploadLevel()
{
      

        //Carga el archivo file.txt
        var filename = "file.txt";
        var sourse = new StreamReader(filename);
        var fileContents = sourse.ReadToEnd();

        //convierte el archivo txt a bytes listo para cargar al servidor
        byte[] levelData = Encoding.UTF8.GetBytes(fileContents);
        sourse.Close();
        

        WWWForm form = new WWWForm();

        //print("form created ");
        form.AddField("action", "level upload");

        form.AddField("file", "file");

        form.AddBinaryData("file", levelData, "file.txt", "text/xml");

        Debug.Log("binary data added ");
        //URL donde se cargará el archivo txt
        WWW w = new WWW("http://www.ingtext.com/ur/pk/LevelUpload.php", form);
        yield return w;
        Debug.Log("Archivo del problema cargado");
    
}




//Para ejecutar el multidrop y retorna los resultados para insertarlos en el archivo "Resultados.txt"
IEnumerator pruebaResul()
{
    WWW query = sqlController.pruebaResultados();
    yield return query;
    string myParams = query.text;
    StreamWriter sw = new StreamWriter("Resultados.txt");
    sw.WriteLine(myParams);
    sw.Close();
}

    //-----------------------
    //READ EXPERIMENT RESULTS
    //-----------------------
    /**
     * Reads the result of a experiment and shows it on the screen
     */
    public void readExperimentResult(int experimentID, int containerID)
    {
        lastId = 0;
        hasAnswer = true;
        destroyAllPackages();
        deleteAllPackageTypes();
        StartCoroutine(getExperimentResult(experimentID,containerID));
        StartCoroutine(getContainers(containerID,true));
    }
    IEnumerator getExperimentResult(int experimentID,int containerID)
    {
        enableMenu(4);
        Debug.Log("Waiting for SQL answer for experiment input");

        WWW query = sqlController.getExperimentInput(experimentID, containerID);
        yield return query;
        Debug.Log("Query done " + query.text);
        createPacks(query.text.Split('\n'), 0);
        Debug.Log("Waiting for SQL answer for experiment results");

        query = sqlController.getExperimentOutput(experimentID,containerID);
        yield return query;
        Debug.Log("Query done "+query.text);
        insertPacks(query.text,0);
    }
    //---------------------------------------
    //READ EXPERIMENT INPUT
    //---------------------------------------
    /**
    *Starts fresh experiment
    */
        public void startFreshExperiment()
        {
            readExperimentInput(0, 0, true);
        }
        /**
     * Reads the Input of a experiment and shows it on the screen
     */
        public void readExperimentInput(int experimentID, int containerID,bool isFresh)
        {
            lastId = 0;
            experimentName = UIExperimentName[0].text;
            experimentDescription = UIExperimentName[1].text;
            hasAnswer = false;
            destroyAllPackages();
            deleteAllPackageTypes();
        if(!isFresh)
            StartCoroutine(getExperimentInput(experimentID, containerID));
            StartCoroutine(getContainers(containerID, !isFresh));
        }
        IEnumerator getExperimentInput(int experimentID, int containerID)
        {
            enableMenu(4);
            
            Debug.Log("Waiting for SQL answer for experiment input");

            WWW query = sqlController.getExperimentInput(experimentID, containerID);
            yield return query;
            Debug.Log("Query done " + query.text);
            createPacks(query.text.Split('\n'),0);
        }
    
    //---------------------------------------
    //-----------INSERT & CREATE PACK--------
    //---------------------------------------
    void insertPacks(string input,int i)
    {
       // Debug.Log("Initial String " + input);
        string[] line = input.Split('\n');
        
        while (i < line.Length)
        {
         //   Debug.Log("This is the string "+i +" "+ line[i]+" LL "+line.Length);
            if (line[i] != "" && line[i] != " " && line[i] != string.Empty && line[i] != "\t" && line[i] != "\n")
            {    //text = reader.ReadLine();
                string[] split = line[i].Split('\t');
                //Is it over?
                //if (split[8] == "0")
                if (float.Parse(split[2]) >= containerSize.x*(int)dbUnit)
                    break;
                //Read Package
                //IDCarga, IDCaja, Coor1x, Coor1y, Coor1z, Coor2x, Coor2y, Coor2z, Peso, Rotacion
                Debug.Log(text);
                Vector3 endPosition = new Vector3(float.Parse(split[5]), float.Parse(split[7]), float.Parse(split[6])) / ((int)dbUnit);
                Vector3 packPosition = new Vector3(float.Parse(split[2]), float.Parse(split[4]), float.Parse(split[3])) / ((int)dbUnit);
                Vector3 packSize = endPosition - packPosition;
                int packId = int.Parse(split[1]);
                
                int packGroup = int.Parse(split[7]);
                int packContainerId = int.Parse(split[8]);
                //----------------------------------------
                float weight = float.Parse(split[8]);
                //Check If package type exist 
                if (!pTypes.ContainsKey(packId))
                    createNewPackageType(packSize, 0, weight, false, packId, new Vector3(), 0, 0, new Vector3());

                PackageType pType = pTypes[packId];
                pType.AddPosition(packPosition, endPosition);
                //updatePackageValues(pType.packageColor, packId, packSize, pType.quantity + 1, pType.weight, new Vector3(), 0, 0, new Vector3());
            }
            i++;
        }
    }
    void createPacks(string[] line, int i)
    {

        string[] split = new string[line.Length];
        while (i < line.Length)
        {
            if (line[i] != "" && line[i] != " " && line[i] != string.Empty && line[i] != "\t" && line[i] != "\n")
            {    //text = reader.ReadLine();
                split = line[i].Split('\t');
                //Debug.Log("..." +line[i] );
                int packId = 0;
                if (int.TryParse(split[0], out packId)&& (split[0] != "" && split[0] != " " && split[0] != string.Empty && split[0] != "\t" && split[0] != "\n"))
                {
                    //Read Package
                    // 0IDCaja, 1Largo, 2LVertical, 3Ancho, 4WVertical, 5Alto
                    // 6AltoVertical,7Cantidad,8Peso,
                    //,9SoporteLW,10SoporteLH,11SoporteWH,12Cliente, 13Grupo
                    // Debug.Log("***" + split);
                    packId--;
                    Vector3 packSize = new Vector3(float.Parse(split[3]), float.Parse(split[5]), float.Parse(split[1])) / ((int)dbUnit);

                    int clientIdP = int.Parse(split[12]);
                    int groupIdP = int.Parse(split[13]);
                    Vector3 verticalPosP = new Vector3(int.Parse(split[4]) , int.Parse(split[6]), int.Parse(split[2]));
                    Vector3 maxForceP = new Vector3(float.Parse(split[10]), float.Parse(split[11]), float.Parse(split[9]));
                    //Vector3 maxForceP = new Vector3(9999, 9999, 9999);
                    //****
                    int quantityP = int.Parse(split[7]);
                    float weight = float.Parse(split[8]);
                    //Check If package type exist 
                    if (!pTypes.ContainsKey(packId))
                        createNewPackageType(packSize, quantityP, weight, false, packId, verticalPosP, clientIdP, groupIdP, maxForceP);

                }
            }
            i++;
        }
    }
    //---------------------------------------
    //-----------DYNAMIC SIMULATION--------
    //---------------------------------------
    /**
     *Determines the starting "Y" position of the packages relative to the container 
     *To be called after the packages have been instantiated
     */
	public void GetStartingYPositions()
	{
		m3 = 0f;
		yDelta = new float[packages.Count]; 
		for(int f = 0; f < packages.Count; f++)
		{
			yDelta [f] = packages [f].GetComponentInChildren<Rigidbody> ().position.y-containerTransform.position.y;
		}
	}

	/**
     *Compares the starting and final "Y" positions of the packages relative to the container 
     *To be called after the dynamic simulation is finished
     */
	public void GetFinalYPositions()
	{
		int fallen = 0;
		float yDeltaFinal = 0;
		for(int f = 0; f < packages.Count; f++)
		{
			yDeltaFinal = packages [f].GetComponentInChildren<Rigidbody> ().position.y-containerTransform.position.y;
			UnityEngine.Debug.Log (packages [f].GetComponentInChildren<Rigidbody> ().velocity.x + ";" 
				+ packages [f].GetComponentInChildren<Rigidbody> ().velocity.y + ";" + packages [f].GetComponentInChildren<Rigidbody> ().velocity.z);
			if (Math.Abs (yDelta [f] - yDeltaFinal) > fallThreshold) {
				UnityEngine.Debug.Log (f + " fell in the simulation");
				fallen++;
			}
		}
		m3 = fallen/boxCount;
		//fallenCounter.text=("Fallen boxes: "+fallen);
		//brokenCounter.text=("Broken boxes: "+m4);
	}

	/**
     *Checks the position of every package in respect to the DBC,
     *To be called during Update, FixedUpdate, or LateUpdate
     */
	public void CheckDamage()
	{
		float relativeVelocity = 0f;
		Vector3 acceleration = Vector3.zero;
		Vector3[] lastPosition = new Vector3[packages.Count+1];
        Vector3[] lastVelocity = new Vector3[packages.Count + 1];
        lastPosition[0] = packages[0].GetComponentInChildren<Rigidbody>().position;
        lastVelocity[0] = Vector3.zero;
        for (int f = 0; f < packages.Count; f++)
        {
            lastPosition[f] = packages[f].GetComponentInChildren<Rigidbody>().position;
            lastVelocity[f] = Vector3.zero;
        }
        for (int f = 0; f < packages.Count; f++)
		{
            relativeVelocity = packages[f].GetComponentInChildren<Package>().GetDeltaVel();
            if (!packages [f].GetComponentInChildren<Package> ().isBroken()) {
				if ((relativeVelocity)> velocityDelta) {
					packages [f].GetComponentInChildren<Package> ().breakPackage();
                    print(relativeVelocity);
                }
				else if ((packages[f].GetComponentInChildren<Package>().GetAccel()).magnitude > allowableAcceleration) {
                    packages [f].GetComponentInChildren<Package> ().breakPackage();
                    print((packages[f].GetComponentInChildren<Package>().GetAccel()).magnitude);
                    nDBC++;
				} 
            }
            lastVelocity[f] = packages[f].GetComponentInChildren<Rigidbody>().velocity;
            m4 = nDBC / boxCount;
        }
		//		print (m4);
		//brokenCounter.text=("Broken boxes: "+m4);
	}

	/**
     *Returns the total mass of the packages
     */
	public float GetMass(){
		return packageMass;
	}


    //---------------------------------------
    //-----------MECHANICAL MODEL--------
    //---------------------------------------
        public void simpleMetrics(){
		loadPackages ();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		int upTopBoxes = 0;
		float maxSlide = 0.1f;
		Vector3 boxPosition = Vector3.zero;
		Vector3 boxSize = Vector3.zero;
		float boxMass = 0f;
		RaycastHit hit;
		for (int f = 0; f < packages.Count; f++) {
			boxPosition = packages [f].transform.position;
			boxSize = packages [f].transform.localScale;
			maxSlide = boxSize.x * 0.5f;
			boxMass = packages [f].GetComponentInChildren<Rigidbody> ().mass;
			if (!Physics.BoxCast (boxPosition, boxSize, Vector3.up, out hit, packages [f].transform.rotation, boxSize.y+0.01f, mask)) {
				upTopBoxes++;
				//+tranform.right es adelante
				if (!Physics.BoxCast (boxPosition-0.1f*Vector3.up, boxSize, -packages [f].transform.right, out hit, packages [f].transform.rotation, 1.5f*boxSize.x+0.1f)&&slideTestForward(9.81f *packages [f].transform.right,boxSize,boxMass,boxSize.x)) {
					predictedFB++;
					//UnityEngine.Debug.Log (f + " theoretically fell");
				}
				else if (!Physics.BoxCast (boxPosition-0.1f*Vector3.up, boxSize, packages [f].transform.right, out hit, packages [f].transform.rotation, 1.5f*boxSize.x+0.1f)&&slideTestForward(2*9.81f * -packages [f].transform.right,boxSize,boxMass,boxSize.x)) {
					predictedFB++;
					//UnityEngine.Debug.Log (f + " theoretically fell");
				}
				else if (!Physics.BoxCast (boxPosition-0.1f*Vector3.up, boxSize, packages [f].transform.forward, out hit, packages [f].transform.rotation, 1.5f*boxSize.z+0.1f)&&slideTestSideways(9.81f * packages [f].transform.forward,boxSize,boxMass,boxSize.z)) {
					predictedFB++;
					//UnityEngine.Debug.Log (f + " theoretically fell");
				}
				else if (!Physics.BoxCast (boxPosition-0.1f*Vector3.up, boxSize, -packages [f].transform.forward, out hit, packages [f].transform.rotation, 1.5f*boxSize.z+0.1f)&&slideTestSideways(9.81f * -packages [f].transform.right,boxSize,boxMass,boxSize.z)) {
					predictedFB++;
					//UnityEngine.Debug.Log (f + " theoretically fell");
				}
			}
		}
		stopwatch.Stop();
		computationTime = stopwatch.ElapsedMilliseconds;
		fallenCounter.text=("Fallen boxes: "+predictedFB);
    }

    public bool slideTestForward(Vector3 accelDirection, Vector3 boxSize, float boxMass, float slideD){
		bool response = false;
		float kineticFriction = 0.3f;
		float staticFriction = 0.5f;
		float timeAccel = 2f;
		float maxSlide = slideD+0.2f;
		float maxAngle = 1f;
		float tippingMoment = boxMass * (9.81f * boxSize.x * 0.5f -accelDirection.magnitude * boxSize.y * 0.5f);
		float inertia = boxMass * (boxMass * (boxSize.x * boxSize.x + boxSize.y * boxSize.y) / 12f) + 0.25f * ((boxSize.x * boxSize.x + boxSize.y * boxSize.y));
		if (accelDirection.magnitude>9.81f*staticFriction&&0.5f * accelDirection.magnitude * kineticFriction * timeAccel * timeAccel > maxSlide) {
			response = true;
		}
		else if (tippingMoment * inertia * timeAccel / 2f > maxAngle) {
			response = true;
		}
		return response;
	}

	public bool slideTestSideways(Vector3 accelDirection, Vector3 boxSize, float boxMass, float slideD){
		bool response = false;
		float kineticFriction = 0.3f;
		float timeAccel = 2f;
		float maxSlide = slideD+0.2f;
		float maxAngle = 0.5f;
		float tippingMoment = boxMass * (9.81f * boxSize.z * 0.5f -accelDirection.magnitude * boxSize.y * 0.5f);
		float inertia = boxMass * (boxMass * (boxSize.z * boxSize.z + boxSize.y * boxSize.y) / 12f) + 0.25f * ((boxSize.z * boxSize.z + boxSize.y * boxSize.y));
		if (0.5f * accelDirection.magnitude * kineticFriction * timeAccel * timeAccel > maxSlide) {
			response = true;
		}
		else if (tippingMoment * inertia * timeAccel / 2f > maxAngle) {
			response = true;
		}
		return response;
	}


    public void m4model(){

    }

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //~~~~~~~~~~~~~~~~~~~~SAVE DATA~~~~~~~~~~~~~~~~~~~~
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    /**
   Starts the Save all data function
       */
    public void saveAllData(){ StartCoroutine(saveAllSQL());}
    /**
    * Saves all the data on SQL
    */
    IEnumerator saveAllSQL()
    {
        //Insert Container
        
        WWW query = sqlController.insertContainer(containerName, containerDescription, containerSize * ((int) dbUnit));
        yield return query;
        int containerId = int.Parse(query.text);
        //Insert Experiment
        int clientNumber = 1;
        int ptypeNumber = pTypes.Count;
        if (ptypeNumber > 0)
        {
            query = sqlController.insertExperiment(clientNumber, ptypeNumber, experimentName, experimentDescription, containerId);
            yield return query;
                
            experimentId = int.Parse(query.text);
                
            //Insert Chep &Temp
            if (hasAnswer)
                sqlController.insertAlgorithmParams(experimentId, p6, p7);
                sqlController.updateExperimentInfo(experimentId, ms, pUtilization, runTime);
                foreach (PackageType pType in pTypes.Values)
                {
                    sqlController.insertExperimentInput(pType.packageSize* ((int)dbUnit), pType.verticalPos,pType.quantity,(int)pType.weight,pType.maxForce,pType.clientId,pType.groupId,containerId,experimentId);
                    for (int i = 0; i < pType.packagePositions.Count; i++)
                    {
                        sqlController.insertExperimentOutput(pType.packageId,pType.packagePositions[i].x* ((int)dbUnit), pType.packagePositions[i].z* ((int)dbUnit), pType.packagePositions[i].y * ((int)dbUnit), 0,pType.endPositions[i].x* ((int)dbUnit), pType.endPositions[i].z * ((int)dbUnit), pType.endPositions[i].y * ((int)dbUnit), pType.weight,containerId,experimentId);
                    }
                }
            
        }
        switchOkSaveUI(true);
    }
    //------------------------------------------------------------------------------
    //TCP CONNECTION 
    //------------------------------------------------------------------------------
    IEnumerator waitForResponse()
    {
        Debug.Log("Waiting another " + timeoutTime + " seconds... Iteration # "+tcpIterations);
        isWaiting = true;
        yield return new WaitForSeconds(timeoutTime);
        tcpIterations++;
        isWaiting = false;
    }

    

    IEnumerator getExperiments()
    {
        enableMenu(1);
        
        UnityEngine.Debug.Log("Waiting for SQL answer for experiments");

        WWW query = sqlController.getExperiments();
        yield return query;
        Debug.Log("SQL Answer Done");
        string myParams = query.text;
        string[] ids = myParams.Split('\n');
        //****** Create arrays*****
        string[] answer;
        string[] names = new string[ids.Length];
        string[] descriptions = new string[ids.Length];
        float[] times = new float[ids.Length];
        float[] utilizations = new float[ids.Length];
        float[] m1s = new float[ids.Length];
        float[] m2s = new float[ids.Length];
        float[] m3s = new float[ids.Length];
        float[] m4s = new float[ids.Length];
        int[] idContenedors = new int[ids.Length];
        for (int i = 0;i<ids.Length;i++)
        {
            string id = ids[i];
            int myid;
            if(int.TryParse(id,out myid))
            {
                query = sqlController.getExperiment(myid);
                yield return query;
                myParams = query.text;
                answer = myParams.Split('\t');
                names[i] = answer[9];
                descriptions[i] = answer[10];
                times[i] = float.Parse(answer[3]);
                utilizations[i] = float.Parse(answer[4]);
                m1s[i] = float.Parse(answer[5]);
                m2s[i] = float.Parse(answer[6]);
                m3s[i] = float.Parse(answer[7]);
                m4s[i] = float.Parse(answer[8]);
                idContenedors[i] = int.Parse(answer[11]);
            }
        }
        
       
        uiExperiment[0].putExperiments(ids,names,descriptions,times,utilizations,m1s,m2s,m3s,m4s,idContenedors);
        
    }
    IEnumerator getExperimentsPackages()
    {
        enableMenu(3);

        UnityEngine.Debug.Log("Waiting for SQL answer for experiment packages");

        WWW query = sqlController.getExperiments();
        yield return query;
        Debug.Log("SQL Answer Done");
        string myParams = query.text;
        UnityEngine.Debug.Log(myParams);
        string[] ids = myParams.Split('\n');
        //****** Create arrays*****
        string[] answer;
        string[] names = new string[ids.Length];
        string[] descriptions = new string[ids.Length];
        int[] quantitys = new int[ids.Length];
        int[] containerIDs = new int[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            string id = ids[i];
            int myid;
            if (int.TryParse(id, out myid))
            {
                query = sqlController.getExperiment(myid);
                yield return query;
                myParams = query.text;
                answer = myParams.Split('\t');
                names[i] = answer[9];
                descriptions[i] = answer[10];
                quantitys[i] = int.Parse(answer[1]);
                containerIDs[i] = int.Parse(answer[11]);
            }
        }


        uiExperiment[1].putExperimentsPackages(ids, names, descriptions,quantitys,containerIDs);

    }

    IEnumerator getContainers(int containerID,bool resizeTrailer)
    {
        enableMenu(4);

        Debug.Log("Waiting for SQL answer for containers");

        WWW query = sqlController.getContainers();
        yield return query;
        Debug.Log("SQL Answer Done");
        string myParams = query.text;
        
        //****** Create arrays*****
        string[] answer = myParams.Split('\n'); ;
        Vector3[] sizes = new Vector3[answer.Length];
        string[] titles = new string[answer.Length];
        int[] containerIds = new int[answer.Length];
        string[] subanswer;
        for (int i = 0; i < answer.Length; i++)
        {

            //Debug.Log(answer[i]);
            if (answer[i] != ""&& answer[i] != " "&& answer[i] != string.Empty && answer[i] != "\t" && answer[i] != "\n")
            {
                subanswer = answer[i].Split('\t');
                //Debug.Log(subanswer[3] + " - " + subanswer[4] + " - " + subanswer[5]);
                if (subanswer[3] != "0" && subanswer[3] != "" && subanswer[3] != " " && subanswer[3] != string.Empty && subanswer[3] != "\t" && subanswer[3] != "\n")
                {
                    sizes[i] = new Vector3(float.Parse(subanswer[3]), float.Parse(subanswer[5]), float.Parse(subanswer[4])) / ((int)dbUnit);
                    titles[i] = subanswer[1];
                    containerIds[i] = int.Parse(subanswer[0]);
                    if (containerIds[i] == containerID && resizeTrailer)
                        resizeContainer(sizes[i], containerID);
                }
            }
            
        }


        uiContainerMaster.createContainersOnUI(sizes, titles, containerIds);

    }
    public void getExperimentsForUI()
    {
        StartCoroutine(getExperiments());
    }
    public void getExperimentsPackagesForUI()
    {
        StartCoroutine(getExperimentsPackages());
    }
    public void enableMenu(int uiId)
    {
        
        for (int i = 0; i < experimentUIElement.Length; i++)
        {
            experimentUIElement[i].SetActive(false);
        }
        experimentUIElement[uiId].SetActive(true);
    }

   
    public void getUIContainers()
    {
        StartCoroutine(getContainers(0,false));
    }

    public void Connect(ArrayList mymessage)
    {

        //Delete existing packages
        //destroyAllPackages();
        //deleteAllPackageTypes();

        //get the message 
        ArrayList messages = new ArrayList();

        messages.Add(mymessage);

        //try
        //{
            /* Debug.Log("Starting Connection");
             // Create a TcpClient.
             // Note, for this client to work you need to have a TcpServer 
             // connected to the same address as specified by the server, port
             // combination.
             Int32 port = 13000;
             TcpClient client = new TcpClient(server, port);

             //-----------------------WRITE   TEXT---------------------
             //string message = "Hello World";
             //---------------------------------------------------------------

             /*
             messages.Add("50\t1\n");
             messages.Add("100\t100\t100\n");
             messages.Add("0\t94\t1\t25\t1\t72\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("1\t11\t1\t84\t1\t96\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("2\t36\t1\t10\t1\t34\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("3\t72\t1\t26\t1\t71\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("4\t64\t1\t73\t1\t56\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("5\t81\t1\t22\t1\t97\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("6\t30\t1\t23\t1\t17\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("7\t46\t1\t27\t1\t47\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("8\t67\t1\t9\t1\t67\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("9\t69\t1\t1\t1\t98\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("10\t88\t1\t69\t1\t64\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("11\t47\t1\t95\t1\t99\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("12\t20\t1\t95\t1\t93\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("13\t23\t1\t8\t1\t32\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("14\t67\t1\t73\t1\t55\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("15\t69\t1\t46\t1\t95\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("16\t67\t1\t40\t1\t85\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("17\t70\t1\t32\t1\t91\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("18\t90\t1\t25\t1\t96\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("19\t45\t1\t100\t1\t84\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("20\t90\t1\t34\t1\t82\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("21\t93\t1\t93\t1\t38\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("22\t82\t1\t43\t1\t95\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("23\t98\t1\t38\t1\t97\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("24\t69\t1\t90\t1\t72\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("25\t66\t1\t90\t1\t25\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("26\t81\t1\t33\t1\t88\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("27\t37\t1\t10\t1\t21\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("28\t84\t1\t17\t1\t69\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("29\t97\t1\t11\t1\t99\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("30\t74\t1\t37\t1\t74\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("31\t84\t1\t27\t1\t73\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("32\t51\t1\t97\t1\t51\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("33\t38\t1\t74\t1\t95\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("34\t23\t1\t42\t1\t26\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("35\t72\t1\t86\t1\t64\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("36\t6\t1\t86\t1\t67\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("37\t86\t1\t25\t1\t88\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("38\t5\t1\t74\t1\t88\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("39\t92\t1\t90\t1\t10\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("40\t92\t1\t12\t1\t70\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("41\t24\t1\t80\t1\t93\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("42\t99\t1\t50\t1\t74\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("43\t66\t1\t77\t1\t86\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("44\t47\t1\t82\t1\t77\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("45\t92\t1\t41\t1\t76\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("46\t93\t1\t10\t1\t99\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("47\t92\t1\t80\t1\t39\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("48\t96\t1\t3\t1\t92\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");
             messages.Add("49\t88\t1\t21\t1\t91\t1\t1\t39\t10000\t10000\t99999\t0\t1\n");

             //---------------------------------------------------------------
             // Get a client stream for reading and writing.
             //  Stream stream = client.GetStream();
            Debug.Log("Data transmision starting");
             Byte[] data;
             NetworkStream stream = client.GetStream();
             foreach (string message in mymessage)
             {
                 // Translate the passed message into ASCII and store it as a Byte array.
                  data = System.Text.Encoding.ASCII.GetBytes(message);            
                 // Send the message to the connected TcpServer. 
                 stream.Write(data, 0, data.Length);
                 Debug.Log("Sent:" + message);

             }
             Debug.Log("Ending Transmition");

             // Translate the passed message into ASCII and store it as a Byte array.
             data = System.Text.Encoding.ASCII.GetBytes("#\n");
             // Send the message to the connected TcpServer. 
             stream.Write(data, 0, data.Length);

             //-----------------------READ   TEXT---------------------
             // Receive the TcpServer.response.

             // Buffer to store the response bytes.
             data = new Byte[256];

             // String to store the response ASCII representation.
             String text = String.Empty;

             // Read the first batch of the TcpServer response bytes.
             Int32 bytes = 0;
             isWaiting = false;
             tcpIterations = 0;
             Debug.Log("Waiting for Server Response");
             while (tcpIterations < 3)
             {
                 while (stream.DataAvailable)
                 {
                     bytes = stream.Read(data, 0, data.Length);
                     text += System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                 }
                 if (text == String.Empty&&!isWaiting)
                     StartCoroutine(waitForResponse());
                 else if(text!=string.Empty)
                     tcpIterations = 3;
             }

             Debug.Log("Received:" + text);
             */


            StreamReader text2;
            //text2 = new StreamReader("C:\\Users\\AIA\\Documents\\Proyecto -PackageCargo\\sebbaraca-packagecargo-cb7868b98100\\sebbaraca-packagecargo-cb7868b98100\\PackageCargo\\Test.txt");
            text2 = new StreamReader("Test.txt");
            text = text2.ReadToEnd();
            Debug.Log(text);
            string[] bigSplit = text.Split('#');
            //Metrics
            string[] metrics = bigSplit[1].Split('\t');
            ms[0] = float.Parse(metrics[1]);
            ms[1] = float.Parse(metrics[2]);
            runTime = float.Parse(metrics[0]);
            pUtilization = float.Parse(metrics[3]);
            //First Line
            string[] line = bigSplit[0].Split('\n');
            string[] split = line[0].Split('\t');
            int totalPcs = int.Parse(split[0]);
            //resizeContainer(new Vector3(float.Parse(split[2]), float.Parse(split[4]), float.Parse(split[3])) / 100,containerId);
            //addNewContainerSizeUI(new Vector3(float.Parse(split[2]), float.Parse(split[4]), float.Parse(split[3])) / 100);
            //Debug.Log("Total pcs = " + split[0] + " # Destinations = " + split[1]);
            //Debug.Log("Container size = " + split[2] + "mm " + split[4] + "mm " + split[3] + "mm");
            //Second Line
            //text = reader.ReadLine();
            split = line[1].Split('\t');

            //Debug.Log("Total Volume = " + split[0] + " Used Volume = " + split[1]);
            //Read the packages
            int i = 2;
            while (i < line.Length)
            {
                //text = reader.ReadLine();
                split = line[i].Split('\t');
                //Is it over?
                //if (split[8] == "0")
                // Debug.Log(split[0] + " split 0");
                //Debug.Log(line[i]+ " line "+i);
                //Debug.Log("Container Size x " + containerSize.x +" split[0] "+split[0]);
                float x;
                if (!float.TryParse(split[0], out x) || float.Parse(split[0]) >= containerSize.x * ((int)dbUnit))
                    break;
                //Debug.Log("Yes");
                //Read Package
                //Debug.Log(text);
                Vector3 endPosition = new Vector3(float.Parse(split[3]), float.Parse(split[5]), float.Parse(split[4])) / ((int)dbUnit);
                Vector3 packPosition = new Vector3(float.Parse(split[0]), float.Parse(split[2]), float.Parse(split[1])) / ((int)dbUnit);
                Vector3 packSize = endPosition - packPosition;
                int packId = int.Parse(split[6]);
                int packGroup = int.Parse(split[7]);
                int packContainerId = int.Parse(split[8]);

                //Check If package type exist 
                if (!pTypes.ContainsKey(packId))
                    createNewPackageType(packSize, 0, 1, false, packId, new Vector3(), 0, 0, new Vector3());

                PackageType pType = pTypes[packId];
                pType.AddPosition(packPosition, endPosition);
                // updatePackageValues(pType.packageColor, packId, packSize, pType.quantity + 1, pType.weight,new Vector3(),0,0,new Vector3());
                i++;
            }
            enableMenu(4);
            hasAnswer = true;
            switchOkAlgorithmUI(true);
            //loadPackages();
            //-------------------------------------------------------
            // Close everything.
            //Debug.Log("Connection Closed");
            //stream.Close();
            //client.Close();
       // }
   
        /*catch (ArgumentNullException)
        {
            Debug.Log("ArgumentNullException" + e);
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }*/

        //Debug.Log("\n Press Enter to continue...");
        //Console.Read();
    }
}
