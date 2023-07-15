using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class MSWaterClass{
    [Tooltip("This property will only be used if the 'Collision_Name' detection option is selected. The name of the object that contains the collider(trigger), responsible for defining the water area.")]
    public string waterName = "Water";
    [Tooltip("This property will only be used if the 'Collision_Tag' detection option is selected. The tag of the object that contains the collider(trigger), responsible for defining the water area")]
    public string waterTag = "Respawn";
    [Tooltip("This property will only be used if the 'Height Of Water Surface' detection option is selected. Here it is possible to define the height of the water surface, where the effect will be activated or deactivated. This option supports only 1 water object per scene, and the selected water will be the index configured in the variable 'Start Water ID'.")]
    public float waterHeight = 0;
    [HideInInspector]
    public LayerMask waterLayer = 16;
    [Tooltip("In this variable it is possible to define the distance that the raycast will travel up and down from the Player, looking for a water collider that is in the water layer.")]
    public float rayCastDistance = 10;

    [Space(10, order = 8)]
    [Header("Auto ID", order=9)][Tooltip("This variable is set automatically, and represents the ID of this water setting.")]
    public int automaticWaterID = 0;

    [Space(15, order = 10)] 
    [Tooltip("The color of the water below the surface")]
	public Color waterColor = new Color32 (15, 150, 125, 0);
	[Range(0.01f,0.7f)][Tooltip("How much the vortex effect will distort the screen image.")]
	public float vortexDistortion = 0.45f;
	[Range(0.01f,0.7f)][Tooltip("The size of the distortion that the image will receive under the water because of the 'fisheye' effect.")]
	public float fisheyeDistortion = 0.3f;
	[Range(0.01f,0.3f)][Tooltip("The speed of the distortion that the image will receive under the water")]
	public float distortionSpeed = 0.2f;
	[Range(0.1f, 0.9f)][Tooltip("The intensity of the color of the water below the surface")]
	public float colorIntensity = 0.3f;
	[Range(0.1f,15)][Tooltip("The visibility underwater")]
	public float visibility = 7;
    [Tooltip("If this variable is true, the 'blur' effect will be disabled completely, and the 'visibility' variable will have no effect.")]
    public bool disableBlur = false;
}

[RequireComponent(typeof(AudioSource))]
public class MSUnderWaterEffect : MonoBehaviour{

    public enum StartMode {
        inTheWater, outOfTheWater
    }
    [Header("Start Mode")][Tooltip("Here you can define whether the player will start in or out of the water. If it starts in water, the effect already starts active ... otherwise, the effect starts inactive.")]
    public StartMode _start = StartMode.outOfTheWater;
    [Tooltip("If 'Start' is set to 'InTheWater', here you can define which water will be selected as the effect to be activated.")]
    public int startWaterID = 0;
    [Tooltip("This variable only takes effect if the water is being detected using Ray Cast Hit. If this variable is true, things will no longer collide with objects in the 'Water' layer. But Raycast will still be able to detect the object.")]
    public bool ignoreCollisionOnWaterLayer = true;


    public enum WaterDetectionMode {
        Collision_Tag, Collision_Name, RayCastHit_Tag, RayCastHit_Name, HeightOfWaterSurface
    };
    [Header("Waters")][Tooltip("Here it is possible to choose how the water will be detected, by collision (OnTriggerEnter), RayCast Hit or height of the water surface.")]
    public WaterDetectionMode detectionMode = WaterDetectionMode.Collision_Tag;
	[Tooltip("Here you must configure all the types of water you have in your game, according to the tag of each object.")]
	public MSWaterClass[] waters;


	[Space(5)][Header("Water Drops")]
	[Tooltip("If this variable is true, the water droplets on the screen will not appear.")]
	public bool disableDropsOnScreen = false;
	[Tooltip("The texture that will give the effect of drops on the screen")]
	public Texture waterDropsTexture;

    [Space(5)][Header("Sounds")]
    [Range(0, 1)][Tooltip("In this variable, it is possible to define the volume that the sounds reproduced by this code will have.")]
    public float audioSourceVolume = 1;
    [Tooltip("The sound that will be played when the player enters the water")]
	public AudioClip soundToEnter;
	[Tooltip("The sound that will be played when the player exits the water")]
	public AudioClip soundToExit;
	[Tooltip("The sound that will be played while the player is underwater")]
	public AudioClip underWaterSound;

	[Space(5)][Header("Resources")]
	[Tooltip("Shader 'SrBlur' must be associated with this variable")]
	public Shader SrBlur;
	[Tooltip("Shader 'SrEdge' must be associated with this variable")]
	public Shader SrEdge;
	[Tooltip("Shader 'SrFisheye' must be associated with this variable")]
	public Shader SrFisheye;
	[Tooltip("Shader 'SrVortex' must be associated with this variable")]
	public Shader SrVortex;
	[Tooltip("Shader 'SrQuad' must be associated with this variable")]
	public Shader SrQuad;

    bool _underWater;
    public bool underWater {
        get {
            return _underWater;
        }
    }

	bool cameOutOfTheWater;
	bool enableQuadDrops;
	float timerDrops;
	GameObject quadDrops;
	Renderer quadDropsRenderer;

	int waterIndex = 0;

	int interactions = 3;
	float strengthX = 0.00f;
	float strengthY = 0.00f;
	float blurSpread = 0.6f;
	float angleVortex = 0;
	float edgesOnly = 0.0f;
    bool disableBlur = false;

	Color edgesOnlyBgColor = Color.white;
	Vector2 centerVortex = new Vector2(0.5f, 0.5f);
	Material materialBlur = null;
	Material edgeDetectMaterial = null;
	Material fisheyeMaterial = null;
	Material materialVortex = null;
	AudioSource audioSourceCamera;
	GameObject audioSourceUnderWater;
	Camera cameraComponent;
	bool error;

    private void OnValidate(){
        if (waters != null) {
            if (waters.Length > 0) {
                Color compareColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                for (int x = 0; x < waters.Length; x++) {
                    if (waters[x].rayCastDistance == 0) {
                        waters[x].rayCastDistance = 15;
                    }
                    waters[x].rayCastDistance = Mathf.Clamp(waters[x].rayCastDistance, 1, 1000);
                    //
                    waters[x].automaticWaterID = x;
                    if (string.IsNullOrEmpty(waters[x].waterTag)) {
                        waters[x].waterTag = "Respawn";
                    }
                    if (string.IsNullOrEmpty(waters[x].waterName)) { //If the string is null, it means that the array has just been created.
                        waters[x].waterName = "WaterName";

                        // new array - set waters layer
                        if (waters[x].waterLayer == 0) {
                            waters[x].waterLayer = 16;
                        }
                    }
                    if (waters[x].waterColor == compareColor) {
                        waters[x].waterColor = new Color(0.05f, 0.5f, 0.5f, 0.0f);
                    }
                    if (waters[x].vortexDistortion == 0) {
                        waters[x].vortexDistortion = 0.45f;
                    }
                    if (waters[x].fisheyeDistortion == 0) {
                        waters[x].fisheyeDistortion = 0.3f;
                    }
                    if (waters[x].distortionSpeed == 0) {
                        waters[x].distortionSpeed = 0.2f;
                    }
                    if (waters[x].colorIntensity == 0) {
                        waters[x].colorIntensity = 0.4f;
                    }
                    if (waters[x].visibility == 0) {
                        waters[x].visibility = 7;
                    }
                }
                //
                startWaterID = Mathf.Clamp(startWaterID, 0, (waters.Length - 1));
            } else {
                startWaterID = 0;
            }
        } 
        else {
            startWaterID = 0;
        }

        OnValidateInternal();
    }

    private void OnValidateInternal() {
        //Add required componnents
        if (detectionMode == WaterDetectionMode.Collision_Name || detectionMode == WaterDetectionMode.Collision_Tag) {
            Rigidbody rbTemp = GetComponent<Rigidbody>();
            if (!rbTemp) {
                this.gameObject.AddComponent<Rigidbody>();
            }
            SphereCollider sphereTemp = GetComponent<SphereCollider>();
            if (!sphereTemp) {
                this.gameObject.AddComponent<SphereCollider>();
            }
            //redundant commands \/
            GetComponent<SphereCollider>().radius = 0.005f;
            GetComponent<SphereCollider>().isTrigger = false;
            Rigidbody rbTemmpValidate = GetComponent<Rigidbody>();
            rbTemmpValidate.isKinematic = true;
            rbTemmpValidate.useGravity = false;
            rbTemmpValidate.interpolation = RigidbodyInterpolation.Extrapolate;
            rbTemmpValidate.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }
        //
        if (detectionMode == WaterDetectionMode.RayCastHit_Tag || detectionMode == WaterDetectionMode.RayCastHit_Name) {
            for (int x = 0; x < waters.Length; x++) {
                if (waters[x].waterLayer == 0) {
                    waters[x].waterLayer = 16;
                }
            }
        }
    }

    private void Awake (){
		error = false;
        OnValidateInternal();

        // basic materials
        materialVortex = new Material(SrVortex);
		materialVortex.hideFlags = HideFlags.HideAndDontSave;
		materialBlur = new Material(SrBlur);
		materialBlur.hideFlags = HideFlags.DontSave;

		// camera component
		cameraComponent = GetComponent<Camera> ();
		if (!cameraComponent) {
			error = true;
			Debug.LogError ("For the code to function properly, it must be associated with an object that has the camera component.");
            this.enabled = false;
			return;
		}

        //auto ID
        for (int x = 0; x < waters.Length; x++) {
            waters[x].automaticWaterID = x;
        }

        //set components properties
        if (detectionMode == WaterDetectionMode.Collision_Tag || detectionMode == WaterDetectionMode.Collision_Name) {
            GetComponent<SphereCollider>().radius = 0.005f;
            GetComponent<SphereCollider>().isTrigger = false;
            Rigidbody rbTemp = GetComponent<Rigidbody>();
            rbTemp.isKinematic = true;
            rbTemp.useGravity = false;
            rbTemp.interpolation = RigidbodyInterpolation.Extrapolate;
            rbTemp.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        //shader Quad (screen drops)
		if (!SrQuad.isSupported || disableDropsOnScreen) {
			enableQuadDrops = false;
		} 
        else {
			float cameraScaleFactor = 1 + cameraComponent.nearClipPlane * 25;
			float quadScaleFactor = 1 + cameraComponent.nearClipPlane * 20;
			enableQuadDrops = true;
			quadDrops = GameObject.CreatePrimitive (PrimitiveType.Quad);
			Destroy (quadDrops.GetComponent<MeshCollider> ());
			quadDrops.transform.localScale = new Vector3 (0.16f * quadScaleFactor, 0.16f * quadScaleFactor, 1.0f);
			quadDrops.transform.parent = transform;
			quadDrops.transform.localPosition = new Vector3 (0, 0, 0.05f * cameraScaleFactor);
			quadDrops.transform.localEulerAngles = new Vector3 (0, 0, 0);
            //quadDrops.gameObject.layer = 4;
			quadDropsRenderer = quadDrops.GetComponent<Renderer> ();
			quadDropsRenderer.material.shader = SrQuad;
			quadDropsRenderer.material.SetTexture ("_BumpMap", waterDropsTexture);
			quadDropsRenderer.material.SetFloat ("_BumpAmt", 0);
		}

        //Audio Source
		if (underWaterSound) {
			audioSourceUnderWater = new GameObject ("UnderWaterSound");
			audioSourceUnderWater.AddComponent (typeof(AudioSource));
			audioSourceUnderWater.GetComponent<AudioSource> ().loop = true;
			audioSourceUnderWater.transform.parent = transform;
			audioSourceUnderWater.transform.localPosition = new Vector3 (0, 0, 0);
			audioSourceUnderWater.GetComponent<AudioSource> ().clip = underWaterSound;
			audioSourceUnderWater.SetActive (false);
		}
		audioSourceCamera = GetComponent<AudioSource> ();
		audioSourceCamera.playOnAwake = false;
        audioSourceCamera.volume = audioSourceVolume;

        //support
        CheckSupport ();

        //Set Physics - Ignore Water Collision
        if (ignoreCollisionOnWaterLayer) {
            if (detectionMode == WaterDetectionMode.RayCastHit_Tag || detectionMode == WaterDetectionMode.RayCastHit_Name) {
                for (int x = 0; x < 30; x++) {
                    Physics.IgnoreLayerCollision(x, 4, true);
                    Physics.IgnoreLayerCollision(x, 4, true);
                    Physics.IgnoreLayerCollision(x, 4, true);
                    Physics.IgnoreLayerCollision(x, 4, true);
                    Physics.IgnoreLayerCollision(x, 4, true);
                    Physics.IgnoreLayerCollision(x, 4, true);
                    Physics.IgnoreLayerCollision(x, 4, true);
                }
            }
        }

        //Start
        if (_start == StartMode.inTheWater) {
            EnableWater(true, startWaterID);
        }
        if (_start == StartMode.outOfTheWater) {
            //
        }
	}

    private void CheckSupport(){
		if (!SrBlur.isSupported) { Debug.LogError ("Shader 'SrBlur' not supported"); }
		if (!SrEdge.isSupported) { Debug.LogError ("Shader 'SrEdge' not supported"); }
		if (!SrFisheye.isSupported) { Debug.LogError ("Shader 'SrFisheye' not supported"); }
		if (!SrVortex.isSupported) { Debug.LogError ("Shader 'SrVortex' not supported"); }
		if (!SrQuad.isSupported) { Debug.LogError ("Shader 'SrQuad' not supported"); }
	}

    private void OnDisable(){
		if (!error) {
			if (underWaterSound) {
				audioSourceUnderWater.SetActive (false);
			}
			if (enableQuadDrops) {
				timerDrops = 0;
				cameOutOfTheWater = false;
				quadDropsRenderer.material.SetFloat ("_BumpAmt", 0);
			}
		}
	}

    private void Update (){
		if (!error) {
            if (waters.Length > 0) {
                switch (detectionMode) {
                    case WaterDetectionMode.HeightOfWaterSurface:
                        waterIndex = startWaterID; //this mode only works with 1 water on the scene.
                        if (transform.position.y < waters[waterIndex].waterHeight && !_underWater) {
                            EnableWater(true, waterIndex);
                        }
                        if (transform.position.y >= waters[waterIndex].waterHeight && _underWater) {
                            EnableWater(false, waterIndex);
                        }
                        break;
                    case WaterDetectionMode.RayCastHit_Tag:
                        for (int x = 0; x < waters.Length; x++) {
                            RaycastHit tempHit;
                            float dist = waters[x].rayCastDistance;
                            if (Physics.Linecast((transform.position + Vector3.up * dist), (transform.position - Vector3.up * dist), out tempHit, waters[x].waterLayer)) {
                                if (!string.IsNullOrEmpty(waters[x].waterTag)) {
                                    if (tempHit.collider.gameObject.CompareTag(waters[x].waterTag)) {
                                        if ((transform.position.y - tempHit.point.y) > 0) {
                                            if (_underWater) {
                                                EnableWater(false, x);
                                            }
                                        } else {
                                            if (!_underWater) {
                                                EnableWater(true, x);
                                                x = waters.Length + 1; //break for
                                            }
                                        }
                                    }
                                }
                            }
                            else {
                                if (_underWater) {
                                    EnableWater(false, x);
                                }
                            }
                        }
                        break;
                    case WaterDetectionMode.RayCastHit_Name:
                        for (int x = 0; x < waters.Length; x++) {
                            RaycastHit tempHit;
                            float dist = waters[x].rayCastDistance;
                            if (Physics.Linecast((transform.position + Vector3.up * dist), (transform.position - Vector3.up * dist), out tempHit, waters[x].waterLayer)) {
                                if (!string.IsNullOrEmpty(waters[x].waterName)) {
                                    if (tempHit.collider.gameObject.name == waters[x].waterName) {
                                        if ((transform.position.y - tempHit.point.y) > 0) {
                                            if (_underWater) {
                                                EnableWater(false, x);
                                            }
                                        } else {
                                            if (!_underWater) {
                                                EnableWater(true, x);
                                                x = waters.Length + 1; //break for
                                            }
                                        }
                                    }
                                }
                            } else {
                                if (_underWater) {
                                    EnableWater(false, x);
                                }
                            }
                        }
                        break;
                }
            }



            //quad drops
            if (enableQuadDrops) {
				if (cameraComponent.enabled) {
					quadDrops.SetActive (true);
				} else {
					quadDrops.SetActive (false);
				}
				if (cameOutOfTheWater) {
					timerDrops -= Time.deltaTime * 20.0f;
					quadDropsRenderer.material.SetTextureOffset ("_BumpMap", new Vector2 (0, -timerDrops * 0.01f));
					quadDropsRenderer.material.SetFloat ("_BumpAmt", timerDrops);
					if (timerDrops < 0) {
						timerDrops = 0;
						cameOutOfTheWater = false;
						quadDropsRenderer.material.SetFloat ("_BumpAmt", 0);
					}
				}
			}

			// under Water effects
			if (_underWater) {
                interactions = (int) (7 - (waters [waterIndex].visibility * 0.38f)); //(15*0.38) = 5.70 = [(int) 5]
				blurSpread = 1 - (waters [waterIndex].visibility * 0.065f);          //(15*0.065) = 0.975
				edgesOnly = waters [waterIndex].colorIntensity;
				edgesOnlyBgColor = waters [waterIndex].waterColor;
                disableBlur = waters [waterIndex].disableBlur;
                //
                float fixedTime = waters [waterIndex].distortionSpeed * Time.time * 2.0f;
				float sinVortexAngle = Mathf.Sin(fixedTime * 0.75f) * 10.0f;     // (-10.0f  ~  +10.0f)
				float sinVortexPosX = Mathf.Sin(fixedTime) * 1.3f;               // (-1.30f  ~  +1.30f)
				float sinVortexPosY = Mathf.Sin(fixedTime * 0.66f) * 0.45f;      // (-0.45f  ~  +0.45f)
				float sinFisheyeX = (1 + Mathf.Sin(fixedTime)) * 0.25f;          // (0  ~  0.5)
				float sinFisheyeY = (1 + Mathf.Sin(fixedTime * 0.618f)) * 0.25f; // (0  ~  0.5)
				//
				angleVortex = Mathf.Lerp(angleVortex, waters [waterIndex].vortexDistortion * sinVortexAngle, Time.deltaTime * 0.5f); //(-7 ~ +7)
				centerVortex = Vector2.Lerp(centerVortex, new Vector2 (0.5f + sinVortexPosX, 0.5f + sinVortexPosY), Time.deltaTime * 0.5f);
				strengthX = Mathf.Lerp(strengthX, 2.0f * sinFisheyeX * waters [waterIndex].fisheyeDistortion, Time.deltaTime * 0.5f); // (0 ~ distortion)
				strengthY = Mathf.Lerp(strengthY, 2.0f * sinFisheyeY * waters [waterIndex].fisheyeDistortion, Time.deltaTime * 0.5f); // (0 ~ distortion)
			}
		}
	}

    private void OnRenderImage (RenderTexture source, RenderTexture destination){
		if (_underWater && !error) {
			//effect1 - fisheye
			RenderTexture tmp1 = RenderTexture.GetTemporary (source.width / 2, source.height / 2);
			fisheyeMaterial = CheckShaderAndCreateMaterial (SrFisheye, fisheyeMaterial);
			float ar = (source.width) / (source.height);
			fisheyeMaterial.SetVector ("intensity", new Vector4 (strengthX * ar * 0.15625f, strengthY * 0.15625f, strengthX * ar * 0.15625f, strengthY * 0.15625f));
			Graphics.Blit (source, tmp1, fisheyeMaterial);
			//effect2 - edge
			RenderTexture tmp2 = RenderTexture.GetTemporary (tmp1.width, tmp1.height);
			RenderTexture.ReleaseTemporary (tmp1);
			edgeDetectMaterial = CheckShaderAndCreateMaterial (SrEdge, edgeDetectMaterial);
			edgeDetectMaterial.SetFloat ("_BgFade", edgesOnly);
			edgeDetectMaterial.SetFloat ("_SampleDistance", 0);
			edgeDetectMaterial.SetVector ("_BgColor", edgesOnlyBgColor);
			edgeDetectMaterial.SetFloat ("_Threshold", 0);
			Graphics.Blit (tmp1, tmp2, edgeDetectMaterial, 4);
			//effect3 - vortex
			RenderTexture tmp3 = RenderTexture.GetTemporary (tmp2.width, tmp2.height);
			RenderDistortion (materialVortex, tmp2, tmp3, angleVortex, centerVortex, new Vector2 (1, 1));
            RenderTexture.ReleaseTemporary(tmp2);
            //effect4 - blur
            if (disableBlur) {
                Graphics.Blit(tmp3, destination);
                RenderTexture.ReleaseTemporary(tmp3);
            } 
            else {
                RenderTexture buffer = RenderTexture.GetTemporary(tmp3.width, tmp3.height, 0);
                DownSample4x(tmp3, buffer);
                for (int i = 0; i < interactions; i++) {
                    RenderTexture buffer2 = RenderTexture.GetTemporary(tmp3.width, tmp3.height, 0);
                    FourTapCone(buffer, buffer2, i);
                    RenderTexture.ReleaseTemporary(buffer);
                    buffer = buffer2;
                }
                Graphics.Blit(buffer, destination);
                RenderTexture.ReleaseTemporary(buffer);
                RenderTexture.ReleaseTemporary(tmp3);
            }
		} else {
			Graphics.Blit (source, destination);
		}
	}

    private void FourTapCone (RenderTexture source, RenderTexture dest, int iteration){
		float off = 0.5f + iteration*blurSpread;
		Graphics.BlitMultiTap (source, dest, materialBlur,
			new Vector2(-off, -off),
			new Vector2(-off,  off),
			new Vector2( off,  off),
			new Vector2( off, -off)
		);
	}

    private void DownSample4x (RenderTexture source, RenderTexture dest){
		float off = 1.0f;
		Graphics.BlitMultiTap (source, dest, materialBlur,
			new Vector2(-off, -off),
			new Vector2(-off,  off),
			new Vector2( off,  off),
			new Vector2( off, -off)
		);
	}

    private void RenderDistortion(Material material, RenderTexture source, RenderTexture destination, float angle, Vector2 center, Vector2 radius){
		bool invertY = source.texelSize.y < 0.0f;
		if (invertY){
			center.y = 1.0f - center.y;
			angle = -angle;
		}
		Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, angle), Vector3.one);
		material.SetMatrix("_RotationMatrix", rotationMatrix);
		material.SetVector("_CenterRadius", new Vector4(center.x, center.y, radius.x, radius.y));
		material.SetFloat("_Angle", angle*Mathf.Deg2Rad);
		Graphics.Blit(source, destination, material);
	}

	private Material CheckShaderAndCreateMaterial (Shader s, Material m2Create){
		if (s.isSupported && m2Create && m2Create.shader == s) {
			return m2Create;
		}
		m2Create = new Material (s);
		m2Create.hideFlags = HideFlags.DontSave;
		return m2Create;
	}

    private void OnTriggerEnter (Collider colisor){
		if (enabled && !error) {
            //detection mode = collision
            if (detectionMode == WaterDetectionMode.Collision_Tag || detectionMode == WaterDetectionMode.Collision_Name) {
                for (int x = 0; x < waters.Length; x++) {
                    //Tag
                    if (detectionMode == WaterDetectionMode.Collision_Tag) {
                        if (!string.IsNullOrEmpty(waters[x].waterTag)) {
                            if (colisor.gameObject.CompareTag(waters[x].waterTag)) {
                                EnableWater(false, x);
                                break;
                            }
                        }
                    }
                    //Name
                    if (detectionMode == WaterDetectionMode.Collision_Name) {
                        if (!string.IsNullOrEmpty(waters[x].waterName)) {
                            if (colisor.gameObject.name == waters[x].waterName) {
                                EnableWater(false, x);
                                break;
                            }
                        }
                    }
                }
            }
		}
	}

    private void OnTriggerExit (Collider colisor){
		if (enabled && !error) {

            //detection mode = collision
            if (detectionMode == WaterDetectionMode.Collision_Tag || detectionMode == WaterDetectionMode.Collision_Name) { 
                for (int x = 0; x < waters.Length; x++) {
                    //Tag
                    if (detectionMode == WaterDetectionMode.Collision_Tag) {
                        if (!string.IsNullOrEmpty(waters[x].waterTag)) {
                            if (colisor.gameObject.CompareTag(waters[x].waterTag)) {
                                EnableWater(true, 0);
                                break;
                            }
                        }
                    }
                    //Name
                    if (detectionMode == WaterDetectionMode.Collision_Name) {
                        if (!string.IsNullOrEmpty(waters[x].waterName)) {
                            if (colisor.gameObject.name == waters[x].waterName) {
                                EnableWater(true, 0);
                                break;
                            }
                        }
                    }
                }
            }
		}
	}

    private void OnDrawGizmos() {
        if (waters != null) {
            if (waters.Length > 0) {
                if (detectionMode == WaterDetectionMode.RayCastHit_Tag || detectionMode == WaterDetectionMode.RayCastHit_Name) {
                    float d = waters[waterIndex].rayCastDistance;
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine((transform.position + Vector3.up * d), (transform.position - Vector3.up * d));
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere((transform.position + Vector3.up * d), 0.1f);
                    Gizmos.DrawWireSphere((transform.position - Vector3.up * d), 0.1f);
                }
                if (detectionMode == WaterDetectionMode.Collision_Name || detectionMode == WaterDetectionMode.Collision_Tag) {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(transform.position, 0.2f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(transform.position, 0.1f);
                }
                if (detectionMode == WaterDetectionMode.HeightOfWaterSurface) {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(transform.position, 0.1f);
                    waterIndex = 0;
                    Vector3 finalPos = new Vector3(transform.position.x, waters[waterIndex].waterHeight, transform.position.z);
                    Gizmos.DrawWireSphere(finalPos, 0.1f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, finalPos);
                }
            }
        }
    }


    //PUBLIC VOID
    public void EnableWater(bool _enable, int index) {
        if (waters.Length > 0) {
            _underWater = _enable;
            waterIndex = Mathf.Clamp(index, 0, waters.Length);

            //quad
            if (enableQuadDrops) {
                cameOutOfTheWater = !_enable;
                if (_enable) {
                    quadDropsRenderer.material.SetFloat("_BumpAmt", 0);
                } else {
                    timerDrops = 40;
                }
            }

            //sounds
            if (_enable) {
                if (soundToEnter) {
                    audioSourceCamera.Stop();
                    audioSourceCamera.clip = soundToEnter;
                    audioSourceCamera.PlayOneShot(audioSourceCamera.clip);
                }
                if (underWaterSound) {
                    audioSourceUnderWater.SetActive(true);
                }
            } else {
                if (soundToExit) {
                    audioSourceCamera.Stop();
                    audioSourceCamera.clip = soundToExit;
                    audioSourceCamera.PlayOneShot(audioSourceCamera.clip);
                }
                if (underWaterSound) {
                    audioSourceUnderWater.SetActive(false);
                }
            }

            //reset variables
            if (_enable) {
                angleVortex = strengthX = strengthY = 0.0f;
                centerVortex = new Vector2(0.5f, 0.5f);
            }
        }
    }
}