using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float speed = 4f;
    Rigidbody2D rb;
    Vector2 move;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        move = Vector2.zero;
        var k = Keyboard.current;
        if (GameState.InputLocked || k == null) return;

        if (k.wKey.isPressed || k.upArrowKey.isPressed) move.y += 1;
        if (k.sKey.isPressed || k.downArrowKey.isPressed) move.y -= 1;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) move.x -= 1;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) move.x += 1;
        move = move.normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = move * speed;
    }
}