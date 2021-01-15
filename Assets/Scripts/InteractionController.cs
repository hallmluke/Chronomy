using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionController : MonoBehaviour
{
    public float MaxInteractionDistance;
    public UICursor uiCursor; // TODO: Decouple with events
    public ObjectCarry objectCarry;
    private PlayerControls m_playerControls;
    bool isCarryingObject;
    Ray ray;
    RaycastHit hit;
    Vector3 ScreenCenter = new Vector3(0.5f, 0.5f, 0);
    LayerMask mask;

    void Awake()
    {
        mask = LayerMask.GetMask("Interactable");
        m_playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        m_playerControls.Enable();
    }

    private void OnDisable()
    {
        m_playerControls.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isCarryingObject)
        {
            ray = Camera.main.ViewportPointToRay(ScreenCenter);

            if (Physics.Raycast(ray, out hit, MaxInteractionDistance, mask))
            {
                IInteractable interact = hit.transform.GetComponentInParent<IInteractable>();
                
                if (interact != null)
                {
                    Rigidbody rb = hit.transform.GetComponentInParent<Rigidbody>();
                    uiCursor.UpdateCursorDot(true);
                    if (GetInputInteract())
                    {
                        objectCarry.transform.position = rb.position;
                        objectCarry.ConnectObject(rb);
                        isCarryingObject = true;
                        //interact.Interact();
                    }
                }
                else
                {
                    uiCursor.UpdateCursorDot(false);
                }
            }
            else
            {
                uiCursor.UpdateCursorDot(false);
            }
        } else {
            if(GetInputInteract()) {
                isCarryingObject = false;
                objectCarry.DisconnectObject();
            }
        }
    }

    protected bool GetInputInteract()
    {
        return m_playerControls.Walking.Interact.triggered;
    }
}
