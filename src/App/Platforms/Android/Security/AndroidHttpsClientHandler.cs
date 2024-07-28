﻿using System.Security.Authentication;
using Bit.Core.Abstractions;
using Bit.Core.Models;
using Java.Security;
using Java.Security.Cert;
using Javax.Net.Ssl;
using Xamarin.Android.Net;

namespace Bit.Droid.Security
{
    // Tag:Nibblewarden
    public class AndroidHttpsClientHandler : AndroidClientHandler, IHttpClientHandler
    {
        private X509CertificateChainSpec ClientCertificate;

        public AndroidHttpsClientHandler() : base()
        {
            ClientCertificate = null;
        }

        public HttpClientHandler AsClientHandler()
        {
            return this;
        }

        public void UseClientCertificate(ICertificateChainSpec clientCertificate)
        {
            ClientCertificate = clientCertificate as X509CertificateChainSpec;
        }

        protected override SSLSocketFactory ConfigureCustomSSLSocketFactory(HttpsURLConnection connection)
        {
            if (ClientCertificate == null) return base.ConfigureCustomSSLSocketFactory(connection);

            X509Certificate[] certChain = ClientCertificate.CertificateChain;
            var privateKey = ClientCertificate.PrivateKeyRef;

            if (privateKey == null || certChain == null || certChain.Length == 0)
                return base.ConfigureCustomSSLSocketFactory(connection);

            KeyStore keyStore = KeyStore.GetInstance("pkcs12");
            keyStore.Load(null, null);
            keyStore.SetKeyEntry($"{ClientCertificate.Alias}_TLS", privateKey, null, certChain.Cast<Certificate>().ToArray());

            var kmf = KeyManagerFactory.GetInstance("x509");
            kmf.Init(keyStore, null);

            var keyManagers = kmf.GetKeyManagers();

            SSLContext sslContext = SSLContext.GetInstance("TLS");
            sslContext.Init(keyManagers, null, null);

            SSLSocketFactory socketFactory = sslContext.SocketFactory;
            if (connection != null)
            {
                connection.SSLSocketFactory = socketFactory;
            }
            return socketFactory;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (Javax.Net.Ssl.SSLProtocolException ex) when (ex.Message.Contains("SSLV3_ALERT_BAD_CERTIFICATE"))
            {
                throw new HttpRequestException(ex.Message, new AuthenticationException());
            }
        }
    }
}
