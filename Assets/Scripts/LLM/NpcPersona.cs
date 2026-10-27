using System.Text;
using UnityEngine;

namespace BorununYolu.LLM
{
    [CreateAssetMenu(fileName = "NpcPersona", menuName = "Borunun Yolu/NPC Persona")]
    public class NpcPersona : ScriptableObject
    {
        [Header("Kimlik")]
        [SerializeField] private string displayName = "Deli Dumrul";
        [SerializeField, TextArea(15, 60)] private string systemPrompt;

        [Header("Oyun kuralları")]
        [SerializeField] private int gecisBedeli = 10;
        [SerializeField, Range(0, 10)] private int baslangicOfkesi = 3;
        [SerializeField, Range(0, 10)] private int kapiIcinMaxOfke = 4;
        [SerializeField, Range(0, 10)] private int kavgaOfkesi = 8;
        [SerializeField] private int iknaIcinMinMesaj = 3;

        public string DisplayName => displayName;
        public int GecisBedeli => gecisBedeli;
        public int BaslangicOfkesi => baslangicOfkesi;
        public int KapiIcinMaxOfke => kapiIcinMaxOfke;
        public int KavgaOfkesi => kavgaOfkesi;
        public int IknaIcinMinMesaj => iknaIcinMinMesaj;

               public string BuildSystemPrompt(int kesedekiAkce, int mevcutOfke, bool bedelOdendi)
        {
            var sb = new StringBuilder(systemPrompt);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("ŞU ANKİ DURUM:");
            sb.AppendLine($"- Geçiş bedeli: {gecisBedeli} akçe");

            if (bedelOdendi)
            {
                sb.AppendLine($"- Yolcu geçiş bedeli olan {gecisBedeli} akçeyi SANA VERDİ. Bedel tamamen ödendi.");
                sb.AppendLine($"- Yolcunun kesesinde kalan akçe: {kesedekiAkce} (bu önemli değil, bedel zaten ödendi)");
            }
            else
            {
                sb.AppendLine("- Geçiş bedeli henüz ödenmedi.");
                sb.AppendLine($"- Yolcunun kesesindeki akçe: {kesedekiAkce}");
            }

            sb.AppendLine($"- Şu anki öfken: {mevcutOfke}");
            return sb.ToString();
        }
    }
}