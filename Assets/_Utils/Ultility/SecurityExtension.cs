// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecurityExtension.cs" company="Dauler Palhares">
//  © Copyright Dauler Palhares da Costa Viana 2017.
//          http://github.com/DaulerPalhares
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System.Text;
namespace Ultility
{
    public static class SecurityExtension
    {
        public static string GetRandomString(System.Random rnd, int length)
        {
            var x = length;
            var charPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890!@#$%&*()[]{}<>,.;:/?";
            var rs = new StringBuilder();

            while (x != 0)
            {
                rs.Append(charPool[(int)(rnd.NextDouble() * charPool.Length)]);
                x--;
            }
            return rs.ToString();
        }
    }
}