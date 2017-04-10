using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Option_Example
{
    public enum LicenseError
    {
        UnknownError,
        NoLicenseKey,
        NoActivation,
        ParseFailed,
        DecodeFailed,
        NoServerConnection,
        InvalidActivationTime
    }
}
