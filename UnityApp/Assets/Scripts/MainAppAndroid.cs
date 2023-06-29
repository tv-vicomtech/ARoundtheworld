namespace AreteAR
{
#if UNITY_ANDROID || UNITY_IOS

    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;
    using OrkestraLib.Message;
    using static OrkestraLib.Orkestra;
    using UnityEngine.XR.ARSubsystems;

    [RequireComponent(typeof(ARRaycastManager))]
    public class MainAppAndroid : MonoBehaviour
    {
        private static readonly List<ARRaycastHit> rayHits = new List<ARRaycastHit>();
        private ARRaycastManager raycastManager;
        private ARPlaneManager planeManager;
        public Multiplatform platform;

        private readonly float touchSpeed = 6f;

        // Viewport for remote user
        public Camera removeUserCamera;

        // Rotation event send rate
        public float timeStepRotation = 0.2f;
        private float timeRotationElapsed;

        // Raycase framelimit
        private readonly float cooldownRaycast = 0.250f;
        private float timeRaycast = 0.0f;
        private bool PlaneVisible = true;

        public float timeDelayThreshold = 2.0f;

        void Awake()
        {
            raycastManager = GetComponent<ARRaycastManager>();
            planeManager = GetComponent<ARPlaneManager>();
        }

        void Start()
        {
            platform = GameObject.Find("Multiplatform").GetComponent<Multiplatform>();
            platform.OnSceneLoaded = (app) =>
            {
                planeManager.planesChanged += (e) =>
                {
                    foreach (ARPlane plane in e.added)
                    {
                        plane.gameObject.SetActive(PlaneVisible);
                    }
                };

                app.OnPlaceScene = () =>
                {
                    app.HideShareButton();
                    app.ResetPlacement();
                    PlaneVisibility(true);
                };

                app.OnLateUpdate = () =>
                {
                    timeRaycast += Time.deltaTime;

                    // if (app.UserInteractionTurn())
                    //     app.ShowShareButton();
                    // else
                    //     app.HideShareButton();

                    // Block all interactions until the player's turn 
                    if (timeRaycast > cooldownRaycast)
                    {
                        // We are touching
                        if (Input.touchCount > 0)
                        {
                            Touch touchZero = Input.GetTouch(0);
                            if (app.AwaitsPlacement()) // When the earth has not been placed -> logic to place the earth
                            {
                                app.ReferenceSystem.SetActive(false);

                                // Place scene
                                if (raycastManager.Raycast(touchZero.position, rayHits, TrackableType.PlaneWithinPolygon))
                                {
                                    timeRaycast = 0.0f;
                                    Pose firstHitPose = rayHits[0].pose;
                                    app.ReferenceSystem.transform.position = firstHitPose.position;
                                    app.ReferenceSystem.SetActive(true);
                                    app.OnPlacement();
                                    PlaneVisibility(false);
                                }
                            }
                            // With the earth placed, logic to control user action when playing
                            else if (app.isPlaying)
                            {
                                // Dragging while the pin picker disabled -> Earth rotation
                                if (!app.IsPickActive() && touchZero.phase == TouchPhase.Moved)
                                {

                                    Vector3 offset = touchZero.deltaPosition;
                                    float rotX = offset.x * touchSpeed * Mathf.Deg2Rad;
                                    float rotY = offset.y * touchSpeed * Mathf.Deg2Rad;
                                    if (platform.HasTarget())
                                    {
                                        platform.Target().transform.Rotate(Vector3.up, -rotX);
                                        //platform.Target().transform.Rotate(Vector3.right, rotY);
                                        timeRotationElapsed += Time.deltaTime;
                                        if (timeRotationElapsed >= timeStepRotation)
                                        {
                                            if (app.IsShared()) app.Dispatch(Channel.Application, new ObjectTransform(app.GetUsername(), platform.Target().gameObject));
                                            // Reset the time elapsed
                                            timeRotationElapsed = 0.0f;
                                        }
                                    }
                                }
                                // Touching with the pin picker enabled -> Pin placement
                                else if (app.IsPickActive() && touchZero.phase == TouchPhase.Began && !app.confirmAnswerWindow)
                                {
                                    Ray ray = Camera.main.ScreenPointToRay(touchZero.position);
                                    Debug.DrawRay(ray.origin, ray.direction);

                                    if (Physics.Raycast(ray, out RaycastHit hit))
                                    {
                                        if (hit.collider.gameObject)
                                        {
                                            Planet obj = hit.collider.gameObject.GetComponent<Planet>();
                                            if (obj)
                                            {
                                                // We want to place a marker
                                                CreateNewPin(hit);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };
            };
        }

        private void PlaneVisibility(bool visibility)
        {
            PlaneVisible = visibility;
            foreach (ARPlane plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(PlaneVisible);
            }
        }

        private bool TryGetTouchPosition(out Vector2 touchPosition)
        {
            if (Input.touchCount > 0)
            {
                touchPosition = Input.GetTouch(0).position;
                return true;
            }

            touchPosition = default;
            return false;
        }

        private void CreateNewPin(RaycastHit hit)
        {
            timeRaycast = 0.0f;
            platform.App.OnUserAnswerClick(hit);
        }
    }
#endif
}