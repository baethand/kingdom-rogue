using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private bool inputEnabled = true;
    
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }
    
    private void Update()
    {
        if (!inputEnabled) return;
        
        // Обработка обычного ввода игрока
    }
}