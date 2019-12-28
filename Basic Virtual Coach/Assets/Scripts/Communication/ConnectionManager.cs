using UnityEngine;

public class ConnectionManager : MonoBehaviour {
    public static int streamingActive;
    public static int recordActive;

    void Start () {
        streamingActive = 0;
        recordActive = 0;
    }
}
