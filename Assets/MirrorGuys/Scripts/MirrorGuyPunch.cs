using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MirrorGuyPunch : MonoBehaviour
{
    public Transform target;
    private Collider[] overlaps = new Collider[100];

    public MirrorGuyRagdollRoot ragdollRoot { get; private set; }

    private void OnEnable()
    {
        ragdollRoot = GetComponentInParent<MirrorGuyRagdollRoot>();
    }

    void FixedUpdate()
    {
        if (ragdollRoot != null)
        {
            if (!ragdollRoot.isRagdoll)
            {
                if (target != null)
                {
                    transform.position = target.position;
                }
                // check radius for body parts
                var overlapCount = Physics.OverlapSphereNonAlloc(transform.position, transform.lossyScale.x, overlaps);
                if (overlapCount > 0)
                {
                    for (int i = 0; i < overlapCount; i++)
                    {
                        var bp = overlaps[i].GetComponent<MirrorGuyBodyPart>();
                        if (bp != null && bp.ragdollRoot != this.ragdollRoot)
                        {
                            // different person body part found.
                            bp.BeRagdolled(this);
                        }
                    }
                }

            }
        }
    }

    internal void Initialize(MirrorGuyLiveControl guy)
    {
        ragdollRoot = guy.ragdollRoot;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, transform.lossyScale.x);
    }
}