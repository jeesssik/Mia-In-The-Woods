using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class FlorController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float stopDistance = 0.2f;

    [Header("Rangos (child colliders)")]
    public Collider2D detectionCollider; // (child) trigger
    public Collider2D attackCollider;    // (child) trigger

    [Header("Targets & Settings")]
    public Transform currentTarget; // normalmente el player (se setea al detectar)
    public string playerTag = "Player";

    // Internos
    private Animator animator;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    [HideInInspector] public bool playerDetected = false;
    [HideInInspector] public bool playerInAttackRange = false;
    public bool isAttacking = false;
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
            FaceTarget();

            // Si está en rango de ataque, empezamos la rutina de ataque
            if (playerInAttackRange)
            {
                // dejamos de movernos porque playerInAttackRange bloquea FixedUpdate movement
                if (canAttack)
                    StartCoroutine(DoAttack());
            }
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // Mover solo cuando detectó, hay target, no está atacando y NO está en rango de ataque
        if (playerDetected && currentTarget != null && !isAttacking && !playerInAttackRange)
        {
            float dx = currentTarget.position.x - rb.position.x;
            float absDx = Mathf.Abs(dx);

            if (absDx > stopDistance)
            {
                Vector2 targetPos = new Vector2(currentTarget.position.x, rb.position.y);
                float step = moveSpeed * Time.fixedDeltaTime;
                Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, step);
                rb.MovePosition(newPos);
            }
        }
    }

    private void UpdateAnimatorSpeed()
    {
        float speedValue = 0f;
        if (playerDetected && currentTarget != null && !isAttacking)
        {
            float dx = Mathf.Abs(currentTarget.position.x - transform.position.x);
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

        // opcional: llamar EnableHitbox via AnimationEvent o usar aquí para breve demo:
        // EnableHitbox();

        float estimatedAttackDuration = 0.6f;
        yield return new WaitForSeconds(estimatedAttackDuration);

        isAttacking = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // --- Métodos públicos llamados por TriggerDelegator en los colliders hijos ---

    public void OnDetectionEnter(Transform player)
    {
        playerDetected = true;
        currentTarget = player;
        animator.SetTrigger("Detect");
    }

    public void OnDetectionExit(Transform player)
    {
        // si sale el player detectado, reiniciar
        playerDetected = false;
        currentTarget = null;
        playerInAttackRange = false;
        animator.SetFloat("Speed", 0f);
    }

    public void OnAttackEnter(Transform player)
    {
        // Al entrar en el rango de ATTACK: deja de correr y empezará a atacar (Update gestionará DoAttack)
        playerInAttackRange = true;
        currentTarget = player;
        // opcional: forzar que deje de moverse
        // isAttacking = false; // no lo ponemos true aquí; DoAttack lo hace
        Debug.Log("Flor: jugador en rango de ataque -> dejar de correr, empezar ataque");
    }

    public void OnAttackExit(Transform player)
    {
        playerInAttackRange = false;
    }

    // --- Daño y muerte ---
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

    // Método opcional llamado por Animation Event
    public void EndAttack()
    {
        isAttacking = false;
    }

    public void OnDieAnimationEnd()
    {
        Destroy(gameObject);
    }

    // Método que el Animation Event puede llamar para aplicar daño al jugador
    public void DamagePlayer()
    {
        if (currentTarget != null)
        {
            Mia player = currentTarget.GetComponent<Mia>();
            if (player != null)
                player.RecibirDaño(1);
        }
    }
}
