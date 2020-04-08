using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSourceAC : Lead
{
    [SerializeField]
    Electrocircuit electrocircuit;
    public float volts = 220f;
    public float phasePosition = 0;
    public float frequency = 50f;

    protected override void Start()
    {
        base.Start();
        electrocircuit = new Electrocircuit("Power AC " + gameObject.name, Joint_A);
        electrocircuit.AddElectricCurrent(new AC(frequency,phasePosition, volts));
        electrocircuit.Reset();
    }


    protected void Update()
    {
        electrocircuit.time += Time.deltaTime;
    }

}
