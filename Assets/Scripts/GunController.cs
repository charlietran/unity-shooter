using UnityEngine;

public class GunController : MonoBehaviour {
    public Transform weaponHold;
    public Gun startingGun;
    public Gun[] allGuns;
    Gun equippedGun;

    void Start() {
    }

    public void EquipGun(Gun gunToEquip) {
        if (equippedGun != null) {
            Destroy(equippedGun.gameObject);
        }
        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation);
        equippedGun.transform.parent = weaponHold;
    }

    public void EquipGun(int gunIndex) {
        EquipGun(allGuns[gunIndex]);
    }

    public void OnTriggerHold() {
        if (equippedGun != null) {
            equippedGun.OnTriggerHold();
        }
    }

    public void OnTriggerRelease() {
        if (equippedGun != null) {
            equippedGun.OnTriggerRelease();
        }
    }

    public void Aim(Vector3 aimPoint) {
        if (equippedGun != null) {
            equippedGun.Aim(aimPoint);
        }
    }

    public void Reload() {
        if (equippedGun != null) {
            equippedGun.Reload();
        }
    }

    // Return the y position of the gun holder object. Used for positioning the crosshair
    public float GunHeight {
        get {
            return weaponHold.position.y;
        }
    }
}
