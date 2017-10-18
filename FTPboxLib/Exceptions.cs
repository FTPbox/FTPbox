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

    public class PermissionDeniedException : Exception
    {
        public PermissionDeniedException(Exception ex) 
            : base(ex.Message, ex.InnerException)
        {

        }
    }
}
