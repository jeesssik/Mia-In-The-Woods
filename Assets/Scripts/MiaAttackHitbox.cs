using UnityEngine;

public class MiaAttackHitbox : MonoBehaviour
{
    public int damageToEnemy = 1; // daño del ataque

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si golpea a la flor, ejecuta su método ReceiveHit()
        FlorController flor = collision.GetComponent<FlorController>();
        if (flor != null)
        {
            flor.ReceiveHit(); // aquí podés pasar daño si lo manejás por vida
        }
    }
}
