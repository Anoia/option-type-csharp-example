using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using Option_Example;

namespace No_Option_Example
{
    public class LicenseExample
    {
        private readonly Parser _parser;
        private ServerConnection _connection;

        public LicenseExample()
        {
            _parser = new Parser();
            _connection = new ServerConnection();
        }
        public LicenseError Error { get; set; } = LicenseError.UnknownError;

        private Activation GetSavedActivation()
        {
            var encodedString = ReadFromRegistry("MyEncodedActivationString");

            if (string.IsNullOrWhiteSpace(encodedString))
            {
                Error = LicenseError.NoActivation;
                return null;
            }

            var decoded = Decode(encodedString);
            if (decoded == null)
            {
                Error = LicenseError.DecodeFailed;
                return null;
            }

            var couldParse = _parser.TryParse(decoded);

            if (!couldParse)
            {
                Error = LicenseError.ParseFailed;
                return null;
            }

            return _parser.ParsedActivation;
        }

        public Activation GetActivation()
        {
            var savedActivation = GetSavedActivation();

            if (savedActivation != null && savedActivation.IsActivationStillValid())
            {
                return savedActivation;
            }

            var key = savedActivation?.Key ?? ReadFromRegistry("key");

            if (string.IsNullOrWhiteSpace(key))
            {
                Error = LicenseError.NoLicenseKey;
                return null;
            }

            var couldConnect = _connection.TryGetServerResponse(key);

            if (!couldConnect)
            {
                Error = LicenseError.NoServerConnection;
                return null;
            }

            var licenseStringFromServer = _connection.Response;

            var couldParse = _parser.TryParse(licenseStringFromServer);

            if (!couldParse)
            {
                Error = LicenseError.ParseFailed;
                return null;
            }

            var activationFromServer = _parser.ParsedActivation;

            if (!HasValidActivationTime(activationFromServer))
            {
                Error = LicenseError.InvalidActivationTime;
                return null;
            }

            SaveActivation(activationFromServer);

            return activationFromServer;
        }

        public void SaveActivation(Activation activation)
        {
            // sideeffects only: save activation in registry
        }

        private bool HasValidActivationTime(Activation activation)
        {
            return IsTimeOfActivationValid(activation) || activation.Ok;
        }

        private bool IsTimeOfActivationValid(Activation activation)
        {
            var difference = activation.TimeOfActivation - DateTime.Now;
            return difference <= TimeSpan.FromMinutes(5) && difference >= -TimeSpan.FromHours(5);
        }


        private string Decode(string encodedString)
        {
            try
            {
                return Base64.Decode(encodedString);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        private string ReadFromRegistry(string value)
        {
            var key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            var subkey = key.OpenSubKey("path");

            return subkey?.GetValue(value)?.ToString();
        }

        public class Parser
        {
            public Activation ParsedActivation { get; set; }

            public bool TryParse(string activationString)
            {
                ParsedActivation = new Activation();
                return true;
            }
        }

        public class ServerConnection
        {
            public string Response { get; set; }

            public bool TryGetServerResponse(string key)
            {
                Response = "MyResponse";
                return true;
            }
        }
    }
}
