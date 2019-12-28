using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetSavePCE : MonoBehaviour
{

    static GameObject resetText;
    static GameObject saveText;

    public static string sceneName;

    // Use this for initialization
    void Start()
    {
        resetText = GameObject.Find("Reset_text");
        saveText = GameObject.Find("Save_text");

        sceneName = SceneManager.GetActiveScene().name;
    }

    // For PC/Daydream use.
    void OnMouseDown()
    {
        // Get the object that was clicked.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.name.Contains("ResetPCE"))
            {
                ResetPCEhit();
            }
            if (hit.collider.name.Contains("SavePCE"))
            {
                SavePCEhit();
            }
        }
    }

    public static void ResetPCEhit()
    {
        // Print name of object clicked to the console.
        Debug.Log("ResetPCE pressed.");

        // Get the time and date now. (To include date, change to "yyyy/MM/dd hh:mm:ss").
        string timeNow = System.DateTime.Now.ToString("HH:mm:ss");

        // Only reset controller settings if in PR Trainer scene.
        if (sceneName.Contains("PRTrainer"))
        {
            // Send Message back to controller. Update TRAIN_FLAG to 999 to reset.
            ThreadSock.sendMessage("pce|set|var|TRAIN_FLAG|999");
        }
        else
        {
            // Send message back to controller. Update TRAIN_FLAG to 998 to reset arm position calibration.
        }

        // If reset or save have been clicked, save all variables on the controller.
        ThreadSock.sendMessage("pce|set|cmd|24");

        // Update text.
        resetText.GetComponent<TextMesh>().text = "Reset\n" + timeNow;
    }

    public static void SavePCEhit()
    {
        // Print name of object clicked to the console.
        Debug.Log("SavePCE pressed.");

        // Get the time and date now. (To include date, change to "yyyy/MM/dd hh:mm:ss").
        string timeNow = System.DateTime.Now.ToString("HH:mm:ss");

        // If reset or save have been clicked, save all variables on the controller.
        ThreadSock.sendMessage("pce|set|cmd|24");

        // Update text.
        saveText.GetComponent<TextMesh>().text = "Save\n" + timeNow;
    }
}
