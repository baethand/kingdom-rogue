using UnityEngine;

public class Coin : MonoBehaviour {
    [SerializeField] private int value = 1;
    [SerializeField] private float collectRadius = 0.5f;
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float timeBeforeAttract = 0.5f;
    
    private Transform player;
    private bool canMoveToPlayer = false;
    private Rigidbody2D rb;
    
    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        Invoke(nameof(EnableAttraction), timeBeforeAttract);
    }
    
    private void Update() {
        if (canMoveToPlayer && player != null) {
            // Плавное движение к игроку
            transform.position = Vector3.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
            
            // Уничтожение при достижении игрока
            if (Vector2.Distance(transform.position, player.position) < collectRadius) {
                CollectCoin();
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            player = other.transform;
            CollectCoin();
        }
    }
    
    private void EnableAttraction() {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        canMoveToPlayer = true;
        
        // Уменьшаем физическое воздействие
        if (rb != null) {
            rb.linearVelocity *= 0.3f;
            rb.gravityScale = 0;
        }
    }
    
    private void CollectCoin() {
    if (PlayerCurrency.Instance != null) {
        PlayerCurrency.Instance.AddCoins(value);
    }
    Destroy(gameObject);
}
}