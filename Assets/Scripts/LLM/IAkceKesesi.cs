namespace BorununYolu.LLM
{
    /// <summary>
    /// Oyuncunun akçe kesesi. Konuşma kodu akçenin nerede tutulduğunu bilmez;
    /// oyunda GameManager'a, testte sabit bir keseye bağlanır.
    /// </summary>
    public interface IAkceKesesi
    {
        int Akce { get; }

        /// <summary>Yeterli akçe varsa düşer ve true döner; yoksa hiçbir şeyi değiştirmez.</summary>
        bool Harca(int miktar);
    }

    /// <summary>Bellekte tutulan basit kese (test penceresi ve GameManager'sız denemeler için).</summary>
    public sealed class SabitKese : IAkceKesesi
    {
        public int Akce { get; private set; }

        public SabitKese(int akce)
        {
            Ayarla(akce);
        }

        public void Ayarla(int akce)
        {
            Akce = akce < 0 ? 0 : akce;
        }

        public bool Harca(int miktar)
        {
            if (miktar < 0 || Akce < miktar) return false;
            Akce -= miktar;
            return true;
        }
    }
}
