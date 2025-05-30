using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyController : EntityWithHealth {

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private float patrolRange = 5f; // Диапазон патрулирования
    [SerializeField] private bool isPatrolling = true; // Патрулирует ли враг
    [SerializeField] private bool isStationary = false; // Стоит ли враг на месте

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public float jumpCooldown = 2f;
    private float lastJumpTime;

    [Header("Combat Settings")]
    public bool isRanged = false;
    public float attackRange = 2f; // Дистанция атаки
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Wall Check")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;

    [Header("Aggression Settings")]
    [SerializeField] private float aggroRange = 8f; // Радиус агрессии
    [SerializeField] private float chaseSpeedMultiplier = 1.5f; // Увеличение скорости при преследовании

    [Header("Knockback Settings")]
    public bool isKnockbacking = true;
    public float knockbackForce = 5f;
    public float knockbackForceY = 1f;
    public float stunDuration = 0.3f;

    [Header("Can cnahge color Settings")]
    public bool isReadyToChangeColor = true;


    [Header("Contact Damage Settings")]
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float contactKnockbackForce = 5f; 
    [SerializeField] private float contactCheckRadius = 1f;
    [SerializeField] private float contactCooldown = 1f;

    [Header("Death Effects")]
    [SerializeField] private ParticleSystem essenceParticles;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinsAmount = 3;
    [SerializeField] private float coinForce = 5f;
    [SerializeField] private float essenceParticlesDuration = 1.5f;

    [Header("Boss Settings")]
    [SerializeField] private bool isBoss = false; // Отметка, является ли враг боссом
    private GameObject victoryPanelPrefab; // Префаб панели победы
    [SerializeField] private string sceneToReload = "MainMenu"; // Имя сцены для перезагрузки
    
    private float lastContactTime;  
    private bool isStunned;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool facingRight = true;
    private Vector2 patrolStartPoint; // Начальная точка патрулирования
    private Transform player; // Ссылка на игрока
    private SpriteRenderer sprite;
    private Animator animator;

    private States State {
        get {
            return (States) animator.GetInteger("State");
        }

        set {
            animator.SetInteger("State", (int) value);
        }
    }

    protected override void Awake() {
        base.Awake();
        sprite = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        victoryPanelPrefab = GameObject.FindGameObjectWithTag("victoryPanel");
        if (victoryPanelPrefab)
            victoryPanelPrefab.SetActive(false);
    }

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        lastJumpTime = -jumpCooldown;
        lastAttackTime = -attackCooldown;
        patrolStartPoint = transform.position; // Запоминаем начальную точку патрулирования
        player = FindAnyObjectByType<CharacterController>().transform;
        
    }

    void Update() {

        if (isStunned) return;

        updateRaycasts();
        HandleMovement();
        HandleCombat();
        CheckPlayerContact();
    }

    void updateRaycasts() {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * wallCheck.transform.localScale.x , wallCheckDistance, wallLayer);

    }

    void HandleMovement() {
        if (isStationary) {
            changeStateWithPriority(States.IDLE);

            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= aggroRange) {
            // Если игрок в радиусе агрессии, преследуем его
            ChasePlayer();
        } else if (isPatrolling) {
            // Если игрок вне радиуса агрессии и враг патрулирует, продолжаем патрулирование
            Patrol();
        }
    }

    void Patrol() {
        // Двигаемся вправо или влево в пределах patrolRange
        changeStateWithPriority(States.RUN);
        if (facingRight) {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            if ((transform.position.x >= patrolStartPoint.x + patrolRange) || isTouchingWall) {
                Flip();
            }
        } else {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
            if ((transform.position.x <= patrolStartPoint.x - patrolRange) || isTouchingWall) {
                Flip();
            }
        }

        // Проверяем, нужно ли прыгать
        if (ShouldJump()) {
            Jump();
        }
    }

    void ChasePlayer() {
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange) {
            // Если игрок в радиусе атаки, останавливаемся и атакуем
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        } else {
            // Если игрок вне радиуса атаки, но в радиусе агрессии, преследуем его
            State = States.RUN;
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(moveSpeed * chaseSpeedMultiplier * direction, rb.linearVelocity.y);

            // Поворачиваем врага в сторону игрока
            if ((direction > 0 && !facingRight) || (direction < 0 && facingRight)) {
                Flip();
            }

            // Проверяем, нужно ли прыгать
            if (ShouldJump()) {
                Jump();
            }
        }
    }

    bool ShouldJump() {
        if (!isGrounded || Time.time - lastJumpTime < jumpCooldown) return false;

        Vector2 rayOrigin = groundCheck.position;
        Vector2 rayDirection = facingRight ? Vector2.right : Vector2.left;
        float rayDistance = 1f;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, groundLayer);
        if (!hit) {
            return true;
        }

        return false;
    }

    void Jump() {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        lastJumpTime = Time.time;
    }

    private void CheckPlayerContact() {
        if (Time.time - lastContactTime < contactCooldown) return;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, contactCheckRadius);
        foreach (var hit in hitColliders) {
            if (hit.CompareTag("Player") && hit.TryGetComponent<CharacterController>(out var player)) {
                player.getDamage(contactDamage);
                if (player.isKnockbacking)
                    player.applyKnockback(transform, contactKnockbackForce);

                if (player)
                    lastContactTime = Time.time;
                break;
            }
        }
    }

    void HandleCombat() {
        if (Time.time - lastAttackTime < attackCooldown) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange) {
            if (isRanged) {
                // Логика для дальнего боя (например, выстрел снарядом)
                RangedAttack();
            } else {
                // Логика для ближнего боя (например, удар)
                MeleeAttack();
            }
            lastAttackTime = Time.time;
        }
    }

    protected override void die() {
        base.die();
        
        
        // Отключаем коллайдеры и физику
        GetComponent<Collider2D>().enabled = false;
        if (TryGetComponent<Rigidbody2D>(out var rb)) rb.simulated = false;
        
        // Отключаем визуал и скрипты
        if (TryGetComponent<SpriteRenderer>(out var sprite)) sprite.enabled = false;
        enabled = false;
        
        // Запускаем эффекты смерти
        SpawnDeathEffects();

        if (isBoss)
        {
            ShowVictory();
        }
        
        // Уничтожаем объект врага
        Destroy(gameObject, essenceParticlesDuration + 0.5f); // Даем время на проигрывание эффектов
    }

    private void SpawnDeathEffects() {
        // Спавн частиц эссенции
        if (essenceParticles != null) {
            ParticleSystem particles = Instantiate(essenceParticles, transform.position, Quaternion.identity);
            particles.Play();
            Destroy(particles.gameObject, essenceParticlesDuration);
        }
        
        // Спавн монеток
        SpawnCoins();
    }

    private void SpawnCoins() {
        if (coinPrefab == null) return;
        
        for (int i = 0; i < coinsAmount; i++) {
            GameObject coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
            Rigidbody2D rb = coin.GetComponent<Rigidbody2D>();
            
            // Случайное направление с силой
            Vector2 randomDirection = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1f)
            ).normalized;
            
            rb.AddForce(randomDirection * coinForce, ForceMode2D.Impulse);
            
            // Случайное вращение
            float torque = Random.Range(-5f, 5f);
            rb.AddTorque(torque, ForceMode2D.Impulse);
        }
    }

    void RangedAttack() {
        Debug.Log("Ranged Attack!");
        // Здесь можно добавить логику для выстрела снарядом
    }

    void MeleeAttack() {
        Debug.Log("Melee Attack!");
        // Здесь можно добавить логику для удара ближнего боя
    }

    void Flip() {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void OnDrawGizmosSelected() {
        if (groundCheck != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(groundCheck.position, groundCheck.position + (facingRight ? Vector3.right : Vector3.left) * 1f);

        // Визуализация радиуса агрессии
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        // Визуализация радиуса атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Визуализация зоны патрулирования
        if (isPatrolling) {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, new Vector2(patrolStartPoint.x + patrolRange, transform.position.y));
            Gizmos.DrawLine(transform.position, new Vector2(patrolStartPoint.x - patrolRange, transform.position.y));
        }

        if (wallCheck != null) {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * wallCheck.transform.localScale.x * wallCheckDistance);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, contactCheckRadius);
    }

    public void applyKnockback(Transform damageSource, float customForce = -1f, float customForceY = -1f, float customStunDuration = -1f) {
        if (!isKnockbacking || isStunned) return;

        float forceX = customForce >= 0 ? customForce : knockbackForce;
        float forceY = customForceY >= 0 ? customForceY : knockbackForceY;
        float stunTime = customStunDuration >= 0 ? customStunDuration : stunDuration;

        Vector2 direction = damageSource != null 
            ? (transform.position - damageSource.position).normalized 
            : Vector2.up;
        
        direction.y = forceY;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction * forceX, ForceMode2D.Impulse);

        if (stunTime > 0) {
            StartCoroutine(StunRoutine(stunTime));
        }
    }

    public void applyChangeColor(float duration, Color? normalColor = null, Color? changedColor = null) {
        StartCoroutine(changeColorRoutine(duration, normalColor, changedColor));
    }

    private IEnumerator StunRoutine(float duration) {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    public IEnumerator changeColorRoutine(float duration, Color? normalColor = null, Color? changedColor = null ) {
         sprite.color = changedColor ?? Color.red;
         yield return new WaitForSeconds(duration);
         sprite.color = normalColor ?? Color.white;
    }

    public void changeStateWithPriority(States newState){
        if (State.GetPriority() >= newState.GetPriority()){
            return;
        }
        else {
            State = newState;
        }
    }

    private void ShowVictory()
    {
        // Создаем панель победы
        if (victoryPanelPrefab != null)
        {
            victoryPanelPrefab.SetActive(true);
            
            // Находим кнопку "Продолжить" (если есть)
            Button continueButton = victoryPanelPrefab.GetComponentInChildren<Button>();
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(ReloadLevel);
            }
            else
            {
                // Если нет кнопки - перезагружаем через 3 секунды
                Invoke("ReloadLevel", 3f);
            }
        }
        else
        {
            // Если нет префаба - просто перезагружаем сцену
            ReloadLevel();
        }
    }

    private void ReloadLevel()
    {
        SceneManager.LoadScene(sceneToReload);
    }
}