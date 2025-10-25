using UnityEngine;

public class ParallaxLoop : MonoBehaviour
{
    public Transform player;
    [Range(0f, 1f)] public float parallaxEffect = 0.5f;

    private float startPosX;
    private float length;

    void Start()
    {
        startPosX = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float dist = player.position.x * parallaxEffect;
        transform.position = new Vector3(startPosX + dist, transform.position.y, transform.position.z);

        float temp = player.position.x * (1 - parallaxEffect);

        if (temp > startPosX + length)
        {
            startPosX += length;
        }
        else if (temp < startPosX - length)
        {
            startPosX -= length;
        }
    }
}