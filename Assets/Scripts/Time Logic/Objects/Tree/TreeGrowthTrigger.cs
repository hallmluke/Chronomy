using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TreeGrowthTrigger : MonoBehaviour
{
    public Transform treeOriginPoint;

    void OnTriggerEnter(Collider other) {
        Seed seedComp = other.GetComponentInParent<Seed>();
        if(seedComp) {
            seedComp.trigger = this;
        }
    }

    void OnTriggerExit(Collider other) {
        Seed seedComp = other.GetComponentInParent<Seed>();
        if(seedComp) {
            seedComp.trigger = null;
        }
    }
}
