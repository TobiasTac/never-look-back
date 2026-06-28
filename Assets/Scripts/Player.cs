using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class Player : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;

    private int facingDirection = 1;
    private bool isDead = false;
    private bool isJumping;
    public bool canMove = true;
    public bool IsDead => isDead;

    private Rigidbody2D rig;
    private Animator anim;
    public float movement;
    private MovingPlatform currentPlatform;

    // Referência ao fantasma
    public Ghost ghost;

    [Header("Orientação inicial")]
    public bool startFacingRight = true;

    [Header("Gelo")]
    public LayerMask iceLayer;
    public float iceAcceleration = 0.3f;
    public float iceDeceleration = 0.05f;

private bool isOnIce = false;

    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Define orientação inicial
        facingDirection = startFacingRight ? 1 : -1;
        transform.eulerAngles = new Vector3(0, startFacingRight ? 0 : 180, 0);

        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions.Disable();
        playerInput.actions.FindActionMap("Player").Enable();
    }

    public void OnMove(InputValue value)
    {
        if (isDead || !canMove) return;

        movement = value.Get<Vector2>().x;

        bool tryingToLookBack = (facingDirection == 1 && movement < 0) ||
                                (facingDirection == -1 && movement > 0);

        if (tryingToLookBack)
        {
            movement = 0f;
            StartCoroutine(LookBackThenDie());
        }
    }

    public void OnJump(InputValue value)
    {
        if (!canMove) return;
        if (!value.isPressed || isJumping) return;

        rig.linearVelocity = new Vector2(rig.linearVelocity.x, 0f);
        rig.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        isJumping = true;
        currentPlatform = null;
    }

    void FixedUpdate()
    {
        Move();
        UpdateAnimations();
    }

    void Move()
    {
        if (!canMove || isDead) return;

        float platformVelocityX = 0f;
        if (currentPlatform != null)
            platformVelocityX = currentPlatform.Movement.x / Time.fixedDeltaTime;

        if (isOnIce)
        {
            // No gelo — acelera e desacelera suavemente
            float targetVelocityX = (movement * speed) + platformVelocityX;
            float lerpFactor = Mathf.Abs(movement) > 0.01f ? iceAcceleration : iceDeceleration;
            float newVelocityX = Mathf.Lerp(rig.linearVelocity.x, targetVelocityX, lerpFactor);
            rig.linearVelocity = new Vector2(newVelocityX, rig.linearVelocity.y);
        }
        else
        {
            // Normal
            rig.linearVelocity = new Vector2((movement * speed) + platformVelocityX, rig.linearVelocity.y);
        }

        if (Mathf.Abs(movement) > 0.01f)
        {
            bool movingForward = (facingDirection == 1 && movement > 0) ||
                                (facingDirection == -1 && movement < 0);

            if (movingForward)
                transform.eulerAngles = new Vector3(0, facingDirection == 1 ? 0 : 180, 0);
        }
    }

    void UpdateAnimations()
    {
        if (isDead) return;

        if (isJumping)
            anim.SetInteger("transition", 2);
        else if (Mathf.Abs(movement) > 0.01f)
            anim.SetInteger("transition", 1);
        else
            anim.SetInteger("transition", 0);
    }

    public void InvertDirection()
    {
        facingDirection *= -1;
        transform.eulerAngles = new Vector3(0, facingDirection == 1 ? 0 : 180, 0);
    }

    public void ExitPortal(Vector2 throwVelocity, bool faceRight, float stunTime)
    {
        facingDirection = faceRight ? 1 : -1;
        transform.eulerAngles = new Vector3(0, faceRight ? 0 : 180, 0);
        rig.linearVelocity = throwVelocity;

        // Reseta o movimento para evitar input preso após o teleporte
        movement = 0f;

        StartCoroutine(RecoverControlRoutine(stunTime));
    }
    private IEnumerator LookBackThenDie()
    {
        isDead = true;
        canMove = false;
        rig.linearVelocity = Vector2.zero;

        // Vira o sprite para trás
        transform.eulerAngles = new Vector3(0, facingDirection == 1 ? 180 : 0, 0);

        // Aguarda um momento com o personagem virado
        yield return new WaitForSeconds(0.5f);

        if (ghost != null)
            ghost.Die();
    }
    private IEnumerator RecoverControlRoutine(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        bool isGround = ((1 << collision.gameObject.layer) & groundLayer) != 0;
        bool isIce = ((1 << collision.gameObject.layer) & iceLayer) != 0;

        if (isGround || isIce)
        {
            // Só reseta o pulo se a colisão for por baixo (normal apontando para cima)
            if (collision.contacts[0].normal.y > 0.5f)
                isJumping = false;

            MovingPlatform platform = collision.gameObject.GetComponent<MovingPlatform>();
            if (platform != null && collision.contacts[0].normal.y > 0.5f)
                currentPlatform = platform;
        }

        if (isIce && collision.contacts[0].normal.y > 0.5f)
            isOnIce = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        bool isIce = ((1 << collision.gameObject.layer) & iceLayer) != 0;

        if (currentPlatform != null && collision.gameObject == currentPlatform.gameObject)
            currentPlatform = null;

        if (isIce)
            isOnIce = false;
    }
    public void TriggerDeath()
    {
        if (isDead) return;

        isDead = true;
        canMove = false;
        rig.linearVelocity = Vector2.zero;
        anim.SetInteger("transition", 3); // animação de death direto
        Invoke(nameof(ReloadScene), 1f);
    }

    void ReloadScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}