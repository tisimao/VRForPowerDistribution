using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadPower : Lead 
{
    public float loadPower = 88000f;
    public float volts = 220f;

    [Header("接口"), SerializeField]
    LeadJoint joint;
    
    public override uint GetDepthValue()
    {
        return 1000000;
    }

    public override IJoint[] GetNextDepthJoint(IJoint joint)
    {
        return null;
    }


    public override float GetPowerLoad()
    {
        return loadPower;
    }
        
    protected void Update()
    {
        Electrocircuit temp = joint.GetJointElectrocircuit();
        if (temp != null)
        {
            temp.AddLoad(this);
        }
    }
}
