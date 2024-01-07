using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManagement : MonoBehaviour
{
    public Animator animator;
    public int weaponIndex = 0;

    void Start()
    {
        SelectWeapon();
    }

    void Update()
    {
        int prevWeap = weaponIndex;
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            weaponIndex = 0;
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            weaponIndex = 1;
        }

        if (prevWeap != weaponIndex) {
            SelectWeapon();
        }
    }

    void SelectWeapon()
    {
        int i = 0;
        foreach (Transform weapon in transform)
        {
            if (i == weaponIndex) {
                weapon.gameObject.SetActive(true);
                animator.SetBool(weapon.name + "_equipped", true);
            } else { 
                weapon.gameObject.SetActive(false);
                animator.SetBool(weapon.name + "_equipped", false);
            }

            i++;
        }
    }
}
