using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class BloodManager : MonoBehaviour
{
    public static BloodManager inst;
    void Awake()
    {
        inst = this;

    }
    Queue<GameObject> bloodPool = new Queue<GameObject>();

    public GameObject bloodPiecePrefab;

    [Header("PS settings")]
    public Vector2 count = new Vector2(7, 12);
    public Vector2 forceRandom = new Vector2(1, 3f);
    public Vector2 forceUp = new Vector2(3, 6f);
    public float lifetime = 1f;

    private void Start()
    {
        // prepare 100 blood
        var bod = new List<GameObject>(100);
        for (int i = 0; i < 100; i++)
        {
            bod.Add(GetBlood());
        }
        for (int i = 0; i < 100; i++)
        {
            ReturnBlood(bod[i]);
        }
    }

    public void Create(Vector3 pos)
    {
        for (int i = 0; i < Random.Range(count.x, count.y); i++)
        {
            var b = GetBlood();
            b.transform.position = pos;
            var rb = b.GetComponent<Rigidbody>();
            rb.AddForce(Random.Range(forceRandom.x, forceRandom.y) * Random.onUnitSphere + Random.Range(forceUp.x, forceUp.y) * Vector3.up, ForceMode.Impulse);
            StartCoroutine(pTween.Wait(lifetime, () =>
            {
                ReturnBlood(b);
            }));
        }
    }

    GameObject GetBlood()
    {
        if (bloodPool.Count > 0)
        {
            return bloodPool.Dequeue();
        }
        else
        {
            var b = Instantiate(bloodPiecePrefab);
            b.transform.SetParent(transform);
            return b;
        }
    }

    void ReturnBlood(GameObject blood)
    {
        blood.SetActive(false);
        bloodPool.Enqueue(blood);
    }
}