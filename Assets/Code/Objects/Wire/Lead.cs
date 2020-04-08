using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 电线
/// </summary>
public class Lead : MonoBehaviour, IElectricComponent
{
    public int colorID;
    public WireEditable wire;
    [SerializeField]
    Material[] colors;
    [SerializeField]
    MeshRenderer[] showColors;

    [Header("接口"), SerializeField]
    LeadJoint jointA,jointB ;
    public LeadJoint Joint_A
    {
        get
        {
            return jointA;
        }
    }
    public LeadJoint Joint_B
    {
        get
        {
            return jointB;
        }
    }


    public int ColorID {
        get
        {
            return colorID;
        }
        set
        {
            if(colorID != value)
            {
                colorID = value;
                SetColor();
            }            
        }
    }

    public float HealthPoint { get ; set; }

    void SetColor()
    {
        if(colors.Length > colorID)
        {
            Material color = colors[Mathf.Clamp(colorID, 0, colors.Length - 1)];
            for (int i = 0; i < showColors.Length; i++)
            {
                if (showColors[i].sharedMaterial != color)
                {
                    showColors[i].material = color;
                    showColors[i].sharedMaterial = color;
                }
            }
        }        
    }

    public float CurrentFlowDirection()
    {
        Electrocircuit eA = Joint_A.GetJointElectrocircuit();
        Electrocircuit eB = Joint_B.GetJointElectrocircuit();
        if (eA == eB && eA != null)
        {
            return eA.GetDirection(Joint_A, Joint_B);
        }
        return 0;
    }
    public Electrocircuit GetElectrocircuit()
    {
        Electrocircuit eA = Joint_A.GetJointElectrocircuit();
        Electrocircuit eB = Joint_B.GetJointElectrocircuit();
        if (eA == eB && eA != null)
        {
            return eA;
        }
        return null;
    }


#if UNITY_EDITOR
    /// <summary>
    /// 绘制辅助线
    /// </summary>
    void OnDrawGizmos()
    {
        if(!Application.isPlaying)
        {
            SetColor();
        }
    }
#endif

    protected virtual void Start()
    {
        SetColor();
    }

    
          
    public virtual IJoint[] GetNextDepthJoint(IJoint joint)
    {
        if((object)jointA == joint)
        {
            return new IJoint[] { jointB };
        }
        else if ((object)jointB == joint)
        {
            return new IJoint[] { jointA };
        }
        return null;
    }

    public virtual uint GetDepthValue()
    {
        return 1;
    }

    public virtual float GetPowerLoad()
    {
        return 0;
    }

    public void Reset()
    {
        
    }
}
