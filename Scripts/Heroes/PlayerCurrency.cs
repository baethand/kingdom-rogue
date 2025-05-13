using TMPro;
using UnityEngine;
using UnityEngine.UI; // Добавьте для работы с UI

public class PlayerCurrency : MonoBehaviour
{
    public static PlayerCurrency Instance; // Синглтон для удобного доступа
    
    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private string coinFormat = "Coins: {0}"; // Формат отображения
    
    private int coins = 0;
    
    public int Coins {
        get => coins;
        private set {
            coins = value;
            UpdateCoinUI(); // Обновляем UI при изменении
        }
    }
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    public void AddCoins(int amount) {
        Coins += amount;
    }
    
    private void UpdateCoinUI() {
        if (coinText != null) {
            coinText.text = string.Format(coinFormat, Coins);
        }
    }
    
    // Для тестирования - можно удалить
    private void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            AddCoins(10); // Тестовая команда
        }
    }
}