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
            //initializare generator numere aleatoare
            Random rand = new Random();
            //cripteaza fiecare caracter din textul initial
            for (int i = 0; i <= (text.Length - 1); i++)
            {
                //genereaza un caracter aleator
                randomChar = (char)(rand.Next(128));
                //XOR intre caracterul curent si caracterul generat aleator
                textCriptat += ((char)(text[i] ^ randomChar)).ToString();
                //adaugam caracterul generat aleator la mesajul criptat 
                //cu XOR 128 - i pentru a-l avea la decriptare
                textCriptat += ((char)(randomChar ^ (128 - i))).ToString();
            }
            return textCriptat;
        }

    }
}
