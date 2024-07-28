using Bit.Core.Models;

namespace Bit.Core.Abstractions
{
    // Tag:Nibblewarden
    public partial interface IApiService
    {
        void UseClientCertificate(ICertificateChainSpec certificateChainSpec);
    }
}
