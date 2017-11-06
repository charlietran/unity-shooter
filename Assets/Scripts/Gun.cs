using UnityEngine;

public class Gun : MonoBehaviour {
    public Transform muzzle;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;

    public Transform shell;
    public Transform shellEjectionPoint;

    MuzzleFlash muzzleFlash;

    float nextShotTime;

    void Start() {
        muzzleFlash = GetComponent<MuzzleFlash>();
    }

    public void Shoot() {
        if (Time.time > nextShotTime) {
            nextShotTime = Time.time + msBetweenShots / 1000;
            Projectile newProjectile = Instantiate(projectile, muzzle.position, muzzle.rotation);
            newProjectile.SetSpeed(muzzleVelocity);

            // Instantiate a Shell prefab at our given shellEjectionPoint position and rotation
            Instantiate(shell, shellEjectionPoint.position, shellEjectionPoint.rotation);

            muzzleFlash.Activate();
        }
    }
}
