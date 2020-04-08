using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerMeter : MonoBehaviour
{
    [SerializeField]
    Text text;
    [SerializeField]
    PowerMeterInputModule inputA, inputB, inputC;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float a = Mathf.Round(inputA.value * 1000f) / 1000f;
        float b = Mathf.Round(inputB.value * 1000f) / 1000f;
        float c = Mathf.Round(inputC.value * 1000f) / 1000f;
        text.text = a + "\n" + b + "\n" + c;
    }
}
