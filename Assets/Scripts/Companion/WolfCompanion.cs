using UnityEngine;
using UnityEngine.AI;
public class WolfCompanion : MonoBehaviour
{
    //State Machine: Börünün o an ne yaptığını tutacak
    public enum State
    {
        TakipEdiyor,
        GoreveGidiyor

    }

    public State currentState = State.TakipEdiyor; // Başlangıçta Aybarsı takip etsin

    private NavMeshAgent agent;

    // OOP ye uygun olmak için: public değişkenler gizlendi (private), ancak Inspector da görünmesi için [SerializeField] eklendi
    [Header("Hedefler ve Etkileşim")]
    [SerializeField] private Transform aybars;  // oyuncu
    [SerializeField] private Transform gorevNesnesi; // Börüyü göndereceğimiz ip veya şalter gibi bir işlevsel nesne
    [SerializeField] private GameObject kapaliKapi;   // ip çekilince ortadan kaybolacak engel/kapı

    [Header("Sensör Ayarları")]
    [SerializeField] private float gorusMesafesi = 0.5f;
    [SerializeField] private float gorusAcisi = 30f;  // sağ ve sol ışınların merkezden sapma açısı

    private Vector2 sonYon = Vector2.up; // Börünün durduğunda baktığı son yönü hafızada tutacak

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // 2D ye uyarlama ayarları
        agent.updateRotation = false;
        agent.updateUpAxis = false;
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
                if (Vector3.Distance(transform.position, aybars.position) > 2f)
                {
                    agent.SetDestination(aybars.position);
                }
                else
                {
                    agent.ResetPath();
                }
                break;

            case State.GoreveGidiyor:
                agent.SetDestination(gorevNesnesi.position);
                break;
        }
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

                    if (kapaliKapi != null)
                    {
                        kapaliKapi.SetActive(false);
                    }

                    currentState = State.TakipEdiyor; // Görev bitti Aybarsa dön
                    return;
                }
            }
        }
    }
}