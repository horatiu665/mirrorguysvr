using Kaae;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using System;

public class MirrorGuyLiveControl : MonoBehaviour
{
    MirrorGuyTrackedObject playerLeft { get { return MirrorGuyTrackedObject.playerLeft; } }
    MirrorGuyTrackedObject playerRight { get { return MirrorGuyTrackedObject.playerRight; } }
    MirrorGuyTrackedObject playerHead { get { return MirrorGuyTrackedObject.playerHead; } }

    Transform playerOffset
    {
        get
        {
            return MirrorGuyPlayerOffset.instance.transform;
        }
    }
    Animator anim;
    public Transform charLeftt, charRight, charHeadd;

    public Transform ragdollHandLeft, ragdollHandRight;

    [SerializeField]
    private float _delay;

    public MirrorGuyAnimationShare idleAnimation;

    public MirrorGuyAnimationShare overrideAnimation;

    public MirrorGuyAnimationShare victoryDance;

    public new AudioSource audio;
    public MirrorGuySoundSet soundSet;

    public void VictorySound()
    {
        if (audio != null)
        {
            audio.clip = soundSet.GetSound(soundSet.victory);
            audio.Play();
        }
    }

    public void SelectionSound()
    {
        if (audio != null)
        {
            audio.clip = soundSet.GetSound(soundSet.selection);
            audio.Play();
        }
    }

    public void GetHitSound()
    {
        if (audio != null)
        {
            audio.clip = soundSet.GetSound(soundSet.getHit);
            audio.Play();
        }
    }

    public void HelloSound()
    {
        if (audio != null)
        {
            audio.clip = soundSet.GetSound(soundSet.hello);
            audio.Play();
        }
    }


    List<MirrorGuyData> idleAnim
    {
        get
        {
            return idleAnimation.data;
        }
        set
        {
            idleAnimation.data = value;
        }
    }

    [HideInInspector]
    [SerializeField]
    new List<MirrorGuyData> animation = new List<MirrorGuyData>();
    bool playingAnim;

    public bool saveIdle;

    public enum States
    {
        Idle,
        LivePreview,
        RecordFight,
        PlaybackFight
    }

    public States state = States.Idle;
    private States prevFrameState;

    [Serializable]
    public struct changestatestructinspector
    {
        public bool idle, livePreview, recordFight, playbackFight;
    }
    public changestatestructinspector debugChangeState;

    public bool canUseViveInputsToRecord = false;

    float curAnimationFrame = 0;
    public float animFramesPerSecond = 90;
    float nextAnimFrame = 0;
    float lastAnimFrameUpdate = 0;

    [Header("Walk anim params")]
    public float walkAnimLerpTime = 0.5f;
    public float distToDestination = 1f;
    public float maxWalkSpeed = 0.5f;

    public bool walking { get; private set; }
    public MirrorGuyRagdollRoot ragdollRoot { get; private set; }
    public bool isRagdoll
    {
        get
        {
            return ragdollRoot.isRagdoll;
        }
    }

    public Vector3 startPosition { get; private set; }

    public Transform tar;

    public bool animationRecordingMode = false;

    public void Initialize()
    {
        startPosition = transform.position;
    }

    public void SetMaterial(Material mat)
    {
        GetComponentInChildren<Renderer>().material = mat;
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
        ragdollRoot = GetComponent<MirrorGuyRagdollRoot>();
        Initialize();
    }

    private void Start()
    {
        HelloSound();
    }

    private void Update()
    {
        if (tar != null)
        {
            WalkTo(tar.position, maxWalkSpeed);
        }

        // change state on inspector
        ChangeStateInspector();

        // editor interface
        if (saveIdle)
        {
            idleAnim = new List<MirrorGuyData>(animation);
            saveIdle = false;
        }

        if (canUseViveInputsToRecord)
            HandleGameInputs();

        if (animationRecordingMode)
        {
            HandleAnimRecordInputs();
        }

        // state changes
        if (prevFrameState != state)
        {
            StateChange(prevFrameState, state);
            prevFrameState = state;
        }

        // update vive ctrl positions
        var l1 = playerOffset.InverseTransformPoint(playerLeft.transform.position);
        var l2 = playerOffset.InverseTransformPoint(playerRight.transform.position);
        var l3 = playerOffset.InverseTransformPoint(playerHead.transform.position);
        var r1 = Quaternion.Inverse(playerOffset.rotation) * playerLeft.transform.rotation;
        var r2 = Quaternion.Inverse(playerOffset.rotation) * playerRight.transform.rotation;
        var r3 = Quaternion.Inverse(playerOffset.rotation) * playerHead.transform.rotation;
        var data = new MirrorGuyData()
        {
            pos1 = l1,
            pos2 = l2,
            pos3 = l3,
            rot1 = r1,
            rot2 = r2,
            rot3 = r3
        };

        StateUpdate(state, data);

    }

    public void PlayAnimOnce(MirrorGuyAnimationShare anim)
    {
        overrideAnimation = anim;
        var oldIdleFrame = curAnimationFrame;
        curAnimationFrame = 0;
        StartCoroutine(pTween.WaitFrames(overrideAnimation.data.Count, () =>
        {
            overrideAnimation = null;
            curAnimationFrame = oldIdleFrame;
        }));
    }

    public void PlayAnim(MirrorGuyAnimationShare anim)
    {
        overrideAnimation = anim;
        curAnimationFrame = 0;
    }

    public void PlayDefaultIdleAnim()
    {
        overrideAnimation = null;
    }

    private void ChangeStateInspector()
    {
        if (debugChangeState.idle == true)
        {
            debugChangeState.idle = false;
            state = States.Idle;
        }
        if (debugChangeState.livePreview == true)
        {
            debugChangeState.livePreview = false;
            state = States.LivePreview;
        }
        if (debugChangeState.recordFight == true)
        {
            debugChangeState.recordFight = false;
            state = States.RecordFight;
        }
        if (debugChangeState.playbackFight == true)
        {
            debugChangeState.playbackFight = false;
            state = States.PlaybackFight;
        }
    }

    private void StateUpdate(States state, MirrorGuyData data)
    {
        switch (state)
        {

        case States.Idle:
            // play idle animation

            curAnimationFrame += ((Time.time - lastAnimFrameUpdate) / (1f / animFramesPerSecond));
            curAnimationFrame = (curAnimationFrame) % (overrideAnimation != null ? overrideAnimation.data.Count : idleAnim.Count);
            lastAnimFrameUpdate = Time.time;

            if (overrideAnimation != null)
            {
                SetVectors(overrideAnimation.data[(int)curAnimationFrame]);
            }
            else
            {
                SetVectors(idleAnim[(int)curAnimationFrame]);
            }
            break;
        case States.LivePreview:
            SetVectors(data);
            break;
        case States.RecordFight:
            SetVectors(data);
            animation.Add(data);
            break;
        case States.PlaybackFight:
            // play fight animation

            curAnimationFrame += ((Time.time - lastAnimFrameUpdate) / (1f / animFramesPerSecond));
            curAnimationFrame = (curAnimationFrame) % animation.Count;
            lastAnimFrameUpdate = Time.time;

            SetVectors(animation[(int)curAnimationFrame]);
            break;
        }
    }

    private void StateChange(States prevState, States state)
    {
        switch (state)
        {
        case States.Idle:
            curAnimationFrame = 0;
            break;
        case States.LivePreview:
            break;
        case States.RecordFight:
            animation.Clear();
            break;
        case States.PlaybackFight:
            curAnimationFrame = 0;
            break;
        default:
            break;
        }
    }

    private void HandleGameInputs()
    {
        if (InputDownTouchpadRight())
        {
            if (state != States.RecordFight)
            {
                state = States.RecordFight;
            }
            else if (state == States.RecordFight)
            {
                state = States.PlaybackFight;
            }
        }

    }

    private void HandleAnimRecordInputs()
    {
        // toggle record or showcase
        if (InputDownTouchpadRight())
        {
            if (state != States.RecordFight)
            {
                state = States.RecordFight;
            }
            else if (state == States.RecordFight)
            {
                state = States.PlaybackFight;
            }
        }

        // save anim to a new share
        if (InputDownTouchpadLeft())
        {
            if (animation.Count > 0)
            {
                var parent = idleAnimation.transform.parent;
                var newAnim = new GameObject("Anim " + Time.time.ToString("F0"), typeof(MirrorGuyAnimationShare)).GetComponent<MirrorGuyAnimationShare>();
                newAnim.transform.SetParent(parent);
                newAnim.data = new List<MirrorGuyData>(animation);
                animation.Clear();
                state = States.LivePreview;
            }
        }
    }


    private bool InputDownTouchpadLeft()
    {
        return playerLeft.controller.GetPressDown(ControllerWrapper.ButtonMask.Touchpad);
    }

    private bool InputDownTouchpadRight()
    {
        return playerRight.controller.GetPressDown(ControllerWrapper.ButtonMask.Touchpad);
    }

    private void SetVectors(MirrorGuyData a)
    {
        SetVectors(a.pos1, a.pos2, a.pos3, a.rot1, a.rot2, a.rot3);
    }

    private void SetVectors(Vector3 l1, Vector3 l2, Vector3 l3, Quaternion r1, Quaternion r2, Quaternion r3)
    {
        var posx1 = transform.TransformPoint(l1);
        var posx2 = transform.TransformPoint(l2);
        var posx3 = transform.TransformPoint(l3);
        var rotx1 = transform.rotation * r1;
        var rotx2 = transform.rotation * r2;
        var rotx3 = transform.rotation * r3;

        if (_delay > 0)
        {
            StartCoroutine(pTween.Wait(_delay, () =>
            {
                charLeftt.position = posx1;
                charRight.position = posx2;
                charHeadd.position = posx3;
                charLeftt.rotation = rotx1;
                charRight.rotation = rotx2;
                charHeadd.rotation = rotx3;
            }));
        }
        else
        {
            charLeftt.position = posx1;
            charRight.position = posx2;
            charHeadd.position = posx3;
            charLeftt.rotation = rotx1;
            charRight.rotation = rotx2;
            charHeadd.rotation = rotx3;
        }
    }

    public void WalkTo(Vector3 worldPos, float maxSpeed, bool walkBackwards = false, Action callback = null)
    {
        if (walking)
        {
            return;
        }
        walking = true;

        var dist = transform.position - worldPos;
        worldPos.y = transform.position.y;
        var t = 0f;
        StartCoroutine(pTween.WaitCondition(() =>
        {
            anim.SetFloat("MoveSpeed", t * (walkBackwards ? -1 : 1));

            if (t < maxSpeed)
            {
                t += Time.deltaTime / walkAnimLerpTime;
            }

            var lookAt = walkBackwards ? (transform.position - (worldPos - transform.position)) : worldPos;
            transform.LookAt(lookAt);

            dist = transform.position - worldPos;

            return dist.sqrMagnitude < distToDestination * distToDestination;
        },
        () =>
        {
            anim.SetFloat("MoveSpeed", 0f);
            walking = false;
            transform.position = worldPos;
            if (callback != null)
            {
                callback();
            }
        }));

    }

    [Serializable]
    public struct MirrorGuyData
    {
        [HideInInspector]
        public Vector3 pos1, pos2, pos3;
        [HideInInspector]
        public Quaternion rot1, rot2, rot3;
    }

}