using UnityEngine;

public class MiaAttackHitbox : MonoBehaviour
{
    [Tooltip("Daño que hace este hitbox")]
    public int damage = 1;

    // evita pegar varias veces en la misma ventana de ataque
    private bool alreadyHitThisSwing = false;

    private void OnEnable()
    {
        // cada vez que el hitbox se activa, reseteamos la bandera
        alreadyHitThisSwing = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyHitThisSwing) return;

        // busco si el collider pertenece a una flor (o a su padre)
        FlorController flor = other.GetComponentInParent<FlorController>();
        if (flor != null)
        {
            // Llamo al método público que reduce vida en la flor
            flor.ReceiveHit(damage); // <-- usaremos la versión con parámetro
            alreadyHitThisSwing = true;
            Debug.Log($"AttackHitbox: golpeó a {flor.name} por {damage}");
            return;
        }
    }
}
