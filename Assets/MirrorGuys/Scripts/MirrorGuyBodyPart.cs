using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MirrorGuyBodyPart : MonoBehaviour
{
    public MirrorGuyLiveControl liveControl { get; private set; }
    public MirrorGuyRagdollRoot ragdollRoot { get; private set; }
    public Rigidbody rb { get; private set; }

    AudioSource aud;

    public AudioClip[] ragdollclips;

    public Vector2 ragdollVelocityRange = new Vector2(2f, 10f);

    void OnEnable()
    {
        rb = GetComponent<Collider>().attachedRigidbody;
        ragdollRoot = GetComponentInParent<MirrorGuyRagdollRoot>();
        liveControl = ragdollRoot.GetComponent<MirrorGuyLiveControl>();
        aud = GetComponent<AudioSource>();
        if (aud == null)
        {
            aud = GetComponentInChildren<AudioSource>();
        }
    }

    public void BeRagdolled(MirrorGuyPunch punch)
    {
        if (aud != null)
        {
            if (ragdollRoot.isRagdoll)
            {
                if (ragdollclips.Length > 0)
                {
                    aud.clip = ragdollclips[Random.Range(0, ragdollclips.Length)];
                }
                aud.Play();
            }
        }

        if (!ragdollRoot.isRagdoll)
        {
            // play death sound
            liveControl.GetHitSound();
        }

        ragdollRoot.GotHit(this, punch);
        Particles((punch.transform.position + transform.position) * 0.5f);

    }

    private void Particles(Vector3 pos)
    {
        BloodManager.inst.Create(pos);
    }

    private void OnCollisionEnter(Collision c)
    {
        // graphics and pizzaz
        if (ragdollRoot.isRagdoll)
        {
            var hit = Mathf.InverseLerp(ragdollVelocityRange.x, ragdollVelocityRange.y, c.relativeVelocity.magnitude);
            aud.volume = hit;
            if (ragdollclips.Length > 0)
            {
                aud.clip = ragdollclips[Random.Range(0, ragdollclips.Length)];
            }
            if (hit > 0)
            {
                aud.Play();
            }
            if (hit > 0.5f)
            {
                Particles(c.contacts[0].point);

            }
        }
    }

}