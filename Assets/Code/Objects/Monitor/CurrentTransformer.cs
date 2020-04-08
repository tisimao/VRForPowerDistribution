using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentTransformer : MonoBehaviour ,IElectricComponent,IFixation
{
    /// <summary>
    /// 装备到的线材
    /// </summary>
    public Lead lead;
    /// <summary>
    /// 锁定在线材上的一个节点
    /// </summary>
    public Transform leadOnNode;

    public LeadJoint s1, s2;
    [SerializeField]
    Electrocircuit output;

    public GameObject brokenVFX;
    [Header("输入的电流"),Min(1f)]
    public float anperageIn = 400f;
    [Header("输出的电流"),Min(1f)]
    public float anperageOut = 5f;

    DC signal = new DC(1f);

    [SerializeField]
    float hp = 1f;
    public float HealthPoint
    {
        get
        {
            return hp;
        }
        set
        {
            hp = value;
            brokenVFX.SetActive(hp <= 0);
        }
    }

    public string Name => gameObject.name;

    public string info = "";

    float loadPower = 0;

    WireEditable wire;
    Dictionary<Collider, int> colliders = new Dictionary<Collider, int>();
    Dictionary<Collider, IFixation> fixations = new Dictionary<Collider, IFixation>();

    private void Start()
    {
        HealthPoint = 1f;
        output = new Electrocircuit(gameObject.name + " AC ", s1);       
        output.AddElectricCurrent(signal);
        
        output.Reset();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!colliders.ContainsKey(other))
        {
            colliders.Add(other, 1);
        }
        else
        {
            colliders[other]++;
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (colliders.ContainsKey(other))
        {
            colliders[other]--;
            if (colliders[other] <= 0)
            {
                if (fixations.ContainsKey(other))
                {
                    if(fixations[other] == wire)
                    {
                        
                        lead = null;
                        wire = null;
                    }

                    fixations[other].RemoveFixation(this);
                    fixations.Remove(other);
                }
                colliders.Remove(other);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        

        output.AddLoad(this);
        //有线
        if (lead != null)
        {
            Electrocircuit electrocircuit = lead.GetElectrocircuit();
           
            if (electrocircuit != null)
            {
                output.time = electrocircuit.time;
                if (electrocircuit.Amperage != 0)
                {
                    uint depthS2 = output.GetDepth(s2);
                    bool link = depthS2 < GetDepthValue();
                    if (link)  //CT两端 连接  
                    {
                        /*二次侧绝对不允许开路，因一旦开路，一次侧电流I1全部成为磁化电流，引起φm和E2骤增，造成铁心过度饱和磁化，发热严重乃至烧毁线圈；
                        同时，磁路过度饱和磁化后，使误差增大。
                        电流互感器在正常工作时，二次侧与测量仪表和继电器等电流线圈串联使用，测量仪表和继电器等电流线圈阻抗很小，二次侧近似于短路。
                        CT二次电流的大小由一次电流决定，二次电流产生的磁势，是平衡一次电流的磁势的。
                        若突然使其开路，则励磁电动势由数值很小的值骤变为很大的值，铁芯中的磁通呈现严重饱和的平顶波，
                        因此二次侧绕组将在磁通过零时感应出很高的尖顶波，其值可达到数千甚至上万伏，危及工作人员的安全及仪表的绝缘性能。
                        另外，二次侧开路使二次侧电压达几百伏，一旦触及将造成触电事故。因此，电流互感器二次侧都备有短路开关，
                        防止二次侧开路。在使用过程中，二次侧一旦开路应马上撤掉电路负载，然后，再停电处理。一切处理好后方可再用。*/
                        loadPower = electrocircuit.Amperage / anperageIn * anperageOut;
                        float pL = GetPowerLoad();
                        float ePL = output.PowerLoad;
                        if (pL < ePL) //互感不能抵消，励磁
                        {
                            info = "互感不能抵消，励磁 " + pL + "  " + ePL;
                            HealthPoint -= Time.deltaTime / 5f;
                        }
                        else // 互感 安全
                        {
                            info = "互感 安全 ";

                        }
                        info += "  electrocircuit.Amperage " + electrocircuit.Amperage;
                    }
                    else //CT 开放 要炸
                    {
                        info = "CT 开放 要炸  " + depthS2;
                        HealthPoint -= Time.deltaTime / 5f;

                        loadPower = 0;
                    }
                }
                else
                {

                    info = "空置状态 1";
                    Clean();
                }
            }
            else //空置状态
            {

                info = "空置状态 2";
                Clean();
            }
            
        }
        else //空置状态
        {

            info = "空置状态 3";
            Clean();

            foreach (Collider collider in colliders.Keys)
            {
                IFixation fixation = collider.GetComponentInParent<IFixation>();
                if (fixation != null)
                {
                    if(!fixations.ContainsKey(collider))
                    {
                        fixations.Add(collider, fixation);
                    }
                    if (GetFixationGroup() == fixation.GetFixationGroup() )
                    {
                        if(fixation is WireEditable)
                        {
                            wire = (WireEditable)fixation;
                            lead = wire.GetTransform(this).GetComponentInParent<Lead>();
                            wire.AddFixation(this, collider.transform);
                        }
                       
                    }
                }
            }

        }

    }


    void Clean()
    {
        loadPower = 0;
        HealthPoint = 1f;
      //  output.Clear();
    }

    public IJoint[] GetNextDepthJoint(IJoint joint)
    {
        if ((object)joint == s1)
        {
            return new IJoint[] { s2 };
        }
        if ((object)joint == s2)
        {
            return new IJoint[] { s1 };
        }
        return null;
    }

    public uint GetDepthValue()
    {
        return 99999999;
    }

    public float GetPowerLoad()
    {
        return loadPower;
    }

    public void Reset()
    {
        HealthPoint = 1f;
    }

    public bool Unmovable(IFixation other)
    {
        if(other is WireEditable && wire == (object)other)
        {
            for (int i = 0; i < wire.hands.Count; i++)
            {
                if(Vector3.Distance(wire.hands[i].transform.position , transform.position) < wire.SplitLength)
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    public int GetFixationGroup()
    {
        return 5;
    }

    public Transform GetTransform(IFixation other)
    {
        return transform;
    }

    public Vector3 GetTransformOffset(IFixation other)
    {
        return Vector3.zero;
    }

    public Quaternion GetTransformRotate(IFixation other)
    {
        return Quaternion.Euler(0,0,0);
    }

    public bool AddFixation(IFixation other, Transform node)
    {
        return false;
    }

    public void RemoveFixation(IFixation other)
    {

    }
}
