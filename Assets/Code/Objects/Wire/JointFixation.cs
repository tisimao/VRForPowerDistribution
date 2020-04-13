using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 触点类（可固定点）
/// 该类只关心可否固定不关心电路
/// </summary>
public class JointFixation : MonoBehaviour ,IFixation
{
    public int groupIndex = 0;
    [SerializeField]
    private Transform center;

    WireEditable wire;//
    IFixation mainFixation;

    public string Name => gameObject.name;

    public Transform Center
    {
        get
        {
            if(center == null)
            {
                return transform;
            }
            return center;
        }
        set => center = value;
    }

    public bool AddFixation(IFixation other, Transform node)
    {
        if (wire == null && other is WireEditable)
        {
            wire = (WireEditable)other;
            return true;
        }
        else if(mainFixation == null && GetFixationGroup() == other.GetFixationGroup())
        {
            if(IsEditing())
            {
                mainFixation = other;
                return true;
            }
        }
        return false;
    }

    public int GetFixationGroup()
    {
        return groupIndex;
    }

    public Transform GetTransform(IFixation other)
    {
        if((object)other == wire)
        {
            return transform;
        }
        else if(mainFixation == other)
        {
            return Center;
        }
        return transform;
    }

    public Vector3 GetTransformOffset(IFixation other)
    {
        return Vector3.zero;
    }

    public Quaternion GetTransformRotate(IFixation other)
    {
        return Quaternion.Euler(0, 0, 0);
    }

    public void RemoveFixation(IFixation other)
    {
        if (mainFixation == other)
        {
            mainFixation = null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Unmovable(IFixation other)
    {
        if (other == null)
        {
            return true;
        }
        bool editing = IsEditing();
        if ((object)other == wire)
        {
            if(mainFixation == null)// 没有固定点 受到线的牵引
            {
                return false;
            }
            else //如果有固定点
            {
                if(editing) //在编辑状态下 收到线的牵引
                {
                    return false;
                }
                else
                {
                    return true; // 牵引线 
                }
                 
            }
        }
        else if (mainFixation == other)
        {
            return editing; //不在编辑模式下受到固定点的牵引
        }
        return true;
    }

    /// <summary>
    /// 手部离的近就属于跟着手移动的编辑模式
    /// </summary>
    /// <returns></returns>
    bool IsEditing()
    {
        if(wire == null)
        {
            return false;
        }
        for (int i = 0; i < wire.hands.Count; i++)
        {
            HandScript hand = wire.hands[i];
            //判断手抓在线上的位置距离我在线上的位置太近
            if(wire.fixationInfos.ContainsKey(hand) && wire.fixationInfos.ContainsKey(this))
            {
                float handMeter = wire.fixationInfos[hand].MeterOnNode + wire.nodeMeterOnWire[wire.fixationInfos[hand].Node];
                float meter = wire.fixationInfos[this].MeterOnNode + wire.nodeMeterOnWire[wire.fixationInfos[this].Node];
                if (Mathf.Abs(meter - handMeter) < wire.SplitLength)
                {
                    return true;
                }
            }
            
        }
        return false;
    }

    Vector3 GetPosWire()
    {
        return wire.GetTransformOffset(this) + wire.GetTransform(this).position;
    }

    Vector3 GetPosFix()
    {
        if (mainFixation == null)
        {
            return transform.position;
        }
        return mainFixation.GetTransformOffset(this) + mainFixation.GetTransform(this).position + (transform.position - Center.position);
    }

    Quaternion GetRotWire()
    {
        return wire.GetTransform(this).rotation * wire.GetTransformRotate(this);
    }

    Quaternion GetRotFix()
    {
        if (mainFixation == null)
        {
            return transform.rotation;
        }
       return  mainFixation.GetTransform(this).rotation * mainFixation.GetTransformRotate(this);
    }

    public void OnEdit()
    {
       
        //if(Vector3.Distance(posWire, posFix) > 0.05f)
        //{
        //    RemoveFixation(mainFixation);
        //    mainFixation = null;
        //}


        if (Unmovable(wire) && mainFixation != null && !Unmovable(mainFixation)) //受到
        {
            Vector3 posWire = GetPosWire();
            Vector3 posFix = GetPosFix();

            //if (Vector3.Distance(posWire, posFix) < wire.DetailMeter * 0.5f)
            //{
            transform.rotation = GetRotFix();
            transform.position = posFix;
            //}
            //else
            //{
            //    transform.rotation = GetRotWire();
            //    transform.position = posWire;
            //}

        }
    }

    public void OnEditDone()
    {
        if (!Unmovable(wire))//处理线的牵引
        {
            transform.rotation = GetRotWire();
            transform.position = GetPosWire();
        }
    }

}
