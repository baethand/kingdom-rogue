using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private CharacterController player;
    [SerializeField] private Image totalHealthBar;
    [SerializeField] private Image currentHealthBar;

    private void Start()
    {
        totalHealthBar.fillAmount = 1;
    }

    private void Update()
    {
        currentHealthBar.fillAmount = player.currentHealth / 6;
    }


}