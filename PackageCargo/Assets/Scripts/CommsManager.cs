using UnityEngine;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct PacketeStruct
{
    // Estructura del bote. En esta estructura se tiene toda la información del bote
    [MarshalAs(UnmanagedType.Bool)]
    public bool estado;
    public float velocidad;
 
    // Método encargado de serializar el struct bote
    public byte[] Serialize()
    {
        byte[] buffer = new byte[Marshal.SizeOf(typeof(PacketeStruct))];
        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        IntPtr pBuffer = handle.AddrOfPinnedObject();

        Marshal.StructureToPtr(this, pBuffer, false);
        handle.Free();

        return buffer;
    }
    // Método encargado de deserializar el struct bote
    public void Deserialize(byte[] data)
    {
        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        this = (PacketeStruct)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(PacketeStruct));
        handle.Free();
    }
}


public class CommsManager : MonoBehaviour
{
    //----------------------------------------------
    // Constantes
    //----------------------------------------------

    // IP del servidor
    private string IP_ADRESS_SERVER = "127.0.0.1";    
    	
    //----------------------------------------------
    // Variables
    //----------------------------------------------
    // Estado de conexión del simulador
    private bool isOnline;

    //-----------------------------------------------------
    // VARIABLES CONEXIÓN TCP
    //-----------------------------------------------------

    // TCP Listener del servidor
    private TcpListener tcpServer = null;

    // Thread encargado de escuchar los mensajes enviados por el simulador dinámico
    private Thread listenThreadSimulador;

    // Arreglo de Bytes donde se va a enviar la información del simulador dinamico
    private byte[] bytesSimulador = new byte[1024];

    // Se encarga de proveer comunicación para el simulador dinámico por medio de TCP
    private TcpClient client;

    // Stream object para leer y escribir
    private NetworkStream stream;


    private IPAddress localAddr;

    //-----------------------------------------------------
    // VARIABLES CONEXIÓN UDP
    //-----------------------------------------------------

    // UDP Listener del servidor
    private UdpClient udpClient;

    // Arreglo de Bytes donde se va a enviar la información de la ametralladora .50
    private byte[] bytesUDPPackage = new byte[1024];

    // Thread encargado de escuchar los mensajes enviados por la .50
    private Thread listenUDPThread;

    private IPEndPoint ipend;

    private Socket socketUDP;

	private PacketeStruct packageStruct;


    //--------------------------------------------------------
    // Métodos
    //--------------------------------------------------------

    // Called when the simulation begins
    public void Start()
    {
    
        // Simulador Dinámico
     
		localAddr = IPAddress.Parse(IP_ADRESS_SERVER);
        
        tcpServer = new TcpListener(localAddr, 13000);
      
        // Start listening for client requests.
        tcpServer.Start();
        listenThreadSimulador = new Thread(new ThreadStart(ListenTCPClients));
        listenThreadSimulador.Start();
       
    }
    
    /**
    *Listens for clients
    */
    /*
    public void ListenUDPClients()
    {
        socketUDP = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socketUDP.Bind(ipend);

        //udpClient = new UdpClient(ipend);
        Debug.Log("Waiting connection UDP");

        while (true)
        {
            try
            {
                
                IPEndPoint tmpIpEnd = new IPEndPoint(localAddr, 14000);
                EndPoint remoteEP = tmpIpEnd;

                int resp = socketUDP.ReceiveFrom(bytesUDPPackage, ref remoteEP);
                //Debug.Log("Recibió : " + bytesUDPPackage[0]);

                //Write a msg udp
                if(Input.GetButtonDown("Jump"))
                {
                    string mensajeInicial = "Hello World";
                    byte[] msg = System.Text.Encoding.UTF8.GetBytes(mensajeInicial);
                    stream.Write(msg, 0, msg.Length);
                    Debug.Log("Wrote: " + mensajeInicial);
                }
                
            }
            catch (Exception e)
            {
                Debug.Log(e);
                socketUDP.Close();

                udpClient.Close();
            }
        }
    }
    */

    // Método encargado de escuchar los mensajes del simulador dinámico
    public void ListenTCPClients()
    {
        try
        {
            Debug.Log("Waiting connection");
            // Perform a blocking call to accept requests.
            // You could also user server.AcceptSocket() here.
            client = tcpServer.AcceptTcpClient();
            Debug.Log("Connected!");
            
            // Get a stream object for reading and writing
            stream = client.GetStream();
            //string data = null;

            int i;
            while ((i = stream.Read(bytesSimulador, 0, bytesSimulador.Length)) != 0)
            {
                GCHandle handle = GCHandle.Alloc(bytesSimulador, GCHandleType.Pinned);
                packageStruct = (PacketeStruct)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(PacketeStruct));
                handle.Free();
                //Debug.Log("Bote info - Velocidad: "+donBoteRec.velocidad+" - Horizontal: "+donBoteRec.horizontalInput+" - Rot X: "+donBoteRec.rotX+ " - Rot Z: "+donBoteRec.rotZ);
            }
            // Shutdown and end connection
            client.Close();
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e);
        }
        finally
        {
            // Stop listening for new clients.
            tcpServer.Stop();
        }
    }


    //Disconnects all the services
    public void endConnections()
    {
        if (tcpServer != null)
        {
            tcpServer.Stop();
        }
        if (client != null)
        {
            client.Close();
        }
        if (stream != null)
        {
            stream.Close();
        }
        if (socketUDP != null)
        {
            socketUDP.Close();
        }
        listenThreadSimulador.Abort();
        listenUDPThread.Abort();
        Debug.Log("Se Cerraron las conexiones");
    }
    public void OnDestroy()
    {
        endConnections();
    }

}
