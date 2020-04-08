using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 手部会在物理帧更新到传感器的位置
/// 如果被抓取物需要修正手的显示位置
/// 在逻辑帧修正到显示位置
/// </summary>

[RequireComponent(typeof(Rigidbody))]
public class HandScript : MonoBehaviour,IFixation
{
    Transform trackingSpace;

    public enum HandState
    {
        Big,
        Small,
        Empty
    }

    [SerializeField]
    protected OVRInput.Controller m_controller;

    [SerializeField]
    Animator animator;

    [SerializeField]
    protected GameObject m_player;

    [SerializeField, Header("拳头碰撞")]
    Transform handRoot; bool colliderActive = false;
    HandState state = HandState.Empty;

    [SerializeField]
    TriggerReport big, small;
    [SerializeField]
    Transform bigRoot, smallRoot, bigForwardRoot, smallForwardRoot;
    Rigidbody mRigidbody;

    float finger0, finger1, finger2;
    IFixation fixation;
    IGrabable currentGrabable;
    public string Name
    {
        get
        {
            return gameObject.name;
        }
    }

    public Rigidbody Rigidbody
    {
        get
        {
            if(mRigidbody == null)
            {
                mRigidbody = GetComponent<Rigidbody>();
            }            
            return mRigidbody;
        }
    }

    public Animator Animator
    {
        get
        {
            return animator;
        }
    }

    public HandState State { get => state;
        set {
            if(state != value)
            {
                switch (value)
                {

                    case HandState.Empty:
                        if (currentGrabable != null)
                        {
                            currentGrabable.Drop(this);
                            currentGrabable = null;
                        }
                        
                        break;
                }
                state = value;
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        SetPlayerIgnoreCollision(gameObject,true);
        trackingSpace = OVRManager.instance.transform.Find("TrackingSpace");
    }

    private void FixedUpdate()
    {        
        Vector3 pos = trackingSpace.TransformPoint(OVRInput.GetLocalControllerPosition(m_controller));
        Quaternion quaternion = OVRInput.GetLocalControllerRotation(m_controller);
        switch(m_controller)
        {
            case OVRInput.Controller.LTouch:
            case OVRInput.Controller.LTrackedRemote:
            case OVRInput.Controller.LHand:
                quaternion = quaternion * Quaternion.Euler(0, 0, 90f);
                break;
            case OVRInput.Controller.RTouch:
            case OVRInput.Controller.RTrackedRemote:
            case OVRInput.Controller.RHand:
                quaternion = quaternion * Quaternion.Euler(0, 0, -90f) ;
                break;
        }

        Rigidbody.MovePosition(pos);
        Rigidbody.MoveRotation(trackingSpace.rotation * quaternion);
    }

    public Vector3 Velocity()
    {
        return trackingSpace.TransformDirection(OVRInput.GetLocalControllerVelocity(m_controller));
    }

    // Update is called once per frame
    void Update()
    {       
        UpdateState();
    }
    
    void UpdateState()
    {
        //大拇指按下
        float f0 = OVRInput.Get(OVRInput.Button.PrimaryShoulder, m_controller) ? 1f : 0;
        //食指按下
        float f1 = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, m_controller);
        //侧键按下
        float f2 = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller);
        switch (State)
        {
            case HandState.Empty:
                if (f1 > 0.5f && finger1 <= 0.5f)
                {
                    Collider collider;
                    IGrabable temp = GetTarget(small, out collider, GrabType.Small, GrabType.SmallForward);
                    if (temp != null)
                    {
                        State = HandState.Small;
                        currentGrabable = temp;
                        currentGrabable.Hold(collider, this);
                        break;
                    }
                }
                else if (f2 > 0.5f && finger2 <= 0.5f)
                {
                    Collider collider;
                    IGrabable temp = GetTarget(big, out collider, GrabType.Big, GrabType.BigForward);
                    if (temp != null)
                    {
                        State = HandState.Big;
                        currentGrabable = temp;
                        currentGrabable.Hold(collider, this);
                        break;
                    }
                }
                AcitveCollider(f1 > 0.8f && f2 > 0.8f);
                Animator.SetFloat("Pinch", finger1);
                Animator.SetFloat("Flex", finger2);
                break;
            case HandState.Small:
                AcitveCollider(false);
                if (f1 < 0.5f )
                {
                    State = HandState.Empty;
                }
                Animator.SetFloat("Pinch", finger1);
                Animator.SetFloat("Flex",  Mathf.Min(finger2,0.25f));
                break;
            case HandState.Big:
                AcitveCollider(false);
                if (f2 < 0.5f)
                {
                    State = HandState.Empty;
                }
                Animator.SetFloat("Pinch", 0.5f);
                Animator.SetFloat("Flex", finger2);
                break;

        }
        finger0 = f0;
        finger1 = f1;
        finger2 = f2;
    }

    void AcitveCollider(bool active)
    {
        if(colliderActive == active)
        {
            return;
        }
        foreach (Collider item in handRoot.GetComponentsInChildren<Collider>(true))
        {
            item.enabled = active;
        }
        colliderActive = active;
    }

    IGrabable GetTarget(TriggerReport report,  out Collider collider,params GrabType[] types)
    {
        collider = null;
        if (currentGrabable != null)
        {
            return null;
        }
        IGrabable grab = null;
        int count = -1;
        foreach (Collider item in report.Colliders.Keys)
        {
            IGrabable temp = item.GetComponentInParent<IGrabable>();
            if (temp != null && temp.IsGrabable)
            {
                int tempCount = report.Colliders[item];
                GrabType type = temp.GrabType;
                for (int i = 0; i < types.Length; i++)
                {
                    if(type == types[i])
                    {
                        if(tempCount > count)
                        {
                            collider = item;
                            grab = temp;
                            count = tempCount;
                            break;
                        }
                       
                    }
                }
            }
        }
        return grab;
    }


    public int GetFixationGroup()
    {
        return 0;
    }

    public Transform GetGrabRoot()
    {
        switch (State)
        {
            case HandState.Big:
            case HandState.Small:
                switch(currentGrabable.GrabType)
                {
                    case GrabType.Big:
                        return bigRoot;
                    case GrabType.BigForward:
                        return bigForwardRoot;
                    case GrabType.Small:
                        return smallRoot;
                    case GrabType.SmallForward:
                        return smallForwardRoot;
                }
                return transform;
        }
        return transform;
    }

    public Transform GetTransform(IFixation other)
    {
        return GetGrabRoot();
    }
    
    public bool AddFixation(IFixation other, Transform node)
    {
        return false;
    }

    public void RemoveFixation(IFixation other)
    {

    }

    public bool Unmovable(IFixation other)
    {
        return true;
    }


    protected void SetPlayerIgnoreCollision(GameObject grabbable, bool ignore)
    {
        if (m_player != null)
        {
            Collider[] playerColliders = m_player.GetComponentsInChildren<Collider>();
            foreach (Collider pc in playerColliders)
            {
                Collider[] colliders = grabbable.GetComponentsInChildren<Collider>();
                foreach (Collider c in colliders)
                {
                    Physics.IgnoreCollision(c, pc, ignore);
                }
            }
        }
    }

    public Vector3 GetTransformOffset(IFixation other)
    {
        return Vector3.zero;
    }

    public Quaternion GetTransformRotate(IFixation other)
    {
        return Quaternion.Euler(0, 0, 0);
    }
}
