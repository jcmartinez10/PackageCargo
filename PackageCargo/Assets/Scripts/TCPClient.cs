using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class TCPClient : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        Debug.Log("TCP Client");
        Connect("localhost", "Hello World");        
	}

    static void Connect(String server, String message)
    {
        try
        {
            // Create a TcpClient.
            // Note, for this client to work you need to have a TcpServer 
            // connected to the same address as specified by the server, port
            // combination.
            Int32 port = 13000;
            TcpClient client = new TcpClient(server, port);

            // Translate the passed message into ASCII and store it as a Byte array.
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

            // Get a client stream for reading and writing.
            //  Stream stream = client.GetStream();

            NetworkStream stream = client.GetStream();

            // Send the message to the connected TcpServer. 
            stream.Write(data, 0, data.Length);            
            Debug.Log("Sent:" + message);

            // Receive the TcpServer.response.

            // Buffer to store the response bytes.
            data = new Byte[256];

            // String to store the response ASCII representation.
            String responseData = String.Empty;

            // Read the first batch of the TcpServer response bytes.
            Int32 bytes = 0;
            while (stream.DataAvailable)
            {
                bytes= stream.Read(data, 0, data.Length);
                responseData += System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Debug.Log("Received:"+ responseData); 
            }            
            
            //Split the data and load it
            
            //-----------------------READ   TEXT---------------------
            /*string[] split = responseData.Split('\n');
            Debug.Log("split lenght " + split.Length);
            foreach(string text in split)
            {
                Debug.Log(" Line - " + text);
                string[] parameters = text.Split('\t');
                
                
            }*/
            //-------------------------------------------------------
            // Close everything.
            stream.Close();
            client.Close();
        }
        catch (ArgumentNullException e)
        {
            Debug.Log("ArgumentNullException" + e);
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: "+ e);
        }

        //Debug.Log("\n Press Enter to continue...");
        //Console.Read();
    }
}
