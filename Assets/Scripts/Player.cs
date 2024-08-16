using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D player;
    private Animator anime;

    [SerializeField] private float moveSpeed;

    [Header("Jump Info")]
    [SerializeField] private float jumpForce;
    private bool isJumping;

    [Header("Attack Info")]
    private bool isAttacking;
    private int comboCounter;
    private float comboTimeWindow;
    [SerializeField] private float comboDuration;
    [SerializeField] private float marchSpeed;
    [SerializeField] private float bufferSpeed;
    private bool attackArrive;

    [Header("Dash Info")]
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashTimer;
    [SerializeField] private float dashCoolDown;
    [SerializeField] private float dashCoolDownTimer;
    private bool isDashing;

    private float xInput;

    private int facingDir;  // 1: right, -1: left
    private bool facingRight;

    [Header("Collision Info")]
    [SerializeField] private float groundCheckDis;
    [SerializeField] private LayerMask ground;
    private bool isGrounded;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Rigidbody2D>();
        anime = GetComponentInChildren<Animator>();

        // Init
        player.velocity = new Vector2(5, player.velocity.y);
        moveSpeed = 7.0F;
        jumpForce = 12;
        facingDir = 1;
        groundCheckDis = 1.4F;
        facingRight = true;
        CheckCollision();
        
        dashSpeed = 25.0F;
        dashDuration = 0.25F;
        dashTimer = -1.0F;
        dashCoolDown = 3.0F;
        dashCoolDownTimer = -1.0F;

        comboDuration = 1.0f;
        marchSpeed = 4.0F;
        bufferSpeed = jumpForce * 0.7F;
    }

    // Update is called once per frame
    void Update()
    {
        CheckInput();
        CheckCollision();
        Movement();
        
        // Timer
        dashTimer -= Time.deltaTime;
        dashCoolDownTimer -= Time.deltaTime;
        comboTimeWindow -= Time.deltaTime;

        FlipController();
        AnimatorController();
    }

    private void StartAttack() {
        if (comboTimeWindow < 0) {
            comboCounter = 0;
        }
            
        isAttacking = true;
        attackArrive = true;
        comboTimeWindow = comboDuration;
    }
    public void AttackOver() {
        isAttacking = false;
        ++comboCounter;
        if (comboCounter > 2) {
            comboCounter = 0;
        }
    }

    private void CheckInput() {
        xInput = Input.GetAxisRaw("Horizontal");
        
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            StartAttack();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            DashAbility();
        }
    }

    private void DashAbility() {
        if (dashCoolDownTimer < 0 && !isAttacking) {
            dashCoolDownTimer = dashCoolDown;
            dashTimer = dashDuration;
        }
    }

    private void CheckCollision() {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDis, ground);
        Debug.Log(isGrounded);
    }

    private void Movement() {
        isDashing = dashTimer >= 0;
        if (isDashing && isJumping && isAttacking) {
            player.velocity = new Vector2(facingDir * math.sqrt(dashSpeed * marchSpeed), jumpForce);
        } else if (isDashing && isJumping) {
            player.velocity = new Vector2(facingDir * dashSpeed, jumpForce);
        } else if (isJumping && isAttacking) {
            player.velocity = new Vector2(xInput * marchSpeed, jumpForce);
        } else if (isDashing && isAttacking) {
            if (isGrounded) {
                player.velocity = new Vector2(facingDir * math.sqrt(dashSpeed * marchSpeed), player.velocity.y);
            } else {
                player.velocity = new Vector2(facingDir * math.sqrt(dashSpeed * marchSpeed), 0);    // 滞空
            }
        } else if (isJumping) {
            player.velocity = new Vector2(player.velocity.x, jumpForce);
        } else if (isAttacking) {
            if (attackArrive) {
                player.velocity = new Vector2(xInput * marchSpeed, bufferSpeed);
            } else {
                player.velocity = new Vector2(xInput * marchSpeed, player.velocity.y);
            }
        } else if (isDashing) {
            if (isGrounded) {
                player.velocity = new Vector2(facingDir * dashSpeed, player.velocity.y);
            } else {
                player.velocity = new Vector2(facingDir * dashSpeed, 0);    // 滞空
            }
        } else {
            player.velocity = new Vector2(xInput * moveSpeed, player.velocity.y);
        }

        // CleanUp
        attackArrive = false;
        isJumping = false;
    }

    private void Jump() {
        if (isGrounded) {
            Debug.Log("Jump");
            isJumping = true;
        }
    }

    private void AnimatorController() {
        bool isMoving = player.velocity.x != 0;
        
        anime.SetFloat("yVelocity", player.velocity.y);
        anime.SetBool("isMoving", isMoving);
        anime.SetBool("isGrounded", isGrounded);
        anime.SetBool("isDashing", isDashing);
        anime.SetBool("isAttacking", isAttacking);
        anime.SetInteger("comboCounter", comboCounter);
    }

    private void Flip() {
        facingDir *= -1;
        facingRight = !facingRight;
        transform.Rotate(0, 180, 0);
    }

    private void FlipController() {
        float vx = player.velocityX;
        if (vx == 0) return;
        bool moveRight = vx > 0;
        if (moveRight ^ facingRight) {
            Flip();
        }
    }

    private void OnDrawGizmos() {
        Gizmos.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - groundCheckDis));
    }
}
