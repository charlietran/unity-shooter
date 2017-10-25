using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (PlayerController))]
[RequireComponent (typeof (GunController))]

public class Player : LivingEntity {
    public float moveSpeed = 5;
    PlayerController controller;
    Camera viewCamera;
    GunController gunController;

	protected override void Start () {
        base.Start();
        controller = GetComponent<PlayerController> ();
        viewCamera = Camera.main;
        gunController = GetComponent<GunController>();
		
	}
	
	void Update () {
        // Movement input
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        // Look Input
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray,out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            // Debug.DrawLine(ray.origin, point, Color.red); 
            controller.LookAt(point);
        }

        // Weapon input
        if (Input.GetMouseButton(0))
        {
            gunController.Shoot();
        }

	}
}
