using UnityEngine;
using UnityEngine.UI;

public class FlowerHealthUI : MonoBehaviour
{
    [Tooltip("Asignar los corazones (Image) en orden")]
    public Image[] hearts;

    /// <summary>
    /// Muestra u oculta corazones según la vida actual.
    /// </summary>
    public void UpdateHearts(int currentHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            hearts[i].gameObject.SetActive(i < currentHealth);
        }
    }
}
