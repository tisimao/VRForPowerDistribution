using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SwitchButton3D : MonoBehaviour ,IGrabable
{
    public enum Axis
    {
        X,Y,Z
    }

    public Axis axis = Axis.Y;

    public UnityAction<bool> onChanged;
    [SerializeField]
    bool state = false;

    [SerializeField]
    float onAngle, offAngle ;
    Vector3 lockPos;
    Quaternion onLocalRotation, offLocalRotation;
    float allAngleDistance;
    [Header("切换的临界程度"),SerializeField]
    float switchLimite = 0.5f;

    Rigidbody mRigidbody;

    bool State
    {
        get
        {
            return state;
        }
        set
        {
            if(state != value)
            {
                state = value;
                if(onChanged!=null)
                {
                    onChanged(state);
                }
            }
        }
    }

    HandScript currentHand = null;

    [SerializeField]
    Transform grabRoot;
    public bool IsGrabing
    {
        get
        {
            return currentHand != null;
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

    public bool IsGrabable => true;

    // Start is called before the first frame update
    void Start()
    {
        lockPos = transform.localPosition;
        switch(axis)
        {
            case Axis.X:
                onLocalRotation = Quaternion.Euler(onAngle, 0, 0);
                offLocalRotation = Quaternion.Euler(offAngle, 0, 0);
                break;
            case Axis.Y:
                onLocalRotation = Quaternion.Euler( 0, onAngle, 0);
                offLocalRotation = Quaternion.Euler( 0, offAngle, 0);
                break;
            case Axis.Z:
                onLocalRotation = Quaternion.Euler(0, 0, onAngle);
                offLocalRotation = Quaternion.Euler(0, 0, offAngle);
                break;
        }
        transform.localRotation = StateRotation();

        allAngleDistance = Quaternion.Angle(offLocalRotation, onLocalRotation);
        mRigidbody = GetComponent<Rigidbody>();
        mRigidbody.centerOfMass = Vector3.zero;
    }

    Quaternion StateRotation()
    {
        if (State)
        {
            return onLocalRotation;
        }
        else
        {
            return offLocalRotation;
        }
    }

    void LimiteRotation()
    {
        if (Mathf.Abs(transform.localEulerAngles.x) > 0 && axis != Axis.X)
        {
            transform.localRotation = Quaternion.Euler( -transform.localEulerAngles.x, 0, 0) * transform.localRotation;
        }
        if (Mathf.Abs(transform.localEulerAngles.y) > 0 && axis != Axis.Y)
        {
            transform.localRotation = Quaternion.Euler(0, -transform.localEulerAngles.y, 0) * transform.localRotation;
        }
        if (Mathf.Abs(transform.localEulerAngles.z) > 0 && axis != Axis.Z)
        {
            transform.localRotation = Quaternion.Euler(0, 0, -transform.localEulerAngles.z) * transform.localRotation;
        }
        switch (axis)
        {
            case Axis.X:
                transform.localEulerAngles = new Vector3(
                    CheckRange(transform.localEulerAngles.x,onAngle,offAngle),0,0
                    );
                break;
            case Axis.Y:
                transform.localEulerAngles = new Vector3(
                    0, CheckRange(transform.localEulerAngles.y, onAngle, offAngle), 0
                   );
                break;
            case Axis.Z:
                transform.localEulerAngles = new Vector3(
                    0, 0, CheckRange(transform.localEulerAngles.z, onAngle, offAngle)
                   );
                break;
        }
    }

    float CheckRange(float v, float on, float off)
    {
        float d = on - off;
        float dd = Mathf.DeltaAngle(off, on);
        if (dd == 0)
        {
            return on;
        }
        float dv = v - off;
        float ddv = Mathf.DeltaAngle(off, v);

        float a = 0;
        if (d * dd >= 0)            
        {
            if (dv * ddv >= 0)
            {
                a = ddv / dd;
            }
            else //反向说明是>180°范围
            {
                a = ddv / d;
            }
        }
        else //反向说明是>180°范围
        {
            if (dv * ddv >= 0)
            {
                a = ddv / d;
            }
            else //反向说明是>180°范围
            {
                a = dv / d;
            }
        }

        if (a >= 1)
        {
            return on;
        }
        else if(a <= 0)
        {
            return off;
        }
        else
        {
            return v;
        }
    }

    private void StateUpdate()
    {
        transform.localPosition = lockPos;

        LimiteRotation();

        float toOn = Quaternion.Angle(transform.localRotation, onLocalRotation);
        float toOff = Quaternion.Angle(transform.localRotation, offLocalRotation);

        if (!State && toOn < allAngleDistance * switchLimite)
        {
            State = true;
        }
        if (State && toOff < allAngleDistance * switchLimite)
        {
            State = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsGrabing)
        {
            StateUpdate();
            mRigidbody.Sleep();
            transform.localRotation =
                Quaternion.RotateTowards(transform.localRotation, StateRotation(), Mathf.DeltaAngle(offAngle,onAngle) * Time.deltaTime);
        }
        else
        {
            Vector3 dir1 = grabRoot.position - transform.position;
            Vector3 dir2 = dir1 + currentHand.Velocity() * Time.deltaTime ;
            transform.rotation = 
                Quaternion.FromToRotation(dir1, dir2) * transform.rotation;
            StateUpdate();

            currentHand.transform.rotation = Quaternion.FromToRotation(currentHand.GetGrabRoot().forward, grabRoot.forward) * currentHand.transform.rotation;
            currentHand.transform.position = grabRoot.position - currentHand.GetGrabRoot().position + currentHand.transform.position;
        }
    }
    

    public bool Hold(Collider handOn, HandScript hand)
    {
        if(IsGrabing)
        {
            return false;
        }
        currentHand = hand;
        return true;
    }

    public void Drop(HandScript hand)
    {
        if(currentHand == hand)
        {

            currentHand = null;
        }
    }

}
