using UnityEngine;
using System.Collections;

public class Shell : MonoBehaviour {
  public Rigidbody shellRigidBody;
  public float forceMin;
  public float forceMax;

  // Defines the amount of time to wait before the shell is faded out and destroyed
  const float lifetime = 2f;

  // Defines how long in seconds the shell fadeout will take
  const float fadeTime = 1f;

  void Start() {
    float force = Random.Range(forceMin, forceMax);

    // Eject the shell along the X axis with a random force
    shellRigidBody.AddForce(transform.right * force);

    // Give the shell a random torque based on our force
    shellRigidBody.AddTorque(Random.insideUnitSphere * force);

    // Start a coroutine to fade out and destroy the shell within the specified lifetime
    StartCoroutine(Fade());
  }

  IEnumerator Fade() {
    yield return new WaitForSeconds(lifetime);

    // Get the material for the shell
    Material shellMaterial = GetComponent<Renderer>().material;

    // Get initial color for the fadeout interpolation
    Color initialColor = shellMaterial.color;

    float percent = 0f;
    float fadeSpeed = 1 / fadeTime;

    while (percent < 1.0f) {
      percent += Time.deltaTime * fadeSpeed;
      shellMaterial.color = Color.Lerp(initialColor, Color.clear, percent);
      yield return null;
    }

    Destroy(gameObject);
  }
}
