using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    public Transform virtualCameraTransform;
    Cinemachine.CinemachineVirtualCamera virtualCamera;

    Player player;

    void Start() {
        player = FindObjectOfType<Player>();
        if (player != null) {
            virtualCamera = virtualCameraTransform.GetComponent<Cinemachine.CinemachineVirtualCamera>();
            virtualCamera.Follow = player.transform;
        }
    }
}
