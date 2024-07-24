using Bit.Core.Models;

namespace Bit.Core.Abstractions
{
    public partial interface IApiService
    {
        void UseClientCertificate(ICertificateChainSpec certificateChainSpec);
    }
}
