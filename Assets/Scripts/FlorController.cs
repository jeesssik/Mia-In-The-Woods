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



    public float fadeDuration = 0.6f;

    public bool doShrink = true;

    public float finalScale = 0.6f;

    public bool fadeFlowerUI = true;

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
        Debug.Log("Flor: OnDetectionEnter llamado");
        playerDetected = true;
        currentTarget = player;
        animator.SetTrigger("Detect");

        if (flowerUI != null)
        {
            flowerUI.SetActive(true);
            Debug.Log("Flor: flowerUI activado");
        }
        else
        {
            Debug.LogWarning("Flor: flowerUI no asignado en el inspector!");
        }
    }

    public void OnDetectionExit(Transform player)
    {
        Debug.Log("Flor: OnDetectionExit llamado");
        playerDetected = false;
        currentTarget = null;
        playerInAttackRange = false;
        animator.SetFloat("Speed", 0f);

        if (flowerUI != null)
        {
            flowerUI.SetActive(false);
            Debug.Log("Flor: flowerUI desactivado");
        }
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

        Debug.Log($"Flor: recibió {damage} de daño. Vida actual = {currentHealth}");

        animator.SetTrigger("Hit");

        if (flowerHealthUI != null)
            flowerHealthUI.UpdateHearts(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // --- morir con fallback a anim.Play y espera dinámica ---
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("Flor.Die() llamada");

        // apagar colisiones y física para que no interfieran
        Collider2D c = GetComponent<Collider2D>();
        if (c != null) c.enabled = false;
        if (rb != null) rb.simulated = false;

        // ocultar UI (si querés que desaparezca al morir)
        if (flowerUI != null) flowerUI.SetActive(false);

        // Intentamos primero con parámetros (por si el Animator está bien configurado)
        try
        {
            animator.SetBool("IsDead", true);
            animator.SetTrigger("Die");
        }
        catch { }

        // FORZAR reproducción directa del clip "Flor-Explodes" como fallback
        // (asegurate que el nombre coincide exactamente con el state/clip)
        string deathClipName = "Flor-Explodes"; // ajustá si tu clip se llama distinto
        animator.Play(deathClipName, 0, 0f);

        // Esperar a que termine el clip y destruir
        StartCoroutine(WaitAnimationAndDestroy(deathClipName));
    }

    private IEnumerator WaitAnimationAndDestroy(string clipName)
    {
        // --- 1) obtener la duración del clip (fallback) ---
        float clipLength = 0.75f; // fallback
        var runtime = animator.runtimeAnimatorController;
        if (runtime != null)
        {
            foreach (var clip in runtime.animationClips)
            {
                if (clip.name == clipName)
                {
                    clipLength = clip.length;
                    break;
                }
            }
        }

        // esperar que termine la animación (si la reproducimos con Play, ya arrancó)
        yield return new WaitForSeconds(clipLength);

        // --- 2) preparar para el fade: obtener todos los SpriteRenderers ---
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>(true);

        // si hay UI y queremos hacer fade y no tiene CanvasGroup, intentamos agregar uno
        CanvasGroup uiCg = null;
        if (flowerUI != null && fadeFlowerUI)
        {
            uiCg = flowerUI.GetComponent<CanvasGroup>();
            if (uiCg == null)
            {
                uiCg = flowerUI.AddComponent<CanvasGroup>();
                // si el UI debería estar oculto después de morir, lo dejamos interactable false
                uiCg.interactable = false;
                uiCg.blocksRaycasts = false;
            }
        }

        // valores iniciales
        float elapsed = 0f;

        // guardamos colores originales (para restaurar alfa relativo)
        Color[] originalColors = new Color[srs.Length];
        for (int i = 0; i < srs.Length; i++)
            originalColors[i] = srs[i].color;

        Vector3 startScale = transform.localScale;
        Vector3 targetScale = doShrink ? startScale * finalScale : startScale;

        // --- 3) fade loop ---
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float a = Mathf.Lerp(1f, 0f, t);

            // aplicar alfa a todos los sprite renderers
            for (int i = 0; i < srs.Length; i++)
            {
                if (srs[i] == null) continue;
                Color c = originalColors[i];
                c.a = a;
                srs[i].color = c;
            }

            // shrink
            if (doShrink)
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            // fade UI
            if (uiCg != null)
                uiCg.alpha = 1f - t;

            yield return null;
        }

        // asegurar valores finales
        for (int i = 0; i < srs.Length; i++)
        {
            if (srs[i] == null) continue;
            Color c = originalColors[i];
            c.a = 0f;
            srs[i].color = c;
        }
        if (doShrink) transform.localScale = targetScale;
        if (uiCg != null) uiCg.alpha = 0f;

        // espera cortita opcional para que el fade termine visualmente
        yield return new WaitForSeconds(0.02f);

        Destroy(gameObject);
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
