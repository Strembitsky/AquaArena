using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterColor : MonoBehaviour {

    public Color SurfaceColor = new Color(0.55f, 0.55f, 0.55f, 0.44f);
    public Color UnderColor = new Color(0.55f, 0.55f, 0.45f, 0.56f);
    [Range(0.1f, 10.0f)]
    public float textureTilling = 1;

    [Header("Resources")]
    public MeshRenderer surface;
    public MeshRenderer under;

    Material matSurf;
    Material matUnd;
    Vector2 offset1, offset2;

    private void OnValidate() {
        Validate();
    }

    public void Validate() {
        Vector2 scaleObj = new Vector2(textureTilling * transform.localScale.x, textureTilling * transform.localScale.z);
        Vector2 textureTillingVec = new Vector2(Mathf.Clamp(scaleObj.x, 0.001f, 1000.0f), Mathf.Clamp(scaleObj.y, 0.001f, 1000.0f));
        if (surface) {
            if (surface.sharedMaterial) {
                surface.sharedMaterial.SetTextureScale("_MainTex", textureTillingVec);
                surface.sharedMaterial.SetTextureScale("_DetailAlbedoMap", textureTillingVec);
            }
        }
        if (under) {
            if (under.sharedMaterial) {
                under.sharedMaterial.SetTextureScale("_MainTex", textureTillingVec);
                under.sharedMaterial.SetTextureScale("_DetailAlbedoMap", textureTillingVec);
            }
        }
    }

    private void Awake () {
        if (surface) {
            matSurf = new Material(surface.material);
            matSurf.color = SurfaceColor;
            surface.material = matSurf;
        }
        if (under) {
            matUnd = new Material(under.material);
            matUnd.color = UnderColor;
            under.material = matUnd;
        }
        Validate();
    }

    private void Update() {
        offset1 = new Vector2(offset1.x + Time.deltaTime * 0.01f, offset1.y + Time.deltaTime * 0.02f);
        offset2 = new Vector2(offset2.x - Time.deltaTime * 0.02f, offset2.y - Time.deltaTime * 0.015f);
        if (surface) {
            surface.material.SetTextureOffset("_MainTex", offset1);
            surface.material.SetTextureOffset("_DetailAlbedoMap", offset2);
        }
        if (under) {
            under.material.SetTextureOffset("_MainTex", offset1);
            under.material.SetTextureOffset("_DetailAlbedoMap", offset2);
        }
    }
}
