using Microsoft.AspNetCore.Identity;
using OtpNet;

namespace HCFileCorrection.Utility
{
    public class GetOtp
    {
        public string Decrypt(string key)
        {
            var secretkey = Base32Encoding.ToBytes(key);
            var totp = new Totp(secretkey);
            var otp = totp.ComputeTotp();
            return otp;
        }
    }
}
