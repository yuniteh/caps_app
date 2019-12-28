using System.Threading;
using UnityEngine;

public class ThreadSock : MonoBehaviour
{
    // COMMUNICATION VARIABLES
    Thread ThreadA;
    bool ThreadA_Alive;
    Thread ThreadB;
    bool ThreadB_Alive;
    Thread ThreadC;
    bool ThreadC_Alive;
    // UDP connection to receive stream of muscle-computer-interface outputs.
    public static UDPConn udp;
    // Variables to indicate that server is connected.
    public static bool SocketRunning;
    public static bool Streaming;
    // Define delays (in ms).
    int closeDelay = 500;
    int writeDelay = 900;
    int readDelay = 25;

    // NON-COMMUNICATION VARIABLES
    public static int confirm = 0;
    public static int classEst;
    public static int classNew;
    public static float contractionMAV;
    public static float[] classMAV;
    public static float[] chanMAV;
    public static float[] propControl;
    public static int[] NTOT;
    public static int[] NREPS;
    public static int[] NPATS;

    private void Start()
    {
        // Create new instance of UDPConn.
        udp = gameObject.AddComponent<UDPConn>();
        // Initialise variables.
        SocketRunning = false;
        Streaming = false;

        // Initialise variables.
        NTOT = new int[100];
        NREPS = new int[100];
        NPATS = new int[100];

        // Start the UDP communication and write to it.
        ThreadA = null;
        ThreadA = new Thread(ThreadJob_A);
        ThreadA.Priority = System.Threading.ThreadPriority.Highest;
        ThreadA_Alive = true;
        
        // Read from the UDP connection.
        ThreadB = null;
        ThreadB = new Thread(ThreadJob_B);
        ThreadB.Priority = System.Threading.ThreadPriority.Highest;
        ThreadB_Alive = true;
        
        // Unpack the received data and process.
        ThreadC = null;
        ThreadC = new Thread(ThreadJob_C);
        ThreadC.Priority = System.Threading.ThreadPriority.Lowest;
        ThreadC_Alive = true;

        // Start all threads.
        ThreadA.Start();
        ThreadB.Start();
        ThreadC.Start();
    }

    void ThreadJob_A()
    {
        // Loop until the thread is aborted.
        while (ThreadA_Alive)
        {
            // Setup socket ready for communication.
            SocketRunning = udp.setupSocket();

            // Loop until the connection is lost.
            while (SocketRunning)
            {
                // Sleep the thread so as not to overload the connection with heartbeats.
                threadWait(writeDelay);

                // Write a HeartBeat message. 
                bool writeSuccess = udp.writeSocket("HeartBeat");

                // If write was not successful, close all.
                if (!writeSuccess)
                {
                    SocketRunning = false;
                }
            }
        }
    }

    void ThreadJob_B()
    {
        // Loop until the thread is aborted.
        while (ThreadB_Alive)
        {
            // Only update while socket is running.
            while (SocketRunning)
            {
                // Sleep the thread to give other processes time to do their thing
                threadWait(readDelay);

                // Read from the socket.
                bool readSuccess = udp.readSocket();

                // If reading was not successful, close all.
                if (!readSuccess)
                {
                    SocketRunning = false;
                }
            }
        }
    }

    void ThreadJob_C()
    {
        // Loop until the thread is aborted.
        while (ThreadC_Alive)
        {
            // Only update while socket is running and new data is available.
            while (SocketRunning & UDPConn.newData)
            {
                // Get received data and process.
                processData(UDPConn.rxMessage);

                // Print received data.
                //Debug.Log(UDPConn.rxMessage.ToString());
            }
        }
    }

    public void processData(string rxData)
    {
        // Reset newData bool to false so that the same data isn't processed multiple times.
        UDPConn.newData = false;

        // ALIVE
        // First packet sent from server to state its awake. Use information to build array sizes.
        if (rxData.Contains("ALIVE"))
        {
            // Remove the 'ALIVE;' part of the string.
            string sizeData = rxData.Split(';')[1];
            // Split the remaining string using comma deliminator.
            string[] sizeDataIdv = sizeData.Split(',');
            // Extract class and channel information from remaining string.
            int classSize = int.Parse(sizeDataIdv[0]);
            int chanSize = int.Parse(sizeDataIdv[1]);

            // Build array sizes using class and chan values.
            classMAV = new float[classSize];
            chanMAV = new float[chanSize];
            propControl = new float[classSize];
            NTOT = new int[classSize];
            NREPS = new int[classSize];
            NPATS = new int[classSize];

            // Start the streaming of data.
            // Configure any hardware before starting engine
            sendMessage("pce|set|cmd|2");
            // Start PCE loop execution
            sendMessage("pce|set|cmd|3");
            // Set streaming active to high.
            ConnectionManager.streamingActive = 1;
        }

        // HANDSHAKE
        // Acknowledge package.
        if (rxData.Contains("ACKCON"))
        {
            // Split message and get result.
            string[] hs_string = rxData.Split('=');
            string[] hs_result = hs_string[1].Split(':');

            // Convert character to integer to determine success (1 = good, 0 = bad).
            confirm = int.Parse(hs_result[0]);
            // Print message to console.
            //Debug.Log("Acknowledge: " + hs_result[0] + " - " + hs_result[1]);
        }

        // NOT STREAMING
        // If the 'start' command hasn't been sent, a 'nil' message will be sent.
        if (rxData.Contains("nil"))
        {
            Streaming = false;
            // Set classifier estimate to its default value.
            classEst = -1;
        }

        // STREAMING
        // String package from CAPS will always contain C_OUT, so check for that.
        if (rxData.Contains("C_OUT"))
        {

            Streaming = true;

            string[] results = rxData.Split(',');
            foreach (string result in results)
            {

                string[] smallRes = result.Split('=');

                // CLASS OUTPUT
                // Each class value corresponds to a different movement type.
                if (smallRes[0].Contains("C_OUT"))
                {
                    float tmpClassOut;
                    // Try to convert to a float.
                    float.TryParse(smallRes[1], out tmpClassOut);
                    // Convert to an integer.
                    classEst = (int)tmpClassOut;
                }

                // NEW CONTRACTION HAS BEEN TRAINED
                // This is an indicator if a new class has been trained. 
                // At high (1) data has been collected, at low (0) the class has been trained.
                if (smallRes[0].Contains("C_NEW"))
                {
                    // Get the previous value of classNew.
                    int prevClassNew = classNew;
                    // Get current value and try to convert to a float.
                    float tmpClassNew;
                    float.TryParse(smallRes[1], out tmpClassNew);
                    // Convert to an integer.
                    classNew = (int)tmpClassNew;

                    // Save all variables automatically.
                    // If there is a new class trained, and the value doesn't equal the previous value, and we're currently not adapting.
                    if ((classNew == 1) & (prevClassNew != classNew))
                    {
                        // Send save message.
                        sendMessage("pce|set|cmd|24");
                        // Print message for user.
                        Debug.Log("Saved @ " + System.DateTime.Now.ToString("HH:mm:ss").ToString());
                    }
                }

                // CONTRACTION MAV
                // Output value (DC only) MAV for DC channels.
                if (smallRes[0].Contains("C_MAV"))
                {
                    float con_mav_float;
                    if (float.TryParse(smallRes[1], out con_mav_float))
                    {
                        contractionMAV = con_mav_float;
                    }
                }

                // CONTRACTION MAV FOR ALL CLASSES
                // Ouput value for all PR classes.
                if (smallRes[0].Contains("C_8MAV"))
                {
                    string[] indClassMAV = smallRes[1].Split(';');
                    int classMAVCounter = 0;
                    foreach (string c_mav in indClassMAV)
                    {
                        float c_mav_float;
                        if (float.TryParse(c_mav, out c_mav_float))
                        {
                            classMAV[classMAVCounter++] = c_mav_float;
                        }
                    }
                }

                // CONTRACTION MAV FOR ALL CHANNELS
                // Ouput value for all EMG channels.
                if (smallRes[0].Contains("CH_8MAV"))
                {
                    string[] indEMGMAV = smallRes[1].Split(';');
                    int chanMAVCounter = 0;
                    foreach (string ch_mav in indEMGMAV)
                    {
                        float ch_mav_float;
                        if (float.TryParse(ch_mav, out ch_mav_float))
                        {
                            chanMAV[chanMAVCounter++] = ch_mav_float;
                        }
                    }
                }

                // PROPORTIONAL CONTROL
                // Output value for proportional control.
                if (smallRes[0].Contains("C_PC"))
                {
                    string[] indPC = smallRes[1].Split(';');
                    int pcCounter = 0;
                    foreach (string pc in indPC)
                    {
                        float pc_float;
                        if (float.TryParse(pc, out pc_float))
                        {
                            propControl[pcCounter++] = pc_float;
                        }
                    }
                }

                // NUMBER OF TOTAL CONTRACTIONS TRAINED
                // This is the total (1-Inf) number of patterns collected for each movement type.
                if (smallRes[0].Contains("N_C"))
                {
                    string[] numTotPatterns = smallRes[1].Split(';');
                    int ncounter = 0;
                    foreach (string n in numTotPatterns)
                    {
                        NTOT[ncounter++] = int.Parse(n);
                    }
                }

                // NUMBER OF CONTRACTIONS TRAINED FOR EACH CLASS TYPE
                // This is the number of repetitions (1-Inf) each movement has been trained.
                if (smallRes[0].Contains("N_R"))
                {
                    string[] numRepetitions = smallRes[1].Split(';');
                    int ncounter = 0;
                    foreach (string n in numRepetitions)
                    {
                        NREPS[ncounter++] = int.Parse(n);
                    }
                }

                // NUMBER OF TEMPORARY CONTRACTIONS TRAINED
                // This is the temporary (1-100) number of patterns collected for each movement type.
                if (smallRes[0].Contains("N_T"))
                {
                    string[] numTempPatterns = smallRes[1].Split(';');
                    int ncounter = 0;
                    foreach (string n in numTempPatterns)
                    {
                        NPATS[ncounter++] = int.Parse(n);
                    }
                }
            }
        }
    }

    // Pause the thread for the specified time (in ms).
    public static void threadWait(int waitTime)
    {
        Thread.CurrentThread.Join(waitTime);
        //Thread.Sleep(waitTime);
    }

    // Send message to the server.
    public static void sendMessage(string messageToCAPS)
    {
        // Reset confirm bit.
        confirm = 0;
        // Transmit message
        udp.writeSocket(messageToCAPS);
    }

    // These are needed to properly dispose of the thread.
    public void OnDestroy()
    {
        // Stop adaptation.
        sendMessage("pce|set|var|ADAPT_ON|0");
        // Stop streaming and recording.
        sendMessage("pce|set|cmd|22");
        sendMessage("pce|set|cmd|4");

        // Pause before closing everything to allow for acknowledgement return messages.
        threadWait(closeDelay);

        // Toggle all bools off.
        SocketRunning = false;
        Streaming = false;
        ThreadA_Alive = false;
        ThreadB_Alive = false;
        ThreadC_Alive = false;

        // Abort all threads and close socket connection.
        ThreadA.Abort();
        ThreadB.Abort();
        ThreadC.Abort();
        udp.closeSocket();
    }

}