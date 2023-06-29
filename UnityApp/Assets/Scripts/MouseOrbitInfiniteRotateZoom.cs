using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[AddComponentMenu("Infinite Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbitInfiniteRotateZoom : MonoBehaviour
{
    public float xSpeed = 12.0f;
    public float ySpeed = 12.0f;
    public float scrollSpeed = 10.0f;
    public float zoomMin = 0.1f;
    public float zoomMax = 50.0f;

    private bool disabled = false;
    private float x = 0.0f;
    private float y = 0.0f;

    public Transform target;
    public float distance;
    public Vector3 position;
    public bool isActivated;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    /// <summary>Returns 'true' if we touched or hovering on Unity UI element.</summary>
    public static bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }

    /// <summary>Returns 'true' if we touched or hovering on Unity UI element.</summary>
    public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }

    /// <summary>Gets all event systen raycast results of current mouse or touch position.</summary>
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        if (EventSystem.current != null)
            EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }

    void LateUpdate()
    {
        if (disabled) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            isActivated = true;
        }

        // Stop rotating camera on mouse button UP
        if (Input.GetMouseButtonUp(0))
        {
            isActivated = false;
        }

        if (IsPointerOverUIElement()) return;

        if (target && isActivated)
        {
            // Compute the distance moved in the respective direction
            x += Input.GetAxis("Mouse X") * xSpeed;
            y -= Input.GetAxis("Mouse Y") * ySpeed;

            // Center on target
            transform.LookAt(target);

            // when mouse moves left and right we actually rotate around local y axis	
            transform.RotateAround(target.position, transform.up, x);

            // when mouse moves up and down we actually rotate around the local x axis	
            transform.RotateAround(target.position, transform.right, y);

            // reset back to 0 so it doesn't continue to rotate while holding the button
            x = 0;
            y = 0;
        }
        else
        {
            // See if mouse wheel was used
            float wheelRotation = Input.GetAxis("Mouse ScrollWheel");

            if (wheelRotation != 0)
            {
                if(target)
                {
                    //compute zoomMin
                    zoomMin = target.localScale.x * 0.6f;

                    //compute possible position to move
                    position = transform.position + Mathf.Clamp(wheelRotation, -1.0f, 1.0f) * transform.forward;

                    //Check if its valid
                    if(ZoomLimit(Vector3.Distance(position, target.position)))
                        transform.position = position;
                }
                else //if we dont have a target free zoom
                {
                    transform.position += Mathf.Clamp(wheelRotation, -1.0f, 1.0f) * transform.forward;
                }
            }
        }
    }

    public bool ZoomLimit(float dist)
    {
        return (zoomMin < dist && dist < zoomMax) ? true : false;      
    }

    public void Disable()
    {
        enabled = false;
        disabled = true;
    }

    public void Enable()
    {
        enabled = true;
        disabled = false;
    }
}