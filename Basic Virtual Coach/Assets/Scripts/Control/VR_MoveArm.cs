using System.Collections.Generic;
using UnityEngine;

public class VR_MoveArm : MonoBehaviour {

    public GameObject CAPSArm;
    // Get all arm objects (including children).
    public static Transform[] AllObjects;
    public static List<Vector3> StartingPos;
    public static List<GameObject> upperarm;
    public static List<GameObject> elbow;
    public static List<GameObject> forearm;
    public static List<GameObject> wrist;
    public static List<GameObject> fingers;
    public static GameObject fingerPoint;

    // Angles for data logging.
    public static float handOC;
    public static float wristFE;
    public static float wristPS;

    // Multipliers
    float handOpenCloseMultiplier = 1.0f;
    float wristFlexExtMultiplier = 1.0f;
    float wristProSupMultiplier = 1.0f;
    float wristAdbAddMultiplier = 1.0f;
    float elbowFlexExtMultiplier = 1.0f;

    // Maximum angle values (positive and negative) for each of the gestures.
    public static int handCloseInitial = 15;
    public static int elbowExtFlexInitial = 50;
    public static int handCloseMax = 75;
    public static int wristExtFlexMax = 75;
    public static int wristProMax = 75;
    public static int wristSupMax = 75;
    public static int wristAdbAddMax = 35;
    public static int elbowExtFlexMax = 130;
    // Keep track of the current radian (angle) or each DOF.
    private static float handCloseCurRad = 0.0f;
    private static float wristExtFlexCurRad = 0.0f;
    private static float wristProSupCurRad = 0.0f;
    private static float wristAdbAddCurRad = 0.0f;
    private static float elbowExtFlexCurRad = 0.0f;

    // EMG information.
    int current_class;
    float[] chan_mav;
    float propcontrol = 0.5f;
    // Boolean to disable the closing of the fingers.
    public static bool disable;
    // Boolean to freeze arm from moving.
    public static bool freezeArm;

    void Start ()
    {
        // Initialise variables.
        StartingPos = new List<Vector3>();
        upperarm = new List<GameObject>();
        elbow = new List<GameObject>();
        forearm = new List<GameObject>();
        wrist = new List<GameObject>();
        fingers = new List<GameObject>();
        
        // Set the disable bool to stop the hand from moving through a block
        disable = false;
        // Disable freeze arm.
        freezeArm = false;

        // Get the transforms of all the objects.
        AllObjects = CAPSArm.GetComponentsInChildren<Transform>();
        // Save all the objects starting rotation so that they can be reset later if necessary.
        foreach (var currentObj in AllObjects)
        {
            // Get the current objects local Euler angle (vector3) and save to a list.
            StartingPos.Add(currentObj.transform.localEulerAngles);

            // Get the upperarm parts with corresponding tags.
            if (currentObj.tag == "Upperarm")
            {
                upperarm.Add(currentObj.gameObject);
            }
            // Get the elbow parts with corresponding tags.
            if (currentObj.tag == "Elbow")
            {
                elbow.Add(currentObj.gameObject);
            }
            // Get the forearm parts with corresponding tags.
            if (currentObj.tag == "Forearm")
            {
                forearm.Add(currentObj.gameObject);
            }
            // Get the wrist parts with corresponding tags.
            if (currentObj.tag == "Wrist")
            {
                wrist.Add(currentObj.gameObject);
            }
            // Get all objects which are fingers (or a thumb) with the tag "Finger".
            if (currentObj.tag == "Finger")
            {
                fingers.Add(currentObj.gameObject);
            }
            // Get comparison points as upperjoint of the index finger.
            if (currentObj.name == "Finger1_3")
            {
                fingerPoint = currentObj.gameObject;
            }
        }
        // Move fingers to an initial position of 'slightly closed' to allow room for 'hand open' from the start.
        startingPosition();
    }

    void Update () {

        // Get class out.
        current_class = ThreadSock.classEst;
        // Get proportional control information.
        if (ThreadSock.SocketRunning & ThreadSock.Streaming & (current_class != -1))
        {
            propcontrol = ThreadSock.propControl[current_class];
        }

        if (!freezeArm)
        {
            // RESET TO ORIGINAL POSITION.
            if (Input.GetKeyDown("r"))
            {
                //Debug.Log("RESET");
                resetPositions();
            }
            // OPEN HAND.
            if (Input.GetKey("1") | (current_class == 1) | (Input.GetMouseButtonDown(1)))
            {
                //Debug.Log("OPEN HAND");
                HandOpenClose(-handOpenCloseMultiplier * propcontrol);
            }
            // CLOSE HAND.
            if ((Input.GetKey("2") | (current_class == 2) | (Input.GetMouseButtonDown(0))) & !disable)
            {
                //Debug.Log("CLOSE HAND");
                HandOpenClose(handOpenCloseMultiplier * propcontrol);
            }
            // WRIST PRO.
            if (Input.GetKey("3") | (current_class == 3))
            {
                //Debug.Log("WRIST PRO");
                WristProSup(-wristProSupMultiplier * propcontrol);
            }
            // WRIST SUP.
            if (Input.GetKey("4") | (current_class == 4))
            {
                //Debug.Log("WRIST SUP");
                WristProSup(wristProSupMultiplier * propcontrol);
            }
            // FLEX WRIST.
            if (Input.GetKey("5") | (current_class == 5))
            {
                //Debug.Log("WRIST FLEXION");
                WristExtFlex(wristFlexExtMultiplier * propcontrol);
            }
            // EXTEND WRIST.
            if (Input.GetKey("6") | (current_class == 6))
            {
                //Debug.Log("WRIST EXTEND");
                WristExtFlex(-wristFlexExtMultiplier * propcontrol);
            }
            // WRIST ABDUCTION.
            if (Input.GetKey("7") | (current_class == 7))
            {
                //Debug.Log("WRIST ABD");
                WristAbdAdd(-wristAdbAddMultiplier * propcontrol);
            }
            // WRIST ADDUCTION.
            if (Input.GetKey("8") | (current_class == 8))
            {
                //Debug.Log("WRIST ADD");
                WristAbdAdd(wristAdbAddMultiplier * propcontrol);
            }
            // EXTEND ELBOW.
            if (Input.GetKey("9") | (current_class == 9))
            {
                //Debug.Log("ELBOW FLEXION");
                ElbowFlexExt(elbowFlexExtMultiplier * propcontrol);
            }
            // EXTEND ELBOW.
            if (Input.GetKey("0") | (current_class == 10))
            {
                //Debug.Log("ELBOW EXTEND");
                ElbowFlexExt(-elbowFlexExtMultiplier * propcontrol);
            }
        }

        handOC = handCloseCurRad;
        wristFE = wristExtFlexCurRad;
        wristPS = wristProSupCurRad;
    }

    public static void resetPositions()
    {
        int count = 0;
        // Increment over all arm objects and reset their rotation to the original value.
        foreach (var currentObj in AllObjects)
        {
            // Reset.
            currentObj.transform.localEulerAngles = StartingPos[count];
            // Increment counter.
            count += 1;
        }
        // Reset radian (angle) or each DOF.
        handCloseCurRad = 0.0f;
        wristExtFlexCurRad = 0.0f;
        wristProSupCurRad = 0.0f;
        wristAdbAddCurRad = 0.0f;
        elbowExtFlexCurRad = 0.0f;
        // Reset hand to starting position.
        startingPosition();
    }

    public static void resetPositionsFingers(bool fullOpen)
    {
        // Loop through every finger/thumb segment
        foreach (var finger in fingers)
        {
            // If we want to fully open the hand, open is set to true
            if (fullOpen)
            {
                // Open hand fully by setting angle to 0 degrees (i.e. fingers and thumb fully extended).
                finger.transform.localRotation = Quaternion.Euler(0.0f, finger.transform.localEulerAngles.y, finger.transform.localEulerAngles.z);
                handCloseCurRad = 0.0f;
            }
            else
            {
                // Close hand fully by setting angle to max (i.e. fingers and thumb clenched into fist).
                finger.transform.localRotation = Quaternion.Euler(handCloseMax, finger.transform.localEulerAngles.y, finger.transform.localEulerAngles.z);
                handCloseCurRad = handCloseMax;
            }
        }
    }

    public static void resetPositionsElbow(bool fullopen)
    {
        // Loop through every elbow segment
        foreach (var armSeg in elbow)
        {
            // If we want to fully open the elbow, open is set to true
            if (fullopen)
            {
                // Open elbow fully by setting angle to 0 degrees (i.e. arm fully extended (straight)).
                armSeg.transform.localRotation = Quaternion.Euler(0.0f, armSeg.transform.localRotation.y, armSeg.transform.localRotation.z);
                elbowExtFlexCurRad = 0.0f;
            }
            else
            {
                // Close elbow fully by setting angle to max (i.e. arm flexed to flex max).
                armSeg.transform.localRotation = Quaternion.Euler(elbowExtFlexMax, armSeg.transform.localRotation.y, armSeg.transform.localRotation.z);
                elbowExtFlexCurRad = elbowExtFlexMax;
            }
        }
    }

    public static void startingPosition()
    {
        // Loop through every finger/thumb segment
        foreach (var finger in fingers)
        {
            // Close hand to initial point.
            finger.transform.localRotation = Quaternion.Euler(handCloseInitial, finger.transform.localEulerAngles.y, finger.transform.localEulerAngles.z);
        }
        // Set the radians to the inital values.
        handCloseCurRad = handCloseInitial;

    }

    void HandOpenClose(float val)
    {
        // If the rotation doesn't exceed a specified min/max.
        if ((handCloseCurRad + val >= 0.0f) && (handCloseCurRad + val <= handCloseMax))
        {
            // Increment/decrement angle, based on value of 'val'.
            handCloseCurRad += val;

            // Loop through every finger/thumb segment
            foreach (var finger in fingers)
            {
                // Open/close hand.
                finger.transform.localRotation = Quaternion.Euler(handCloseCurRad, finger.transform.localEulerAngles.y, finger.transform.localEulerAngles.z);
            }
        }
    }

    void WristProSup(float val)
    {
        // If the rotation doesn't exceed a specified min/max.
        if ((wristProSupCurRad + val >= -wristProMax) && (wristProSupCurRad + val <= wristSupMax))
        {
            // Increment/decrement angle, based on value of 'val'.
            wristProSupCurRad += val;

            // Loop through every arm segment.
            foreach (var armSeg in forearm)
            {
                // Pronate/supinate wrist.
                armSeg.transform.localRotation = Quaternion.Euler(armSeg.transform.localEulerAngles.x, wristProSupCurRad, armSeg.transform.localEulerAngles.z);
            }
        }
    }

    void WristExtFlex(float val)
    {
        // If the rotation doesn't exceed a specified min/max.
        if ((wristExtFlexCurRad + val >= -wristExtFlexMax) && (wristExtFlexCurRad + val <= wristExtFlexMax))
        {
            // Increment/decrement angle, based on value of 'val'.
            wristExtFlexCurRad += val;

            // Loop through every wrist segment (only one current).
            foreach (var wristSeg in wrist)
            {
                // Extend/flex wrist.
                wristSeg.transform.localRotation = Quaternion.Euler(wristSeg.transform.localEulerAngles.x, wristSeg.transform.localEulerAngles.y, wristExtFlexCurRad);
            }
        }
    }

    void WristAbdAdd(float val)
    {
        // If the rotation doesn't exceed a specified min/max.
        if ((wristAdbAddCurRad + val >= -wristAdbAddMax) && (wristAdbAddCurRad + val <= wristAdbAddMax))
        {
            // Increment/decrement angle, based on value of 'val'.
            wristAdbAddCurRad += val;

            // Loop through every wrist segment.
            foreach (var wristSeg in wrist)
            {
                // Abduct/adduct wrist.
                wristSeg.transform.localRotation = Quaternion.Euler(wristAdbAddCurRad, wristSeg.transform.localEulerAngles.y, wristSeg.transform.localEulerAngles.z);
            }
        }
    }

    void ElbowFlexExt(float val)
    {
        // If the rotation doesn't exceed a specified min/max.
        if ((elbowExtFlexCurRad + val >= 0.0f) && (elbowExtFlexCurRad + val <= elbowExtFlexMax))
        {
            // Increment/decrement angle, based on value of 'val'.
            elbowExtFlexCurRad += val;

            // Loop through every elbow segment.
            foreach (var armSeg in elbow)
            {
                // Extend/flex elbow. Unlike the other gestures, the y and z values must be 'localrotations' and not 'localeulerangles'.
                armSeg.transform.localRotation = Quaternion.Euler(elbowExtFlexCurRad, armSeg.transform.localRotation.y, armSeg.transform.localRotation.z);
            }
        }
    }

    // To hardcode the hand to specific positions, use the following functions.
    public static void HandOpenClose_hardcode(float val)
    {
        // Loop through every finger/thumb segment
        foreach (var finger in fingers)
        {
            // Open/close hand.
            finger.transform.localRotation = Quaternion.Euler(val, finger.transform.localEulerAngles.y, finger.transform.localEulerAngles.z);
        }
    }

    public static void WristProSup_hardcode(float val)
    {
        // Loop through every arm segment.
        foreach (var armSeg in forearm)
        {
            // Pronate/supinate wrist.
            armSeg.transform.localRotation = Quaternion.Euler(armSeg.transform.localEulerAngles.x, val, armSeg.transform.localEulerAngles.z);
        }
    }

    public static void WristExtFlex_hardcode(float val)
    {
        // Loop through every wrist segment (only one current).
        foreach (var wristSeg in wrist)
        {
            // Extend/flex wrist.
            wristSeg.transform.localRotation = Quaternion.Euler(wristSeg.transform.localEulerAngles.x, wristSeg.transform.localEulerAngles.y, val);
        }
    }

    public static void WristAbdAdd_hardcode(float val)
    {
        // Loop through every wrist segment.
        foreach (var wristSeg in wrist)
        {
            // Abduct/adduct wrist.
            wristSeg.transform.localRotation = Quaternion.Euler(val, wristSeg.transform.localEulerAngles.y, wristSeg.transform.localEulerAngles.z);
        }
    }

    public static void ElbowFlexExt_hardcode(float val)
    {
        // Loop through every elbow segment.
        foreach (var armSeg in elbow)
        {
            // Extend/flex elbow. Unlike the other gestures, the y and z values must be 'localrotations' and not 'localeulerangles'.
            armSeg.transform.localRotation = Quaternion.Euler(val, armSeg.transform.localRotation.y, armSeg.transform.localRotation.z);
        }
    }
}
