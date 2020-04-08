using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 线材
/// </summary>
[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer))]
public class Wire : MonoBehaviour //,IHoldable
{
    /// <summary>
    /// 剖面几边形
    /// </summary>
    const int numberOfSectionVertice = 4;
    /// <summary>
    /// 转角的分割
    /// </summary>
    const int numberOfTurnSplit = 5;
    const float sectionFactor = 5f;
    //[Header("硬度，相邻两段之间的角度限制，越硬能弯曲角度越小")]
    //public float stiffiness = 90f;

    [Header("直径"), SerializeField]
    protected float diameter = 0.04f;
    [Header("路点")]
    public List<Transform> points = new List<Transform>();

    Vector3[] mSectionVertices;
    /// <summary>
    /// 剖面顶点位置
    /// </summary>
    Vector3[] SectionVertices
    {
        get
        {
            SetSection();
            return mSectionVertices;
        }
    }
    /// <summary>
    /// 直径
    /// </summary>
    public float Diameter {
        get
        {
           return diameter;
        }
        set
        {
            if(diameter != value)
            {
                diameter = value;
                SetSection();
            }
        }
    }

    MeshFilter meshFilter;


    public Mesh Mesh
    {
        get
        {
            if (meshFilter == null)
            {
                meshFilter = GetComponent<MeshFilter>();
                meshFilter.mesh = new Mesh();
            }
            return meshFilter.mesh;
        }
    }

    /// <summary>
    /// 获取精细的程度
    /// </summary>
    public float DetailMeter
    {
        get
        {
            return Diameter ;
        }
    }

    class SegmentInfo
    {
        //记录姿态，用于对比判断是否产生变化。
        /// <summary>
        /// 相对位置
        /// </summary>
        public Vector3 loaclPosFrom;
        /// <summary>
        /// 相对旋转
        /// </summary>
        public Quaternion loaclRotFrom;
        /// <summary>
        /// 前一个点相对位置
        /// </summary>
        public Vector3 loaclPosTo;
        /// <summary>
        /// 前一个点相对旋转
        /// </summary>
        public Quaternion loaclRotTo;

        public int startIndex = 0;

        /// <summary>
        /// 顶点
        /// </summary>
        public List<Vector3> vertices = new List<Vector3>();
        /// <summary>
        /// 法线
        /// </summary>
        public List<Vector3> normals = new List<Vector3>();
        /// <summary>
        /// 三角形
        /// </summary>
        public List<int> triangles = new List<int>();
        /// <summary>
        /// 蒙皮信息
        /// </summary>
        public List<Vector2> uv = new List<Vector2>();
    }

    /// <summary>
    /// 记录线段信息
    /// </summary>
    Dictionary<Transform, SegmentInfo> segmentInfos = new Dictionary<Transform, SegmentInfo>();
    
    /// <summary>
    /// 设置剖面坐标顶点形状
    /// </summary>
    void SetSection()
    {
        if(mSectionVertices == null || mSectionVertices.Length <= 0)
        {
            mSectionVertices = new Vector3[numberOfSectionVertice];
        }
        
        for (int i = 0; i < mSectionVertices.Length; i++)
        {
            //因为面的朝向因素，取负角度。
            Quaternion quaternion = Quaternion.Euler(0, 0, -360f / mSectionVertices.Length * i);
            mSectionVertices[i] = quaternion * Vector3.up * diameter * 0.5f;
        }
    }

    protected void SetPoint(Transform t, int index)
    {
        int i = points.IndexOf(t);
        if(i < 0)
        {
            if(index >= points.Count)
            {                
                points.Add(t);                
            }
            else
            {
                points.Insert(index, t);
            }
        }
        else if (i != index)
        {
            
            if(i < index)
            {
                //Transform child = points[i + 1];
                //child.SetParent = 
                points.Insert(index, t);
                points.RemoveAt(i);
            }
            else if(i > index)
            {
                points.RemoveAt(i);
                points.Insert(index, t);
            }
        }
    }

    protected virtual void LateUpdate()
    {
        ReSetMesh();
    }

    void ReSetMesh()
    {
        bool meshChanged = false;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        int startIndex = 0;

        //找到所有子节点并记录位置和分段的距离
        for (int i = 0; i < points.Count; i++)
        {
            Transform from, to;
            if (i == 0)
            {
                from = transform;
            }
            else
            {
                from = points[i - 1];
            }

            to = points[i];

            SegmentInfo info;
            //标记是否产生变化
            bool changed = false;
            if (!segmentInfos.ContainsKey(to))
            {
                changed = true;
                segmentInfos.Add(to, new SegmentInfo());
            }
            else if (i > 0) //首个节点永远跟着父节点走 不会变化
            {
                changed = Vector3.Distance(segmentInfos[to].loaclPosFrom, GetLoaclPosition(from)) > 0
                    || Quaternion.Angle(segmentInfos[to].loaclRotFrom, GetLoaclQuaternion(from)) > 0
                    || Vector3.Distance(segmentInfos[to].loaclPosTo, GetLoaclPosition(to)) > 0
                    || Quaternion.Angle(segmentInfos[to].loaclRotTo, GetLoaclQuaternion(to)) > 0;
            }
            else
            {
                changed = Vector3.Distance(segmentInfos[to].loaclPosTo, GetLoaclPosition(to)) > 0
                    || Quaternion.Angle(segmentInfos[to].loaclRotTo, GetLoaclQuaternion(to)) > 0;
            }
            info = segmentInfos[to];

            if (changed)
            {
                if (!meshChanged)
                {
                    meshChanged = true;
                    for (int j = 0; j < i; j++)
                    {
                        Transform t = points[j];
                        vertices.AddRange(segmentInfos[t].vertices);
                        normals.AddRange(segmentInfos[t].normals);
                        triangles.AddRange(segmentInfos[t].triangles);
                        uv.AddRange(segmentInfos[t].uv);
                    }
                }
                info.startIndex = startIndex;

                //更新位置信息
                if (from == transform)
                {
                    info.loaclPosFrom = Vector3.zero;
                    info.loaclRotFrom = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    info.loaclPosFrom = GetLoaclPosition(from);
                    info.loaclRotFrom = GetLoaclQuaternion(from);
                }
                info.loaclPosTo = GetLoaclPosition(to);  //to.localPosition;
                info.loaclRotTo = GetLoaclQuaternion(to);  //to.localRotation;

                info.vertices.Clear();
                info.normals.Clear();
                info.triangles.Clear();
                info.uv.Clear();

                float dis = Vector3.Distance(info.loaclPosFrom,info.loaclPosTo) - DetailMeter ;

                Quaternion rotation0 = info.loaclRotFrom;
                Quaternion rotation1 = info.loaclRotTo;

                Vector3 forward0 = rotation0 * Vector3.forward ;
                Vector3 forward1 = rotation1 * Vector3.forward ;

                Vector3 offset0 = info.loaclPosTo - forward0 * DetailMeter * 0.5f;
                Vector3 offset1 = info.loaclPosTo + forward1 * DetailMeter * 0.5f;                
                
                for (int j = 0; j < numberOfTurnSplit; j++)
                {
                    float t0 = j * 1f / numberOfTurnSplit;
                    float t1 = (j + 1f)  / numberOfTurnSplit;

                    Vector3 p0 = MyBezier.Basier(offset0, offset1, forward0, forward1, t0, DetailMeter * 0.5f);
                    Vector3 p1 = MyBezier.Basier(offset0, offset1, forward0, forward1, t1, DetailMeter * 0.5f);

                    Vector3 d0 = MyBezier.BezierTangent(offset0, offset1, forward0, forward1, t0, DetailMeter * 0.5f);
                    Vector3 d1 = MyBezier.BezierTangent(offset0, offset1, forward0, forward1, t1, DetailMeter * 0.5f);

                    Vector3 u0 = Quaternion.Lerp(rotation0, rotation1, t0) * Vector3.up;
                    Vector3 u1 = Quaternion.Lerp(rotation0, rotation1, t1) * Vector3.up;

                    Quaternion q0 = Quaternion.LookRotation(d0, u0);
                    Quaternion q1 = Quaternion.LookRotation(d1, u1);

                    //前端
                    CreatANodeVertices(p0, q0, info.vertices, info.normals);
                    for (int k = 0; k < SectionVertices.Length * 2; k++)
                    {
                        info.uv.Add(new Vector2(1f, k % 2 * 1f));
                    }
                    //后端
                    CreatANodeVertices(p1, q1, info.vertices, info.normals);
                    for (int k = 0; k < SectionVertices.Length * 2; k++)
                    {
                        info.uv.Add(new Vector2(0f, k % 2 * 1f));
                    }
                    LinkNode(startIndex, info.triangles);
                    startIndex += SectionVertices.Length * 4;
                }

                if(i > 0 && dis > 0)
                {
                    Vector3 p0 = offset0 - forward0 * dis;
                    Vector3 p1 = offset0;

                    //前端
                    CreatANodeVertices(p0, rotation0, info.vertices, info.normals);
                    for (int k = 0; k < SectionVertices.Length * 2; k++)
                    {
                        info.uv.Add(new Vector2(1f, k % 2 * 1f));
                    }
                    //后端
                    CreatANodeVertices(p1, rotation0, info.vertices, info.normals);
                    for (int k = 0; k < SectionVertices.Length * 2; k++)
                    {
                        info.uv.Add(new Vector2(0f, k % 2 * 1f));
                    }
                    LinkNode(startIndex, info.triangles);
                    startIndex += SectionVertices.Length * 4;
                }

            }
            else
            {
                //没有变化的情况下，需要根据前边顶点数的变化更新一下三角形的序号
                if (i > 0)
                {
                    if (info.startIndex != startIndex)
                    {
                        int oldIndex = info.startIndex;
                        info.startIndex = startIndex;

                        for (int vi = 0; vi < info.triangles.Count; vi++)
                        {
                            info.triangles[vi] = info.triangles[vi] - oldIndex + info.startIndex;
                        }
                    }
                }
                startIndex += info.vertices.Count;
            }

            if (meshChanged)
            {
                vertices.AddRange(info.vertices);
                normals.AddRange(info.normals);
                triangles.AddRange(info.triangles);
                uv.AddRange(info.uv);
            }
        }



        //首尾封口 待实现
        //*******************



        if (meshChanged)
        {
            Mesh.Clear();
            Mesh.vertices = vertices.ToArray();
            Mesh.normals = normals.ToArray();
            Mesh.triangles = triangles.ToArray();
            Mesh.uv = uv.ToArray();
        }

    }
   
    Vector3 GetLoaclPosition(Transform tran)
    {
        return transform.InverseTransformPoint(tran.position);
    }

    Quaternion GetLoaclQuaternion(Transform tran)
    {
        return Quaternion.LookRotation(transform.InverseTransformDirection(tran.forward), transform.InverseTransformDirection(tran.up));
    }
    
    /// <summary>
    /// 用位置和旋转获取一个位置上的顶点和法线
    /// </summary>
    /// <param name="pos">位置</param>
    /// <param name="quaternion">旋转 此处最好不用向量 单个向量会有万向节  如果用两个向量就相当于有旋转了</param>
    /// <param name="vertices"></param>
    /// <param name="normals"></param>
    void CreatANodeVertices(Vector3 pos, Quaternion quaternion, List<Vector3> vertices, List<Vector3> normals)
    {
        //备选顶点
        Vector3[] v = new Vector3[SectionVertices.Length];
        for (int i = 0; i < SectionVertices.Length; i++)
        {
            v[i] = quaternion * SectionVertices[i] + pos;
        }
        //遍历所有边
        for (int i = 0; i < v.Length; i++)
        {
            int fromIndex = i;
            int toIndex = (i + 1) % v.Length;
            Vector3 from = v[fromIndex], to = v[toIndex];
            //顶点
            vertices.Add(from);
            //法线
            normals.Add((quaternion * SectionVertices[fromIndex]).normalized);
            vertices.Add(to);
            normals.Add((quaternion * SectionVertices[toIndex]).normalized);
        }
    }

    /// <summary>
    /// 当前节点往前边连接 避免溢出
    /// </summary>
    /// <param name="nodeStartIndex">这个节点</param>
    /// <param name="triangles"></param>
    void LinkNode(int nodeStartIndex, List<int> triangles)
    {
        //面循环 线材的剖面 顶点数和边数相等
        for (int i = 0; i < SectionVertices.Length; i++)
        {
            int from = nodeStartIndex + i * 2; //关于乘2 顶点位置上，因为顶点和法线的一一对应，为了区分曲面和棱角，所以会有两套顶点信息。
            int to = from + 1;
            //远端位置序号 同样因为顶点信息的需要 所以乘2
            int fromNext = from + SectionVertices.Length * 2;
            int toNext = to + SectionVertices.Length * 2;

            triangles.AddRange(BuildAtQuadrangle(new int[] { fromNext,toNext,to, from }));

        }
    }
    /// <summary>
    /// 用四个顶点制造两个三角形拼成的一个四边形
    /// </summary>
    /// <param indexs=""></param>
    /// <returns></returns>
    int[] BuildAtQuadrangle(int[] indexs)
    {
        return new int[]
        {
            indexs[0],indexs[1],indexs[3],indexs[1],indexs[2],indexs[3]
        };
    }

  


#if UNITY_EDITOR
    /// <summary>
    /// 绘制辅助线
    /// </summary>
    protected virtual void OnDrawGizmos()
    {
        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.color = new Color(1f,0,0,0.5f);
            Transform from, to;
            if (i > 0)
            {
                from = points[i-1];
            }
            else
            {
                from = transform;
                Gizmos.DrawSphere(from.position, DetailMeter /5f);
            }
            to = points[i];
            string name = "Point_" + i;
            if ( points[i].gameObject.name != name)
            {
                points[i].gameObject.name = name;
            }

            Gizmos.DrawSphere(to.position, DetailMeter / 5f);

            //Gizmos.color = Color.red;
            //Gizmos.DrawLine(from.position, from.position + from.forward * DetailMeter);
            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine(to.position, to.position - to.forward * DetailMeter);
            Gizmos.color = new Color(1f,0,1f);
            Gizmos.DrawLine(from.position + from.up * diameter * 0.5f, to.position + from.up * diameter * 0.5f);
            Gizmos.DrawLine(from.position - from.up * diameter * 0.5f, to.position - from.up * diameter * 0.5f);
            Gizmos.DrawLine(from.position + from.right * diameter * 0.5f, to.position + from.right * diameter * 0.5f);
            Gizmos.DrawLine(from.position - from.right * diameter * 0.5f, to.position - from.right * diameter * 0.5f);
            //Gizmos.color = new Color(0, 0.5f, 0, 0.5f);
            //Gizmos.DrawLine(from.position, to.position);
        }
    }
#endif

}
