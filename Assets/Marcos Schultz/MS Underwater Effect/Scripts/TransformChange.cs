using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TransformChange : MonoBehaviour {
    #if UNITY_EDITOR
	void Update () {
        if (transform.hasChanged) {
            GetComponent<WaterColor>().Validate();
        }
    }
    #endif
}
