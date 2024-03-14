using System;
using System.Security.Cryptography;
using System.Text;

namespace LicenseChecker.Helpers
{
    /// <summary>
    /// Rsa秘钥
    /// </summary>
    public struct RsaSecretKey
    {
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }

        public override string ToString()
        {
            return $"PrivateKey: {PrivateKey}\r\nPublicKey: {PublicKey}";
        }
    }

    public class RsaHelper
    {
        static string Algorithm = "MD5";

        #region RSA数字签名

        #region 获取Hash描述表

        /// <summary>  
        /// 获取Hash描述表  
        /// </summary>  
        /// <param name="strSource">待签名的字符串</param>  
        /// <param name="hashData">Hash描述</param>  
        /// <returns></returns>  
        public bool GetHash<T>(string strSource, out T hashData)
        {
            var md5 = HashAlgorithm.Create(Algorithm);
            if (md5 == null)
                throw new ArgumentException("can't find algorithm!", nameof(Algorithm));

            var buffer = Encoding.GetEncoding("UTF-8").GetBytes(strSource);
            if (typeof(T) == typeof(byte[]))
                hashData = (T)(object)md5.ComputeHash(buffer);
            else if (typeof(T) == typeof(string))
                hashData = (T)(object)Convert.ToBase64String(md5.ComputeHash(buffer));
            else
                hashData = default;

            return true;
        }

        /// <summary>  
        /// 获取Hash描述表  
        /// </summary>  
        /// <param name="objFile">待签名的文件</param>  
        /// <param name="hashData">Hash描述</param>  
        /// <returns></returns>  
        public bool GetHash<T>(System.IO.FileStream objFile, out T hashData)
        {
            var md5 = HashAlgorithm.Create(Algorithm);
            if (md5 == null)
                throw new ArgumentException("can't find algorithm!", nameof(Algorithm));

            var computeHash = md5.ComputeHash(objFile);
            if (typeof(T) == typeof(byte[]))
                hashData = (T)(object)computeHash;
            else if (typeof(T) == typeof(string))
                hashData = (T)(object)Convert.ToBase64String(computeHash);
            else
                hashData = default;

            objFile.Close();
            return true;
        }
        #endregion

        /// <summary>  
        /// RSA签名验证  
        /// </summary>  
        /// <param name="strKeyPublic">公钥</param>  
        /// <param name="hash">Hash描述</param>  
        /// <param name="deformatterData">签名后的结果</param>  
        /// <returns></returns>  
        public bool SignatureDeformatter<T1, T2>(string strKeyPublic, T1 hash, T2 deformatterData)
        {
            byte[] bytes;
            if (typeof(T1) == typeof(string))
                bytes = Convert.FromBase64String((string)(object)hash);
            else
                bytes = (byte[])(object)hash;

            var rsa = new RSACryptoServiceProvider(8192);
            rsa.FromXmlString(strKeyPublic);
            var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
            rsaDeformatter.SetHashAlgorithm("MD5");
            if (typeof(T2) == typeof(byte[]))
                return rsaDeformatter.VerifySignature(bytes, (byte[])(object)deformatterData);

            return rsaDeformatter.VerifySignature(bytes, Convert.FromBase64String((string)(object)deformatterData));
        }

        #endregion
    }
}