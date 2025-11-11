using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class FlorController : MonoBehaviour
{
   
    [Header("Movimiento")]
    public float moveSpeed = 2f;
    public float stopDistance = 0.2f;

    [Header("Rangos (child colliders)")]
    public Collider2D detectionCollider; // asignar child trigger
    public Collider2D attackCollider;    // asignar child trigger

    [Header("Targets & Settings")]
    public Transform currentTarget; // normalmente el player (se setea al detectar)
    public string playerTag = "Player";

    [Header("UI")]
    [Tooltip("Asignar el GameObject del UI que muestra la vida de la flor (ej: FlorHealth)")]
    public GameObject flowerUI; // GameObject que contiene FlowerHealthUI

    [Header("Vida")]
    public int maxHealth = 3;

    [Header("Cooldowns (ataque)")]
    public float minAttackCooldown = 1.5f;
    public float maxAttackCooldown = 3.0f;

    [Header("Attack Timing")]
    [Tooltip("Tiempo hasta el frame de impacto dentro de la animación (segundos)")]
    public float impactDelay = 0.25f;
    [Tooltip("Tiempo estimado de duración de la animación de ataque (fallback)")]
    public float estimatedAttackDuration = 0.4f;

    // INTERNOS
    private Animator animator;
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    [HideInInspector] public bool playerDetected = false;
    [HideInInspector] public bool playerInAttackRange = false;

    public bool isAttacking = false;
    private bool isDead = false;
    private bool canAttack = true;

    // vida
    private int currentHealth;
    private FlowerHealthUI flowerHealthUI;

    void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (detectionCollider == null || attackCollider == null)
            Debug.LogWarning("Asigná Detection y Attack colliders en el inspector.");

        // inicializar vida
        currentHealth = Mathf.Max(1, maxHealth);

        // configuracion UI
        if (flowerUI != null)
        {
            // ocultar por defecto
            flowerUI.SetActive(false);
            // cachear componente UI
            flowerHealthUI = flowerUI.GetComponent<FlowerHealthUI>();
            if (flowerHealthUI != null)
                flowerHealthUI.UpdateHearts(currentHealth);
        }
    }

    void Update()
    {
        if (isDead) return;

        UpdateAnimatorSpeed();

        if (playerDetected && currentTarget != null && !isAttacking)
        {
            FaceTarget();

            // Si está en rango de ataque, intentamos atacar
            if (playerInAttackRange)
            {
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
        // protección: evitar múltiples corrutinas
        if (isAttacking || !canAttack) yield break;

        isAttacking = true;
        canAttack = false;

        animator.SetTrigger("Attack");

        // Esperar hasta el frame del golpe (ajustar según tu animación)
        yield return new WaitForSeconds(impactDelay);

        // Aplicar daño si sigue en rango
        if (playerInAttackRange && currentTarget != null)
            DamagePlayer();

        // Esperar el resto de la animación (fallback)
        yield return new WaitForSeconds(estimatedAttackDuration);

        isAttacking = false;

        // cooldown aleatorio entre ataques
        float cd = Random.Range(minAttackCooldown, maxAttackCooldown);
        yield return new WaitForSeconds(cd);
        canAttack = true;
    }

    // ------------- Detección (estos métodos los llama el TriggerDelegator en los child triggers) -------------
    public void OnDetectionEnter(Transform player)
    {
        playerDetected = true;
        currentTarget = player;
        animator.SetTrigger("Detect");

        // Mostrar UI cuando detecta al jugador
        if (flowerUI != null)
            flowerUI.SetActive(true);
    }

    public void OnDetectionExit(Transform player)
    {
        // si sale el player detectado, reiniciar
        playerDetected = false;
        currentTarget = null;
        playerInAttackRange = false;
        animator.SetFloat("Speed", 0f);

        // Ocultar UI cuando pierde al jugador
        if (flowerUI != null)
            flowerUI.SetActive(false);
    }

    public void OnAttackEnter(Transform player)
    {
        // Al entrar en el rango de ATTACK: deja de correr y empezará a atacar (Update gestionará DoAttack)
        playerInAttackRange = true;
        currentTarget = player;
    }

    public void OnAttackExit(Transform player)
    {
        playerInAttackRange = false;
    }

    // ------------- Vida, recibir daño y morir -------------
    // Este método será llamado por el Hitbox del jugador (ej. AttackHitbox.cs)
    public void ReceiveHit(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        // animación de hit
        animator.SetTrigger("Hit");

        // actualizar UI local si existe
        if (flowerHealthUI != null)
            flowerHealthUI.UpdateHearts(currentHealth);

        Debug.Log($"Flor: recibió {damage} de daño. Vida actual = {currentHealth}");

        if (currentHealth <= 0)
            Die();
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.SetTrigger("Die");
        animator.SetBool("IsDead", true);

        // ocultar UI al morir
        if (flowerUI != null)
            flowerUI.SetActive(false);
    }

    // Llamado por Animation Event al finalizar la muerte (si lo tenés configurado)
    public void OnDieAnimationEnd()
    {
        Destroy(gameObject);
    }

    // Llamado por Animation Event al finalizar el ataque (opcional)
    public void EndAttack()
    {
        isAttacking = false;
    }

    // Aplica daño al player (llamado durante DoAttack en el frame de impacto)
    private void DamagePlayer()
    {
        if (currentTarget != null)
        {
            Mia player = currentTarget.GetComponent<Mia>();
            if (player != null)
            {
                player.RecibirDaño(1);
                Debug.Log("🌼 La flor dañó a Mia (-1 vida)");
            }
        }
    }
}
