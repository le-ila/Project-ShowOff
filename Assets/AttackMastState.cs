using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackMastState : EnemyState
{
    [Header("Attack Data")]
    [SerializeField] float attackDamage;

    [Header("Animation")] [SerializeField] Animator animator;
    [SerializeField] GameObject weaponGameObject;

    MastScript mast;

    #region Unity Events

    private void Awake()
    {
        mast = EnemyManager.instance.mastTransform.GetComponent<MastScript>();
    }

    #endregion

    #region State Handling

    public override void Handle()
    {
        LookAtMast();

        // play sawing mast sound
        SoundManager.PlaySound(SoundManager.Sound.SawingMast, transform);

        // play player voice lines
        if (Random.Range(1, 100) > 10)
        {
            SoundManager.PlaySound(SoundManager.Sound.VoiceLine_PLAYER_ENEMY_CUTTING_MAST);
        }

        // play enemy voice lines
        if (Random.Range(1, 100) > 20)
        {
            SoundManager.PlaySound(SoundManager.Sound.VoiceLine_ENEMY_CUTTING_MAST, transform);
        }

        //lastAttackTime = Time.time;
    }

    public void DamageMast()
    {
        mast.TakeDamage(attackDamage);
    }

    public override void OnStateEnter()
    {
        weaponGameObject.SetActive(false);

        if (animator != null)
        {
            animator.SetBool("IsSawing", true);
            Debug.Log("Set Sawing to true");
        }
        else
        {
            Debug.LogError("Animator is not assigned");
        }

        LookAtMast();
    }

    public override void OnStateExit()
    {
        weaponGameObject.SetActive(true);
        animator.SetBool("IsSawing", false);
    }

    #endregion

    #region Helper Methods

    private void LookAtMast()
    {
        if (mast != null)
        {
            Vector3 direction = (mast.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    #endregion
}