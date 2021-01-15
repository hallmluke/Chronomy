using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seed : TimeState, IInteractable
{
    public TimeState TreeState;
    public TimeState DecayState;
    public TreeGrowthTrigger trigger;
    public Transform setPointOnInteract; // Testing before rigidbody pickup

    public override TimeState GetNextTimeState()
    {
        if(trigger) {

            NextState = TreeState;
            TimeTravelObject parentTimeObject = GetComponentInParent<TimeTravelObject>();
            parentTimeObject.transform.position = trigger.treeOriginPoint.position;
            parentTimeObject.transform.rotation = trigger.treeOriginPoint.rotation;
            transform.position = parentTimeObject.transform.position;
            transform.rotation = parentTimeObject.transform.rotation;
            Debug.Log("In trigger on advance");

        } else {
            NextState = DecayState;
            Debug.Log("Not in trigger on advance");
        }

        return NextState;
    }

    public void Interact() {
        //transform.position = setPointOnInteract.position;
        
    }
}
