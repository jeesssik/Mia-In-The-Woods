using UnityEngine;

public class ParallaxLoop : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField, Range(0f, 1f)] private float parallaxFactor = 0.1f;

    private float startX;
    private float startY;

    void Start()
    {
        startX = transform.position.x;
        startY = transform.position.y;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        // Desplazamiento horizontal inverso al jugador
        float distanceX = (player.position.x * parallaxFactor);
        transform.position = new Vector3(startX - distanceX, startY, transform.position.z);
    }
}