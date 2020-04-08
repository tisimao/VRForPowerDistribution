using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRDebug : MonoBehaviour
{
    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float fps = Mathf.Round( 1f / Time.deltaTime * 100f)/100f; 
        text.text = "FPS : " + fps + "\n";
    }
}
