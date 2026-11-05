using UnityEngine;
using UnityEngine.AI;
using System.Collections;//zamanlayıcı

public class WolfCompanion : MonoBehaviour
{
    //State Machine: Börünün o an ne yaptığını tutacak
    public enum State
    {
        TakipEdiyor,
        GoreveGidiyor,
        IpiKesiyor
    }

    public State currentState = State.TakipEdiyor; // Başlangıçta Aybarsı takip etsin

    private NavMeshAgent agent;
    private Animator anim; 
    private string suAnkiAnimasyon;

    // OOP ye uygun olmak için: public değişkenler gizlendi (private), ancak Inspector da görünmesi için [SerializeField] eklendi
    [Header("Hedefler ve Etkileşim")]
    [SerializeField] private Transform aybars;  // oyuncu
    [SerializeField] private Transform gorevNesnesi; // Börüyü göndereceğimiz ip veya şalter gibi bir işlevsel nesne
    [SerializeField] private GameObject kapaliKapi;   // ip çekilince ortadan kaybolacak engel/kapı


    [Header("Sensör Ayarları")]
    [SerializeField] private float gorusMesafesi = 0.5f;
    [SerializeField] private float gorusAcisi = 20f;  // sağ ve sol ışınların merkezden sapma açısı

    private Vector2 sonYon = Vector2.up; // Börünün durduğunda baktığı son yönü hafızada tutacak
    private Vector3 orjinalBoyut;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // 2D ye uyarlama ayarları
        anim = GetComponent<Animator>(); 

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        orjinalBoyut = transform.localScale;
    }

    void Update()
    {
        // Her karede Börü nün önünde etkileşim nesnesi var mı diye sensörle bak (RayCast)

        EtkilesimKontroluYap();

        // E tuşuna basılırsa Börü göreve gitsin
        if (Input.GetKeyDown(KeyCode.E) && gorevNesnesi != null)
        {
            currentState = State.GoreveGidiyor;
        }

        // Börü'nün mevcut durumuna göre ne yapacağına karar ver
        switch (currentState)
        {
            case State.TakipEdiyor:
                float mesafe = Vector3.Distance(transform.position, aybars.position);
                float normalHiz = 4f;
                float deparHizi = 6f;

                if (mesafe > 6f)
                {
                    agent.speed = deparHizi;
                    agent.SetDestination(aybars.position);
                }
                else if (mesafe > 2f)
                {
                    agent.speed = normalHiz;
                    agent.SetDestination(aybars.position);
                }
                else
                {
                    agent.ResetPath();
                    agent.speed = normalHiz;
                }
                break;

            case State.GoreveGidiyor:
                agent.SetDestination(gorevNesnesi.position);
                break;

            case State.IpiKesiyor:
                // ipi keserken Börü olduğu yerde durur hareket etmez
                break;
        }


        HareketAnimasyonlariniYonet();
    }

    void HareketAnimasyonlariniYonet()
    {

        float hiz = agent.velocity.magnitude;


        if (agent.velocity.x > 0.1f)
        {
            transform.localScale = new Vector3(Mathf.Abs(orjinalBoyut.x), orjinalBoyut.y, orjinalBoyut.z); // Sağa bak
        }
        else if (agent.velocity.x < -0.1f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(orjinalBoyut.x), orjinalBoyut.y, orjinalBoyut.z); // Sola bak
        }

        // Hız 0.1 den büyükse hareket ediyordur koşma animasyonunu oynat
        if (hiz > 0.1f)
        {

            AnimasyonOynat("Siberian-Husky-run");
        }
        else
        {
            AnimasyonOynat("Siberian-Husky-idle");
        }
    }


    void AnimasyonOynat(string yeniAnimasyon)
    {
        if (suAnkiAnimasyon == yeniAnimasyon) return;

        anim.Play(yeniAnimasyon);
        suAnkiAnimasyon = yeniAnimasyon;
    }

    void EtkilesimKontroluYap()
    {
        // Börü sadece ipe giderken sensörlerini açsın
        if (currentState != State.GoreveGidiyor) return;

        Vector2 yon = agent.velocity.normalized;
        if (yon != Vector2.zero)
        {
            sonYon = yon;
        }

        float[] acilar = { 0f, -gorusAcisi, gorusAcisi };

        foreach (float aci in acilar)
        {
            Vector3 isinYonu = Quaternion.Euler(0, 0, aci) * sonYon;

            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, isinYonu, gorusMesafesi);
            Debug.DrawRay(transform.position, isinYonu * gorusMesafesi, Color.yellow);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.CompareTag("Etkilesim"))
                {
                    Debug.Log($"Sensör ipe ulaştı! Açı: {aci}, Mesafe: {hit.distance}. Kapı açılıyor...");

                    // direk kapıyı açmak yerine 2 saniyelik uğraşıyormuşçasına bekle
                    StartCoroutine(IpKesmeZamanlayicisi(hit.collider.gameObject));

                    currentState = State.IpiKesiyor;
                    return;
                }
            }
        }
    }

    private IEnumerator IpKesmeZamanlayicisi(GameObject kesilenIp)
    {
        agent.ResetPath(); // Börü ipin önünde durdur

        yield return new WaitForSeconds(2f); // 2 saniye bekler

        // 2 saniye sonra kütük yok olur
        if (kapaliKapi != null)
        {
            kapaliKapi.SetActive(false);
        }

        // Sensörün çarptığı ip de yok olur
        if (kesilenIp != null)
        {
            kesilenIp.SetActive(false);
        }

        Debug.Log("İp koptu, görev tamamlandı!");
        currentState = State.TakipEdiyor; // Görev bitti, Aybars'ı takip etmeye geri dön
    }
}