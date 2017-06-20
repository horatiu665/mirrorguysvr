using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MirrorGuyPlayerOffset : MonoBehaviour
{
    MirrorGuyTrackedObject playerRight { get { return MirrorGuyTrackedObject.playerRight; } }
    MirrorGuyTrackedObject playerHead { get { return MirrorGuyTrackedObject.playerHead; } }
    ArenaFeedback arenaFeedback;
    Collider c;

    public static MirrorGuyPlayerOffset instance;
    private float arenaLookTime;
    private float arenaPressTime;

    public float selectLookTime = 0.2f;
    public float selectPressTime = 0.1f;

    void Awake()
    {
        instance = this;
        arenaFeedback = GetComponentInChildren<ArenaFeedback>();
        c = arenaFeedback.GetComponent<Collider>();
    }

    private void Update()
    {
        Vector3 lookDir = playerHead.transform.forward;
        RaycastHit info;
        // if looking at some obj
        if (Physics.Raycast(playerHead.transform.position, lookDir, out info, 1000f))
        {

            // if it's this collider
            if (info.collider == c)
            {
                // player is looking at us
                // better highlight.
                arenaLookTime += Time.deltaTime;
                if (arenaLookTime > selectLookTime)
                {
                    if (InputPressedCalibrate())
                    {
                        arenaPressTime += Time.deltaTime;
                        // feedback on arena start
                        arenaFeedback.DoFeedback();
                    }
                    else
                    {
                        arenaFeedback.DoPreFeedback();
                        arenaPressTime = 0f;
                    }
                }
                else
                {
                    arenaPressTime = 0;
                }
            }
            else
            {
                arenaLookTime = 0f;
                arenaPressTime = 0f;
            }
        }
        else
        {
            arenaLookTime = 0f;
            arenaPressTime = 0f;
        }

        if (arenaPressTime > selectPressTime)
        {
            // calibrate!
            Calibrate();
            arenaPressTime = 0f;
        }
    }

    private void Calibrate()
    {
        var pos = playerHead.transform.position;
        pos.y = playerHead.transform.root.position.y;
        transform.position = pos;

        transform.rotation = playerHead.transform.rotation;
        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
    }

    private bool InputPressedCalibrate()
    {
        return RightTouchpad();
    }

    private bool RightTouchpad()
    {
        return playerRight.controller.GetPress(ControllerWrapper.ButtonMask.Touchpad);
    }

}

