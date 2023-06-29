using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasRenderer))]
public class AppleAutodisable : MonoBehaviour
{
    void Awake()
    {
#if UNITY_IOS
        gameObject.SetActive(false);
#endif
    }
}
