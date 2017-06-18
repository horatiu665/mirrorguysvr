using Kaae;
using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MirrorGuyRagdollRoot : MonoBehaviour
{
    [Header("Punch params")]
    public float hitForce = 10f;


    Animator anim;
    FullBodyBipedIK ik;
    RagdollUtility ru;
    List<MirrorGuyBodyPart> bodyParts = new List<MirrorGuyBodyPart>();

    [SerializeField, ReadOnlyInInspector]
    private bool _isRagdoll;
    public bool isRagdoll
    {
        get
        {
            return _isRagdoll;
        }
        private set
        {
            _isRagdoll = value;
        }
    }

    void OnEnable()
    {
        anim = GetComponent<Animator>();
        bodyParts = GetComponentsInChildren<MirrorGuyBodyPart>().ToList();
        ik = GetComponent<FullBodyBipedIK>();
        ru = GetComponent<RagdollUtility>();
    }

    public void GotHit(MirrorGuyBodyPart bp, MirrorGuyPunch punch)
    {
        Ragdollify();

        bp.rb.AddForce((bp.rb.position - punch.transform.position) * hitForce);
    }

    [DebugButton]
    public void Ragdollify()
    {
        if (isRagdoll)
        {
            return;
        }
        isRagdoll = true;

        ru.EnableRagdoll();


    }

    [DebugButton]
    public void UnRagdollify()
    {
        if (!isRagdoll)
        {
            return;
        }
        isRagdoll = false;

        ru.DisableRagdoll();

    }

    void SetKinematic(bool active)
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].rb.isKinematic = active;
        }
    }
}