﻿using LicenseChecker.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CommonModel;

namespace LicenseChecker
{
    public class LicenseCheck
    {
        public readonly RsaHelper Rsa = new RsaHelper();
        private static readonly DeviceInfo deviceInfo = new();
        private readonly ILogger<LicenseCheck> logger;

        public LicenseCheck()
        {
        }
        public LicenseCheck(ILogger<LicenseCheck> logger)
        {
            this.logger = logger;
        }

        readonly byte[] _pkBytes =
            {
                0x3c, 0x52, 0x53, 0x41, 0x4b, 0x65, 0x79, 0x56,
                0x61, 0x6c, 0x75, 0x65, 0x3e, 0x3c, 0x4d, 0x6f,
                0x64, 0x75, 0x6c, 0x75, 0x73, 0x3e, 0x7a, 0x64,
                0x44, 0x6b, 0x64, 0x54, 0x47, 0x36, 0x54, 0x66,
                0x31, 0x76, 0x41, 0x52, 0x4c, 0x31, 0x6d, 0x39,
                0x46, 0x68, 0x71, 0x44, 0x59, 0x43, 0x35, 0x74,
                0x53, 0x65, 0x36, 0x71, 0x71, 0x4d, 0x41, 0x78,
                0x49, 0x4a, 0x72, 0x55, 0x41, 0x63, 0x59, 0x4c,
                0x2f, 0x66, 0x68, 0x37, 0x69, 0x54, 0x4b, 0x49,
                0x46, 0x65, 0x50, 0x61, 0x66, 0x7a, 0x79, 0x37,
                0x42, 0x50, 0x64, 0x55, 0x70, 0x44, 0x55, 0x2b,
                0x78, 0x30, 0x6b, 0x4d, 0x47, 0x54, 0x4b, 0x6f,
                0x30, 0x63, 0x45, 0x32, 0x35, 0x7a, 0x39, 0x54,
                0x65, 0x52, 0x4f, 0x30, 0x32, 0x51, 0x48, 0x55,
                0x59, 0x6e, 0x78, 0x53, 0x77, 0x62, 0x4f, 0x37,
                0x4b, 0x79, 0x30, 0x44, 0x4b, 0x64, 0x33, 0x35,
                0x58, 0x39, 0x41, 0x67, 0x78, 0x47, 0x74, 0x73,
                0x6f, 0x39, 0x57, 0x6d, 0x6a, 0x70, 0x30, 0x45,
                0x78, 0x76, 0x4b, 0x73, 0x56, 0x38, 0x58, 0x45,
                0x38, 0x6a, 0x79, 0x6a, 0x53, 0x65, 0x62, 0x69,
                0x5a, 0x31, 0x4f, 0x55, 0x4a, 0x45, 0x6a, 0x6d,
                0x67, 0x65, 0x67, 0x62, 0x59, 0x53, 0x35, 0x32,
                0x33, 0x71, 0x4a, 0x31, 0x6f, 0x69, 0x71, 0x6b,
                0x62, 0x79, 0x47, 0x41, 0x79, 0x68, 0x39, 0x49,
                0x2b, 0x48, 0x51, 0x62, 0x6b, 0x6c, 0x34, 0x73,
                0x6d, 0x68, 0x67, 0x53, 0x6d, 0x6e, 0x2f, 0x6b,
                0x4b, 0x59, 0x39, 0x7a, 0x56, 0x36, 0x58, 0x45,
                0x49, 0x4f, 0x33, 0x42, 0x70, 0x4f, 0x4e, 0x71,
                0x5a, 0x79, 0x50, 0x4f, 0x56, 0x68, 0x57, 0x41,
                0x33, 0x4a, 0x7a, 0x6f, 0x52, 0x4d, 0x6c, 0x6a,
                0x53, 0x4a, 0x61, 0x79, 0x6f, 0x77, 0x7a, 0x43,
                0x58, 0x47, 0x72, 0x59, 0x62, 0x6f, 0x75, 0x73,
                0x38, 0x65, 0x50, 0x54, 0x6f, 0x46, 0x62, 0x33,
                0x6a, 0x4f, 0x63, 0x70, 0x63, 0x59, 0x67, 0x62,
                0x79, 0x38, 0x59, 0x6e, 0x6c, 0x7a, 0x2b, 0x4f,
                0x70, 0x4e, 0x6d, 0x6c, 0x58, 0x58, 0x54, 0x4e,
                0x44, 0x70, 0x4c, 0x6f, 0x75, 0x49, 0x53, 0x6e,
                0x46, 0x4b, 0x2f, 0x33, 0x68, 0x38, 0x39, 0x6c,
                0x35, 0x72, 0x37, 0x7a, 0x38, 0x79, 0x30, 0x4e,
                0x78, 0x34, 0x56, 0x46, 0x6f, 0x47, 0x78, 0x50,
                0x2b, 0x68, 0x74, 0x36, 0x6c, 0x47, 0x6a, 0x63,
                0x76, 0x53, 0x55, 0x73, 0x5a, 0x32, 0x77, 0x79,
                0x78, 0x39, 0x71, 0x32, 0x43, 0x59, 0x75, 0x2b,
                0x56, 0x63, 0x6d, 0x76, 0x76, 0x67, 0x5a, 0x41,
                0x65, 0x65, 0x57, 0x6a, 0x36, 0x44, 0x65, 0x38,
                0x58, 0x67, 0x69, 0x70, 0x48, 0x65, 0x67, 0x78,
                0x67, 0x45, 0x59, 0x39, 0x36, 0x78, 0x57, 0x4b,
                0x51, 0x7a, 0x6d, 0x59, 0x37, 0x6c, 0x58, 0x48,
                0x4c, 0x32, 0x67, 0x5a, 0x4a, 0x63, 0x74, 0x43,
                0x43, 0x52, 0x39, 0x59, 0x4d, 0x72, 0x6c, 0x78,
                0x45, 0x71, 0x76, 0x65, 0x52, 0x58, 0x31, 0x2b,
                0x2b, 0x58, 0x47, 0x5a, 0x57, 0x49, 0x7a, 0x59,
                0x48, 0x76, 0x4d, 0x43, 0x59, 0x76, 0x59, 0x74,
                0x68, 0x38, 0x47, 0x6f, 0x79, 0x48, 0x35, 0x65,
                0x52, 0x58, 0x31, 0x43, 0x4c, 0x2b, 0x64, 0x75,
                0x50, 0x7a, 0x31, 0x30, 0x67, 0x71, 0x35, 0x42,
                0x52, 0x44, 0x58, 0x43, 0x38, 0x41, 0x77, 0x6d,
                0x4f, 0x4c, 0x30, 0x4d, 0x55, 0x42, 0x61, 0x32,
                0x2f, 0x33, 0x44, 0x6c, 0x63, 0x44, 0x72, 0x79,
                0x33, 0x55, 0x33, 0x4c, 0x55, 0x58, 0x48, 0x37,
                0x47, 0x2f, 0x36, 0x71, 0x43, 0x37, 0x6b, 0x71,
                0x42, 0x32, 0x71, 0x4e, 0x5a, 0x38, 0x79, 0x56,
                0x6a, 0x78, 0x58, 0x51, 0x77, 0x66, 0x36, 0x46,
                0x63, 0x64, 0x43, 0x4d, 0x79, 0x31, 0x76, 0x34,
                0x69, 0x6c, 0x32, 0x41, 0x48, 0x4f, 0x56, 0x65,
                0x5a, 0x46, 0x2b, 0x32, 0x30, 0x63, 0x38, 0x43,
                0x57, 0x75, 0x75, 0x35, 0x35, 0x6b, 0x62, 0x79,
                0x2f, 0x61, 0x35, 0x6a, 0x72, 0x59, 0x6b, 0x42,
                0x4a, 0x2b, 0x74, 0x46, 0x45, 0x67, 0x6d, 0x6f,
                0x52, 0x42, 0x4a, 0x4a, 0x67, 0x6d, 0x33, 0x4a,
                0x62, 0x47, 0x57, 0x34, 0x6b, 0x30, 0x32, 0x33,
                0x50, 0x48, 0x6d, 0x77, 0x2b, 0x33, 0x4c, 0x6e,
                0x4f, 0x61, 0x41, 0x2f, 0x54, 0x4c, 0x4d, 0x39,
                0x35, 0x68, 0x4a, 0x43, 0x7a, 0x41, 0x6c, 0x37,
                0x4a, 0x30, 0x76, 0x72, 0x37, 0x6d, 0x70, 0x41,
                0x2b, 0x68, 0x2b, 0x2f, 0x35, 0x43, 0x75, 0x59,
                0x56, 0x69, 0x31, 0x7a, 0x36, 0x61, 0x65, 0x4a,
                0x34, 0x50, 0x66, 0x47, 0x50, 0x62, 0x41, 0x79,
                0x68, 0x69, 0x32, 0x64, 0x4e, 0x76, 0x69, 0x73,
                0x7a, 0x46, 0x77, 0x2f, 0x39, 0x6c, 0x2f, 0x69,
                0x37, 0x36, 0x79, 0x6a, 0x67, 0x72, 0x62, 0x45,
                0x32, 0x71, 0x67, 0x4a, 0x43, 0x33, 0x48, 0x39,
                0x5a, 0x6c, 0x4a, 0x36, 0x2f, 0x39, 0x48, 0x42,
                0x64, 0x56, 0x77, 0x6f, 0x4f, 0x70, 0x59, 0x32,
                0x4c, 0x71, 0x68, 0x6b, 0x6a, 0x6d, 0x69, 0x49,
                0x78, 0x46, 0x73, 0x69, 0x70, 0x47, 0x38, 0x32,
                0x33, 0x45, 0x39, 0x57, 0x65, 0x48, 0x56, 0x79,
                0x43, 0x47, 0x4f, 0x51, 0x34, 0x54, 0x66, 0x77,
                0x7a, 0x39, 0x72, 0x42, 0x78, 0x43, 0x6a, 0x4a,
                0x59, 0x68, 0x48, 0x55, 0x50, 0x74, 0x41, 0x64,
                0x6f, 0x66, 0x6a, 0x30, 0x33, 0x78, 0x57, 0x7a,
                0x62, 0x4a, 0x61, 0x75, 0x32, 0x6c, 0x51, 0x58,
                0x63, 0x78, 0x4a, 0x56, 0x70, 0x55, 0x6e, 0x58,
                0x52, 0x70, 0x6d, 0x7a, 0x4b, 0x62, 0x74, 0x47,
                0x5a, 0x75, 0x49, 0x73, 0x67, 0x54, 0x66, 0x47,
                0x6e, 0x33, 0x44, 0x77, 0x53, 0x43, 0x35, 0x66,
                0x4d, 0x7a, 0x4f, 0x62, 0x65, 0x4e, 0x36, 0x45,
                0x6e, 0x4d, 0x64, 0x32, 0x38, 0x68, 0x49, 0x50,
                0x34, 0x64, 0x34, 0x79, 0x43, 0x4b, 0x57, 0x55,
                0x36, 0x42, 0x30, 0x48, 0x53, 0x6c, 0x64, 0x62,
                0x6f, 0x52, 0x2f, 0x4a, 0x43, 0x70, 0x55, 0x58,
                0x47, 0x75, 0x33, 0x49, 0x45, 0x39, 0x4a, 0x57,
                0x39, 0x57, 0x4c, 0x34, 0x71, 0x4a, 0x65, 0x61,
                0x4e, 0x4c, 0x5a, 0x49, 0x4d, 0x44, 0x58, 0x47,
                0x6c, 0x65, 0x57, 0x77, 0x75, 0x68, 0x7a, 0x35,
                0x31, 0x4a, 0x6b, 0x79, 0x44, 0x47, 0x54, 0x41,
                0x4d, 0x2f, 0x33, 0x64, 0x46, 0x75, 0x6c, 0x45,
                0x72, 0x57, 0x46, 0x77, 0x44, 0x49, 0x5a, 0x2b,
                0x76, 0x6f, 0x7a, 0x67, 0x4b, 0x7a, 0x47, 0x35,
                0x50, 0x39, 0x6e, 0x78, 0x4a, 0x6c, 0x2b, 0x69,
                0x58, 0x51, 0x52, 0x6f, 0x67, 0x4c, 0x44, 0x71,
                0x38, 0x58, 0x6c, 0x35, 0x70, 0x6d, 0x37, 0x76,
                0x53, 0x39, 0x4c, 0x65, 0x32, 0x70, 0x78, 0x65,
                0x58, 0x45, 0x71, 0x48, 0x39, 0x32, 0x62, 0x56,
                0x57, 0x2b, 0x72, 0x50, 0x58, 0x72, 0x56, 0x44,
                0x6f, 0x6f, 0x33, 0x54, 0x30, 0x54, 0x6e, 0x58,
                0x47, 0x2b, 0x6f, 0x4e, 0x38, 0x4e, 0x79, 0x38,
                0x45, 0x78, 0x36, 0x49, 0x69, 0x36, 0x32, 0x2b,
                0x6d, 0x37, 0x48, 0x72, 0x45, 0x49, 0x45, 0x69,
                0x4d, 0x35, 0x73, 0x6f, 0x42, 0x76, 0x61, 0x6c,
                0x4b, 0x66, 0x2b, 0x69, 0x6f, 0x73, 0x63, 0x30,
                0x35, 0x64, 0x69, 0x4f, 0x59, 0x48, 0x49, 0x69,
                0x31, 0x50, 0x72, 0x61, 0x4e, 0x4e, 0x39, 0x78,
                0x49, 0x72, 0x61, 0x4f, 0x30, 0x57, 0x4f, 0x31,
                0x70, 0x56, 0x69, 0x4a, 0x6c, 0x51, 0x76, 0x6a,
                0x61, 0x63, 0x6b, 0x54, 0x31, 0x6d, 0x54, 0x57,
                0x61, 0x35, 0x74, 0x79, 0x7a, 0x33, 0x45, 0x77,
                0x2b, 0x7a, 0x55, 0x35, 0x49, 0x33, 0x78, 0x68,
                0x55, 0x54, 0x54, 0x78, 0x64, 0x7a, 0x38, 0x59,
                0x37, 0x71, 0x32, 0x75, 0x55, 0x32, 0x47, 0x34,
                0x6e, 0x4a, 0x6d, 0x66, 0x74, 0x48, 0x57, 0x75,
                0x35, 0x78, 0x79, 0x49, 0x2b, 0x43, 0x61, 0x2b,
                0x6d, 0x50, 0x44, 0x49, 0x71, 0x2b, 0x2b, 0x63,
                0x70, 0x50, 0x55, 0x61, 0x32, 0x78, 0x6e, 0x75,
                0x49, 0x56, 0x31, 0x32, 0x6d, 0x53, 0x57, 0x49,
                0x55, 0x57, 0x69, 0x57, 0x64, 0x4b, 0x44, 0x71,
                0x59, 0x44, 0x35, 0x51, 0x34, 0x39, 0x69, 0x75,
                0x51, 0x71, 0x4f, 0x35, 0x67, 0x74, 0x73, 0x7a,
                0x32, 0x69, 0x66, 0x43, 0x41, 0x6e, 0x57, 0x35,
                0x69, 0x2f, 0x47, 0x77, 0x57, 0x69, 0x51, 0x61,
                0x4b, 0x2b, 0x77, 0x42, 0x6f, 0x71, 0x36, 0x50,
                0x37, 0x74, 0x2b, 0x66, 0x78, 0x55, 0x34, 0x68,
                0x71, 0x4b, 0x44, 0x4a, 0x67, 0x34, 0x70, 0x2f,
                0x52, 0x65, 0x6f, 0x76, 0x4d, 0x37, 0x66, 0x57,
                0x77, 0x55, 0x4b, 0x71, 0x77, 0x58, 0x50, 0x6c,
                0x7a, 0x2b, 0x55, 0x50, 0x4e, 0x4e, 0x4b, 0x59,
                0x58, 0x5a, 0x36, 0x75, 0x72, 0x43, 0x68, 0x74,
                0x78, 0x2b, 0x44, 0x5a, 0x72, 0x73, 0x64, 0x45,
                0x31, 0x4b, 0x57, 0x6c, 0x6a, 0x71, 0x66, 0x44,
                0x44, 0x70, 0x6c, 0x61, 0x32, 0x6b, 0x78, 0x55,
                0x37, 0x42, 0x4b, 0x34, 0x44, 0x41, 0x66, 0x65,
                0x5a, 0x61, 0x4a, 0x31, 0x36, 0x35, 0x53, 0x55,
                0x48, 0x70, 0x69, 0x34, 0x4d, 0x32, 0x68, 0x6b,
                0x58, 0x79, 0x73, 0x33, 0x4a, 0x51, 0x75, 0x39,
                0x55, 0x67, 0x49, 0x73, 0x45, 0x35, 0x4d, 0x36,
                0x5a, 0x4d, 0x71, 0x4c, 0x43, 0x62, 0x39, 0x52,
                0x55, 0x35, 0x51, 0x69, 0x4c, 0x41, 0x4e, 0x49,
                0x66, 0x38, 0x4f, 0x67, 0x65, 0x77, 0x53, 0x36,
                0x31, 0x30, 0x37, 0x42, 0x61, 0x38, 0x69, 0x68,
                0x46, 0x50, 0x62, 0x68, 0x58, 0x77, 0x74, 0x38,
                0x72, 0x67, 0x42, 0x38, 0x69, 0x66, 0x61, 0x59,
                0x49, 0x4a, 0x4a, 0x36, 0x56, 0x57, 0x38, 0x4b,
                0x45, 0x72, 0x6d, 0x75, 0x45, 0x6e, 0x71, 0x36,
                0x6e, 0x58, 0x6e, 0x31, 0x6e, 0x43, 0x2f, 0x6f,
                0x38, 0x61, 0x73, 0x69, 0x2b, 0x39, 0x53, 0x46,
                0x39, 0x71, 0x56, 0x4a, 0x4b, 0x58, 0x41, 0x4a,
                0x47, 0x4d, 0x4e, 0x72, 0x71, 0x38, 0x70, 0x69,
                0x70, 0x2b, 0x41, 0x66, 0x59, 0x68, 0x72, 0x38,
                0x73, 0x67, 0x4b, 0x4d, 0x76, 0x36, 0x45, 0x73,
                0x6c, 0x51, 0x35, 0x47, 0x63, 0x72, 0x7a, 0x44,
                0x2b, 0x67, 0x5a, 0x70, 0x4e, 0x69, 0x6b, 0x37,
                0x64, 0x4a, 0x44, 0x35, 0x32, 0x44, 0x42, 0x55,
                0x58, 0x58, 0x44, 0x6f, 0x79, 0x50, 0x53, 0x56,
                0x77, 0x36, 0x79, 0x51, 0x3d, 0x3d, 0x3c, 0x2f,
                0x4d, 0x6f, 0x64, 0x75, 0x6c, 0x75, 0x73, 0x3e,
                0x3c, 0x45, 0x78, 0x70, 0x6f, 0x6e, 0x65, 0x6e,
                0x74, 0x3e, 0x41, 0x51, 0x41, 0x42, 0x3c, 0x2f,
                0x45, 0x78, 0x70, 0x6f, 0x6e, 0x65, 0x6e, 0x74,
                0x3e, 0x3c, 0x2f, 0x52, 0x53, 0x41, 0x4b, 0x65,
                0x79, 0x56, 0x61, 0x6c, 0x75, 0x65, 0x3e,
        };

        public License Decrypt(EncryptDetails details, string publicKey = "")
        {
            if (string.IsNullOrWhiteSpace(publicKey))
                publicKey = Encoding.UTF8.GetString(_pkBytes);

            try
            {
                Rsa.GetHash(details.AesContent, out string hashData);
                var flag = Rsa.SignatureDeformatter(publicKey, hashData, details.RsaContent);
                if (!flag) return null;
                var license = JsonConvert.DeserializeObject<License>(AesHelper.Decrypt(details.AesContent, details.AesKey));
                return license;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<License> Licenses = new();
        public bool Correct(string code, string productName = "")
        {
            var serial = deviceInfo.SerialNumber(productName);
            if (string.IsNullOrEmpty(code))
            {
                logger?.LogInformation("未查询到授权码!");
                return false;
            }
            License license;
            if (productName.StartsWith("OEM-"))
            {
                var value = Licenses.FirstOrDefault(x => x.ProductName.Equals(productName.Substring(4)));
                if (value is null)
                {
                    logger.LogInformation($"验证产品失败:[{productName}]!");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(value.OemPublicKey))
                {
                    logger.LogInformation($"OEM公钥信息异常!");
                    return false;
                }
                license = Decrypt(new EncryptDetails(code), value.OemPublicKey);
            }
            else
            {
                if (!CheckTicks(productName) && !productName.Equals("OEMLicenseGenerator")) return false;

                license = Decrypt(new EncryptDetails(code));
            }

            if (license == null)
            {
                logger?.LogInformation("解析失败!");
                return false;
            }

            if (DateTime.Now >= license.EndDate || DateTime.Now < license.BeginDate)
            {
                logger?.LogInformation("超期!");
                return false;
            }

            if (!license.ProductName.Equals(productName) && !productName.StartsWith("OEM-"))
            {
                logger?.LogInformation($"未验证的序列号!");
                return false;
            }


            if (!license.ProductName.Equals("OEMLicenseGenerator") && !license.MachineCode.Equals(serial))
            {
                logger?.LogInformation($"非本机激活码[{license.MachineCode}]!");
                return false;
            }

            if (license.IsBlock)
            {
                logger?.LogInformation("异常,请联系制造商!");
                return false;
            }

            //if (license.Count != -1)
            //{
            //    logger?.LogInformation("异常!");
            //    return false;
            //}
            logger?.LogInformation("通过!");
            Licenses.Add(license);
            return true;
        }

        public bool CheckProduct(string product)
        {
            //reg add HKLM\SOFTWARE\WOW6432Node\JKSoft /v product /t REG_SZ /d "激活码"
            using RegistryKey localMachine64 =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            // 打开 SOFTWARE 子键
            using RegistryKey softwareNode = localMachine64.OpenSubKey("SOFTWARE", writable: false);
            var software = softwareNode.OpenSubKey("WOW6432Node");
            var hasSubKey = software.GetSubKeyNames()!.Contains("JKSoft");
            if (!hasSubKey) return false;

            var regKey = software.OpenSubKey("JKSoft");
            if (regKey == null) return false;

            var hasValue = regKey.GetValueNames().Contains(product);
            if (!hasValue) return false;

            var value = regKey.GetValue(product, "-1").ToString();
            if (value.Equals("-1")) return false;

            return Correct(value, product);
        }
        public bool CheckOEMProduct(string product)
        {
            product = $"OEM-{product}";
            //reg add HKLM\SOFTWARE\WOW6432Node\JKSoft /v product /t REG_SZ /d "激活码"
            using RegistryKey localMachine64 =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            // 打开 SOFTWARE 子键
            using RegistryKey softwareNode = localMachine64.OpenSubKey("SOFTWARE", writable: false);
            var software = softwareNode.OpenSubKey("WOW6432Node");
            var hasSubKey = software.GetSubKeyNames()!.Contains("JKSoft");
            if (!hasSubKey) return false;

            var regKey = software.OpenSubKey("JKSoft");
            if (regKey == null) return false;

            var hasValue = regKey.GetValueNames().Contains(product);
            if (!hasValue) return false;

            var value = regKey.GetValue(product, "-1").ToString();
            if (value.Equals("-1")) return false;

            return Correct(value, product);
        }


        /// <summary>
        /// 删除产品
        /// </summary>
        /// <param name="productName"></param>
        public static void RemoveProduct(string productName)
        {
            using RegistryKey localMachine64 =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using RegistryKey softwareNode = localMachine64.OpenSubKey("SOFTWARE", writable: false);
            var software = softwareNode.OpenSubKey("WOW6432Node");
            var hasSubKey = software.GetSubKeyNames()!.Contains("JKSoft");
            if (!hasSubKey) return;

            using var regKey = software.OpenSubKey("JKSoft", true);
            if (regKey == null) return;

            if (regKey.GetValue(productName) == null) return;
            regKey.DeleteValue(productName);
        }

        /// <summary>
        /// 设置Ticks
        /// </summary>
        /// <param name="time"></param>
        /// <param name="productName"></param>
        public static void SetProductTicks(DateTime time, string productName)
        {
            var ticks = GetProductTicks(productName);
            if (ticks.HasValue)
            {
                if (ticks.Value > time.Ticks) return;
            }

            using RegistryKey localMachine64 =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using RegistryKey softwareNode = localMachine64.OpenSubKey("SOFTWARE", writable: false);
            var software = softwareNode.OpenSubKey("WOW6432Node");
            var hasSubKey = software.GetSubKeyNames()!.Contains("JKSoft");
            if (!hasSubKey) return;

            using var regKey = software.OpenSubKey("JKSoft", true);
            if (regKey == null) return;

            if (!regKey.GetSubKeyNames().Contains(productName))
            {
                regKey.CreateSubKey(productName);
            }

            using var pKey = regKey.OpenSubKey(productName, true);
            pKey.SetValue("Ticks", time.Ticks.ToString());
        }

        /// <summary>
        /// 获取产品上一次启动的Ticks
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public static long? GetProductTicks(string productName)
        {

            using RegistryKey localMachine64 =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            using RegistryKey softwareNode = localMachine64.OpenSubKey("SOFTWARE", writable: false);
            var software = softwareNode.OpenSubKey("WOW6432Node");
            var hasSubKey = software.GetSubKeyNames()!.Contains("JKSoft");
            if (!hasSubKey) return null;

            using var regKey = software.OpenSubKey("JKSoft");
            if (regKey == null) return null;

            using var targetK = regKey.OpenSubKey(productName);
            if (targetK == null) return null;

            var value = targetK.GetValue("Ticks");
            if (value == null) return null;

            if (!long.TryParse(value.ToString(), out long ticks)) return null;
            return ticks;
        }

        /// <summary>
        /// 检查Ticks
        /// </summary>
        /// <param name="productName"></param>
        /// <returns></returns>
        public static bool CheckTicks(string productName)
        {
            var ticks = GetProductTicks(productName);
            if (!ticks.HasValue) return false;
            return ticks.Value < DateTime.Now.Ticks;
        }

        public static void GenerateOemMachineCode(string productName) => GenerateMachineCode($"OEM-{productName}");
        public static void GenerateMachineCode(string productName)
        {
            var content = GetMachineCode(productName);
            File.WriteAllText($"{DesktopPath}\\{RegisterCodePath}", content);
        }

        public static string GetOemMachineCode(string productName) => GetMachineCode($"OEM-{productName}");
        public static string GetMachineCode(string productName)
        {
            var licenseCode = new LicenseCode()
            {
                ProductName = productName,
                Code = GetSerialNumber(productName)
            };
            return JsonConvert.SerializeObject(licenseCode);
        }

        public static string GetSerialNumber(string productName) => deviceInfo.SerialNumber(productName);

        public static readonly string RegisterCodePath = "register-code.key";
        public static readonly string LicensePath = "license.key";
        public static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
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
}