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

    // This will override the projectile's default speed
    public float projectileSpeed = 35;

    public Transform shell;
    public Transform shellEjectionPoint;

    MuzzleFlash muzzleFlash;

    float nextShotTime;

    bool triggerReleasedSinceLastShot;
    int clipSize;
    int shotsRemainingInClip;

    void Start() {
        muzzleFlash = GetComponent<MuzzleFlash>();

        if (fireMode == FireMode.Burst) {
            clipSize = burstCount;
        }
        else if (fireMode == FireMode.Single)
        {
            clipSize = 1;
        }

        shotsRemainingInClip = clipSize;
    }

    public void OnTriggerHold() {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease() {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInClip = clipSize;
    }

    public void Aim(Vector3 aimPoint) {
        transform.LookAt(aimPoint);
    }

    void Shoot() {
        if (Time.time > nextShotTime) {
            if (fireMode != FireMode.Auto && shotsRemainingInClip == 0) {
                return;
            }

            nextShotTime = Time.time + msBetweenShots / 1000;
            InstantiateProjectiles();
            Instantiate(shell, shellEjectionPoint.position, shellEjectionPoint.rotation);
            muzzleFlash.Activate();
        }
    }

    void InstantiateProjectiles() {
        // For each of our projectile spawns, instantiate a projectile at our given speed
        foreach (Transform projectileSpawn in projectileSpawns) {
            Projectile newProjectile = Instantiate(projectile, projectileSpawn.position, projectileSpawn.rotation);
            newProjectile.SetSpeed(projectileSpeed);
        }
    }
}