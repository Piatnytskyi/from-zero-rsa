using System;

namespace RSAApplication
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
