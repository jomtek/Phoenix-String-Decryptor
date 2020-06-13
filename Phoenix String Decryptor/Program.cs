using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;

namespace Phoenix_String_Decryptor
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.Title = "Phoenix String Decryptor (by Jomtek and Mindsystemm)";

                string modulePath = "";
                string methodToken = "";
                bool dynamicAnalysis = true;
                Assembly assembly = null;

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

                // Huge thanks to https://github.com/MindSystemm
                try
                {
                    assembly = Assembly.LoadFrom(args[0]);
                }
                catch
                {
                    Console.WriteLine("[Assembly load failed, static analysis chosen]");
                    dynamicAnalysis = false;
                }

                if (dynamicAnalysis)
                {
                    Console.Write("Do you want to use dynamic analysis (Y/N): ");
                    dynamicAnalysis = Console.ReadLine().ToUpper() == "Y";
                    Console.Title += " (" + (dynamicAnalysis ? "dynamic" : "static") + ")";
                }


                Console.Write("Method token : ");
                methodToken = Console.ReadLine().ToUpper();

                if (methodToken.StartsWith("0X"))
                    methodToken = methodToken.Substring(2);

                if (methodToken.Length != 8)
                {
                    Console.WriteLine("Error : invalid method token...");
                    Thread.Sleep(700);
                    continue;
                }

                StringDecryptor myDecryptor = new StringDecryptor(modulePath, methodToken, dynamicAnalysis, assembly);

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
