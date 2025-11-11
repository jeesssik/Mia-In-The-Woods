using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class FlorController : MonoBehaviour

   {
    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float stopDistance = 0.2f; // distancia mínima en X antes de superponer

    [Header("Rangos (child colliders)")]
    public Collider2D detectionCollider; // CircleCollider2D (IsTrigger)
    public Collider2D attackCollider;    // CircleCollider2D (IsTrigger)

    [Header("Targets & Settings")]
    public Transform currentTarget; // normalmente el player
    public string playerTag = "Player";

    // Internos
    private Animator animator;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private bool playerDetected = false;
    private bool playerInAttackRange = false;
    private bool isAttacking = false;
    private bool isDead = false;

    // Cooldowns
    public float attackCooldown = 1f;
    private bool canAttack = true;

    void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (detectionCollider == null || attackCollider == null)
            Debug.LogWarning("Asigná Detection y Attack colliders en el inspector.");
    }

    void Update()
    {
        if (isDead) return;

        UpdateAnimatorSpeed();

        if (playerDetected && currentTarget != null && !isAttacking)
        {
            // Mirar al jugador (solo en X)
            FaceTarget();

            // Si está en rango de ataque, intentamos atacar (se controla por triggers)
            if (playerInAttackRange)
            {
                if (canAttack)
                    StartCoroutine(DoAttack());
                    Debug.Log("Atacando al jugador");
            }
            // si no está en rango de ataque, el movimiento se hace en FixedUpdate (físico)
        }
    }
    private void DamagePlayer()
{
    if (currentTarget != null)
    {
        Mia player = currentTarget.GetComponent<Mia>();
        if (player != null)
            player.RecibirDaño(1); // daño de la flor
    }
}

    void FixedUpdate()
    {
        if (isDead) return;

        // Movimiento físico: avanzar solo en X hacia el target
        if (playerDetected && currentTarget != null && !isAttacking && !playerInAttackRange)
        {
            float dx = currentTarget.position.x - rb.position.x; // diferencia horizontal
            float absDx = Mathf.Abs(dx);

            if (absDx > stopDistance)
            {
                // objetivo: misma Y actual del enemigo, X = player's X
                Vector2 targetPos = new Vector2(currentTarget.position.x, rb.position.y);
                float step = moveSpeed * Time.fixedDeltaTime;
                Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, step);
                rb.MovePosition(newPos);
            }
        }
    }

    private void UpdateAnimatorSpeed()
    {
        // Speed usa la distancia horizontal (más representativo)
        float speedValue = 0f;
        if (playerDetected && currentTarget != null && !isAttacking)
        {
            float dx = Mathf.Abs(currentTarget.position.x - transform.position.x);
            // escala simple: si dx pequeño -> 0, si mayor -> hasta moveSpeed
            speedValue = Mathf.Clamp01(dx / 1.5f) * moveSpeed;
        }
        animator.SetFloat("Speed", speedValue);
    }

    private void FaceTarget()
    {
        if (currentTarget == null) return;
        float dx = currentTarget.position.x - transform.position.x;
        sr.flipX = dx < 0;
    }

    IEnumerator DoAttack()
    {
        isAttacking = true;
        canAttack = false;

        animator.SetTrigger("Attack");

        float estimatedAttackDuration = 0.6f;
        yield return new WaitForSeconds(estimatedAttackDuration);

        isAttacking = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag(playerTag))
        {
            if (other == detectionCollider)
            {
                OnPlayerDetected(other.transform);
            }
            else if (other == attackCollider)
            {
                OnPlayerEnterAttackRange();
            }
            else
            {
                OnPlayerDetected(other.transform);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag(playerTag))
        {
            if (other == detectionCollider)
            {
                OnPlayerLost();
            }
            else if (other == attackCollider)
            {
                OnPlayerExitAttackRange();
            }
        }
    }

    private void OnPlayerDetected(Transform player)
    {
        playerDetected = true;
        currentTarget = player;
        animator.SetTrigger("Detect");
    }

    private void OnPlayerLost()
    {
        playerDetected = false;
        currentTarget = null;
        playerInAttackRange = false;
        animator.SetFloat("Speed", 0f);
    }

    private void OnPlayerEnterAttackRange()
    {
        playerInAttackRange = true;
    }

    private void OnPlayerExitAttackRange()
    {
        playerInAttackRange = false;
    }

    public void ReceiveHit()
    {
        if (isDead) return;
        animator.SetTrigger("Hit");
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.SetTrigger("Die");
        animator.SetBool("IsDead", true);
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    public void OnDieAnimationEnd()
    {
        Destroy(gameObject);
    }
}