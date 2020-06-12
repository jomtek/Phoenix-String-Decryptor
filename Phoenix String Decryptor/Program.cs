using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Phoenix_String_Decryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();

                string modulePath = "";
                string methodToken = "";

                try
                {
                    modulePath = args[0];
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("Error : no module selected... press ENTER to exit.");
                    Console.ReadLine();
                    Environment.Exit(0);
                }

                Console.Write("Method token : ");
                methodToken = Console.ReadLine().ToUpper();

                if (methodToken.StartsWith("0X"))
                    methodToken = methodToken.Substring(2);

                if (methodToken.Length != 8)
                {
                    Console.WriteLine("Invalid method token...");
                    Thread.Sleep(700);
                    continue;
                }

                StringDecryptor myDecryptor = new StringDecryptor(modulePath, methodToken);

                myDecryptor.OpenModule();

                int totalDecryptedStrings = myDecryptor.DecryptStrings();
                Console.WriteLine("Decrypted " + totalDecryptedStrings + " strings successfully.");

                myDecryptor.SaveModule();

                break;
            }

            Thread.Sleep(650);
        }
    }
}
