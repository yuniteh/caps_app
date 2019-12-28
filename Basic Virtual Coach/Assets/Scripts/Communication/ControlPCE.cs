using UnityEngine;

public class ControlPCE : MonoBehaviour
{
    static GameObject streamingText;
    static GameObject streamingButton;
    static GameObject recordButton;
    static GameObject recordText;
    static string timeNow;

    // Use this for initialization
    void Start()
    {
        streamingButton = GameObject.Find("StreamingPCE");
        streamingText = GameObject.Find("Streaming_text");
        recordButton = GameObject.Find("RecordPCE");
        recordText = GameObject.Find("Record_text");

        // Every time the scene is loaded 'start' will run. Determine the state of the streaming and recording, and set the GUI based on what the current state is.
        // Streaming and recording off: Standard starting state.
        if ((ConnectionManager.streamingActive == 0) & (ConnectionManager.recordActive == 0))
        {
            streamingText.GetComponent<TextMesh>().text = "Start";
            streamingButton.GetComponent<Renderer>().material.color = Color.green;
            recordText.GetComponent<TextMesh>().text = "Record";
            recordButton.GetComponent<Renderer>().material.color = Color.grey;
        }
        // Streaming on, recording off: Second most typical state which will be available when leaving and reentering the scene.
        if ((ConnectionManager.streamingActive == 1) & (ConnectionManager.recordActive == 0))
        {
            streamingText.GetComponent<TextMesh>().text = "Stop";
            streamingButton.GetComponent<Renderer>().material.color = Color.red;
            recordText.GetComponent<TextMesh>().text = "Record";
            recordButton.GetComponent<Renderer>().material.color = Color.green;
        }
        // Streaming and recording on: Not typical, but possible.
        if ((ConnectionManager.streamingActive == 1) & (ConnectionManager.recordActive == 1))
        {
            streamingText.GetComponent<TextMesh>().text = "Stop";
            streamingButton.GetComponent<Renderer>().material.color = Color.red;
            recordText.GetComponent<TextMesh>().text = "Stop\nRec.";
            recordButton.GetComponent<Renderer>().material.color = Color.red;
        }
        // Stremaing off and recording on: Not possible.
    }

    // For use with Vive. 
    public static void buttonHit(string hitObject)
    {
        if (hitObject.Contains("StreamingPCE"))
        {
            StreamingPCEhit();
        }
        if (hitObject.Contains("RecordPCE"))
        {
            RecordPCEhit();
        }
    }

    // For PC/Daydream use.
    void OnMouseDown()
    {
        // Get the object that was clicked.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.name.Contains("StreamingPCE"))
            {
                StreamingPCEhit();
            }
            if (hit.collider.name.Contains("RecordPCE"))
            {
                RecordPCEhit();
            }
        }
    }

    public static void StreamingPCEhit()
    {
        // Print name of cube clicked to the console.
        Debug.Log("StreamingPCE pressed.");

        if (ConnectionManager.streamingActive == 0)
        {
            // Configure any hardware before starting engine
            ThreadSock.sendMessage("pce|set|cmd|2");
            // Start PCE loop execution
            ThreadSock.sendMessage("pce|set|cmd|3");

            // Update streaming UI.
            streamingText.GetComponent<TextMesh>().text = "Stop";
            streamingButton.GetComponent<Renderer>().material.color = Color.red;

            // Set the record button to 'active'.
            recordButton.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            if (ConnectionManager.recordActive == 1)
            {
                // Stop file recording.
                ThreadSock.sendMessage("pce|set|cmd|22");
                recordText.GetComponent<TextMesh>().text = "Record";
                ConnectionManager.recordActive = 0;
            }
            // Set the record button to 'deactive'.
            recordButton.GetComponent<Renderer>().material.color = Color.grey;

            // Stop PCE loop execution
            ThreadSock.sendMessage("pce|set|cmd|4");
            streamingText.GetComponent<TextMesh>().text = "Start";
            streamingButton.GetComponent<Renderer>().material.color = Color.green;
        }
        // Toggle active var.
        ConnectionManager.streamingActive ^= 1;
    }

    public static void RecordPCEhit()
    {
        // Print name of cube clicked to the console.
        //Debug.Log("RecordPCE pressed.");

        // If the data is streaming.
        if (ConnectionManager.streamingActive == 1)
        {
            if (ConnectionManager.recordActive == 0)
            {
                // Get the time and date now. (To include date, change to "yyyy/MM/dd hh:mm:ss").
                timeNow = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

                // Set PCE variable for path to output file.
                // Option1: "/config/modes/caps/vr_cmu_pr_v4/DATA/data_" + timeNow + ".DAQ"
                // Option2: "/home/appusr/CAPSv2/USERS/CMU_VirtualCoach/DATA/DAQ/data_" + timeNow + ".DAQ"
                //ThreadSock.sendMessage("pce|set|var|DAQ_OUT_FNAME|/config/modes/caps/upper_limb/vr_cmu_pr_v15/DATA/data_" + timeNow + ".DAQ");
                ThreadSock.sendMessage("pce|set|var|DAQ_OUT_FNAME|/home/appusr/CAPSv2/USERS/VirtualCoach_PR/DATA/DAQ/data_" + timeNow + ".DAQ");
                // Request file recording.
                ThreadSock.sendMessage("pce|set|cmd|21");

                // As the record function can be called outside of the PR wall, this try/catch will allow the GUI update to fail.
                try
                {
                    recordText.GetComponent<TextMesh>().text = "Stop\nRec.";
                    recordButton.GetComponent<Renderer>().material.color = Color.red;
                }
                // Don't want to do anything if it fails so do nothing.
                catch { }
            }
            else
            {
                // Stop file recording.
                ThreadSock.sendMessage("pce|set|cmd|22");
                // Toggle streaming off and on again to reduce buffer overload.
                toggleOffOn();
                // As the record function can be called outside of the PR wall, this try/catch will allow the GUI update to fail.
                try
                {
                    recordText.GetComponent<TextMesh>().text = "Record";
                    recordButton.GetComponent<Renderer>().material.color = Color.green;
                }
                // Don't want to do anything if it fails so do nothing.
                catch { }
            }
            // Toggle active var.
            ConnectionManager.recordActive ^= 1;
        }
    }

    // Function to toggle streaming off, and back on again.
    private static void toggleOffOn()
    {
        // Stop PCE loop execution
        ThreadSock.sendMessage("pce|set|cmd|4");
        // Configure any hardware before starting engine
        ThreadSock.sendMessage("pce|set|cmd|2");
        // Start PCE loop execution
        ThreadSock.sendMessage("pce|set|cmd|3");
    }
}