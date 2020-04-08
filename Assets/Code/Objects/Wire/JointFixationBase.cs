using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointFixationBase : Fixation
{
    public JointFixation jointFixation;
    private void Start()
    {
        jointFixation.AddFixation(this, jointFixation.transform);
    }
}
