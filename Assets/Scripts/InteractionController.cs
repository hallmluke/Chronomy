using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    public float MaxInteractionDistance;
    public UICursor uiCursor; // TODO: Decouple with events
    private PlayerControls m_playerControls;
    Ray ray;
    RaycastHit hit;
    Vector3 ScreenCenter = new Vector3(0.5f, 0.5f, 0);
    LayerMask mask;

    void Awake() {
        mask = LayerMask.GetMask("Interactable");
        m_playerControls = new PlayerControls();
    }

    private void OnEnable() {
        m_playerControls.Enable();
    }

    private void OnDisable() {
        m_playerControls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        ray = Camera.main.ViewportPointToRay(ScreenCenter);

        if(Physics.Raycast(ray, out hit, MaxInteractionDistance, mask)) {
            Interactable interact = hit.transform.GetComponentInParent<Interactable>();
            if(interact) {
                uiCursor.UpdateCursorDot(true);
                if(GetInputInteract()) {
                    interact.Interact();
                }
            } else {
                uiCursor.UpdateCursorDot(false);
            }
        } else {
            uiCursor.UpdateCursorDot(false);
        }
    }

    protected bool GetInputInteract() {
        return m_playerControls.Walking.Interact.triggered;
    }
}
