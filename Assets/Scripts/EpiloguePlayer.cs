using UnityEngine;
using UnityEngine.InputSystem;

public class EpiloguePlayer : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 10f;
    public LayerMask groundLayer;

    private Rigidbody2D rig;
    private Animator anim;
    private float movement;
    private bool isJumping;

    void Start()
    {
        rig = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions.Disable();
        playerInput.actions.FindActionMap("Player").Enable();
    }

    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>().x;
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed || isJumping) return;

        rig.linearVelocity = new Vector2(rig.linearVelocity.x, 0f);
        rig.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        isJumping = true;
    }

    void FixedUpdate()
    {
        rig.linearVelocity = new Vector2(movement * speed, rig.linearVelocity.y);

        if (movement > 0)
            transform.eulerAngles = new Vector3(0, 0, 0);
        else if (movement < 0)
            transform.eulerAngles = new Vector3(0, 180, 0);
    }

    void Update()
    {
        if (anim == null) return;

        if (isJumping)
            anim.SetInteger("transition", 2);
        else if (Mathf.Abs(movement) > 0.01f)
            anim.SetInteger("transition", 1);
        else
            anim.SetInteger("transition", 0);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            if (collision.contacts[0].normal.y > 0.5f)
                isJumping = false;
    }

    // Chamado pelo EpilogueManager para travar durante o encontro
    public void Freeze()
    {
        movement = 0f;
        rig.linearVelocity = Vector2.zero;
        rig.bodyType = RigidbodyType2D.Kinematic;
        if (anim != null) anim.SetInteger("transition", 0);
    }
}