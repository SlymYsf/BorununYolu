using BorununYolu.LLM;

namespace BorununYolu.NPC
{
    /// <summary>
    /// Kişi A'nın GameManager'ındaki akçeyi (sandıklardan toplanan) IAkceKesesi olarak sunar.
    /// GameManager'a dokunmaz; harcamayı CoinEkle(-miktar) ile yapar ki ekrandaki "Akçe" yazısı da güncellensin.
    /// </summary>
    public sealed class GameManagerKesesi : IAkceKesesi
    {
        private readonly GameManager _gameManager;

        public GameManagerKesesi(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public int Akce => _gameManager != null ? _gameManager.toplamCoin : 0;

        public bool Harca(int miktar)
        {
            if (_gameManager == null || miktar < 0 || Akce < miktar) return false;
            _gameManager.CoinEkle(-miktar);
            return true;
        }
    }
}
