using UnityEngine;
using UnityEngine.SceneManagement;

public class ActionCollect : MonoBehaviour {

    public static AudioSource completeSound;
    public static int num;
    public static string hitName;
    public static string sceneName;

    private void Start()
    {
        completeSound = GameObject.Find("ActiveObjects").GetComponent<AudioSource>();
    }

    // For PC/Daydream use.
    void OnMouseDown()
    {
        // Get the object that was clicked.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            int.TryParse(hit.collider.name.Split('_')[0], out num);
            hitName = hit.collider.name.Split('_')[1];
            if (Physics.Raycast(ray, out hit))
            {
                TrainDOF(num, hitName);
            }
        }
    }

    public static void TrainDOF(int dof, string hitObject)
    {
        if (ConnectionManager.streamingActive == 1)
        {
            Debug.Log(hitObject + " pressed.");
            ThreadSock.sendMessage("pce|set|var|TRAIN_FLAG|" + dof.ToString());
        }
    }
} 
