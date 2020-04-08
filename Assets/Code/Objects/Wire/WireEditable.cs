using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 能够被编辑的线
/// 用transform 旋转来扭曲电线
/// 用子节点Z轴偏移来控制每段的长度
/// 碰撞体从Z轴方向上偏移来给每段增加碰撞体
/// 
/// </summary>
public class WireEditable : Wire, IGrabable ,IFixation
{
    public List<Transform> tran = new List<Transform>();
    public List<float> dis = new List<float>();
    public List<HandScript> hands = new List<HandScript>();

    public bool grabable = false;

    /// <summary>
    /// 手部信息
    /// </summary>
    [System.Serializable]
    public class FixationInfo
    {
        public bool unmovable = false;
        [SerializeField]
        string FixationName = "";
        /// <summary>
        /// 被固定的节点
        /// </summary>
        [SerializeField]
        Transform node;
        public Transform Node { get => node; }

        IFixation fixation;
        public IFixation Fixation
        {
            get
            {
                return fixation;
            }
        }

        /// <summary>
        /// 节点相对固定的位置
        /// </summary>
        Vector3 nodePositionInFixation;
        public Vector3 NodePositionInFixation
        {
            set
            {
                nodePositionInFixation = value;
            }
            get
            {
                return nodePositionInFixation;
            }
        }
        /// <summary>
        /// 节点相对固定的前方
        /// </summary>
        Vector3 nodeForwardInFixation;
        public Vector3 NodeForwardInFixation
        {
            set
            {
                nodeForwardInFixation = value;
            }
            get
            {
                return nodeForwardInFixation;
            }
        }
        /// <summary>
        /// 节点相对固定的上方
        /// </summary>
        Vector3 nodeUpInFixation;
        public Vector3 NodeUpInFixation
        {
            set
            {
                nodeUpInFixation = value;
            }
            get
            {
                return nodeUpInFixation;
            }
        }
        [SerializeField]
        float meterOnNode = 0;
        public float MeterOnNode
        {
            set
            {
                meterOnNode = value;
            }
            get
            {
                return meterOnNode;
            }
        }

        public FixationInfo(IFixation fixation)
        {
            
            FixationName = fixation.Name;
            this.fixation = fixation;
        }
               
        public void SetNode(Transform node, Vector3 localOffset, Vector3 localForward, Vector3 localUp,float meterOnNode)
        {
            this.node = node;
            nodePositionInFixation = localOffset;
            nodeForwardInFixation = localForward;
            nodeUpInFixation = localUp;
            this.meterOnNode = meterOnNode;
        }
    }
    //[Header("长度"), SerializeField]
    //public float maxLength = 1f;
    [Header("长度"), SerializeField]
    float splitLength = 0.05f;
    public float SplitLength
    {
        get
        {
            return splitLength;
        }
    }
    /// <summary>
    /// 记录所有固定点信息
    /// </summary>
    public Dictionary<IFixation, FixationInfo> fixationInfos = new Dictionary<IFixation, FixationInfo>();
    /// <summary>
    /// 用来记录固定顺序 
    /// 两个相邻的固定点之间进行修改判断
    /// </summary>
    [SerializeField]
    List<FixationInfo> fixationList = new List<FixationInfo>();

    public Dictionary<Transform, float> nodeMeterOnWire = new Dictionary<Transform, float>();
    

    public bool IsGrabing
    {
        get
        {
            return hands.Count > 0;
        }
    }
    [SerializeField]
    protected GrabType grabType;
    public GrabType GrabType
    {
        get
        {
            return grabType;
        }
    }
    public string Name => gameObject.name;

    public bool IsGrabable => grabable;

    [SerializeField]
    JointFixation jointFixationA, jointFixationB;

     void Start()
    {
        //不能让Point数量过少
        //if(points.Count == 0)
        //{            
        //    SetPoint(AddNextNode(-1,maxLength).transform, 0);
        //}
        for (int i = 0; i < points.Count; i++)
        {
            if(i < points.Count - 1)
            {
                SetCollider(points[i], points[i+1].localPosition.z);
            }
        };
        UpdateNodeMeterOnWire(0, points.Count - 1);


        jointFixationA.transform.position = points[0].position;
        jointFixationA.transform.rotation = Quaternion.LookRotation(-points[0].forward, points[0].up);
        jointFixationA.AddFixation(this, jointFixationA.transform);
        AddFixation(jointFixationA, points[0]);

        jointFixationB.transform.position = points[points.Count - 1].position;
        jointFixationB.transform.rotation = points[points.Count - 1].rotation;
        jointFixationB.AddFixation(this, jointFixationB.transform);
        AddFixation(jointFixationB, points[points.Count - 1]);

    }

       

    /// <summary>
    /// 为节点固定关系指定节点
    /// </summary>
    /// <param name="info"></param>
    /// <param name="node"></param>
    void SetNode(FixationInfo info,Transform node)
    {
        Vector3 fwd = info.Fixation.GetTransform(this).InverseTransformDirection(node.forward);
        fwd = new Vector3(0, 0, Mathf.Sign(fwd.z));
        Vector3 up = Vector3.ProjectOnPlane(info.Fixation.GetTransform(this).InverseTransformDirection(node.up), Vector3.forward);
        Vector3 pos = info.Fixation.GetTransform(this).InverseTransformPoint(node.position);
        pos.x = 0;pos.y = 0;

        info.SetNode(node,pos,fwd,up,
            node.InverseTransformPoint(info.Fixation.GetTransform(this).position).z);
    }

    //public float GetNodeMeterOnWire(Transform node)
    //{
    //    if(nodeMeterOnWire.ContainsKey(node))
    //    {
    //        return nodeMeterOnWire[node];
    //    }
    //    return float.MinValue;
    //}

    /// <summary>
    /// 固定点的最靠近起点的位置
    /// </summary>
    /// <returns></returns>
    Vector3 GetFixationWorldPosBegin(FixationInfo info)
    {
        return info.Fixation.GetTransform(this).TransformPoint(-info.NodeForwardInFixation).normalized * SplitLength * 0.5f;
    }

    /// <summary>
    /// 固定点靠近终点的位置
    /// </summary>
    /// <returns></returns>
    Vector3 GetFixationWorldPosEnd(FixationInfo info)
    {
        return info.Fixation.GetTransform(this).TransformPoint(info.NodeForwardInFixation).normalized * SplitLength * 0.5f;
    }

    /// <summary>
    /// 获取Node需要同步到的 位置
    /// </summary>
    /// <returns></returns>
    public Vector3 GetNodeSyncPosition(FixationInfo info)
    {
        return info.Fixation.GetTransform(this).TransformPoint(info.NodePositionInFixation);
    }
    /// <summary>
    /// 获取Node需要同步到的 方向
    /// </summary>
    /// <returns></returns>
    public Quaternion GetNodeSyncRotation(FixationInfo info)
    {
        //Vector3 forward = info.Fixation.GetTransform(this).TransformDirection(info.NodeForwardInFixation);
        //Vector3 up = info.Fixation.GetTransform(this).TransformDirection(info.NodeUpInFixation);
        return Quaternion.LookRotation(GetNodeSyncForward(info), GetNodeSyncUp(info));
    }

    public Vector3 GetNodeSyncUp(FixationInfo info)
    {
        return info.Fixation.GetTransform(this).TransformDirection(info.NodeUpInFixation);
    }

    public Vector3 GetNodeSyncForward(FixationInfo info)
    {
        return info.Fixation.GetTransform(this).TransformDirection(info.NodeForwardInFixation);
    }

    /// <summary>
    /// 添加固定关系
    /// </summary>
    /// <param name="fixation"></param>
    /// <param name="node"></param>
    public bool AddFixation(IFixation fixation,Transform node)
    {
        if(!fixationInfos.ContainsKey(fixation))
        {
            //判断节点是否已经重复了 并且找出插入点
            int index = 0;
            float disNew = node.InverseTransformPoint(fixation.GetTransform(this).position).z;
            for (int i = 0; i < fixationList.Count; i++)
            {
                FixationInfo temp = fixationList[i];
                if (node == temp.Node)
                {
                    //通过距离判断插入序号                   
                    float disTemp = temp.MeterOnNode;
                    if (disNew <= disTemp)
                    {
                        index = i;
                        break;
                    }
                    else
                    {
                        //向后查找相同
                        index = i + 1;
                    }
                }
                else
                {
                    int nodeIndex = points.IndexOf(node);
                    if (nodeIndex < 0) //非法节点 跳出
                    {
                        index = -1;
                        break;
                    }
                    int tempIndex = points.IndexOf(fixationList[i].Node);
                    if (nodeIndex < tempIndex)
                    {
                        index = i;
                        break;
                    }
                    else if (nodeIndex > tempIndex)
                    {
                        if(i == fixationList.Count - 1)
                        {
                            index = fixationList.Count;
                        }
                    }
                }
            }
            Debug.Log(index);
            if(index < 0)
            {
                return false;
            }
            else
            {
                //将新固定信息插入
                FixationInfo info = new FixationInfo(fixation);
                SetNode(info, node);
                fixationInfos.Add(fixation, info);               
                if (index < fixationList.Count)
                {
                    fixationList.Insert(index, info);
                    //if(index < fixationList.Count - 1)
                    //{
                    //    UpdateTransformBetweenFixations(index, index + 1);
                    //}
                }
                else
                {
                    fixationList.Add(info);
                }
                //if (index > 0)
                //{
                //    UpdateTransformBetweenFixations(index - 1, index);
                //}

                return true;
            }
        }
        return false;
    }
    
    public void RemoveFixation(IFixation fixation)
    {
        if (fixationInfos.ContainsKey(fixation))
        {
            FixationInfo info = fixationInfos[fixation];
            fixationInfos.Remove(fixation);
            fixationList.Remove(info);
        }
    }

    protected override void LateUpdate()
    {
        jointFixationA.OnEdit(); jointFixationB.OnEdit();
        Edit();
        base.LateUpdate();
        jointFixationA.OnEditDone(); jointFixationB.OnEditDone();
    }

 
    void Edit()
    {
        int i = 0;
        int last = -1;
        while ( i < fixationList.Count)
        {
            FixationInfo info = fixationList[i];
            info.unmovable = info.Fixation.Unmovable(this);
            if (info.unmovable)
            {
                if (last < 0)
                {
                    SetTransformBaseOnFixation(fixationList[i]);
                    last = i;
                    i++;
                }
                else
                {
                    if (UpdateTransformBetweenFixations(last, i)) //如果假 说明有删除
                    {
                        last = i;
                        i++;
                    }
                    else
                    {
                        RemoveFixation(info.Fixation); //移除失效的固定点
                    }
                }
            }
            else
            {
                //临时
                if ((object)info.Fixation == jointFixationB)
                {
                    info.Node.localEulerAngles = Vector3.zero;
                }


                i++;
            }

            



        }
       
    }
    /// <summary>
    /// 设定物体根据固定点移动
    /// </summary>
    /// <param name="info"></param>
    void SetTransformBaseOnFixation(FixationInfo info)
    {
        //先获得根根节点相对 当前节点的偏移
        Vector3 rootOffset = info.Node.InverseTransformPoint(transform.position);
        Vector3 rootForward = info.Node.InverseTransformDirection(transform.forward);
        Vector3 rootUp = info.Node.InverseTransformDirection(transform.up);

        Quaternion rotate = GetNodeSyncRotation(info);

        transform.rotation = Quaternion.LookRotation(rotate * rootForward, rotate * rootUp);
        transform.position = GetNodeSyncPosition(info) + info.Node.rotation * rootOffset;
    }


    bool HaveChange(FixationInfo info)
    {
        //检测相对位置变化来确定
        Vector3 pos0 = transform.InverseTransformPoint(info.Node.position);
        Vector3 pos1 = transform.InverseTransformPoint(GetNodeSyncPosition(info));
        bool change = Vector3.Distance(pos0, pos1) > Diameter * 0.1f
            || Quaternion.Angle(GetNodeSyncRotation(info), info.Node.rotation) > 0.5f;

        return change;
    }
    bool UpdateTransformBetweenFixations(int fromInfoIndex, int toInfoIndex)
    {
        FixationInfo from = fixationList[fromInfoIndex];
        FixationInfo to = fixationList[toInfoIndex];

        int fromNodeIndex = points.IndexOf(from.Node);
        int toNodeIndex = points.IndexOf(to.Node);
        if (fromNodeIndex > toNodeIndex || fromNodeIndex == points.Count - 1)
        {
            //移除失效的固定点
            return false;
        }
        if (HaveChange(from) || HaveChange(to))
        {
            if (fromNodeIndex == toNodeIndex)
            {
                //计算主动固定物的结束位置 和 从动固定物额起始位置
                float p0 = from.MeterOnNode + SplitLength * 0.5f;
                float p1 = to.MeterOnNode - SplitLength * 0.5f;

                float l = nodeMeterOnWire[points[fromNodeIndex + 1]] - nodeMeterOnWire[points[fromNodeIndex]];

                float dis0 = p0;
                float dis1 = SplitLength;

                if (l > SplitLength * 2 && p1 > p0)
                {
                    dis0 = (p0 + p1) / 2f;
                }
                else
                {
                    //移除失效的固定点
                    return false;
                }
                                
                Transform newNode = AddNextNode(fromNodeIndex, dis0);
                ReplaceNode(toInfoIndex, fixationList.Count - 1, from.Node, newNode);
                toNodeIndex = fromNodeIndex + 1;
            }

            //计算主动和从动固定点中间的距离

            if(toNodeIndex - fromNodeIndex > 1)
            {
                Vector3 startPos = points[fromNodeIndex + 1].position;
                Vector3 endOld = points[toNodeIndex].position;
                Vector3 endNew = GetNodeSyncPosition(to);

                Vector3 offset_Old = endOld - startPos;
                Vector3 offset_New = endNew - startPos;

                float maxDis = nodeMeterOnWire[points[toNodeIndex]] - nodeMeterOnWire[points[fromNodeIndex + 1]];
                
                

                //判断距离
            
                //Vector3.ProjectOnPlane

                //points[toNodeIndex - 1].LookAt(endNew);
                //points[toNodeIndex - 1].localRotation = LimitQuaternion(points[toNodeIndex - 1].localRotation);
            }

            to.Node.rotation = Quaternion.LookRotation(
                to.Fixation.GetTransform(this).position - to.Node.position,
                to.Fixation.GetTransform(this).TransformDirection(to.NodeUpInFixation));

            to.Node.localRotation = LimitQuaternion(to.Node.localRotation);

            if (toNodeIndex < points.Count - 1)
            {
                if (nodeMeterOnWire[points[toNodeIndex + 1]] - nodeMeterOnWire[points[toNodeIndex]] < to.MeterOnNode)
                {                   
                    return false;
                }
            }
        }
        return true;
    }

    Quaternion LimitQuaternion(Quaternion quaternion)
    {
        return quaternion;
        Quaternion a = Quaternion.Euler(0, 0, 0);
        float angle = Vector3.Angle(Vector3.forward ,quaternion* Vector3.forward);
        if(angle == 0)
        {
            return quaternion;
        }
        return Quaternion.Lerp(a, quaternion, 90f / angle);

    }
    
    void ReplaceNode(int begin, int end,Transform find,Transform node)
    {
        int to = Mathf.Min(end , fixationList.Count - 1);
        for (int i = begin; i <= to; i++)
        {
            if (fixationList[i].Node == find)
            {
                SetNode(fixationList[i], node);
            }
            else
            {
                break;
            }
        }
    }


    /// <summary>
    /// 获取到达节点用的距离
    /// </summary>
    /// <returns></returns>
    void UpdateNodeMeterOnWire(int from,int to)
    {
        float distacne = 0;
        
        for (int i = from; i <= to; i++)
        {
            if(i > 0)
            {
                distacne = nodeMeterOnWire[points[i - 1]] + points[i].localPosition.z;
                //distacne = nodeMeterOnWire[points[i - 1]] + Vector3.Distance(points[i].position, points[i-1].position) ;
            }
            if (nodeMeterOnWire.ContainsKey(points[i]))
            {
                nodeMeterOnWire[points[i]] = distacne;
            }
            else
            {
                nodeMeterOnWire.Add(points[i], distacne);
            }
        }
        tran = new List<Transform>(nodeMeterOnWire.Keys);
        dis = new List<float>(nodeMeterOnWire.Values);
    }

    void SetCollider(Transform node,float dis)
    {
        BoxCollider boxCollider = node.GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            boxCollider = node.gameObject.AddComponent<BoxCollider>();
        }
        boxCollider.center = Vector3.forward * dis * 0.5f;
        boxCollider.size = new Vector3(diameter, diameter, dis);
    }


    /// <summary>
    /// 添加一个新的节点
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    Transform AddNextNode(int index, float distance)
    {
        //points 至少要有两个元素

        GameObject newObject = new GameObject();
        newObject.layer = gameObject.layer;
        Transform newNode = newObject.transform;

        newNode.SetParent(points[index]);
        newNode.localPosition = Vector3.forward * distance;
        newNode.localEulerAngles = Vector3.zero;
        SetCollider(points[index], distance);

        float remain = points[index + 1].localPosition.z - distance;

        points[index + 1].SetParent(newNode);
        points[index + 1].localPosition = Vector3.forward * remain;

        SetCollider(newNode, points[index + 1].localPosition.z);
        SetPoint(newNode, index + 1);
        UpdateNodeMeterOnWire(index, points.Count - 1);

        return newNode;
    }

    void RemoveNode(Transform node)
    {

    }

    public bool Hold(Collider handOn, HandScript hand)
    {
        if(!grabable)
        {
            return false;
        }
        Debug.Log(handOn + "  " + hand);
        if(AddFixation(hand, handOn.transform))
        {
            if(!hands.Contains(hand))
            {
                hands.Add(hand);
            }
            return true;
        }
        return false;
    }

    public void Drop(HandScript hand)
    {
        if (hands.Contains(hand))
        {
            hands.Remove(hand);
        }
        RemoveFixation(hand);
    }

    public bool Unmovable(IFixation other)
    {
        return !other.Unmovable(this);
    }

    public int GetFixationGroup()
    {
        return 5;
    }

    public virtual Transform GetTransform(IFixation other)
    {
        if(fixationInfos.ContainsKey(other))
        {
            return fixationInfos[other].Node;
        }
        return transform;
    }

    public Vector3 GetTransformOffset(IFixation other)
    {
        if (fixationInfos.ContainsKey(other))
        {
            return fixationInfos[other].Node.forward * fixationInfos[other].MeterOnNode;
        }
        return Vector3.zero;
    }

    public Quaternion GetTransformRotate(IFixation other)
    {
        if((object)other == jointFixationB)
        {
            return Quaternion.Euler(0, 0, 0);
        }
        if (fixationInfos.ContainsKey(other))
        {
            FixationInfo info = fixationInfos[other];
            Quaternion quaternion = Quaternion.LookRotation(info.NodeForwardInFixation, info.NodeUpInFixation);
            return Quaternion.Inverse(quaternion);
        }
        return Quaternion.Euler(0, 0, 0);
    }
}
