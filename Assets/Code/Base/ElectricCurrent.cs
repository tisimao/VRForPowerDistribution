using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 电路的抽象类
/// </summary>
[System.Serializable]
public class Electrocircuit
{
    public string name = "";
    /// <summary>
    /// 电路的起点
    /// </summary>
    public IJoint startJoint;

    ///// <summary>
    ///// 电流方向是否是反向
    ///// </summary>
    //public bool inversion = false;

    /// <summary>
    /// 电路的终点 以后用Map优化
    /// </summary>
    List<IJoint> endJoints = new List<IJoint>();

    /// <summary>
    /// 电路的节点 以后用Map优化
    /// </summary>
    Dictionary<IJoint,uint> depthInfos = new Dictionary<IJoint, uint>();
    /// <summary>
    /// 线路上的组件
    /// </summary>
    List<IElectricComponent> components = new List<IElectricComponent>();

    /// <summary>
    /// 电流的时间参数
    /// </summary>
    public float time;

    /// <summary>
    /// 多个电流波形可以叠加
    /// </summary>
    List<AElectricCurrent> electricCurrents = new List<AElectricCurrent>();

    public Electrocircuit(string name, IJoint startJoint)
    {
        this.name = name;
        this.startJoint = startJoint;
    }
    public float Volt = 0;
    /// <summary>
    /// 电压
    /// </summary>
    public float Volts {
        get
        {
            float reslut = 0;
            for (int i = 0; i < electricCurrents.Count; i++)
            {
                reslut += electricCurrents[i].GetVolt(time);
            }
            reslut = Mathf.Round(reslut * 100000f) / 100000f;

            Volt = reslut;
            return reslut;
        }
    }
    public float PowerLoad
    {
        get
        {
            float reslut = 0;
            for (int i = 0; i < components.Count; i++)
            {
                reslut += components[i].GetPowerLoad();
            }

            return reslut;
        }
    }
    /// <summary>
    /// 当前电流
    /// </summary>
    public float Amperage
    {
        get
        {
            float v = Volts;
            if (v != 0)
            {
                return PowerLoad / v ;
            }
            return 0;
        }
    }

    
    void CheckLink()
    {
        //从起点开始 
        IJoint start = startJoint;
        Dictionary<IJoint, uint> newInfo = new Dictionary<IJoint, uint>();
        AddLinkInfo(0, start, newInfo);

        List<IJoint> joints = new List<IJoint>(this.depthInfos.Keys);

        //清理原来的信息
        for (int i = 0; i < joints.Count; i++)
        {
            IJoint joint = joints[i];
            if (!newInfo.ContainsKey(joint))
            {
                joint.RemoveElectrocircuit(this);
            }
        }
        depthInfos = newInfo;
    }

    void AddLinkInfo(uint depth, IJoint joint, Dictionary<IJoint, uint> info)
    {   
        if (info.ContainsKey(joint))
        {
            if(info[joint] > depth)
            {
                info[joint] = depth;
            }
            else
            {
                return;
            }
        }
        else
        {
            info.Add(joint, depth);
            joint.AddElectrocircuit(this);
        }
        IJoint[] joints = joint.GetLinks();
        for (int i = 0; i < joints.Length; i++)
        {
            AddLinkInfo(depth, joints[i], info);
        }
        //如果接头短路了，就认为和后续熔断了。
        if (!joint.Short)
        {
            IElectricComponent component = joint.GetElectricComponent();
            IJoint[] nexts = joint.GetNextDepthJoint();
            if (nexts != null)
            {
                uint nextLevel = component.GetDepthValue() + depth;
                for (int i = 0; i < nexts.Length; i++)
                {
                    AddLinkInfo(nextLevel, nexts[i], info);

                }
            }
        }
    }


    public void AddLoad(IElectricComponent component)
    {
        if(!components.Contains(component))
        {
            components.Add(component);
        }
    }

    public void RemoveLoad(IElectricComponent component)
    {
        if (components.Contains(component))
        {
            components.Remove(component);
        }
    }

    public void Reset()
    {
        components.Clear();
        CheckLink();
    }

    /// <summary>
    /// 获取电流方向
    /// </summary>
    /// <returns></returns>
    public float GetDirection(IJoint jointA, IJoint jointB)
    {
        uint a = depthInfos[jointA];
        uint b = depthInfos[jointB];
        if (a >= 0 && b >= 0)
        {
            if(a > b)
            {
                return -1;
            }
            else if(a < b)
            {
                return 1;
            }
        }
        return 0;
    }

    public bool Contains(IJoint joint)
    {
        return depthInfos.ContainsKey(joint);
    }

    public uint GetDepth(IJoint joint)
    {
        if(depthInfos.ContainsKey(joint))
        {
            return depthInfos[joint];
        }
        return uint.MaxValue;
    }

    public void Clear()
    {
        components.Clear();
        foreach (var item in depthInfos.Keys)
        {
            item.RemoveElectrocircuit(this);
        }
        depthInfos.Clear();
    }

    public void AddElectricCurrent(AElectricCurrent current)
    {
        if(!electricCurrents.Contains(current))
        {
            electricCurrents.Add(current);
        }
    }

    public void RemoveElectricCurrent(AElectricCurrent current)
    {
        if (electricCurrents.Contains(current))
        {
            electricCurrents.Remove(current);
        }
    }

}

/// <summary>
/// 电流波形
/// </summary>
public abstract class AElectricCurrent
{
    protected float volt = 0;

    public virtual void SetMaxVolt(float value)
    {
        volt = value;
    }
    /// <summary>
    /// 电压
    /// </summary>
    public abstract float GetVolt(float time);
}

/// <summary>
/// 交流电
/// </summary>
[System.Serializable]
public class AC: AElectricCurrent
{
    public AC(float frequency, float phasePosition, float volt)
    {
        Frequency = frequency;
        PhasePosition = phasePosition;
        this.volt = volt;
    }

    /// <summary>
    /// 频率
    /// </summary>
    public float Frequency { get; set; }
    /// <summary>
    /// 相位
    /// </summary>
    public float PhasePosition { get; set; }


    public override float GetVolt(float time)
    {
        float v = Mathf.Sin((time * Frequency + PhasePosition / 360) * 2f * Mathf.PI) * volt * 1.41421f;
        return v;
    }

}

/// <summary>
/// 直流电
/// </summary>
public class DC : AElectricCurrent
{
    public DC(float volt)
    {
        this.volt = volt;
    }

    public override float GetVolt(float time)
    {
        return volt;
    }

}
