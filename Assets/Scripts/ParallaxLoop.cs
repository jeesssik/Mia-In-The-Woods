using UnityEngine;

[System.Serializable]
public class ParallaxLayer
{
    public Transform layerTransform;
    [Range(0f, 1f)] public float parallaxFactor = 0.1f;
    [HideInInspector] public Vector3 startPosition;
}

public class ParallaxLoop : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private ParallaxLayer[] parallaxLayers;
    
    private Vector3 initialPlayerPosition;
    private bool isInitialized = false;void Start()
    {
        // Encontrar el jugador si no está asignado
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("ParallaxLoop: No se encontró el jugador con tag 'Player'");
                return;
            }
        }        Debug.Log($"ParallaxLoop: Jugador encontrado: {player.name}");
        Debug.Log($"ParallaxLoop: Número de capas configuradas: {parallaxLayers.Length}");        // La posición inicial se capturará en el primer LateUpdate
        // para asegurar que el jugador esté completamente inicializado
        Debug.Log($"ParallaxLoop: Esperando capturar posición inicial del jugador en LateUpdate");

        // Guardar las posiciones iniciales de cada capa
        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            if (parallaxLayers[i].layerTransform != null)
            {
                parallaxLayers[i].startPosition = parallaxLayers[i].layerTransform.position;
                Debug.Log($"Capa {i}: {parallaxLayers[i].layerTransform.name}, Factor: {parallaxLayers[i].parallaxFactor}, Pos inicial: {parallaxLayers[i].startPosition}");
            }
            else
            {
                Debug.LogWarning($"ParallaxLoop: La capa {i} no tiene Transform asignado");
            }
        }
    }    void LateUpdate()
    {
        if (player == null) return;

        // Si es la primera vez que se ejecuta LateUpdate, guardar la posición inicial
        if (!isInitialized)
        {
            initialPlayerPosition = player.position;
            isInitialized = true;
            Debug.Log($"ParallaxLoop: Posición inicial capturada en LateUpdate: {initialPlayerPosition}");
        }

        // Actualizar cada capa de parallax
        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            ParallaxLayer layer = parallaxLayers[i];
            if (layer.layerTransform == null) continue;            // Calcular el desplazamiento total del jugador desde su posición inicial
            float totalPlayerMovement = player.position.x - initialPlayerPosition.x;
            
            // Aplicar el factor de parallax al movimiento del jugador
            float parallaxMovement = totalPlayerMovement * layer.parallaxFactor;
            
            // Mover la capa en dirección opuesta al jugador
            Vector3 newPosition = new Vector3(
                layer.startPosition.x - parallaxMovement,
                layer.startPosition.y,
                layer.layerTransform.position.z
            );
            
            layer.layerTransform.position = newPosition;

            // Debug temporal (comentar después de verificar que funciona)
            if (Time.frameCount % 60 == 0) // Solo cada 60 frames para no saturar el log
            {
                Debug.Log($"Capa {i}: Player pos: {player.position.x}, Initial: {initialPlayerPosition.x}, Movement: {totalPlayerMovement}, Parallax: {parallaxMovement}, New pos: {newPosition.x}");
            }
        }
    }
}