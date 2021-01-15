using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact() {
        // Hardcode set linked object time state
        TimeTravelObject tObject = GetComponent<TimeTravelObject>();
        if(tObject) {
            tObject.SetStateLinkedObject();
        }
    }
}
