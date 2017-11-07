using UnityEngine;

public class Gun : MonoBehaviour {

    // Firing mode
    public enum FireMode {Auto, Burst, Single}
    public FireMode fireMode;
    public int burstCount;

    // An array of projectile spawns, since our gun may shoot multiple bullets at a time
    public Transform[] projectileSpawns;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;

    public Transform shell;
    public Transform shellEjectionPoint;

    MuzzleFlash muzzleFlash;

    float nextShotTime;

    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;

    void Start() {
        muzzleFlash = GetComponent<MuzzleFlash>();
        resetBurstCount();
    }

    public void OnTriggerHold() {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease() {
        triggerReleasedSinceLastShot = true;
        resetBurstCount();
    }

    public void Aim(Vector3 aimPoint) {
        transform.LookAt(aimPoint);
    }

    void resetBurstCount() {
        shotsRemainingInBurst = burstCount;
    }

    void Shoot() {
        if (Time.time > nextShotTime) {
            if(fireMode == FireMode.Burst) {
                if(shotsRemainingInBurst == 0) {
                    return;
                }
                shotsRemainingInBurst--;
            } else if(fireMode == FireMode.Single) {
                if(!triggerReleasedSinceLastShot) {
                    return;
                }
            }

            foreach(Transform projectileSpawn in projectileSpawns) {
                nextShotTime = Time.time + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile, projectileSpawn.position, projectileSpawn.rotation);
                newProjectile.SetSpeed(muzzleVelocity);
            }

            // Instantiate a Shell prefab at our given shellEjectionPoint position and rotation
            Instantiate(shell, shellEjectionPoint.position, shellEjectionPoint.rotation);
            muzzleFlash.Activate();
        }
    }
}