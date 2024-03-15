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
            var publicKey = "<RSAKeyValue><Modulus>ybQGWO81RGKkiL0UTuQ7Z12/FysmlCHnyZglHbZxVc5WZsb+L3CSh54qLdVhmLS++RvQjme/Q+69bgKz/fSuG/R6d4dDCyaYxNc6JBMeCkMXvbD0C/F8vgmeiByBguJ7xFhcAjl2lVqmZAGZ+m2nDA4ztl7OVrT7R/OpMU86uEWj2XWAsN1BZxeyztfZHRUrNFiQ17p/0s4nxjznlS0kRtoKoyNWdBQAGLu5jnLafyA/6f//nMXuZuvGsfiINgOMyYYtbwETYtwK1aS0em97/l5/47Ob5Q4jzLA5I0Wt//6byTLMUZaR1aF7ikwcpTjCJ9qbH/WA4WAPNxp30BEQRMzZTUrgVO5Viq+2XlA7Kia+jOTBltxU2FtiKo1hP6uFHiBezsoeupp4yEYAn03EITvxFX6gcXmFGMr8VbVmK46XiSTLhHdVp04tlhCytDMMQqw6xz7TvtFwfPJbR/+p54qgyUH1aKQbDzsrrK7s+8V9VZ68xJTG44rQaG31Qux9Auo91UCjEnNS1K+zRL4ioCZpnhailVt725k35BvYKqCCtP92aXd8L0NZEHHWQykF+8/fkXX5iC4QZPQmvuagTZHa0kK67uOiIr5Biwto+iyjQNzHyCm9rFlYq+QHvpY/Oy4jscSTmj7LWx2nhIvnZY/Z/4bBrSpTj7pyUxlf0AxoZyaYFkKMNGW1UJBJeX2NKIsAj1o/TEPTs2VJzKIGQNoCJJC8VpvmCJg19DuUaLpgx9OB3/D0SfU95S8hbyY+jdlTA3I8ha+/EsltceG9sM84o5Vq8A1um9TkwYn19SlsEOiKEP+rhEHvrFXMf1tRYBd8kvozsyEuhVBKQqlHIR68FQIEyDrb167ITzV7oMuCmrWK6AO8iNvVYF5Ok1Ywdn/mWyffein1qDHkHuvyrKPi0jGBkYA+p4rdc6sHaeCz8A3Hh4yfcrXpkTxRjEZtVTUNFu4sVNVLRWyzPuga4evQZig5ubK6+BuQNZNvnJ+9PIf7WNk93U+SlUgJpJ1rOm2v0VlThQNls6E+MGA9tX/slR6rrO95EGkrlRpDmr4v7TmxmHvhcaPlgG5odcqJvJ0GOrZX32cVXzMwG8oXqDimIQEV9q9Aa0aA7tUN+PgJyQU0b/QFkeTmxfeBQSmHg1GfMzRuwBONuRpNZpdDoePUbftxcT0vfLFGo2heYmwlHj0Q00YN9yQ5X9gKjoGlEySveT+JuRwxhnOjzl460kCDkg8Ue/H/faQaejoFd2FPof6i/t+TVUs4hZY0M0RexkGY/XauCU6W9bx2wFZfkFxhZghYdxSWTvmXUWgxdf86zl+vyByDa8F1nhCW4KKzSpepHGO66kSqThpxUmMT5Q==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
            Rsa.GetHash(details.AesContent, out string hashData);
            var flag = Rsa.SignatureDeformatter(publicKey, hashData, details.RsaContent);
            if (!flag) return null;
            var license = JsonConvert.DeserializeObject<License>(AesHelper.Decrypt(details.AesContent, details.AesKey));
            return license;
        }

        public bool Correct(string code)
        {
            var serial = DeviceInfo.SerialNumber();
            Debug.Print(serial);
            if (string.IsNullOrEmpty(code)) return false;
            var license = Decrypt(new EncryptDetails(code));
            if (license == null) return false;
            if (license.IsBlock) return false;
            if (DateTime.Now >= license.EndDate || DateTime.Now < license.BeginDate) return false;
            if (license.Count != -1) return false;
            if (!license.MachineCode.Equals(serial)) return false;
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
