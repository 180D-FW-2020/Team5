using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDeleteComponent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Preserve this object throughout every scene
        DontDestroyOnLoad(transform.gameObject);
    }
}
