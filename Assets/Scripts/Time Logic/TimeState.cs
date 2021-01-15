using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeState : MonoBehaviour
{
    public TimeState PreviousState;
    public TimeState NextState;
    
    public virtual TimeState GetNextTimeState() {
        return NextState;
    }

    public virtual TimeState GetPreviousState() {
        return PreviousState;
    }
}
