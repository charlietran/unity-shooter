using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (GunController))]

public class Player : LivingEntity {
    public float moveSpeed = 5;
    public Crosshairs crosshairs;

    PlayerController playerController;
    Camera viewCamera;
    GunController gunController;

	protected override void Start () {
        base.Start();
        playerController = GetComponent<PlayerController> ();
        viewCamera = Camera.main;
        gunController = GetComponent<GunController>();
		
	}
	
	void Update () {
        MovementInput();
        LookInput();
        WeaponInput();
	}

    void MovementInput() {
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        playerController.Move(moveVelocity);
    }

    void LookInput() {
        // Make a ray from our main camera at the cursor
        Ray cameraRay = viewCamera.ScreenPointToRay(Input.mousePosition);

        // Make a plane at gun height above origin
        Plane groundPlane = new Plane(Vector3.up, Vector3.up * gunController.GunHeight);

        // Holds the distance out from Raycast
        float rayDistance;

        // If our camera ray hits the plane, point the player at it and draw the crosshair
        if (groundPlane.Raycast(cameraRay, out rayDistance)) {
            Vector3 point = cameraRay.GetPoint(rayDistance);
            playerController.LookAt(point);
            crosshairs.transform.position = point;
            crosshairs.DetectTarget(cameraRay);

            float cursorDistanceFromGun = (new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).magnitude;
            if (cursorDistanceFromGun > 1) {
                gunController.Aim(point);
            }
        }
    }

    void WeaponInput() {
        if (Input.GetMouseButton(0)) {
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0)) {
            gunController.OnTriggerRelease();
        }
    }

}
