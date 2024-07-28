using Bit.Core.Models;

namespace Bit.Core.Abstractions
{
    // Tag:Nibblewarden
    public interface IHttpClientHandler
    {
        HttpClientHandler AsClientHandler();

        void UseClientCertificate(ICertificateChainSpec clientCertificate);
    }
}
