using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSwitch : MonoBehaviour ,IElectricComponent
{
    [SerializeField]
    LeadJoint inA, inB, inC,outA,outB,outC;
    [SerializeField]
    SwitchButton3D button;


    bool isOn = false;      

    public bool IsOn {
        get
        {
            return isOn;
        }
        set
        {
            if(isOn != value)
            {
                isOn = value;
                inA.Refresh();
                inB.Refresh();
                inC.Refresh();
                outA.Refresh();
                outB.Refresh();
                outC.Refresh();
            }
        }
    }

    public float HealthPoint { get; set; }

    private void Start()
    {
        button.onChanged = OnChange;
    }

    public uint GetDepthValue()
    {
        return 1;
    }

    public IJoint[] GetNextDepthJoint(IJoint joint)
    {
        if(IsOn)
        {
            if ((object)inA == joint)
            {
                return new IJoint[] { outA };
            }
            else if ((object)inB == joint)
            {
                return new IJoint[] { outB };
            }
            else if ((object)inC == joint)
            {
                return new IJoint[] { outC };
            }
            else if ((object)outA == joint)
            {
                return new IJoint[] { inA };
            }
            else if ((object)outB == joint)
            {
                return new IJoint[] { inB };
            }
            else if ((object)outC == joint)
            {
                return new IJoint[] { inC };
            }
        }
        return null;
    }

    void OnChange(bool value)
    {
        IsOn = value;
    }

    public float GetPowerLoad()
    {
        return 0;
    }

    public void Reset()
    {
        isOn = false;
        inA.Reset();
        inB.Reset();
        inC.Reset();
        outA.Reset();
        outB.Reset();
        outC.Reset();
    }


    //public Electrocircuit GetOutput(IJoint joint)
    //{
    //    //if ((object)inA == joint)
    //    //{
    //    //    return isOn ? cOutA : null;
    //    //}
    //    //else if ((object)inB == joint)
    //    //{
    //    //    return isOn ? cOutB : null;
    //    //}
    //    //else if ((object)inC == joint)
    //    //{
    //    //    return isOn ? cOutB : null;
    //    //}
    //    //else if ((object)outA == joint)
    //    //{
    //    //    return isOn ? cInA : null;
    //    //}
    //    //else if ((object)outB == joint)
    //    //{
    //    //    return isOn ? cInB : null;
    //    //}
    //    //else if ((object)outC == joint)
    //    //{
    //    //    return isOn ? cInC : null;
    //    //}
    //    return null;
    //}



}
