using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectCarry : MonoBehaviour
{

    SpringJoint sj;

    void Awake() {

        sj = GetComponent<SpringJoint>();

    }

    
    public void ConnectObject(Rigidbody connectedObj) {
        sj.connectedBody = connectedObj;
    }

    public void DisconnectObject() {
        sj.connectedBody = null;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(this.transform.position, .25f);
    }
}
