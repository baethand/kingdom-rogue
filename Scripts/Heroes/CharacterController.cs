using System;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterController : EntityWithHealth
{

    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    public float airAcceleration = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    public float variableJumpHeightMultiplier = 0.5f;
    public float doubleJumpForce = 10f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public bool canDashInAir = false;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Wall Check")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;
    

    [Header("Attack Settings")]
    public float attackRange = 1f; 
    public float attackDamage = 1f; 
    public float attackCooldown = 0.5f;
    public Transform attackTrigger;
    public LayerMask enemyLayer;
    private float lastAttackTime;
    private bool isAttacking;

    [Header("Knockback Settings")]
    public bool isKnockbacking = true;
    public float knockbackForce = 5f;
    public float knockbackForceY = 1f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool canDoubleJump;
    public float moveInput {get; set;}
    private bool isJumping;

    private bool isTouchingWall;
    private bool isDashing; // В процессе ли рывка
    private float dashTimeLeft; // Оставшееся время рывка
    private float lastDashTime; // Время последнего рывка
    private int dashDirection; // Направление рывка (1 - вправо, -1 - влево)

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

    protected override void Awake () {
        sprite = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        base.Awake();
    }

    void Update()
    {
        if (Input.GetKeyDown("k")){
            base.getDamage(1f);
            State = States.TAKE_HIT;
            Debug.Log(currentHealth);
            
        }
        if (alive == false) {
            return;
        }
        // Обработка ввода 
        moveInput = Input.GetAxisRaw("Horizontal");

        if (isAttacking) {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                isAttacking = false;
            }
            updateRun();
            return;
        }

        updateRaycasts();
        updateMovementAnimation();
        updateAttack();
        updateJump();
        updateRun();
        updateDash();
    }

    #region ANIMATION

    public void changeStateWithPriority(States newState){
        if (State.GetPriority() >= newState.GetPriority()){
            return;
        }
        else {
            State = newState;
        }
    }

    #endregion

    #region OVERRIDES

    protected override void die()
    {
        base.die();
        State = States.DIE;
    }

    #endregion

    #region FIGHT

    void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        State = States.ATTACK1;

        Vector2 attackDirection = sprite.flipX ? Vector2.left : Vector2.right;
        
        Vector2 attackOrigin = attackTrigger.position;

        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            attackOrigin,
            attackRange,
            attackDirection,
            0f,
            enemyLayer
        );

        // Наносим урон всем врагам в зоне
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider.CompareTag("Enemy")) {
                EnemyController enemyController = hit.collider.GetComponent<EnemyController>();

                enemyController.getDamage(attackDamage);
                if (enemyController.isKnockbacking)
                    enemyController.applyKnockback(transform, -1, -1, -1f);
                if (enemyController.isReadyToChangeColor){
                    enemyController.applyChangeColor(0.1f, Color.white, Color.red);
                }
                Debug.Log("Игрок нанес урон " + attackDamage + " у врага: " + hit.collider.GetComponent<EnemyController>().currentHealth);
            }
        }
    }

    public void applyKnockback(Transform damageSource, float customForce) {
        if (rb == null) return;
        
        float force = customForce >= 0 ? customForce : knockbackForce;
        
        Vector2 direction;
        
        if (damageSource != null) {
            direction = (transform.position - damageSource.position).normalized;
            direction.y = knockbackForceY;
        }
        else {
            
            direction = Vector2.up;
        }
        
        // Применяем силу
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
    }

    #endregion

    #region MOVEMENT
    private void Run()
    {
        if (isGrounded && !isTouchingWall)
            changeStateWithPriority(States.RUN);
            // State = States.RUN;

        // Определяем направление движения
        float moveDirection = Input.GetAxis("Horizontal");
        Vector3 dir = transform.right * moveDirection;

        // Двигаем персонажа
        if (!isTouchingWall)
            transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, moveSpeed * Time.deltaTime);

        // Поворачиваем спрайт
        sprite.flipX = dir.x < 0.0f;

        // Поворачиваем wallCheck в сторону движения
        if (moveDirection != 0) // Проверяем, что персонаж двигается
        {
            Vector3 wallCheckScale = wallCheck.transform.localScale;
            wallCheckScale.x = Mathf.Sign(moveDirection); // 1 для движения вправо, -1 для движения влево
            wallCheck.transform.localScale = wallCheckScale;
        }
    }

    void Jump(float force)
    {

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Сброс вертикальной скорости перед прыжком
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time;

        dashDirection = (moveInput != 0) ? (int)Mathf.Sign(moveInput) : (int)Mathf.Sign(transform.localScale.x);
    }

    void StopDash()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * wallCheck.transform.localScale.x * wallCheckDistance);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    #region UPDATE

    private void updateJump(){
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump(jumpForce);
                canDoubleJump = true;
            }
            else if (canDoubleJump)
            {
                Jump(doubleJumpForce);
                canDoubleJump = false;
            }
        }

        // Переменная высота прыжка
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpHeightMultiplier);
        }
    }

    private void updateDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) // Используйте Shift для рывка
        {
            if (Time.time >= lastDashTime + dashCooldown && (isGrounded || canDashInAir)) // Проверка перезарядки и условий
            {
                StartDash();
            }
        }

        if (isDashing)
        {
            if (dashTimeLeft > 0)
            {
                rb.linearVelocity = new Vector2(dashSpeed * dashDirection, 0); // Движение в сторону рывка
                dashTimeLeft -= Time.deltaTime;
            }
            else
            {
                StopDash();
            }
        }
    }

    private void updateRun()
    {
        if (Input.GetButton("Horizontal")) {
            Run();
        }
    }

    private void updateRaycasts()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        isTouchingWall = Physics2D.Raycast(wallCheck.position, Vector2.right * wallCheck.transform.localScale.x , wallCheckDistance, wallLayer);

    }

    private void updateAttack()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= lastAttackTime + attackCooldown) // Используйте кнопку Fire1 (обычно левая кнопка мыши)
        {
            Attack();
        }
    }

    private void updateMovementAnimation()
    {
        if (isGrounded)
            State = States.IDLE;
        else 
            if (rb.linearVelocityY <= 0 )
                State = States.FALL;
            else
                State = States.JUMP;
    }

}

#endregion