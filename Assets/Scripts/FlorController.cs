using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
public class FlorController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float stopDistance = 0.2f; // distancia mínima antes de superponer

    [Header("Rangos (child colliders)")]
    public Collider2D detectionCollider; // CircleCollider2D (IsTrigger)
    public Collider2D attackCollider;    // CircleCollider2D (IsTrigger)

    [Header("Targets & Settings")]
    public Transform currentTarget; // normalmente el player
    public string playerTag = "Player";

    // Internos
    private Animator animator;
    private SpriteRenderer sr;
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

        if (detectionCollider == null || attackCollider == null)
            Debug.LogWarning("Asigná Detection y Attack colliders en el inspector.");
    }

    void Update()
    {
        if (isDead) return;

        UpdateAnimatorSpeed();

        if (playerDetected && currentTarget != null && !isAttacking)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.position);

            // Mirar al jugador
            FaceTarget();

            // Si no está aún en rango de ataque, mover hacia el jugador
            if (!playerInAttackRange)
            {
                // mover hacia target pero detenerse un poco si querés (stopDistance)
                if (dist > stopDistance)
                {
                    Vector2 newPos = Vector2.MoveTowards(transform.position,
                    currentTarget.position,
                    moveSpeed * Time.deltaTime);
                    transform.position = newPos;
                }
            }
            else
            {
                // En rango de ataque: intentar atacar
                if (canAttack)
                    StartCoroutine(DoAttack());
            }
        }
    }

    private void UpdateAnimatorSpeed()
    {
        // Speed será la magnitud horizontal del movimiento objetivo (más simple)
        float speedValue = 0f;
        if (playerDetected && currentTarget != null && !isAttacking)
        {
            float dist = Vector2.Distance(transform.position, currentTarget.position);
            // definimos speed en una escala simple
            speedValue = Mathf.Clamp01(dist / 1.5f) * moveSpeed;
        }
        animator.SetFloat("Speed", speedValue);
    }

    private void FaceTarget()
    {
        if (currentTarget == null) return;
        float dx = currentTarget.position.x - transform.position.x;
        // si dx < 0 -> player a la izquierda -> flipX true (tenés sprite mirando a la derecha por defecto)
        sr.flipX = dx < 0;
    }

    IEnumerator DoAttack()
    {
        isAttacking = true;
        canAttack = false;

        // Disparamos el Trigger de ataque en el Animator
        animator.SetTrigger("Attack");

        // Esperamos la duración real del ataque: lo ideal es usar Animation Event para EndAttack().
        // Aquí ponemos un fallback: si no hay Animation Event, esperamos un tiempo (ajustar).
        float estimatedAttackDuration = 0.6f; // ajustar según sprite/anim
        yield return new WaitForSeconds(estimatedAttackDuration);

        // Fin de ataque
        isAttacking = false;

        // Cooldown
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // Estos métodos son llamados por los child trigger colliders (delegación),
    // o podemos recibir los OnTrigger en el parent si el parent tiene Rigidbody2D + uno de los child con IsTrigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag(playerTag))
        {
            // Determinar si entró en detection o attack (según qué collider fue)
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
                // Si el player tiene varios colliders, alternativamente chequeamos distancia
                // Pero lo ideal es asignar colliders únicos en inspector.
                // Fallback:
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

    // --- Eventos de rango ---
    private void OnPlayerDetected(Transform player)
    {
        playerDetected = true;
        currentTarget = player;
        // lanzar animación Detect (Trigger)
        animator.SetTrigger("Detect");
        // Si detect anim dura y querés empezar a correr después, el transition Detect->Run deberá esperar a Speed>0.1 o usar Animation Event.
        // Aquí, podríamos comenzar a moverse inmediatamente: activar Speed en Update suficiente para pasar a Run.
    }

    private void OnPlayerLost()
    {
        playerDetected = false;
        currentTarget = null;
        playerInAttackRange = false;
        // Reset Speed -> Idle automáticamente
        animator.SetFloat("Speed", 0f);
    }

    private void OnPlayerEnterAttackRange()
    {
        playerInAttackRange = true;
        // activamos ataque desde Update/coroutine
    }

    private void OnPlayerExitAttackRange()
    {
        playerInAttackRange = false;
    }


    // --- Daño y muerte ---
    public void ReceiveHit()
    {
        if (isDead) return;
        animator.SetTrigger("Hit");
        // aplicar logica de vida, etc.
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.SetTrigger("Die");
        animator.SetBool("IsDead", true);
        // opcional: destruir el objeto al final de la animación con Animation Event
    }

    // Método opcional llamado por un Animation Event al final del Attack
    public void EndAttack()
    {
        isAttacking = false;
    }

    // Método opcional llamado por un Animation Event al final de la muerte
    public void OnDieAnimationEnd()
    {
        Destroy(gameObject);
    }
}

