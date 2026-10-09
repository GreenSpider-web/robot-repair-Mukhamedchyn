using UnityEngine;

public class SpeedModifierZone : MonoBehaviour
{
    [Tooltip("Множник швидкості: 0.5f — уповільнення вдвічі, 2.0f — прискорення вдвічі")]
    public float speedMultiplier = 0.5f;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.speed *= speedMultiplier; // Уповільнюємо або прискорюємо
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        PlayerController controller = other.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.speed /= speedMultiplier; // Повертаємо початкову швидкість
        }
    }
}