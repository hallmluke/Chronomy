using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTravelObject : MonoBehaviour
{
    public List<TimeState> TimeStates;
    public TimeState ActiveState;
    public TimeTravelObject PastObject;
    public TimeTravelObject FutureObject;

    public void SetTimeState(TimeState timeState) {
        ActiveState = timeState;

        foreach(TimeState state in TimeStates) {
            state.gameObject.SetActive(false);
        }

        ActiveState.gameObject.SetActive(true);
    }

    public void SetStateLinkedObject() {
        if(PastObject) {
            PastObject.SetTimeState(ActiveState);
        }
        if(FutureObject) {
            FutureObject.SetTimeState(ActiveState);
        }
    }

    public virtual void SetToNextTimeState() {
        TimeState next = ActiveState.GetNextTimeState();
        if(next) {
            SetTimeState(next);
        }
    }

    public virtual void SetPreviousTimeState() {
        TimeState previous = ActiveState.GetPreviousState();
        if(previous) {
            SetTimeState(previous);
        }
    }

}
