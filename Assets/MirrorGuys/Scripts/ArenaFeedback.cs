using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class ArenaFeedback : MonoBehaviour
{
    bool feedback;
    Renderer r;

    public float colorAlphaChangeSpeed = 0.5f;

    public float maxPreFeedback = 0.33f;

    bool preFeedback;

    private void Awake()
    {
        r = GetComponent<Renderer>();
    }

    public void DoFeedback()
    {
        ColorAlpha(Time.deltaTime * colorAlphaChangeSpeed);
        feedback = true;
    }

    private void ColorAlpha(float deltaAlpha, float max = 1f)
    {
        var c = r.material.color;
        c.a = Mathf.Clamp(c.a + deltaAlpha, 0f, max);
        r.material.color = c;
    }

    private void Update()
    {
        if (feedback)
        {
            feedback = false;
        }
        else if (preFeedback)
        {
            preFeedback = false;
        }
        else if (r.material.color.a > 0)
        {
            ColorAlpha(-Time.deltaTime * colorAlphaChangeSpeed);
        }
    }

    public void DoPreFeedback()
    {
        preFeedback = true;
        ColorAlpha(Time.deltaTime * colorAlphaChangeSpeed, maxPreFeedback);
    }
}
