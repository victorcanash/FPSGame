using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArm : Enemy
{
    private float damage = 1f; //const
    private bool canAttack = true;

    private void Awake()
    {
        playerTouched = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && other.isTrigger && CurrentFunctionalState == Enemy.FUNCTIONAL_STATES.ATTACK && canAttack)
        {
            canAttack = false;
            playerTouched = true;
            playerScript.damagePlayerController.Damage(damage);
            Invoke("CanAttack", 0.5f);
        }
    }

    private void CanAttack()
    {
        canAttack = true;
    }
}
