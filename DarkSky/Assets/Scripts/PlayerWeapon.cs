using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour {

    private Animator animator;

    private int attackBool;                          // Animator variable related to attacking.
    private bool attack = false;                     // Boolean to determine whether or not the player attacked.

    public int currentWeapon = 0;
    public GameObject[] weapons;

    public Transform weaponPos;
    GameObject rightHand;


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

        // Set up the references.
        attackBool = Animator.StringToHash("Attack");

        //get hand
        rightHand = GameObject.FindWithTag("RightHand");

        //get weapon position
        weaponPos = rightHand.transform.FindChild("weaponPosition");

        if(weildWeapon){
            //equipt weapon
            equiptWeapon();
        }
        
	}
	
	// Update is called once per frame
	void Update () {
        //player attacks
        if (Input.GetButtonDown("Fire1") && !animator.GetBool(attackBool))
        {
            attack = true;

            // Set fly related variables on the Animator Controller.
            animator.SetBool(attackBool, attack);

            //wait till animation is over and turn attack to false

        }
        else
        {
            if (this.animator.GetCurrentAnimatorStateInfo(0).IsName("standing_melee_attack_downward"))
            {
                // is attacking.
            }
            else
            {
                // No longer attacking
                attack = false;

                animator.SetBool(attackBool, attack);
            }
        }

        if (weildWeapon && !attack)
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
        //first find spine

       
        weapon.transform.parent = rightHand.transform; //gameObject.transform.FindChild("mixamorig:RightHand");

        //only use this if weapons root is the holding position
        weapon.transform.position = rightHand.transform.position;

        //weapon.transform.rotation = rightHand.transform.rotation;

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
                //animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandWeaponPos.position);
                animator.SetIKPosition(AvatarIKGoal.RightHand, weaponPos.position);
            }

            //position left hand
            if (leftHandWeaponPos != null && useLeftHand)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
                //animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandWeaponPos.position);
                animator.SetIKPosition(AvatarIKGoal.LeftHand, weaponPos.position);
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
