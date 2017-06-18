using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MirrorGuyTrophyManager : MonoBehaviour
{
    public static MirrorGuyTrophyManager inst;

    public List<GameObject> trophyPrefabs = new List<GameObject>();

    public GameObject curTrophy;
    public Transform curTroPos;

    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
        inst = this;
    }

    private void Start()
    {
        SetFreshTroophy();
    }

    public void SetFreshTroophy()
    {
        if (curTrophy != null)
        {
            Destroy(curTrophy);
        }

        var go = Instantiate(trophyPrefabs[Random.Range(0, trophyPrefabs.Count)]);
        go.transform.position = curTroPos.position;
        go.transform.rotation = curTroPos.rotation;
        go.transform.SetParent(curTroPos);
        //var rb = go.GetComponent<Rigidbody>();
        //rb.isKinematic = true;

        curTrophy = go;

        anim.SetTrigger("ShowTrophy");
    }


    public void WinTrophy(MirrorGuyLiveControl guy)
    {
        var hand = guy.ragdollHandRight.childCount == 0 ? guy.ragdollHandRight : (guy.ragdollHandLeft.childCount == 0 ? guy.ragdollHandLeft : Random.Range(0, 1f) > 0.5f ? guy.ragdollHandRight : guy.ragdollHandLeft);
        for (int i = 0; i < hand.childCount; i++)
        {
            Destroy(hand.GetChild(0).gameObject);
        }

        curTrophy.transform.SetParent(hand);
        curTrophy.transform.localPosition = Vector3.zero;
        curTrophy.transform.localRotation = Quaternion.identity;

        //curTrophy.GetComponent<Rigidbody>().isKinematic = false;
        var punches = curTrophy.GetComponentsInChildren<MirrorGuyPunch>();
        for (int i = 0; i < punches.Length; i++)
        {
            punches[i].Initialize(guy);
        }

        curTrophy = null;
    }

}