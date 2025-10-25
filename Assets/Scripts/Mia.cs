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

    private Rigidbody2D rb;
    private Animator animator;
    private bool mirandoDerecha = true;
    private float inputX;
    private float proximoAtaque;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Movimiento lateral (flechas o A/D)
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
        if (Time.time < proximoAtaque) return;
        proximoAtaque = Time.time + ataqueCooldown;

        if (animator) animator.SetTrigger("Attack");
        // TODO: activar hitbox/daño aquí
    }

    private bool EstaEnSuelo()
    {
        if (!groundCheck) return true; // Permite probar sin asignar groundCheck
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
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadio);
    }
}