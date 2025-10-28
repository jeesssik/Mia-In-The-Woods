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
    [SerializeField] private float groundCheckRadio = 0.12f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Ataque")]
    [SerializeField] private float ataqueCooldown = 0.3f;

    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 3;
    private int vidaActual;

    private Rigidbody2D rb;
    private Animator animator;
    private bool mirandoDerecha = true;
    private float inputX;
    private float proximoAtaque;
    private bool estaMuerta = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        vidaActual = vidaMaxima;
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
        if (EstaEnSuelo())
        {
            var v = rb.velocity; v.y = 0f; rb.velocity = v;
            rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
            if (animator) animator.SetTrigger("Jump");
        }
    }

    private void IntentarAtacar()
    {
        if (Time.time < proximoAtaque || !EstaEnSuelo()) return;
        proximoAtaque = Time.time + ataqueCooldown;

        if (animator) animator.SetTrigger("Attack");
        // Aquí podrías activar una hitbox o llamar a un script de daño
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

        animator.SetFloat("Speed", Mathf.Abs(inputX));
        animator.SetBool("IsGrounded", EstaEnSuelo());

        // Detectar si está cayendo
        bool estaCayendo = rb.velocity.y < -0.1f && !EstaEnSuelo();
        animator.SetBool("IsFalling", estaCayendo);
    }

    public void RecibirDaño(int cantidad)
    {
        if (estaMuerta) return;

        vidaActual -= cantidad;
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
        // Desactivar colisiones o controles si querés
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadio);
    }
}