using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Mia : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 6f;

    [Header("Salto")]
    [SerializeField] private float fuerzaSalto = 12f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadio = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ataque")]
    [SerializeField] private float ataqueCooldown = 0.3f;

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 3;
    private int vidaActual;

    [Header("Hitbox (asignar en inspector)")]
    [Tooltip("Arrastrar el child AttackHitbox (GameObject) aquí")]
    [SerializeField] private GameObject attackHitbox;
    [Tooltip("Duración estimada de la ventana de impacto (fallback si el AnimationEvent no se dispara)")]
    [SerializeField] private float estimatedAttackDuration = 0.35f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool mirandoDerecha = true;
    private float inputX;
    private float proximoAtaque;
    private bool estaMuerta = false;
    private bool estaSaltando = false;
    private bool estabaEnSuelo = true; // detectar aterrizaje

    private Coroutine attackRoutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        vidaActual = vidaMaxima;

        // Asegurarnos que el hitbox esté desactivado por defecto
        if (attackHitbox != null)
            attackHitbox.SetActive(false);

            if (HealthUI.Instance != null)
    HealthUI.Instance.UpdateHearts(vidaActual);
    }

    void Update()
    {
        if (estaMuerta) return;

        // Movimiento lateral (A/D o flechas)
        inputX = Input.GetAxisRaw("Horizontal");

        // Salto con barra espaciadora
        if (Input.GetKeyDown(KeyCode.Space))
            IntentarSaltar();

        // Ataque con click izquierdo
        if (Input.GetMouseButtonDown(0))
            IntentarAtacar();

        VoltearSiEsNecesario();
        ActualizarAnimaciones();
    }

    void FixedUpdate()
    {
        if (estaMuerta) return;
        rb.velocity = new Vector2(inputX * velocidad, rb.velocity.y);
    }

    private void IntentarSaltar()
    {
        if (EstaEnSuelo() && !estaSaltando)
        {
            estaSaltando = true;

            var v = rb.velocity;
            v.y = 0f;
            rb.velocity = v;

            rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);

            if (animator)
                animator.SetTrigger("Jump");
        }
    }

    private void IntentarAtacar()
    {
        if (Time.time < proximoAtaque || !EstaEnSuelo()) return;
        proximoAtaque = Time.time + ataqueCooldown;

        if (animator)
            animator.SetTrigger("Attack");

        // Fallback: activamos un coroutine que active el hitbox y lo desactive luego.
        if (attackRoutine != null) StopCoroutine(attackRoutine);
        attackRoutine = StartCoroutine(AttackHitboxRoutine());
    }

    // Coroutine fallback: activa el hitbox por un lapso estimado y luego lo desactiva.
    private IEnumerator AttackHitboxRoutine()
    {
        if (attackHitbox == null) yield break;

        attackHitbox.SetActive(true);
        yield return new WaitForSeconds(estimatedAttackDuration);
        attackHitbox.SetActive(false);
        attackRoutine = null;
    }

    private bool EstaEnSuelo()
    {
        if (!groundCheck) return true;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadio, groundLayer);
    }

    private void VoltearSiEsNecesario()
    {
        if (inputX > 0 && !mirandoDerecha) Voltear();
        else if (inputX < 0 && mirandoDerecha) Voltear();
    }

    private void Voltear()
    {
        mirandoDerecha = !mirandoDerecha;
        var escala = transform.localScale;
        escala.x *= -1f;
        transform.localScale = escala;
    }

    private void ActualizarAnimaciones()
    {
        if (!animator) return;

        bool enSuelo = EstaEnSuelo();
        bool estaCayendo = rb.velocity.y < -0.1f && !enSuelo;
        if (enSuelo)
        {
            estaSaltando = false;
        }

        // Detectar aterrizaje
        if (!estabaEnSuelo && enSuelo)
        {
            animator.SetTrigger("Land");
        }

        estabaEnSuelo = enSuelo;

        // Parámetros comunes
        animator.SetFloat("Speed", EstaEnSuelo() ? Mathf.Abs(inputX) : 0f);
        animator.SetBool("IsGrounded", enSuelo);
        animator.SetBool("IsFalling", estaCayendo);
        animator.SetFloat("YVelocity", rb.velocity.y);
    }

    public void RecibirDaño(int cantidad)
{
    if (estaMuerta) return;

    vidaActual -= cantidad;

    // Actualizar UI
    if (HealthUI.Instance != null)
        HealthUI.Instance.UpdateHearts(vidaActual);

    if (vidaActual <= 0)
    {
        Muerte();
    }
    else
    {
        if (animator) animator.SetTrigger("Hurt");
    }
}

    private void Muerte()
    {
        estaMuerta = true;
        rb.velocity = Vector2.zero;
        if (animator) animator.SetBool("Dead", true);
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadio);
    }

    // -------------------------
    // Hitbox control (Animation Events + fallback)
    // -------------------------

    // Método llamado por Animation Event para activar el hitbox
    public void ActivarHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
            Debug.Log("Hitbox activada (AnimationEvent)");
        }

        // Si existe fallback en ejecución, cancelarlo (porque el Event se encargó)
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    // Método llamado por Animation Event para desactivar el hitbox
    public void DesactivarHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
            Debug.Log("Hitbox desactivada (AnimationEvent)");
        }

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }
}