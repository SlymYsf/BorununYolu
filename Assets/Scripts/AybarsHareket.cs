using UnityEngine;

public class AybarsHareket : MonoBehaviour
{
    public float hiz = 5f;
    private Rigidbody2D rb;
    private Vector2 hareket;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Yön tuşlarından gelen girdiyi al
        hareket.x = Input.GetAxisRaw("Horizontal");
        hareket.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        // Karakteri fizik motoruyla (Rigidbody) hareket ettir
        rb.MovePosition(rb.position + hareket.normalized * hiz * Time.fixedDeltaTime);
    }
}