using UnityEngine;

public class AybarsHareket : MonoBehaviour
{
    public float hiz = 5f;
    private Rigidbody2D rb;
    private Vector2 hareket;
    private Animator anim;
    private string suAnkiAnimasyon;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        hareket.x = Input.GetAxisRaw("Horizontal");
        hareket.y = Input.GetAxisRaw("Vertical");

        
        if (hareket.x > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (hareket.x < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        
        if (hareket.x != 0 || hareket.y != 0)
        {
            AnimasyonOynat("Run"); 
        }
        else
        {
            AnimasyonOynat("Idle"); 
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + hareket.normalized * hiz * Time.fixedDeltaTime);
    }

    void AnimasyonOynat(string yeniAnimasyon)
    {
        if (suAnkiAnimasyon == yeniAnimasyon) return;

        anim.Play(yeniAnimasyon);
        suAnkiAnimasyon = yeniAnimasyon;
    }
}