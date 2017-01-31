using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour {

    private Animator animator;

    public int currentWeapon = 0;
    public GameObject[] weapons;

    public Transform weaponPos;

    public GameObject weapon;

    public bool weildWeapon = false;
    public bool useIK = true;
    public bool useLeftHand = false;
    public bool useRightHand = true;

    public Transform leftHandWeaponPos = null;
    public Transform rightHandWeaponPos = null;

	// Use this for initialization
	void Start () {
        //Get Animator Controller
        animator = GetComponent<Animator>();

        //get weapon position
        weaponPos = transform.FindChild("weaponPosition");

        if(weildWeapon){
            //equipt weapon
            equiptWeapon();
        }
        
	}
	
	// Update is called once per frame
	void Update () {
        if (weildWeapon)
        {
            //equipt weapon
            equiptWeapon();
        }
        else if(!weildWeapon && weapon != null)
        {
            Destroy(weapon);
        }
	}

    public void equiptWeapon()
    {
        //equipt weapon
        Destroy(weapon); //deletes old weapon

        //creates new weapon
        weapon = Instantiate(weapons[currentWeapon], new Vector3(weaponPos.position.x, weaponPos.position.y, weaponPos.position.z), weaponPos.rotation);

        //make this object the weapons parent
        weapon.transform.parent = gameObject.transform;

        //get hand positions from weapon prefab
        if (weapon.transform.childCount == 1)
        {
            leftHandWeaponPos = weapon.transform.GetChild(0).transform.FindChild("leftHand");
            rightHandWeaponPos = weapon.transform.GetChild(0).transform.FindChild("rightHand");
        }
        else
        {
            leftHandWeaponPos = weapon.transform.FindChild("leftHand");
            rightHandWeaponPos = weapon.transform.FindChild("rightHand");
        }
    }

    void OnAnimatorIK()
    {
        //position hands
        if(useIK)
        {
            //position right hand
            if (rightHandWeaponPos != null && useRightHand)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
                animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandWeaponPos.position);
            }

            //position left hand
            if (leftHandWeaponPos != null && useLeftHand)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandWeaponPos.position);
            }
        }
        

        //if (leftHandWeaponPos != null && rightHandWeaponPos != null && useIK)
        //{
        //    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        //    animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandWeaponPos.position);
        //    //anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
        //    //anim.SetIKRotation(AvatarIKGoal.LeftHand, leftHandRot);

        //    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        //    animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandWeaponPos.position);
        //    //anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);
        //    //anim.SetIKRotation(AvatarIKGoal.RightHand, rightHandRot);
        //}
    }
}
