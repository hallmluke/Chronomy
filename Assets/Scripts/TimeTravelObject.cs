using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeTravelObject : MonoBehaviour
{
    public int StateIndex;
    public List<GameObject> TimeStates;
    public TimeTravelObject PastObject;
    public TimeTravelObject FutureObject;

    public void SetTimeState(int stateIndex) {
        StateIndex = stateIndex;

        foreach(GameObject state in TimeStates) {
            state.SetActive(false);
        }

        TimeStates[stateIndex].SetActive(true);
    }

    public void SetStateLinkedObject() {
        if(PastObject) {
            PastObject.SetTimeState(StateIndex);
        }
        if(FutureObject) {
            FutureObject.SetTimeState(StateIndex);
        }
    }
}
