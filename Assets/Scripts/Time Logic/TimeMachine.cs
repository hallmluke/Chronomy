using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeMachine : MonoBehaviour
{
    public List<TimeTravelObject> objects;
    bool IsFuture;

    void OnTriggerEnter() {
        if(!IsFuture) {
            IsFuture = true;
            foreach(TimeTravelObject obj in objects) {
                obj.SetToNextTimeState();
            }
        } else {
            IsFuture = false;
            foreach(TimeTravelObject obj in objects) {
                obj.SetPreviousTimeState();
            }
        }
    }
}
