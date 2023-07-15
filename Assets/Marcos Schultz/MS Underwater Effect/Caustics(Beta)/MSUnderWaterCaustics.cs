using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MSUnderWaterCaustics : MonoBehaviour {

    [Header("Water Height")][Tooltip("In this variable, the height of the water surface is defined. From that point on, the Caustics effect will be generated wherever the projector is pointing. The effect is generated from that point by pointing downwards.")]
    public float heightOfWaterSurface = 0;

    [Space(10)][Tooltip("Here it is possible to define which layers the projector will ignore ... In these layers, the Caustics effect is not generated.")]
    public LayerMask _ignoreLayers = 0;
    [Tooltip("Here it is possible to define the color that the 'caustics' effect will have.")]
    public Color causticsColor = new Color(0.8f, 0.8f, 0.8f, 0.4f);

    [Space(10)]
    [Range(1.0f, 1000.0f)][Tooltip("Here it is possible to define the size of the projector responsible for generating the caustics effect.")]
    public float projectorSize = 200;
    [Range(0.1f, 5.0f)][Tooltip("Here it is possible to define the size of the 'caustics' effect without the need to resize the projector.")]
    public float causticScale = 1;
    [Range(1, 200)][Tooltip("Here it is possible to define the depth of the water ... it is the height up to which the caustics effect will be generated.")]
    public float depth = 100;
    [Range(1, 5)][Tooltip("Here you can define the intensity of the effect.")]
    public float intensity = 2;
    [Range(0.0f, 1.0f)][Tooltip("Here it is possible to define the diffraction of the effect.")]
    public float diffraction = 0.0f;

    [Header("Resources")]
    public Material causticMaterial;

    Projector _projector;

    private void Validate() {
        //projector
        _projector = GetComponentInChildren<Projector>();
        if (_projector) {
            _projector.nearClipPlane = 0.01f;
            _projector.farClipPlane = depth;
            _projector.fieldOfView = 1;
            _projector.aspectRatio = 1;
            _projector.orthographic = true;
            _projector.orthographicSize = projectorSize;
            _projector.ignoreLayers = _ignoreLayers;
            if (causticMaterial) {
                _projector.material = causticMaterial;
            }
        }
        //material
        Material tempMat = GetComponentInChildren<Projector>().material;
        if (tempMat) {
            float scaleP = (200.0f / projectorSize);
            float scaleC = (100 / (causticScale * scaleP));
            tempMat.mainTextureScale = new Vector2(scaleC, scaleC);
            tempMat.color = causticsColor;
            heightOfWaterSurface = Mathf.Clamp(heightOfWaterSurface, -10000, 10000);
            tempMat.SetFloat("_Height", heightOfWaterSurface - 1.0f);
            tempMat.SetFloat("_EdgeBlend", 0.9f);
            tempMat.SetFloat("_DepthBlend", depth*30.0f);
            tempMat.SetFloat("_DepthFade", depth*0.10f);
            tempMat.SetFloat("_Multiply", intensity);
            tempMat.SetFloat("_Diffraction", diffraction);
        }
    }

    private void OnValidate() {
        Validate();
    } 

    void Start() {
        Validate();
    }
}
