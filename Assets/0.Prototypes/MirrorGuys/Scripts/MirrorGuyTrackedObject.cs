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
        Right
    }
    public Type type;
    public static List<MirrorGuyTrackedObject> objects = new List<MirrorGuyTrackedObject>();

    public static MirrorGuyTrackedObject playerLeft, playerRight, playerHead;

    public ControllerWrapper controller { get; private set; }

    private void Awake()
    {
        objects.Add(this);
        controller = GetComponent<ControllerWrapper>();
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
            }
        }

    }
}