using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Elpis.Windows.OPC.Server
{
    public class Util
    {
        public static bool ValidateIPAddress(string value)
        {
            bool isCorrectIP = Regex.IsMatch(value, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");
            return isCorrectIP;
        }
        public static bool CheckValidName(string name)
        {
            Regex regularExp = new Regex("^[A-Za-z0-9_]{1,40}$");
            if (string.IsNullOrEmpty(name))
                return false;

            else if (regularExp.IsMatch(name))
            {
                if (name.Length <= 40)
                    return true;
                else
                    return false;
            }
            return false;
        }
       
        /// <summary>
        ///     Converts two Int16 values into a Int32.
        /// </summary>
        public static int GetInt32(ushort highOrderValue, ushort lowOrderValue)
        {
            byte[] value = BitConverter.GetBytes(lowOrderValue)
                .Concat(BitConverter.GetBytes(highOrderValue))
                .ToArray();

            return BitConverter.ToInt32(value, 0);
        }
    }
}
