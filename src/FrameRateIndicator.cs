using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FrameRateIndicator : MonoBehaviour
{
    private float deltaTime;
    private TextMeshPro tm;

    private void Start()
    {
        tm = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        string fpsString = Mathf.Ceil(fps).ToString();
        tm.text = fpsString;
    }
}
