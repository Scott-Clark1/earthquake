using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{

    public float damage = 10f;
    public float range = 100f;
    public float knockback = 30f;
    public float fireRate = 4f;

    public float reloadTime = 1f;
    private bool isReloading = false;
    // private bool isShooting = false;
    public int maxAmmo = 6;
    public int currentAmmo;

    public bool usesAmmo = true;


    public ParticleSystem muzzleFlash;
    // Start is called before the first frame update

    public Camera cameraObj;


    public Animator animator;


    private float timeToFire = 0f;

    void Start()
    {
        currentAmmo = maxAmmo;
    }

    void OnEnable()
    {
        isReloading = false;
        // isShooting = false;
        animator.SetBool("Reloading", false);
        animator.SetBool("Shooting", false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isReloading)
            return;
        if (currentAmmo <= 0 && usesAmmo) {
            StartCoroutine(Reload());
            return;
        }
        if (Input.GetButtonDown("Fire1") && Time.time >= timeToFire)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        timeToFire = Time.time + 1f / fireRate;
        int layerMask = 1 << 11 | 1 << 10; // mask to only hit  "targets," "ground"
        if (muzzleFlash != null) {
            muzzleFlash.Play();
        }
        StartCoroutine(HandleShootAnim());

        RaycastHit hit;
        bool didHit = Physics.Raycast(
            cameraObj.transform.position,
            cameraObj.transform.forward,
            out hit,
            range,
            layerMask);

        if (didHit) {
            Debug.Log(hit.transform.name);
            if (hit.rigidbody != null)
                hit.rigidbody.AddForce(-hit.normal * knockback);

            Target target = hit.transform.GetComponent<Target>();
            if (target != null)
                target.TakeDamage(damage);

        }
        if (usesAmmo)
            currentAmmo--;
    }

    IEnumerator HandleShootAnim()
    {
        // isShooting = true;

        animator.SetBool("Shooting", true);
        yield return new WaitForSeconds(1f / fireRate - 0.1f);
        animator.SetBool("Shooting", false);
        yield return new WaitForSeconds(1f / fireRate - 0.1f);
        // isShooting = false;
    }

    IEnumerator Reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(.125f);
        animator.SetBool("Reloading", true);
        yield return new WaitForSeconds(reloadTime - .25f);
        animator.SetBool("Reloading", false);
        yield return new WaitForSeconds(.125f);
        currentAmmo = maxAmmo;
        isReloading = false;
    }
}
