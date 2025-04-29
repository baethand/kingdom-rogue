using UnityEngine;

public class MainHero : MonoBehaviour {
    [SerializeField] private float speed = 3f;
    [SerializeField] private int lifes = 7;
    [SerializeField] private float jumpForce = 1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator animator;
    private bool isGrounded;

    public static MainHero Instance { get; set; }

    private States State {
        get {
            return (States) animator.GetInteger("State");
        }

        set {
            animator.SetInteger("State", (int) value);
        }
    }

    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate() {
        CheckGrounded();
    }

    private void Update() {
        // if (isGrounded) {
        //     State = States.idle;
        // }

        if (Input.GetKeyDown("1")) {
            State = States.idle;
        }
        if (Input.GetKeyDown("2")) {
            State = States.run;
        }
        if (Input.GetKeyDown("3")) {
            State = States.jump;
        }
        if (Input.GetKeyDown("4")) {
            State = States.fall;
        }
        if (Input.GetKeyDown("5")) {
            State = States.attack1;
        }
        if (Input.GetKeyDown("6")) {
            State = States.attack2;
        }
        if (Input.GetKeyDown("7")) {
            State = States.takeHit;
        }
        if (Input.GetKeyDown("8")) {
            State = States.die;
        }
        if (Input.GetKeyDown("9")) {
            State = States.dead;
        }

        // if (Input.GetButton("Horizontal")) {
        //     Run();
        // }
        // if (isGrounded && Input.GetButton("Jump")) {
        //     Jump();
        // }
    }
    
    private void Run() {
        if (isGrounded) {
            State = States.run;
        }

        Vector3 dir = transform.right * Input.GetAxis("Horizontal");

        transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, speed * Time.deltaTime);

        sprite.flipX = dir.x < 0.0f;
    }

    private void Jump() {
        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
    }

    private void CheckGrounded() {
        float rayLength = 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayLength, groundLayer);
        isGrounded = hit.collider != null;

        if (!isGrounded) {
            State = States.jump;
        }
    }
}

public enum States {
    idle,
    run,
    jump,
    fall,
    attack1,
    attack2,
    takeHit,
    die,
    dead
}
