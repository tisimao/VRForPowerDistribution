using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 电线接头
/// 判断 接头短路 
/// 短路后烧毁 输出为0
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class LeadJoint : MonoBehaviour ,IJoint
{
    private List<IJoint> links = new List<IJoint>();

    [SerializeField]
    protected Transform centerRoot;

    IElectricComponent electricComponent;
    IElectricComponent ElectricComponent
    {
        get
        {
            if(electricComponent == null)
            {
                electricComponent = GetComponentInParent<IElectricComponent>();
            }
            return electricComponent;
        }
    }
   
    bool isShort = false;
    public bool Short {
        get
        {
            return isShort;
        }
        set
        {
            if(isShort != value)
            {
                isShort = value;
                ShowShortEFX(isShort);
                CheckCircuit();
            }
        }
    }

    public string Name {
        get
        {
            return transform.parent .name  +"_"+ gameObject.name;
        }
    }

    [SerializeField]
    GameObject shortEFX;

    /// <summary>
    /// 接入的电路
    /// </summary>
    [SerializeField]
    List<Electrocircuit> electrocircuitInfos = new List<Electrocircuit>();

    
    protected virtual void Start()
    {
        ShowShortEFX(false);
    }



    void ShowShortEFX(bool show)
    {
        if (shortEFX)
        {
            shortEFX.SetActive(show);
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        CheckShort();
        
    }

    /// <summary>
    /// 更新电路信息
    /// </summary>
    void CheckCircuit()
    {
        for (int i = 0; i < electrocircuitInfos.Count; i++)
        {
            electrocircuitInfos[i].Reset();
        }
       
    }

    /// <summary>
    /// 检查接头短路
    /// </summary>
    void CheckShort()
    {
        bool shorted = false;
        for (int i = 0; i < electrocircuitInfos.Count; i++)
        {
            Electrocircuit temp1 = electrocircuitInfos[i];
            for (int j = i + 1; j % electrocircuitInfos.Count < i; j++)
            {
                Electrocircuit temp2 = electrocircuitInfos[j % electrocircuitInfos.Count];
                if(temp1.Volts - temp2.Volts != 0)
                {
                    shorted = true;
                    break;
                }
            }
        }
        Short = shorted;
    }

    private void OnCollisionStay(Collision collision)
    {
        IJoint joint = collision.collider.GetComponentInParent<IJoint>();
        if(joint != null)
        {
            AddJoint(joint);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        IJoint joint = collision.collider.GetComponentInParent<IJoint>();
        if (joint != null )
        {
            RemoveJoint(joint);
        }
    }

    public object GetOwner()
    {
        return ElectricComponent;
    }
        

    public void AddJoint(IJoint joint)
    {
        if (!links.Contains(joint))
        {
            links.Add(joint);
            CheckCircuit();
        }        
    }

    public void RemoveJoint(IJoint joint)
    {
        if (links.Contains(joint))
        {
            links.Remove(joint);
            CheckCircuit();
        }
    }

    public IJoint[] GetLinks()
    {
        return links.ToArray();
    }

    public IElectricComponent GetElectricComponent()
    {
        return ElectricComponent;
    }
    
    public Electrocircuit GetJointElectrocircuit()
    {
        if(Short)
        {
            return null;
        }
        if(electrocircuitInfos.Count > 0)
        {
            return electrocircuitInfos[0];
        }
        return null;
    }

    public void AddElectrocircuit(Electrocircuit electricCurrent)
    {
        if(!electrocircuitInfos.Contains(electricCurrent))
        {
            electrocircuitInfos.Add(electricCurrent);
        }
    }

    public void RemoveElectrocircuit(Electrocircuit electricCurrent)
    {
        if (electrocircuitInfos.Contains(electricCurrent))
        {
            electrocircuitInfos.Remove(electricCurrent);
        }
    }

    public IJoint[] GetNextDepthJoint()
    {
        return ElectricComponent.GetNextDepthJoint(this);
    }

    public void Reset()
    {
        Short = false;
        links.Clear();
        electrocircuitInfos.Clear();
    }

    public void Refresh()
    {
        CheckCircuit();
    }

 
}
