using LicenseChecker.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicenseChecker
{
    public class LicenseCheck
    {
        public RsaHelper Rsa = new RsaHelper();
        public RsaSecretKey SecretKey { get; set; }

        public License Decrypt(EncryptDetails details)
        {
            var publicKey = "<RSAKeyValue><Modulus>0YIV7tZKEOWvB5AlG0hSFOXtlQNl8Esk0CHezJadAHeI8zOV3OUHsPPJ8ehVMXHdhvWzW3aqFfaV1TrO0xPjx8HBs1EepGe9wayoykc6NLE04VfhCI/cS7yHVzjRB6pEv1jV3NSvDcnBAz2COivx6JqWzh349O37kVRiS0Jqf4FZcOcv05GJZsH8HzYd6RRvW/BOhDgGlK8Nq4I7m6iaEFvy6H1SElSLI/tCeRzoDKfopXQSPAoVOxZuGWnM3nltxDuslchi/ZnvYSotsurv1RpntdsIhYGEcW9NpEF1f0OieRppUQPaBbpOjmuPgzHPSGBPf8E5F3XAuVAKbWpqN9ehvnKJoeuzjqHIKI3zbxFB5v5MmFGKYO/BnbgjN1LYBB25ucAtQobOcpORuwhqq9blz6IMpAHcoxL65g+tGMX1CuemsZALnj9XwddGZlgyK2wXcLx/rynewaN8yNgBXhBZpFUBZz2+enuGgE4/dSMcqw8ON6TrUOFvUvp8XtZGOOOrcaVj7ksOTdcd3XhGid8GQYUCnp4IXxr4JnMnpLT6QlBu3RcQcEEGP/xuzjQWs/n1Qxfg4iB74aByV/4DqgHogvu0iiJQWJ5iWWtQBXybhaZF4Esyfj5c32tJfoY85eGPUAicbCBzf+FqI1609/jND1jHfyk3/Z1WcZJRhT6fWLK/bLbQsp+IuEBudJkdPMV5+do8m5FV+J8IG+zTlrzjejvwbHQcwuCjDADLUld0eTFiTxpL7C2eFpWhYgawF5KyXiVacFUAbLmYzXR79fMtbTJdpQEhBIONmdxB0izH29mSWQq4rOY7ouHXjeDkv5enyhAE06jfneZXQvPcAXrPGA009ltQtjZyNoVefSXHUdjH6KrVSggBRDmiIkRjL+q7F2uPQworxy6+8WdtttDtwX8f3vib/NGAuIfkwFXg8G2W1zBFFXBTNv1BG5/q8UhDqi2bO5hBEl/BAgVHKnmGj6/rauoTVBBA0BMfkcpRwQpHS3A/eAffUV9nJ3yDB+gBdvNcjgFpy5nf6Ptkh7bWwKewVzhr6Ug1B8CS8VuUQtjpj2Vuhhc4e7Uo/+n+a2Qgb8CWbcn21p4DGZCMP9VocZzzGfECS1kJBQSsUs/YB7NUycrGHrg0m8hhBibzbxHlpNaIHG4quhPDtxRSKCvUYywozQ/XNNnCSilkc1vSAGQfko/nw7QkicAPwCPYAWxuKZ4/mkk/WuophG/fN/g2RrB5sPaSoEcMcLae5YHijdlsvLEVqrUpLOSXJrcF1H9oVQ/EDyixy6jpG9Nh/UILKsEX4eLQZMBt/z+8cJEvQoFcuOfepybWrcKhtqoWdbfqdxjyvOEVM7+IeiUuLQ==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            Rsa.GetHash(details.AesContent, out string hashData);
            var flag = Rsa.SignatureDeformatter(publicKey, hashData, details.RsaContent);
            var license = JsonConvert.DeserializeObject<License>(AesHelper.Decrypt(details.AesContent, details.AesKey));
            return license;
        }

        public bool Correct(string code)
        {
            Debug.Print(DeviceInfo.SerialNumber);
            if (string.IsNullOrEmpty(code)) return false;
            var license = Decrypt(new EncryptDetails(code));
            if (license == null) return false;
            if (license.IsBlock) return false;
            if (DateTime.Now >= license.EndDate || DateTime.Now < license.BeginDate) return false;
            if (license.Count != -1) return false;
            if (!license.MachineCode.Equals(DeviceInfo.SerialNumber)) return false;
            return true;
        }
    }
    public class EncryptDetails
    {
        public string AesKey { get; set; }
        public int AesLength { get; set; }
        public string AesContent { get; set; }
        public string RsaContent { get; set; }
        public EncryptDetails(string str)
        {
            AesKey = str.Substring(0, 16);
            AesLength = Convert.ToInt32(str.Substring(16, 4), 16);
            AesContent = str.Substring(20, AesLength);
            RsaContent = str.Substring(20 + AesLength);
        }

        public EncryptDetails()
        {

        }

        public override string ToString()
            => AesKey + AesLength.ToString("X") + AesContent + RsaContent;
    }

    public class License
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string MachineCode { get; set; }
        public bool IsBlock { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Count { get; set; }
        public string DisableFunctionList { get; set; }
        public string DisableVersionList { get; set; }
    }
}
