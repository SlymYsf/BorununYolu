namespace BorununYolu.LLM
{
    /// <summary>
    /// Deli Dumrul'un tek bir yanıtının ayrıştırılmış hali.
    /// Oyunun durumunu değiştiren kararlar (kapı, kavga) yalnızca bu sınıftan okunur,
    /// konuşma metni üzerinde kelime araması yapılmaz.
    /// </summary>
    public sealed class NpcResponse
    {
        /// <summary>NPC'nin oyuncuya söylediği metin.</summary>
        public string Cevap { get; }

        /// <summary>Model kapının açılmasına karar verdi mi?</summary>
        public bool KapiyiAc { get; }

        /// <summary>Model kavganın başlamasına karar verdi mi?</summary>
        public bool KavgaBaslat { get; }

        /// <summary>NPC'nin öfke seviyesi (0 = sakin, 10 = çok öfkeli).</summary>
        public int Ofke { get; }

        public NpcResponse(string cevap, bool kapiyiAc, bool kavgaBaslat, int ofke)
        {
            Cevap = cevap;
            KapiyiAc = kapiyiAc;
            KavgaBaslat = kavgaBaslat;
            Ofke = ofke;
        }

        public override string ToString() =>
            $"cevap=\"{Cevap}\", kapiyi_ac={KapiyiAc}, kavga_baslat={KavgaBaslat}, ofke={Ofke}";
    }
}