using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    public string walkFront = "WalkFront";
    public string walkBack = "WalkBack";
    public string walkSide = "WalkSide";
    public bool sideSpriteFacesRight = true;

    Animator anim;
    Rigidbody2D rb;
    SpriteRenderer sr;
    string current = "";

    void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector2 v = rb.linearVelocity;

        if (v.sqrMagnitude < 0.01f)
        {
            anim.speed = 0f;
            return;
        }

        anim.speed = 1f;
        string next;

        if (Mathf.Abs(v.x) > Mathf.Abs(v.y))
        {
            next = walkSide;
            sr.flipX = sideSpriteFacesRight ? v.x < 0 : v.x > 0;
        }
        else
        {
            next = v.y > 0 ? walkBack : walkFront;
            sr.flipX = false;
        }

        if (next != current)
        {
            anim.Play(next);
            current = next;
        }
    }
}