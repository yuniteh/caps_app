using UnityEngine;
using UnityEngine.SceneManagement;

public class JumpToScene : MonoBehaviour {

    public static int num;

    private void OnMouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.transform.name);
            int.TryParse(hit.transform.name.Split('_')[0], out num);
            jump2scene(num);
        }
    }

    public static void jump2scene(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    public static void exitGame()
    {
        Debug.Log("CloseAndExit:exitGame");
        // save any game data here
        #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so UnityEditor.EditorApplication.isPlaying is needed to quit.
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
