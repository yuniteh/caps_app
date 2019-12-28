using System;
using UnityEngine;

public class UpdateUI : MonoBehaviour {

    int classNum;
    static int maxSize = 100;
    static int midWay = maxSize / 2;
    float colourIncrement = (255 / midWay) % 256;
    static float sizeIncrement = 4.5F / maxSize;     // The value 4.5 was found by trial and error.
    float[] colourGradientIncrement = new float[maxSize];
    Color32[] colourGradient = new Color32[maxSize];
    float[] sizeGradient = new float[maxSize];

    // Use this for initialization
    void Start()
    {
        // Define the colour gradient.
        colourGradientIncrement[0] = colourIncrement;
        colourGradient[0] = new Color32(255, (byte)colourGradientIncrement[1], 0, 0);
        sizeGradient[0] = (float)0.5;

        for (int i = 1; i < maxSize; i++)
        {
            // Create array for size gradient.
            sizeGradient[i] = sizeGradient[i - 1] + (float)sizeIncrement;

            // If the click is equal to or less than the midway point, increment green up to 255.
            if (i <= midWay)
            {
                colourGradientIncrement[i] = colourGradientIncrement[i - 1] + colourIncrement;
                colourGradient[i] = new Color32(255, (byte)colourGradientIncrement[i], 0, 0);
            }
            // If the click is equal to or less than the max point, and is greater than the midway point, decrement the red from 255 to 0.
            else if (i > midWay)
            {
                colourGradientIncrement[i] = colourGradientIncrement[i - 1] - colourIncrement;
                colourGradient[i] = new Color32((byte)colourGradientIncrement[i], 255, 0, 0);
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
        // Get the current N value.
        int[] n = ThreadSock.NPATS;

        // Get all objects under 'ActiveObjects' (this will also include the parent object "ActiveObjects".
        Transform[] children = Array.FindAll(GetComponentsInChildren<Transform>(), child => child != transform.parent);

        // Loop through all children.
        for (int i = 0; i < children.Length; i++)
        {
            // Convert children from transform to gameObject.
            GameObject child = children[i].gameObject;

            // Get the class number.
            int.TryParse(child.name.Split('_')[0], out classNum);

            // Get the objects first character (i.e. 0 for no movement) and check this against the training number (NPATS).
            int child_n = n[classNum];

            // If the object is a 'puck', set the size and colour according to the number of training samples collected.
            if (child.name.Contains("_puck"))
            {
                // Set the puck size.
                child.transform.localScale = new Vector3(2F, (float)sizeGradient[child_n], 0.01F);
                // Set the puck colour.
                child.GetComponent<Renderer>().material.color = colourGradient[child_n];
            }
            // If the object is a 'complete' sign and training is 100% complete, push to the front.
            if ((child.name.Contains("_complete")) & (child_n == (maxSize - 1)))
            {
                child.transform.localPosition = new Vector3(0F, 0F, 0F);
            }
            // Otherwise, push back.
            else if ((child.name.Contains("_complete")) & (child_n != (maxSize - 1)))
            {
                child.transform.localPosition = new Vector3(0F, -0.5F, 0F);
            }
            // If the object is a 'repTicker', update its value to the number of training repetitions.
            if (child.name.Contains("_repTicker"))
            {
                child.GetComponent<TextMesh>().text = (ThreadSock.NREPS[classNum]).ToString();
            }
            // If the object is a 'totalTicker', update its value to the number of total windows trained.
            if (child.name.Contains("_totalTicker"))
            {
                child.GetComponent<TextMesh>().text = (ThreadSock.NTOT[classNum]).ToString();
            }
        }
    }
}
