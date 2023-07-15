using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CharacterController))]
public class ExampleFPSController : MonoBehaviour {

    [Range(1,20)]
	public float speed = 6.0f;
    [Range(1, 20)]
    public float jumpForce = 8.0f;

    [Space(10)]
    [Range(1, 20)]
    public float cameraHorizontalSpeed = 5.0f;
    [Range(1, 20)]
    public float cameraVerticalSpeed = 7.0f;

	GameObject cameraFPS;
	Vector3 moveDirection = Vector3.zero;
	CharacterController controller;
	float rotacaoX = 0.0f, rotacaoY = 0.0f;

	void Start () {
		transform.tag = "Player";
		Camera cameraComponent = GetComponentInChildren<Camera>();
        if (!cameraComponent) {
            return;
        }
        cameraFPS = cameraComponent.transform.gameObject;
		cameraFPS.transform.localPosition = new Vector3 (0, 1, 0);
		cameraFPS.transform.localRotation = Quaternion.identity;
		controller = GetComponent<CharacterController> ();
	}

	void Update () {
        if (cameraFPS) {
            Vector3 direcaoFrente = new Vector3(cameraFPS.transform.forward.x, 0, cameraFPS.transform.forward.z);
            Vector3 direcaoLado = new Vector3(cameraFPS.transform.right.x, 0, cameraFPS.transform.right.z);
            direcaoFrente.Normalize();
            direcaoLado.Normalize();
            direcaoFrente = direcaoFrente * Input.GetAxis("Vertical");
            direcaoLado = direcaoLado * Input.GetAxis("Horizontal");
            Vector3 direcFinal = direcaoFrente + direcaoLado;

            if (direcFinal.sqrMagnitude > 1) {
                direcFinal.Normalize();
            }

            if (controller.isGrounded) {
                moveDirection = new Vector3(direcFinal.x, 0, direcFinal.z);
                moveDirection *= speed;
                if (Input.GetButton("Jump")) {
                    moveDirection.y = jumpForce;
                }
            }

            moveDirection.y -= 20.0f * Time.deltaTime;
            controller.Move(moveDirection * Time.deltaTime);
            CameraPrimeiraPessoa();
        }
	}

	void CameraPrimeiraPessoa(){
		rotacaoX += Input.GetAxis ("Mouse X") * cameraVerticalSpeed;
		rotacaoY += Input.GetAxis ("Mouse Y") * cameraHorizontalSpeed;
		rotacaoX = ClampAngleFPS (rotacaoX, -360, 360);
		rotacaoY = ClampAngleFPS (rotacaoY, -80, 80);
		Quaternion xQuaternion = Quaternion.AngleAxis (rotacaoX, Vector3.up);
		Quaternion yQuaternion = Quaternion.AngleAxis (rotacaoY, -Vector3.right);
		Quaternion rotacFinal = Quaternion.identity * xQuaternion * yQuaternion;
		cameraFPS.transform.localRotation = Quaternion.Lerp (cameraFPS.transform.localRotation, rotacFinal, Time.deltaTime * 10.0f);
	}

	float ClampAngleFPS(float angulo, float min, float max){
		if (angulo < -360) {
			angulo += 360;
		}
		if (angulo > 360) {
			angulo -= 360;
		}
		return Mathf.Clamp (angulo, min, max);
	}
}