using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum ANIM_STATES { IDLE = 0, WALK, RUN, ATACK, GETHIT, DEAD }
    public enum FUNCTIONAL_STATES { SEARCH = 0, CHASE, ATACK, GETHIT, DEAD }
    [SerializeField] private FUNCTIONAL_STATES currentFunctionalState = FUNCTIONAL_STATES.SEARCH;
    [SerializeField] private ANIM_STATES currentAnimState = ANIM_STATES.IDLE;

    [SerializeField] private Animator anim = null;
    [SerializeField] private CharacterController charController = null;
    private Transform playerTransform = null;

    [SerializeField, Range(1f, 100f)] private float distanceToFindPlayer = 5f;
    [SerializeField, Range(1f, 100f)] private float velocityToChasePlayer = 5f;

    private void Awake()
    {
        SetState(FUNCTIONAL_STATES.SEARCH, ANIM_STATES.IDLE);
    }

    private void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        if (currentFunctionalState == FUNCTIONAL_STATES.SEARCH)
        {
            SearchPlayer();
        }
        else if (currentFunctionalState == FUNCTIONAL_STATES.CHASE)
        {
            ChasePlayer();
        }
    }

    private void SearchPlayer()
    {
        if (Mathf.Abs(Vector3.Distance(transform.position, playerTransform.position)) < distanceToFindPlayer)
        {
            SetState(FUNCTIONAL_STATES.SEARCH, ANIM_STATES.WALK);
        }
    }

    private void ChasePlayer()
    {
        transform.LookAt(playerTransform);
        charController.Move(Vector3.forward * Time.deltaTime * velocityToChasePlayer);
    }

    private void SetState(FUNCTIONAL_STATES functionalState, ANIM_STATES animState)
    {
        currentFunctionalState = functionalState;
        currentAnimState = animState;
        anim.SetInteger("state", (int)currentAnimState);
    }
}
