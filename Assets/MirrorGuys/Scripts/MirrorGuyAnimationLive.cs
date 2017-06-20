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
    MirrorGuyTrackedObject playerLeftLeg { get { return MirrorGuyTrackedObject.playerLeftLeg; } }
    MirrorGuyTrackedObject playerRightLeg { get { return MirrorGuyTrackedObject.playerRightLeg; } }
    
    private void OnEnable()
    {
        data = new List<MirrorGuyData>(1);
        data.Add(new MirrorGuyData());
    }

    private void Update()
    {
        data[0] = MirrorGuyData.GetFromPlayer();
    }
}

