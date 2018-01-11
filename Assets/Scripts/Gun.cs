using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {
    // Firing mode
    public enum FireMode { Auto, Burst, Single }
    public FireMode fireMode;
    public int burstCount;
    public int shotsPerMagazine;

    // An array of projectile spawns, since our gun may shoot multiple bullets at a time
    public Transform[] projectileSpawns;
    public Projectile projectile;
    public float msBetweenShots = 100;

    // This will override the projectile's default speed
    public float projectileSpeed = 35;

    [Header("Shell")]
    public Transform shell;
    public Transform shellEjectionPoint;

    [Header("Recoil")]
    public float recoilMin;
    public float recoilMax;
    public float recoilAngleMin;
    public float recoilAngleMax;
    public float recoilSettleSpeed;
    public float recoilAngleSettleSpeed;

    [Header("Sound Effects")]
    public AudioClip shootAudio;
    public AudioClip reloadAudio;

    MuzzleFlash muzzleFlash;

    float nextShotTime;

    [Header("Reloading")]
    public float reloadTime = 0.3f;
    public float reloadAngleMax = 30f;

    int burstSize;
    int shotsRemainingInBurst;
    int shotsRemainingInMagazine;
    bool isReloading = false;
    Vector3 initialRotation;

    Vector3 recoilSmoothDampVelocity;
    float recoilRotationSmoothDampVelocity;
    float recoilAngle;

    void Start() {
        muzzleFlash = GetComponent<MuzzleFlash>();

        if (fireMode == FireMode.Burst) {
            burstSize = burstCount;
        } else if (fireMode == FireMode.Single) {
            burstSize = 1;
        }

        shotsRemainingInBurst = burstSize;
        shotsRemainingInMagazine = shotsPerMagazine;
    }

    void LateUpdate() {
        // Animate recoil back to original position
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilSettleSpeed);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotationSmoothDampVelocity, recoilAngleSettleSpeed);
        transform.localEulerAngles = transform.localEulerAngles + Vector3.left * recoilAngle;
        if (!isReloading && shotsRemainingInMagazine == 0) {
            Reload();
        }
    }

    public void OnTriggerHold() {
        Shoot();
    }

    public void OnTriggerRelease() {
        shotsRemainingInBurst = burstSize;
    }

    public void Aim(Vector3 aimPoint) {
        if (!isReloading) {
            transform.LookAt(aimPoint);
        }
    }

    public void Reload() {
        if (shotsRemainingInMagazine != shotsPerMagazine) {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
        }
    }

    IEnumerator AnimateReload() {
        isReloading = true;

        float percent = 0;
        float speed = 1 / reloadTime;
        initialRotation = transform.localEulerAngles;
        while (percent < 1f) {
            percent += Time.deltaTime * speed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, reloadAngleMax, interpolation);
            transform.localEulerAngles = initialRotation + Vector3.left * reloadAngle;

            yield return null;
        }

        isReloading = false;
        shotsRemainingInMagazine = shotsPerMagazine;
    }


    void Shoot() {
        bool canShoot = !isReloading && (Time.time > nextShotTime) && (shotsRemainingInMagazine > 0);
        if (!canShoot) {
            return;
        }

        // If we're not in auto (infinite) firing mode, check our ammo count 
        if (fireMode != FireMode.Auto) {
            if (shotsRemainingInBurst > 0) {
                shotsRemainingInBurst--;
            } else {
                return;
            }
        }

        nextShotTime = Time.time + msBetweenShots / 1000;
        InstantiateProjectiles();
        Instantiate(shell, shellEjectionPoint.position, shellEjectionPoint.rotation);
        muzzleFlash.Activate();

        // Recoil
        transform.localPosition -= Vector3.forward * Random.Range(recoilMin, recoilMax);
        recoilAngle += Random.Range(recoilAngleMin, recoilAngleMax);
        recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

        AudioManager.instance.PlaySound(shootAudio, transform.position);
    }

    void InstantiateProjectiles() {
        // For each of our projectile spawns, instantiate a projectile at our given speed
        foreach (Transform projectileSpawn in projectileSpawns) {
            if (shotsRemainingInMagazine == 0) {
                break;
            }
            Projectile newProjectile = Instantiate(
                projectile,
                projectileSpawn.position,
                projectileSpawn.rotation
            );
            shotsRemainingInMagazine--;
            newProjectile.SetSpeed(projectileSpeed);
        }
    }
}