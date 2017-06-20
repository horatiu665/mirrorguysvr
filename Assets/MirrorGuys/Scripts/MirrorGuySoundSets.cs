using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class MirrorGuySoundSets : MonoBehaviour
{
    public List<MirrorGuySoundSet> soundSets;

}

[System.Serializable]
public struct MirrorGuySoundSet
{
    public AudioClip[] hello, victory, getHit, selection;

    public AudioClip GetSound(AudioClip[] clips)
    {
        return clips[Random.Range(0, clips.Length)];
    }
}
