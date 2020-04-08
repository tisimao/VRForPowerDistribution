using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerMeterInputModule : MonoBehaviour , IElectricComponent
{
    public LeadJoint s1, s2,volt;

    public float value = 0;

    public float HealthPoint { get ; set ; }

    public string info = "";

#if UNITY_EDITOR
    /// <summary>
    /// 绘制辅助线
    /// </summary>
    protected void OnDrawGizmos()
    {
        
    }
#endif


    // Update is called once per frame
    void Update()
    {
        Electrocircuit inputA = s1.GetJointElectrocircuit();
        Electrocircuit inputVolt = volt.GetJointElectrocircuit();
        if (inputA != null &&  inputVolt != null)
        {
            info = " inputA.Amperage " + inputA.Amperage + "  inputVolt.Volts " + inputVolt.Volts;
            value = inputA.Amperage * inputVolt.Volts * inputA.GetDirection(s1, s2) ;
        }
        else
        {
            info = inputA + " , " + inputVolt;
            value = 0;
        }
    }
    
   

    public IJoint[] GetNextDepthJoint(IJoint joint)
    {
        if ((object)s1 == joint)
        {
            return new IJoint[] { s2 };
        }
        if ((object)s2 == joint)
        {
            return new IJoint[] { s1 };
        }
        return null;
    }

    public uint GetDepthValue()
    {
        return 1;
    }

    public float GetPowerLoad()
    {
        return 0;
    }

    public void Reset()
    {
        s1.Reset();
        s2.Reset();
        volt.Reset();
    }
}
