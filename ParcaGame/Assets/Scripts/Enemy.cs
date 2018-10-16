using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //COMPONENTS
    [SerializeField] private Animator anim = null;
    [SerializeField] private CharacterController charController = null;
    //COMPONENTS

    //STATES
    public enum ANIM_STATES { IDLE = 0, WALK, RUN, ATTACK, GETHIT, DEATH }
    public enum FUNCTIONAL_STATES { SEARCH = 0, CHASE, ATTACK, GETHIT, DEATH }
    public FUNCTIONAL_STATES CurrentFunctionalState { get; set; }
    public ANIM_STATES CurrentAnimState { get; set; }
    private Coroutine setStateCoroutine = null;
    //STATES

    //ATTACK STATE
    private Coroutine attackPlayerCoroutine = null;
    private float distanceToAttackPlayer = 2.2f; //const
    protected bool playerTouched = false;
    //ATTACK STATE

    //SEARCH STATE
    private float distanceToFindPlayer = 30f; //const
    //SEARCH STATE

    //CHASE STATE
    private float velocityToChasePlayer = 1.8f; //const
    //CHASE STATE

    //PLAYER
    private Transform playerTransform = null;
    protected Player playerScript = null;
    //PLAYER

    private void Awake()
    {
        SetState(FUNCTIONAL_STATES.SEARCH, ANIM_STATES.IDLE);
    }
    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        playerScript = playerTransform.GetComponent<Player>();
    }
    private void Update()
    {
        if (CurrentFunctionalState == FUNCTIONAL_STATES.SEARCH)
        {
            SearchPlayer();
        }
        else if (CurrentFunctionalState == FUNCTIONAL_STATES.CHASE)
        {
            ChasePlayer();
        }
    }

    //STATES
    private void SetState(FUNCTIONAL_STATES functionalState, ANIM_STATES animState)
    {
        setStateCoroutine = StartCoroutine(SetStateCoroutine(functionalState, animState));
    }
    private IEnumerator SetStateCoroutine(FUNCTIONAL_STATES functionalState, ANIM_STATES animState)
    {
        CurrentAnimState = animState;
        anim.SetInteger("state", (int)CurrentAnimState);
        yield return new WaitWhile(() => !anim.GetCurrentAnimatorStateInfo(0).IsName(animState.ToString())); //esperamos que acabe la animacion anterior antes de cambiar el estado funcional
        CurrentFunctionalState = functionalState;

        if (CurrentFunctionalState == FUNCTIONAL_STATES.ATTACK)
        {
            attackPlayerCoroutine = StartCoroutine(AttackPlayer());
        }

        StopCoroutine(setStateCoroutine);
    }
    //STATES

    //ATTACK STATE
    private void StopAttack(FUNCTIONAL_STATES next_functional_state, ANIM_STATES next_anim_state)
    {
        if (CurrentFunctionalState == FUNCTIONAL_STATES.ATTACK)
        {
            playerTouched = false;
            StopCoroutine(attackPlayerCoroutine);
            SetState(next_functional_state, next_anim_state);
        }
    }
    private IEnumerator AttackPlayer()
    {
        yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).IsName(ANIM_STATES.ATTACK.ToString()));
        if (playerScript.damagePlayerController.CurrentHealth <= 0) //si el player muere hacemos que vuelva al estado de patrulla
        {
            StopAttack(FUNCTIONAL_STATES.SEARCH, ANIM_STATES.IDLE);
        }
        else //si el player NO muere
        {
            if (playerTouched) //si el enemigo ha alcanzado al jugador con su ataque
            {
                playerTouched = false;
                if (Mathf.Abs(Vector3.Distance(transform.position, playerTransform.position)) > distanceToAttackPlayer * 0.9f) //si el jugador se aleja paramos de atacar y hacemos que vuelva al estado de perseguir
                {
                    StopAttack(FUNCTIONAL_STATES.CHASE, ANIM_STATES.WALK);
                }
                else //si no se ha alejado el jugador reiniciamos el ataque
                {
                    yield return new WaitWhile(() => anim.GetCurrentAnimatorStateInfo(0).IsName(ANIM_STATES.IDLE.ToString()));
                    StopCoroutine(attackPlayerCoroutine);
                    attackPlayerCoroutine = StartCoroutine(AttackPlayer());
                }
            }
            else //si el enemigo NO ha alcanzado al jugador con su ataque paramos de atacar y hacemos que vuelva al estado de perseguir
            {
                StopAttack(FUNCTIONAL_STATES.CHASE, ANIM_STATES.WALK);
            }
        }
    }
    //ATTACK STATE

    //SEARCH STATE
    private void SearchPlayer()
    {
        LookAtPlayer();
        if (Mathf.Abs(Vector3.Distance(transform.position, playerTransform.position)) <= distanceToFindPlayer) //si el jugador esta a la distancia que ve el enemigo pasamos al estado de perseguir
        {
            SetState(FUNCTIONAL_STATES.CHASE, ANIM_STATES.WALK);
        }
    }
    //SEARCH STATE

    //CHASE STATE
    private void ChasePlayer()
    {
        LookAtPlayer();
        if (anim.GetCurrentAnimatorStateInfo(0).IsName(ANIM_STATES.WALK.ToString()))
        {
            charController.Move(transform.forward * Time.deltaTime * velocityToChasePlayer);
            if (Mathf.Abs(Vector3.Distance(transform.position, playerTransform.position)) > distanceToFindPlayer || playerScript.damagePlayerController.CurrentHealth <= 0) //si se aleja el jugador demasiado o muere vuelve al estado de patrulla
            {
                if (CurrentAnimState != ANIM_STATES.IDLE) SetState(FUNCTIONAL_STATES.SEARCH, ANIM_STATES.IDLE);
            }
            else if (Mathf.Abs(Vector3.Distance(transform.position, playerTransform.position)) <= distanceToAttackPlayer / 0.85f) //si se acerca lo suficiente al jugador hacemos que pase al estado de ataque
            {
                if (CurrentAnimState != ANIM_STATES.ATTACK) SetState(FUNCTIONAL_STATES.ATTACK, ANIM_STATES.ATTACK);
            }
        }
    }
    //CHASE STATE

    //PLAYER
    private void LookAtPlayer()
    {
        Vector3 dir = playerTransform.position - transform.position;
        dir.y = 0; //keep the direction strictly horizontal
        Quaternion rot = Quaternion.LookRotation(dir);
        //slerp to the desired rotation over time
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, 2.5f * Time.deltaTime);
    }
    //PLAYER
}
