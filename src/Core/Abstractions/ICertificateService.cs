﻿using Bit.Core.Models;

namespace Bit.Core.Abstractions
{
    // Tag:Nibblewarden
    public interface ICertificateService
    {
        bool TryRemoveCertificate(string certUri);

        Task<ICertificateChainSpec> GetCertificateAsync(string certUri);

        Task<string> ImportCertificateAsync();

        Task<string> ChooseSystemCertificateAsync();
    }
}
