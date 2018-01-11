using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {
	public Transform virtualCameraTransform;
	public Transform playerTransform;
	Cinemachine.CinemachineVirtualCamera virtualCamera;

	void Awake() {
		virtualCamera = virtualCameraTransform.GetComponent<Cinemachine.CinemachineVirtualCamera>();
		virtualCamera.Follow = playerTransform;
	}
}
