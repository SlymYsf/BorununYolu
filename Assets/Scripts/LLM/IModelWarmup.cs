using System.Threading;
using System.Threading.Tasks;
using BorununYolu.Core;

namespace BorununYolu.LLM
{
    /// <summary>
    /// Modeli konuşma başlamadan önce belleğe yükleyebilen istemciler için ayrı sözleşme.
    /// ILLMClient'a eklenmedi: her istemcinin ısıtma desteği olmak zorunda değil (Interface Segregation).
    /// </summary>
    public interface IModelWarmup
    {
        Task<LLMResult> WarmUpAsync(CancellationToken cancellationToken = default);
    }
}
