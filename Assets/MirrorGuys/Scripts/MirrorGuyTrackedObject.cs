using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MirrorGuyTrackedObject : MonoBehaviour
{
    public enum Type
    {
        Head,
        Left,
        Right,
        LeftLeg,
        RightLeg
    }
    public Type type;
    public static List<MirrorGuyTrackedObject> objects = new List<MirrorGuyTrackedObject>();

    public static MirrorGuyTrackedObject playerLeft, playerRight, playerHead, playerLeftLeg, playerRightLeg;

    public ControllerWrapper controller { get; private set; }

    SteamVR_TrackedObject trackedObject;

    public bool isValid
    {
        get
        {
            if (type == Type.Head)
            {
                return true;
            }
            return trackedObject != null && trackedObject.isValid;
        }
    }

    private void Awake()
    {
        objects.Add(this);
        controller = GetComponent<ControllerWrapper>();
        trackedObject = GetComponent<SteamVR_TrackedObject>();
    }

    private void Start()
    {
        var fo = objects;
        for (int i = 0; i < fo.Count; i++)
        {
            switch (fo[i].type)
            {
            case Type.Head:
                playerHead = fo[i];
                break;
            case Type.Left:
                playerLeft = fo[i];
                break;
            case Type.Right:
                playerRight = fo[i];
                break;
            case Type.LeftLeg:
                playerLeftLeg = fo[i];
                break;
            case Type.RightLeg:
                playerRightLeg = fo[i];
                break;
            }
        }

    }
}