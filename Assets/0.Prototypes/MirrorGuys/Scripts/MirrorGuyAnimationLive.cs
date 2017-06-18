using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MirrorGuyAnimationLive : MirrorGuyAnimationShare
{
    MirrorGuyTrackedObject playerLeft { get { return MirrorGuyTrackedObject.playerLeft; } }
    MirrorGuyTrackedObject playerRight { get { return MirrorGuyTrackedObject.playerRight; } }
    MirrorGuyTrackedObject playerHead { get { return MirrorGuyTrackedObject.playerHead; } }

    [Header("Overrides animation with the live tracking")]
    public Transform playerOffset;

    private void OnEnable()
    {
        data = new List<MirrorGuyLiveControl.MirrorGuyData>(1);
        data.Add(new MirrorGuyLiveControl.MirrorGuyData());
    }

    private void Update()
    {
        var l1 = playerOffset.InverseTransformPoint(playerLeft.transform.position);
        var l2 = playerOffset.InverseTransformPoint(playerRight.transform.position);
        var l3 = playerOffset.InverseTransformPoint(playerHead.transform.position);
        var r1 = Quaternion.Inverse(playerOffset.rotation) * playerLeft.transform.rotation;
        var r2 = Quaternion.Inverse(playerOffset.rotation) * playerRight.transform.rotation;
        var r3 = Quaternion.Inverse(playerOffset.rotation) * playerHead.transform.rotation;

        data[0] = new MirrorGuyLiveControl.MirrorGuyData()
        {
            pos1 = l1,
            pos2 = l2,
            pos3 = l3,
            rot1 = r1,
            rot2 = r2,
            rot3 = r3
        };
    }
}

