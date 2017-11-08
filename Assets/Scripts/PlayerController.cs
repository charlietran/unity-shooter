using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Rigidbody))]
 
public class PlayerController : MonoBehaviour {
    Vector3 velocity;
    Rigidbody myRigidBody;
    float distToGround;

	void Start () {
        myRigidBody = GetComponent<Rigidbody>();
        distToGround = GetComponent<Collider>().bounds.extents.y;
	}

    public void Move(Vector3 inputVelocity) {
        velocity = inputVelocity; 
    }

    public void Jump() {
        bool isTouchingGround = Physics.Raycast(myRigidBody.position, Vector3.down, distToGround + 0.1f);
        if (isTouchingGround) {
            myRigidBody.AddForce(0, 300, 0);
        }
    }

    private void FixedUpdate() {
        myRigidBody.MovePosition(myRigidBody.position + velocity * Time.fixedDeltaTime); 
    }

    public void LookAt(Vector3 lookPoint) {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }

}
