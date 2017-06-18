using Kaae;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class tempfix : MonoBehaviour
{

    [DebugButton]
    void doit()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var t = transform.GetChild(i);
            if (t.GetComponent<Renderer>())
            {
                var go = new GameObject(t.name);
                go.transform.SetParent(t);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                go.transform.SetParent(t.parent);
                go.transform.localRotation = Quaternion.identity;
                t.SetParent(go.transform);
                
            }
        }
    }

}