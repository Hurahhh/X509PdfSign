using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Web.Common
{
    public static class CertUtil
    {
        public static readonly string CertViettelBase64 = "MIIEKDCCAxCgAwIBAgIKYQ4N5gAAAAAAETANBgkqhkiG9w0BAQUFADB+MQswCQYDVQQGEwJWTjEzMDEGA1UEChMqTWluaXN0cnkgb2YgSW5mb3JtYXRpb24gYW5kIENvbW11bmljYXRpb25zMRswGQYDVQQLExJOYXRpb25hbCBDQSBDZW50ZXIxHTAbBgNVBAMTFE1JQyBOYXRpb25hbCBSb290IENBMB4XDTE1MTAwMjAyMzIyMFoXDTIwMTAwMjAyNDIyMFowOjELMAkGA1UEBhMCVk4xFjAUBgNVBAoTDVZpZXR0ZWwgR3JvdXAxEzARBgNVBAMTClZpZXR0ZWwtQ0EwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDLdiGZcPhwSm67IiLUWELaaol8kHF+qHPmEdcG0VDKf0FtpSWiE/t6NPzqqmoF4gbIrue1/TzUs7ZeAj28o6Lb2BllA/zB6YFrXfppD4jKqHMO139970MeTbDrhHTbVugX4t2QHS+B/p8+8lszJpuduBrnZ/LWxbhnjeQRr21g89nh/W5q1VbIvZnq4ci5m0aDiJ8arhK2CKpvNDWWQ5E0L7NTVoot8niv6/Wjz19yvUCYOKHYsq97y7eBaSYmpgJosD1VtnXqLG7x4POdb6Q073eWXQB0Sj1qJPrXtOqWsnnmzbbKMrnjsoE4gg9B6qLyQS4kRMp0RrUV0z041aUFAgMBAAGjgeswgegwCwYDVR0PBAQDAgGGMBIGA1UdEwEB/wQIMAYBAf8CAQAwHQYDVR0OBBYEFAhg5h8bFNlIgAtep1xzJSwgDfnWMB8GA1UdIwQYMBaAFM1iceRhvf497LJAYNOBdd06rGvGMDwGA1UdHwQ1MDMwMaAvoC2GK2h0dHA6Ly9wdWJsaWMucm9vdGNhLmdvdi52bi9jcmwvbWljbnJjYS5jcmwwRwYIKwYBBQUHAQEEOzA5MDcGCCsGAQUFBzAChitodHRwOi8vcHVibGljLnJvb3RjYS5nb3Yudm4vY3J0L21pY25yY2EuY3J0MA0GCSqGSIb3DQEBBQUAA4IBAQCHtdHJXudu6HjO0571g9RmCP4b/vhK2vHNihDhWYQFuFqBymCota0kMW871sFFSlbd8xD0OWlFGUIkuMCz48WYXEOeXkju1fXYoTnzm5K4L3DV7jQa2H3wQ3VMjP4mgwPHjgciMmPkaBAR/hYyfY77I4NrB3V1KVNsznYbzbFtBO2VV77s3Jt9elzQw21bPDoXaUpfxIde+bLwPxzaEpe7KJhViBccJlAlI7pireTvgLQCBzepJJRerfp+GHj4Z6T58q+e3a9YhyZdtAHVisWYQ4mY113K1V7Z4D7gisjbxExF4UyrX5G4W0h0gXAR5UVOstv5czQyDraTmUTYtx5J";

        public static X509Certificate GetCertFrom(string certBase64)
        {
            var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(Convert.FromBase64String(certBase64));
            if (cert == null)
            {
                throw new CertificateNullException("Error when parsing base64 string");
            }

            return DotNetUtilities.FromX509Certificate(cert);
        }

        public static X509Certificate[] GetCertChainFrom(string commaSeparatedCertChainBase64)
        {
            var certChain = new List<X509Certificate>();

            string[] certChainBase64 = commaSeparatedCertChainBase64.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            certChain.Add(GetCertFrom(certChainBase64[0]));

            if (certChainBase64.Length == 1)
            {
                certChain.Add(GetCertFrom(CertViettelBase64));
            }
            else
            {
                for (int i = 1; i < certChainBase64.Length; i++)
                {
                    certChain.Add(GetCertFrom(certChainBase64[i]));
                }
            }

            return certChain.ToArray();
        }
    }

    public class CertificateNullException : Exception
    {
        public CertificateNullException()
        {
        }

        public CertificateNullException(string message) : base(message)
        {
        }

        public CertificateNullException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected CertificateNullException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}