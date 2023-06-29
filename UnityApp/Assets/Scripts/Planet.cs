using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Planet : MonoBehaviour
{
    public float distanceSun = 1f;
    public float radius = 1f;
    public float minZoom = 1f;
    public MainApp app;

    void Start()
    {
        transform.localPosition = new Vector3(distanceSun, 0, 0);
        transform.localScale = new Vector3(radius, radius, radius);
    }

    // #if UNITY_DESKTOP
    //     void OnMouseDown()
    //     {
    //         app.Activate(this);
    //     }

    // void Update()
    // {
    //     RaycastHit hit = new RaycastHit();
    //     for (int i = 0; i < Input.touchCount; ++i)
    //     {
    //         if (Input.GetTouch(i).phase.Equals(TouchPhase.Began))
    //         {
    //             Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(i).position);
    //             if (Physics.Raycast(ray, out hit))
    //                 hit.transform.gameObject.SendMessage("OnMouseDown");
    //         }
    //     }
    // }
    // #endif
}
