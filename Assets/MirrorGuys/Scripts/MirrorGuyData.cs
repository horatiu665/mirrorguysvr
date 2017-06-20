using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public struct MirrorGuyData
{
    static MirrorGuyTrackedObject playerLeft { get { return MirrorGuyTrackedObject.playerLeft; } }
    static MirrorGuyTrackedObject playerRight { get { return MirrorGuyTrackedObject.playerRight; } }
    static MirrorGuyTrackedObject playerHead { get { return MirrorGuyTrackedObject.playerHead; } }
    static MirrorGuyTrackedObject playerLeftLeg { get { return MirrorGuyTrackedObject.playerLeftLeg; } }
    static MirrorGuyTrackedObject playerRightLeg { get { return MirrorGuyTrackedObject.playerRightLeg; } }
    static Transform playerOffset { get { return MirrorGuyPlayerOffset.instance.transform; } }

    [HideInInspector]
    public Vector3 pos1, pos2, pos3, leg1, leg2;
    [HideInInspector]
    public Quaternion rot1, rot2, rot3, reg1, reg2;

    public static MirrorGuyData GetFromPlayer()
    {
        var l1 = playerOffset.InverseTransformPoint(playerLeft.transform.position);
        var l2 = playerOffset.InverseTransformPoint(playerRight.transform.position);
        var l3 = playerOffset.InverseTransformPoint(playerHead.transform.position);
        var r1 = Quaternion.Inverse(playerOffset.rotation) * playerLeft.transform.rotation;
        var r2 = Quaternion.Inverse(playerOffset.rotation) * playerRight.transform.rotation;
        var r3 = Quaternion.Inverse(playerOffset.rotation) * playerHead.transform.rotation;

        Vector3 leg1 = Vector3.zero;
        Vector3 leg2 = Vector3.zero;
        Quaternion reg1 = Quaternion.identity;
        Quaternion reg2 = Quaternion.identity;

        if (playerLeftLeg.isValid)
        {
            leg1 = playerOffset.InverseTransformPoint(playerLeftLeg.transform.position);
            reg1 = Quaternion.Inverse(playerOffset.rotation) * playerLeftLeg.transform.rotation;
        }

        if (playerRightLeg.isValid)
        {
            leg2 = playerOffset.InverseTransformPoint(playerRightLeg.transform.position);
            reg2 = Quaternion.Inverse(playerOffset.rotation) * playerRightLeg.transform.rotation;
        }

        var data = new MirrorGuyData()
        {
            pos1 = l1,
            pos2 = l2,
            pos3 = l3,
            rot1 = r1,
            rot2 = r2,
            rot3 = r3,
            leg1 = leg1,
            leg2 = leg2,
            reg1 = reg1,
            reg2 = reg2
        };

        return data;
    }
}
