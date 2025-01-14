using System;
public class Cripting
{
    private static Random rand = new Random();

    public static (char, char) CriptareChar(char ch, int index)
    {
        char randomChar = (char)(rand.Next(128));
        char encryptedChar = (char)(ch ^ randomChar);
        char keyChar = (char)(randomChar ^ (128 - index));
        return (encryptedChar, keyChar);
    }

    public static char DecriptareChar(char encryptedChar, char keyChar, int index)
    {
        char randomChar = (char)(keyChar ^ (128 - index));
        return (char)(encryptedChar ^ randomChar);
    }
}

