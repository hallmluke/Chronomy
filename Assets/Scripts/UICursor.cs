using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICursor : MonoBehaviour
{

    public Image CursorDot;
    public Color DefaultColor;
    public Color InteractableColor;
    
    // TODO Allow more information to be passed in
    public void UpdateCursorDot(bool interactable) {

        if(interactable) {
            CursorDot.color = InteractableColor;
        } else {
            CursorDot.color = DefaultColor;
        }
    }
}
