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

    MirrorGuyTrackedObject playerLeftLeg { get { return MirrorGuyTrackedObject.playerLeftLeg; } }
    MirrorGuyTrackedObject playerRightLeg { get { return MirrorGuyTrackedObject.playerRightLeg; } }

    Transform playerOffset
    {
        get
        {
            return MirrorGuyPlayerOffset.instance.transform;
        }
    }
    Animator anim;
    public Transform charLeftt, charRight, charHeadd, charLegLe, charLegRi;

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

        var data = MirrorGuyData.GetFromPlayer();

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

    private void SetVectors(MirrorGuyData data)
    {
        var pos1 = transform.TransformPoint(data.pos1);
        var pos2 = transform.TransformPoint(data.pos2);
        var pos3 = transform.TransformPoint(data.pos3);
        var rot1 = transform.rotation * data.rot1;
        var rot2 = transform.rotation * data.rot2;
        var rot3 = transform.rotation * data.rot3;

        Vector3 leg1 = transform.TransformPoint(data.leg1);
        Vector3 leg2 = transform.TransformPoint(data.leg2);
        Quaternion reg1 = transform.rotation * data.reg1;
        Quaternion reg2 = transform.rotation * data.reg2;

        bool leftLeg = data.leg1 != Vector3.zero;
        bool rightLeg = data.leg2 != Vector3.zero;

        if (_delay > 0)
        {
            StartCoroutine(pTween.Wait(_delay, () =>
            {
                charLeftt.position = pos1;
                charRight.position = pos2;
                charHeadd.position = pos3;
                charLeftt.rotation = rot1;
                charRight.rotation = rot2;
                charHeadd.rotation = rot3;
                if (leftLeg)
                {
                    SetLegActive(true, true);
                    charLegLe.position = leg1;
                    charLegLe.rotation = reg1;
                }
                else
                {
                    SetLegActive(true, false);
                }
                if (rightLeg)
                {
                    SetLegActive(false, true);
                    charLegRi.position = leg2;
                    charLegRi.rotation = reg2;
                }
                else
                {
                    SetLegActive(false, false);
                }
            }));
        }
        else
        {
            charLeftt.position = pos1;
            charRight.position = pos2;
            charHeadd.position = pos3;
            charLeftt.rotation = rot1;
            charRight.rotation = rot2;
            charHeadd.rotation = rot3;
            if (leftLeg)
            {
                SetLegActive(true, true);
                charLegLe.position = leg1;
                charLegLe.rotation = reg1;
            }
            else
            {
                SetLegActive(true, false);
            }
            if (rightLeg)
            {
                SetLegActive(false, true);
                charLegRi.position = leg2;
                charLegRi.rotation = reg2;
            }
            else
            {
                SetLegActive(false, false);
            }
        }
    }

    private void SetLegActive(bool left, bool active)
    {
        ragdollRoot.ik.solver.SetEffectorWeights(left ? RootMotion.FinalIK.FullBodyBipedEffector.LeftFoot : RootMotion.FinalIK.FullBodyBipedEffector.RightFoot,
            active ? 1f : 0f, 0f);
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
            SetLegActive(true, false);
            SetLegActive(false, false);

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
}