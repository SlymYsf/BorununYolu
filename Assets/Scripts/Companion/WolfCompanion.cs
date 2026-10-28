using UnityEngine;
using UnityEngine.AI;

public class WolfCompanion : MonoBehaviour
{
    //State Machine: Börünün o an ne yaptığını tutacak
    public enum State
    {
        TakipEdiyor,
        GoreveGidiyor,
        EngeldeDurdu
    }

    public State currentState = State.TakipEdiyor; // Başlangıçta Aybarsı takip etsin

    private NavMeshAgent agent;

    [Header("Hedefler")]
    public Transform aybars;        // oyuncu
    public Transform gorevNesnesi;  // Börüyü göndereceğimiz ip veya şalter gibi bir işlevsel nesne

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // 2D ye uyarlama ayarları
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Update()
    {
        // Her karede Börü nün önünde engel var mı diye sensörle bak (RayCast)
        EngelKontroluYap();

        // E tuşuna basılırsa Börü göreve gitsin
        if (Input.GetKeyDown(KeyCode.E) && gorevNesnesi != null)
        {
            currentState = State.GoreveGidiyor;
        }

        // Börü'nün mevcut durumuna göre ne yapacağına karar ver
        switch (currentState)
        {
            case State.TakipEdiyor:
                // Aybars'tan 2 birim uzaklaştıysa peşinden git
                if (Vector3.Distance(transform.position, aybars.position) > 2f)
                {
                    agent.SetDestination(aybars.position);
                }
                else
                {
                    agent.ResetPath(); // Çok yaklaşınca dur
                }
                break;

            case State.GoreveGidiyor:
                // Şaltere/İpe doğru git
                agent.SetDestination(gorevNesnesi.position);

                // İpe yeterince yaklaştı mı?
                if (Vector3.Distance(transform.position, gorevNesnesi.position) < 1f)
                {
                    Debug.Log("Börü ipe ulaştı ve çekti! Tekrar takibe dönüyor.");
                    currentState = State.TakipEdiyor; // İş bitince tekrar Aybars'ı takibe dön
                }
                break;

            case State.EngeldeDurdu:
                // Sensör engel bulduğu için hareket etmeyi bırak
                agent.ResetPath();
                break;
        }
    }

    // 2. SENSÖR (Raycast) MEKANİZMASI
    void EngelKontroluYap()
    {
        // Börü nün hareket ettiği yönü al
        Vector2 yon = agent.velocity.normalized;
        if (yon == Vector2.zero) return; //suruyorsa engel kontolü yapmaya gerek yok

        // Börü nün önünden 3 birim ileri bir görünmez çizgi yolla
        RaycastHit2D hit = Physics2D.Raycast(transform.position, yon, 3f);

        // Bu çizgiyi Scene ekranında görebilmek için:
        Debug.DrawRay(transform.position, yon * 3f, Color.red);

        // Işın bir şeye çarptıysa ve o şeyin etiketi Engel ise:
        if (hit.collider != null && hit.collider.CompareTag("Engel"))
        {
            Debug.Log("Önümde engel var! Mesafe: " + hit.distance);
            currentState = State.EngeldeDurdu; // Börü nün durumunu durmaya al
        }
    }
}