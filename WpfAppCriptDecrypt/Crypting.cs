using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppCriptDecrypt
{
    public class Cripting
    {
        public static string Criptare(string text)
        {
            string textCriptat = "";
            char randomChar;
            Random rand = new Random();
            for (int i = 0; i <= (text.Length - 1); i++)
            {
                randomChar = (char)(rand.Next(128));
                textCriptat += ((char)(text[i] ^ randomChar)).ToString();
                textCriptat += ((char)(randomChar ^ (128 - i))).ToString();
            }
            return textCriptat;
        }

    }
}
