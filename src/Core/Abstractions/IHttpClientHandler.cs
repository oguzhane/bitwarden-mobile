﻿using Bit.Core.Models;

namespace Bit.Core.Abstractions
{
    public interface IHttpClientHandler
    {
        HttpClientHandler AsClientHandler();

        void UseClientCertificate(ICertificateChainSpec clientCertificate);
    }
}
