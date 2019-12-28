using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;

public class UDPConn : MonoBehaviour
{
    // Socket specific variables.
    Socket sock;
    EndPoint serverEndPoint;
    // Bool to specify new data is ready to be processed.
    public static bool newData;
    // String receive data is written to.
    public static string rxMessage;
    // IP address of server (embedded system).
    string ipaddr = "192.168.20.2";
    // Ports to establish connections, must match UDPStreamer.py
    int udpPort = 27015;
    // Poll timeout value (in milliseconds).
    int pollTimeout = 1750;
    // Delay for new connections (in milliseconds).
    int newConnDelay = 25;
    // Bytes to read.
    int bytes2Read = 256;
    // Bool to indicate that the socket has been created.
    bool socketGo;

    private void Start()
    {
        // Nullify bools and rxMessage.
        rxMessage = "";
        newData = false;
        socketGo = false;

        // Create end point.
        serverEndPoint = (EndPoint)new IPEndPoint(IPAddress.Parse(ipaddr), udpPort);

        // Create socket.
        sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        // Indicate socket is ready.
        socketGo = true;
    }

    // Try to initiate connection.
    public bool setupSocket()
    {
        // Wait for socket to be ready.
        if (!socketGo)
        {
            return false;
        }

        // Attempt read.
        try
        {
            // Do write/read to 'wake' the other side and start streaming.
            bool writeSuccess = writeSocket("HeartBeat");
            bool readSuccess = readSocket();

            // If setup was successful ('ALIVE' is returned), return true, else return false.
            if (writeSuccess & readSuccess)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            // If setup failed, return false.
            Debug.Log("UDPConn:setupSocket(EXCEPTION): " + e);
            return false;
        }
    }

    // Send message to server.
    public bool writeSocket(string txMessage)
    {
        // Try to write to server, and catch if exception is thrown.
        try
        {
            // Send message using server endpoint.
            sock.SendTo(System.Text.Encoding.ASCII.GetBytes(txMessage), serverEndPoint);
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("UDPConn:writeSocket(EXCEPTION): " + e);
            return false;
        }
    }

    // Read message from server.
    public bool readSocket()
    {
        // Try to read from the server, and catch if exception is thrown.
        try
        {
            // Add x millisecond timeout for the read command (Poll is in microseconds).
            if (sock.Poll((pollTimeout * 1000), SelectMode.SelectRead))
            {
                // Create byte and receive data from server endpoint.
                byte[] msg = new Byte[bytes2Read];
                sock.ReceiveFrom(msg, ref serverEndPoint);

                // Convert byte to string and save for retrieval later by ThreadC.
                rxMessage = System.Text.Encoding.ASCII.GetString(msg);

                // If the string contains 'ALIVE' then the connection has just been started and a 25ms pause (matches the UDP pause on the server) is required.
                if (rxMessage.Contains("ALIVE"))
                {
                    ThreadSock.threadWait(newConnDelay);
                }

                // Update bool to specify new data is ready.
                newData = true;
                // Success
                return true;
            }
            else
            {
                Debug.Log("UDPConn:readSocket(TIMEOUT)");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.Log("UDPConn:readSocket(EXCEPTION): " + e);
            return false;
        }
    }

    // Close socket.
    public void closeSocket()
    {
        sock.Close();
    }
}
