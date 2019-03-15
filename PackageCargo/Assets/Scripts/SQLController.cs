using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
/*
 * Use this class to connect with MySQL database
 */
public class SQLController : MonoBehaviour
{
    //----------------------------------
    //PUBLIC VARIABLES
    //----------------------------------
    public string[] items;
    //----------------------------------
    //PRIVATE VARIABLES
    //----------------------------------

    //----------------------------------
    //METHODS
    //----------------------------------

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
           // insertExperiment(i, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
    }
    /* IEnumerator Start()
     {
         Debug.Log("SQL Query starts");
         //WWW query = makeSQLQuery("SELECT * FROM CARGA");
         WWW query = makeSQLQuery("");
         yield return query;
         string answer = query.text;
         Debug.Log(answer);
         items = answer.Split(';');
         Debug.Log(items.Length);
         foreach (string item in items)
         {

             Debug.Log(item);
             Debug.Log(GetDataValue(item, "ID:"));
         }

     }*/

    string GetDataValue(string data, string index)
    {
        string value = "";
        if (data!="")
        {
            value = data.Substring(data.IndexOf(index) + index.Length);
            if (value.Contains("|"))
                value = value.Remove(value.IndexOf("|"));
        }
        
        return value;
    }
  
    /**
      *This method is used to make SQL querys to the SQL Server
      *
      */
    public WWW makeSQLQuery(string query)
    {
        Debug.Log(query);
        query =query.Replace(" ", "%20");            
        WWW itemsData = new WWW(query);
        
        return itemsData;        
    }
	
    /**
    *Returns the input of an experiment given an id
    *@param experimentId the id of the experiment
    */
	public WWW getExperimentInput(int experimentId,int containerID)
    {
        return makeSQLQuery("http://estdin.ingtext.com/xm.php?IDExperimento="+experimentId+"&IDContenedor="+containerID);
    }
    /**
   *Returns the input of an experiment given an id
   *@param experimentId the id of the experiment
   */
    public WWW getContainers()
    {
        return makeSQLQuery("http://estdin.ingtext.com/idCon.php");
    }
    /**
    *Insert a new experiment
    * @param clientNumber the number of diferentCostumers
    * @param ptypeNumber the different number of packages
    * @param exprimentName the name of the experiment
    * @param exprimentDescription the description of the experiment
    * @param containerID the id of the containerUsed
    */
    public WWW insertExperiment(int clientNumber,int ptypeNumber,string exprimentName,string exprimentDescription,int containerId)
    {
        return makeSQLQuery("http://estdin.ingtext.com/idex.php?CantidadClientes="+clientNumber+"&TiposCajasTotales="+ptypeNumber+"&Fecha=1&Nombre=%27"+exprimentName+"%27&Descripcion=%27"+ exprimentDescription+"%27&IDContenedor="+containerId);
    }
    /**
  *Returns the list of experiments
  *@param experimentId the id of the experiment
  */
    public WWW getExperiments()
    {
        return makeSQLQuery("http://estdin.ingtext.com/idex2.php");
    }
    /**
    *Returns the input of an experiment given an id
    *@param experimentId the id of the experiment
    */
    public WWW getExperiment(int id)
    {
        return makeSQLQuery("http://estdin.ingtext.com/idex3.php?IDExperimento="+id);
    }
    /**
   *Returns the input of an experiment given an id
   *@param experimentId the id of the experiment
   */
    public void insertExperimentOutput(int idCaja,float coordenada1x, float coordenada1y, float coordenada1z, int rotacion, float coordenada2x, float coordenada2y, float coordenada2z,float peso,int IDContenedor,int IDExperimento)
    {
        makeSQLQuery("http://estdin.ingtext.com/InSa.php?IDCaja="+ idCaja + "&Coor1x="+ coordenada1x + "&Coor1y="+ coordenada1y + "&Coor1z="+ coordenada1z + "&Coor2x="+ coordenada2x + "&Coor2y="+ coordenada2y + "&Coor2z="+ coordenada2z 
            + "&Peso="+peso+"&Rotacion="+rotacion+"&IDContenedor="+IDContenedor+"&IDExperimento="+IDExperimento);
    }

    /**
  *Returns the input of an experiment given an id
  *@param experimentId the id of the experiment
  */
    public void insertExperimentInput(Vector3 size,Vector3 vertical,int cantidad,int peso,Vector3 soporte,int idCliente,int idGrupo,int idContainer,int idExperimento)
    {
       makeSQLQuery("http://estdin.ingtext.com/IdEn.php?Largo="+size.z+"&Lvertical="+vertical[2]+"&Ancho="+size.x+"&Wvertical="+vertical[0]+
           "&Alto="+size.y+"&Hvertical="+vertical[1]+"&Cantidad="+cantidad+"&Peso="+peso+"&SoporteLW="+soporte[1]+
           "&SoporteLH="+soporte[2]+"&SoporteWH="+soporte[0]+"&Cliente="+idCliente+"&Grupo="+idGrupo+"&IDContenedor="+idContainer+"&IDExperimento="+ idExperimento);
    }

    /**
     * gets and experiment output
     * @param containerID the id of the container 
     * @param experimentID the id of the experiment
     */
    public WWW getExperimentOutput(int experimentID,int containerID)
    {
        return makeSQLQuery("http://estdin.ingtext.com/coSa.php?IDContenedor="+containerID +"&IDExperimento="+experimentID);
    }

    /**
     * Inserts a container and returns the new container ID
     * @param containerName the name of the container
     * @param containerDescription the description of the container
     * @param containerSize the size of the container
     */
    public WWW insertContainer(string  containerName, string containerDescription,Vector3 containerSize)
    {
        return makeSQLQuery("http://estdin.ingtext.com/idCon2.php?&Nombre=%27"+containerName+"%27&Descripcion=%27"+ containerDescription + "%27&Largo="+containerSize.x+ "&Ancho=" + containerSize.z + "&Alto=" + containerSize.y );
    }
    
        /**
     * Inserts the algorithm params of a experiment
     * @param containerName the name of the container
     * @param containerDescription the description of the container
     * @param containerSize the size of the container
     */
    public WWW insertAlgorithmParams(int experimentID,int p6, int p7)
    {
        return makeSQLQuery("http://estdin.ingtext.com/pamultd.php?IDExperimento=" +experimentID+"&P2=1&P3=1&P4=1&P5=1&P6="+p6+"&P7="+p7);
    }

    /**
  * Updates the experiment info
  * @param experiment id the id of the experiment
  * @param ms m1m2m3m4 the m1..m4
  * @param pUtilization
  * @param time
  */
    public WWW updateExperimentInfo(int experimentID,float []ms,float pUtilization,float time)
    {
        return makeSQLQuery("http://estdin.ingtext.com/upms.php?IDExperimento="+experimentID+"&M1="+ms[0]+"&M2="+ms[1]+"&M3="+ms[2]+"&M4="+ms[3]+"&TiempoComputo="+time+"&PorcentajeUtilizacion="+pUtilization);
    }
    public WWW pruebaResultados()
    {
        return makeSQLQuery("https://www.ingtext.com/ur/pk/index2.php");
    }
}
