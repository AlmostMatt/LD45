using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneScaler : MonoBehaviour
{
    // These numbers indicate a desired aspect ratios
    private int expectedW = 1920;
    private int expectedH = 1080;
    //
    private int prevW = 1920;
    private int prevH = 1080;

    // Start is called before the first frame update
    void Start()
    {
        Resize();
    }

    // Update is called once per frame
    void Update()
    {
        Resize();
    }

    private void Resize()
    {
        int w = Screen.width;
        int h = Screen.height;
        if (w == prevW && h == prevH)
        {
            // Only resize when the resolution changes
            return;
        }
        // For ratios less wide than 16:9, scale down by setting newheight to 9/16 of actualwidth
        if ((float)w/(float)h < (float)expectedW/(float)expectedH)
        {
            Debug.Log("Resizing scene.");
            transform.localScale = (((float)w* 9f)/((float)h*16f)) * Vector3.one;
        } else
        {
            Debug.Log("Resizing scene.");
            transform.localScale = Vector3.one;
        }
        prevW = w;
        prevH = h;
    }
}
