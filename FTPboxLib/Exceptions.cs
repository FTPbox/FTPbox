using System;
using System.Security.Authentication;

namespace FTPboxLib
{
    public class CertificateDeclinedException : Exception
    {
        public CertificateDeclinedException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
