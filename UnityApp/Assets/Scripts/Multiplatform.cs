using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Transform = UnityEngine.Transform;

public class Multiplatform : MonoBehaviour
{
    public List<Action> LateUpdateEvents = new List<Action>();
    public string Scene = "Scenes/MultiplaformScene";
    public MouseOrbitInfiniteRotateZoom Controls;
    public Action<MainApp> OnSceneLoaded;
    public GameObject SceneCamera;
    public GameObject RemoteSceneCamera;
    public GameObject RemoteUser;
    public MainApp App;

#if UNITY_ANDROID || UNITY_IOS
    private Planet currentTarget;
#endif

    public void LoadLevel(string levelName)
    {
        StartCoroutine(LoadSceneAsync(levelName));
    }

    IEnumerator LoadSceneAsync(string levelName)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            // float progress = Mathf.Clamp01(op.progress / .9f);
            yield return null;
        }
        App = GameObject.Find("MainApp").GetComponent<MainApp>();

#if UNITY_ANDROID || UNITY_IOS
        currentTarget = GameObject.Find("Earth").GetComponent<Planet>();
#else
        Controls.target = GameObject.Find("Earth").GetComponent<Transform>();
#endif
        OnSceneLoaded?.Invoke(App);
    }

    void Start()
    {
        if (GameObject.Find("MainApp") == null)
        {
            LoadLevel(Scene);
        }
    }

    public void DisableControls()
    {
        Controls?.Disable();
    }

    public void EnableControls()
    {
        Controls?.Enable();
    }

    public bool IsActive(UnityEngine.Transform t)
    {
#if UNITY_ANDROID || UNITY_IOS
        return currentTarget == t;
#else
        return Controls.target == t;
#endif
    }

    public bool HasTarget()
    {
#if UNITY_ANDROID || UNITY_IOS
        return currentTarget != null;
#else
        return Controls.target != null;
#endif
    }

    public void SetTarget(Planet t)
    {
#if UNITY_ANDROID || UNITY_IOS
        currentTarget = t;
#else
        if (!Controls) return;
        Controls.target = null;
        if(t != null)
        {
            Controls.target = t.transform;
            Controls.zoomMin = t.minZoom;
        }
#endif
    }

    public Planet Target()
    {
#if UNITY_ANDROID || UNITY_IOS
        return currentTarget;
#else
        return Controls.target.GetComponent<Planet>();
#endif
    }

}