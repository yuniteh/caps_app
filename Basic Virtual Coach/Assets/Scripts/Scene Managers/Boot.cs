using UnityEngine;
using UnityEngine.SceneManagement;

public class Boot : MonoBehaviour {
	void Start () {
        DontDestroyOnLoad(transform.gameObject);
		SceneManager.LoadScene(1);
    }
}
