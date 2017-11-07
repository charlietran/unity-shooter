using UnityEngine;

public class Crosshairs : MonoBehaviour {
    public LayerMask targetMask;
    public Color dotHighlightColor;
    public SpriteRenderer dot;

    Color initialDotColor;
    Vector3 initialScale;

    void Start() {
        initialDotColor = dot.color;
        initialScale = dot.transform.localScale;
        Cursor.visible = false;
    }

    void Update() {
        transform.Rotate(Vector3.forward * -50 * Time.deltaTime);
    }

    // Detect whether the given ray is colliding with anything on our target Mask
    // Used in PlayerController
    public void DetectTarget(Ray ray) {
        if(Physics.Raycast(ray, 100, targetMask)) {
            dot.color = dotHighlightColor;
            dot.transform.localScale = initialScale * 1.2f;
        } else {
            dot.color = initialDotColor;
            dot.transform.localScale = initialScale;
        }
    }
}