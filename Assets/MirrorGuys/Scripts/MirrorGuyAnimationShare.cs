using Kaae;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MirrorGuyAnimationShare : MonoBehaviour
{
    public List<MirrorGuyData> data;

    [DebugButton]
    void GenerateSeamlessFrames(int framesAtEnd)
    {
        var first = data.First();
        var last = data.Last();
        for (int i = 1; i <= framesAtEnd; i++)
        {
            var inBetween = i / (float)(framesAtEnd);
            var newData = new MirrorGuyData()
            {
                pos1 = Vector3.Lerp(last.pos1, first.pos1, inBetween),
                pos2 = Vector3.Lerp(last.pos2, first.pos2, inBetween),
                pos3 = Vector3.Lerp(last.pos3, first.pos3, inBetween),
                rot1 = Quaternion.Slerp(last.rot1, first.rot1, inBetween),
                rot2 = Quaternion.Slerp(last.rot2, first.rot2, inBetween),
                rot3 = Quaternion.Slerp(last.rot3, first.rot3, inBetween),
                leg1 = Vector3.Lerp(last.leg1, first.leg1, inBetween),
                leg2 = Vector3.Lerp(last.leg2, first.leg2, inBetween),
                reg1 = Quaternion.Slerp(last.reg1, first.reg1, inBetween),
                reg2 = Quaternion.Slerp(last.reg2, first.reg2, inBetween),
            };
            data.Add(newData);
        }
    }

    [DebugButton]
    void DeleteLast(int count)
    {
        data.RemoveRange(data.Count - count, count);
    }

    [DebugButton]
    void SetCurrentIdleAnimation()
    {
        FindObjectOfType<MirrorGuyLiveControl>().idleAnimation = this;
    }

}