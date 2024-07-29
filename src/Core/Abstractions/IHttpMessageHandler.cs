using Bit.Core.Models;

namespace Bit.Core.Abstractions
{
    // Tag:Nibblewarden
    public interface IHttpMessageHandler
    {
        HttpMessageHandler AsMessageHandler();

        void UseClientCertificate(ICertificateChainSpec clientCertificate);
    }
}
