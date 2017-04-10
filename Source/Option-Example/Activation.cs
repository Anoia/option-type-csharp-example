using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Option_Example
{
    public class Activation
    {
        public string Key { get; set; }
        public string Licensee { get; set; }

        public DateTime ActivatedTill { get; set; }
        public DateTime LicenseExpires { get; set; }
        public DateTime TimeOfActivation { get; set; }
        public bool Ok { get; set; }

        public bool IsActivationStillValid()
        {
            return DateTime.Now <= ActivatedTill && Ok;
        }

        public bool IsLicenseStillValid()
        {
            return DateTime.Now <= LicenseExpires && Ok;
        }
    }
}
