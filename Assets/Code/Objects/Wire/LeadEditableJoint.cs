using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeadEditableJoint : LeadJoint
{
    [System.Serializable]
    public class Config
    {
        public float radius = 0 ,lenght = 0.01f ;
        public int group;
    }

    BoxCollider collider;
    Wire wire;
    JointFixation fixation;
    public int configIndex = 0;
    Lead lead;
    [SerializeField]
    Config[] configs;

    protected override void Start()
    {
        fixation = GetComponent<JointFixation>();
        fixation.Center = centerRoot;
        lead = GetComponentInParent<Lead>();
        wire = GetComponentInChildren<Wire>();
        wire.Diameter = lead.wire.Diameter * 0.5f;
        collider = GetComponentInChildren<BoxCollider>();
        SetConfig(configIndex);
        base.Start();
    }

    void SetConfig(int index)
    {
        if(configs.Length == 0)
        {
            return;
        }
        Config config = configs[index% configs.Length];
        for (int i = 0; i < wire.points.Count; i++)
        {
            float a = 1f * i / wire.points.Count;
            if (config.radius > 0)
            {
                float angle = 360f * a * config.lenght / (2f * Mathf.PI * config.radius);

                wire.points[i].localPosition = Quaternion.Euler(0, angle, 0) * Vector3.back * config.radius;
                if(i > 0)
                {
                    wire.points[i - 1].localRotation = Quaternion.LookRotation(wire.points[i].localPosition - wire.points[i-1].localPosition, Vector3.up);
                    if (i == wire.points.Count - 1)
                    {
                        wire.points[i].localRotation = Quaternion.LookRotation(wire.points[0].localPosition - wire.points[i].localPosition, Vector3.up); 
                    }
                }
                
            }
            else
            {
                wire.points[i].localPosition = Vector3.forward * (a - 0.5f) * config.lenght ;
                wire.points[i].localEulerAngles = Vector3.zero;
            }
        }
        
        if (config.radius > 0)
        {
            centerRoot.localPosition = Vector3.forward * (config.radius + lead.wire.DetailMeter *0.7f);
            collider.size = new Vector3(config.radius * 2f, wire.Diameter, config.radius * 2f);
        }
        else
        {
            centerRoot.localPosition = Vector3.forward * (config.lenght * 0.5f + lead.wire.DetailMeter * 0.7f);
            collider.size = new Vector3(wire.Diameter, wire.Diameter, config.lenght);
        }
    }

    protected override void Update()
    {
        base.Update();
        fixation.groupIndex = configIndex;
    }
}
