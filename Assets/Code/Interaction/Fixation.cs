using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fixation : MonoBehaviour,IFixation
{
    public int groupIndex = 0;
    [SerializeField]
    private Transform center;

    public string Name => gameObject.name;

    public Transform Center {
        set
        {
            center = value;
        }
        get
        {
            if(center == null)
            {
                return transform;
            }
            return center;
        }
    }

    Dictionary<Collider, int> colliders = new Dictionary<Collider, int>();
    Dictionary<Collider, IFixation> fixations = new Dictionary<Collider, IFixation>();

    private void Update()
    {
        foreach (Collider collider in colliders.Keys)
        {
            IFixation fixation = collider.GetComponentInParent<IFixation>();
            if (fixation != null)
            {
                if (GetFixationGroup() == fixation.GetFixationGroup())
                {
                    if(fixation.AddFixation(this, collider.transform))
                    {
                        if(!fixations.ContainsKey(collider))
                        {
                            fixations.Add(collider, fixation);
                        }
                    }
                }
            }
        }
    }

    public bool AddFixation(IFixation other, Transform node)
    {
        return false;
    }

    public int GetFixationGroup()
    {
        return groupIndex;
    }

    public Transform GetTransform(IFixation other)
    {
        return Center;
    }

    public void RemoveFixation(IFixation other)
    {
       
    }


    public bool Unmovable(IFixation other)
    {
        return true;
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
                if(fixations.ContainsKey(other))
                {
                    fixations[other].RemoveFixation(this);
                    fixations.Remove(other);
                }
                colliders.Remove(other);
            }
        }
    }

    public Vector3 GetTransformOffset(IFixation other)
    {
        return Vector3.zero;
    }

    public Quaternion GetTransformRotate(IFixation other)
    {
        return Quaternion.Euler(0,0,0);
    }
}
