using UnityEngine;

public class DisplayAction : MonoBehaviour {

    GameObject[] activeObj;
    int current_class;

    // Reset all signs to red.
    public void resetToRed()
    {
        for (int i = 0; i < activeObj.Length; i++)
        {
            activeObj[i].GetComponent<Renderer>().material.color = Color.red;
        }
    }

    // Use this for initialization
    void Start () {
        activeObj = new GameObject[] {
            GameObject.Find("0_noMotion_active"),
            GameObject.Find("1_handOpen_active"),
            GameObject.Find("2_handClose_active"),
            GameObject.Find("3_rotPro_active"),
            GameObject.Find("4_rotSup_active"),
            GameObject.Find("5_wristFlex_active"),
            GameObject.Find("6_wristExt_active"),
            //GameObject.Find("7_wristAdd_active"),
            //GameObject.Find("8_wristAbd_active"),
            //GameObject.Find("9_elbowFlex_active"),
            //GameObject.Find("10_elbowExt_active")
        };
        resetToRed();
    }

    // Update is called once per frame
    void Update() {
        // Use try catch. threadSock.classout doesn't exist for the first few frames.
        try
        {
            // Get class out.
            current_class = ThreadSock.classEst;
            // Set every sphere to red.
            resetToRed();

            // -1 --- No Action
            // 0  --- No Motion
            // 1  --- Hand Open
            // 2  --- Hand Close
            // 3  --- Wrist Pronation
            // 4  --- Wrist Supination
            // 5  --- Wrist Flexion
            // 6  --- Wrist Extension
            // 7  --- Wrist Adduction
            // 8  --- Wrist Abduction
            // 9  --- Elbow Flexion
            // 10 --- Elbow Extension
            if (current_class >= 0)
            {
                activeObj[current_class].GetComponent<Renderer>().material.color = Color.green;
            }
        }
        finally
        {
            current_class = -1;
        }
    }
}
