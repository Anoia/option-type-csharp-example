using System;
using System.Net;
using Microsoft.Win32;
using Optional;

namespace Option_Example
{
    public class LicenseExample
    {

        // ================================FUNCTION IMPLEMENTATION START================================ \\
        public Option<Activation, LicenseError> GetActivation()
        {
            var savedActivation = ReadActivationString()
                .FlatMap(Decode)
                .FlatMap(TryParseActivation);

            if (savedActivation.Exists(a => a.IsActivationStillValid()))
                return savedActivation;

            var key = savedActivation.Match(
                some: a => a.Key.SomeNotNull(LicenseError.NoLicenseKey),
                none: e => ReadKey());

            var onlineActivation = key.FlatMap(GetActivationStringFromLicenseServer)
                .FlatMap(TryParseActivation)
                .Filter(HasValidActivationTime, LicenseError.InvalidActivationTime);

            onlineActivation.MatchSome(SaveActivation);

            return onlineActivation.HasValue ? onlineActivation : savedActivation;
        }
        // ================================FUNCTION IMPLEMENTATION END================================ \\

        private bool HasValidActivationTime(Activation activation)
        {
            return IsTimeOfActivationValid(activation) || activation.Ok;
        }

        private bool IsTimeOfActivationValid(Activation activation)
        {
            var difference = activation.TimeOfActivation - DateTime.Now;
            return difference <= TimeSpan.FromMinutes(5) && difference >= -TimeSpan.FromHours(5);
        }

        private Option<string, LicenseError> Decode(string encodedString)
        {
            try
            {
                var decoded = Base64.Decode(encodedString);
                return decoded.SomeNotNull(LicenseError.DecodeFailed);
            }
            catch (FormatException)
            {
                return Option.None<string, LicenseError>(LicenseError.DecodeFailed);
            }
        }

        private Option<string, LicenseError> ReadKey()
        {
            var val = ReadFromRegistry("TheLicenseKey");
            return val.SomeNotNull(LicenseError.NoLicenseKey);
        }

        private Option<string, LicenseError> ReadActivationString()
        {
            var val = ReadFromRegistry("MyEncodedActivationString");
            return val.SomeNotNull(LicenseError.NoActivation);
        }

        private string ReadFromRegistry(string value)
        {
            var key = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
            var subkey = key.OpenSubKey(@"Some\Registry\Path");

            return subkey?.GetValue(value)?.ToString();
        }

        public void SaveActivation(Activation activation)
        {
            // sideeffects only: save activation in registry
        }

        private Option<Activation, LicenseError> TryParseActivation(string responseString)
        {
            // Check some things
            // Do all the parsing!
            // Make sure parsed activation is alright (hash, correct product, correct machine)
            return Option.Some<Activation, LicenseError>(new Activation());
        }

        public Option<string, LicenseError> GetActivationStringFromLicenseServer(string key)
        {
            var url = "myurl.com/some/api/and/query";

            try
            {
                //var response = _webClient.DownloadString(url);
                var response = $"The string I downloaded from {url}";
                return response.SomeNotNull(LicenseError.NoServerConnection);
            }
            catch (WebException e)
            {
                return Option.None<string, LicenseError>(LicenseError.NoServerConnection);
            }
        }
    }
}
