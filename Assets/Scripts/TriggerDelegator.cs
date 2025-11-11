using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class TriggerDelegator : MonoBehaviour
{
    public enum TriggerType { Detection, Attack }
    public TriggerType triggerType = TriggerType.Detection;

    [Tooltip("Asigná el FlorController (normalmente el parent)")]
    public FlorController florController;

    private void Reset()
    {
        // intento automático de enlazar al padre
        if (florController == null && transform.parent != null)
            florController = transform.parent.GetComponent<FlorController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (florController == null) return;
        if (!other.CompareTag(florController.playerTag)) return;

        if (triggerType == TriggerType.Detection)
            florController.OnDetectionEnter(other.transform);
        else
            florController.OnAttackEnter(other.transform);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (florController == null) return;
        if (!other.CompareTag(florController.playerTag)) return;

        if (triggerType == TriggerType.Detection)
            florController.OnDetectionExit(other.transform);
        else
            florController.OnAttackExit(other.transform);
    }
}
