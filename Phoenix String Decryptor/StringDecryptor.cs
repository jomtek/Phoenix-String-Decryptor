using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Pdb;
using dnlib.DotNet.Writer;
using Microsoft.SqlServer.Server;

namespace Phoenix_String_Decryptor
{
    class StringDecryptor
    {
        private ModuleDefMD module;
        private string modulePath;
        private string methodToken;
        private bool dynamicAnalysis;
        private Assembly assembly;

        public StringDecryptor(string modulePath, string methodToken, bool dynamicAnalysis, Assembly assembly)
        {
            this.modulePath = modulePath;
            this.methodToken = methodToken;
            this.dynamicAnalysis = dynamicAnalysis;
            this.assembly = assembly;
        }

        private bool IsTypeAnalyzable(TypeDef moduleType) =>
            moduleType.HasMethods && !moduleType.Namespace.Contains(".My");

        private string PhoenixAlgorithm(string inputStr) // for static analysis
        {
            int length = inputStr.Length;
            char[] array = new char[length];
            for (int i = 0; i < array.Length; i++)
            {
                char c = inputStr[i];
                byte b = (byte)((int)c ^ length - i);
                byte b2 = (byte)((int)(c >> 8) ^ i);
                array[i] = (char)((int)b2 << 8 | (int)b);
            }
            return string.Intern(new string(array));
        }

        public void OpenModule()
        {
            ModuleContext modCtx = ModuleDef.CreateModuleContext();
            module = ModuleDefMD.Load(modulePath, modCtx);
        }

        private int AnalyzeMethodBody(MethodDef method)
        {
            if (method.Body == null) return 0;

            int decryptedStrings = 0;
            int counter = 0;

            foreach (Instruction instruction in method.Body.Instructions)
            {
                if (instruction.OpCode == OpCodes.Call)
                {
                    try
                    {
                        MethodDef calledMethod = (MethodDef)instruction.Operand;
                        if (calledMethod.MDToken.ToString() == methodToken)
                        {
                            instruction.OpCode = OpCodes.Nop;

                            Instruction ldstrInstr = method.Body.Instructions[counter - 1];
                            string encryptedString = (string)ldstrInstr.Operand;
                            string resolvedString;

                            if (dynamicAnalysis)
                            {
                                resolvedString =
                                    (string)assembly.ManifestModule.ResolveMethod(calledMethod.MDToken.ToInt32()).Invoke(null, new object[] { encryptedString });
                            } else
                            {
                                resolvedString = PhoenixAlgorithm(encryptedString);
                            }

                            ldstrInstr.Operand = resolvedString;
                            decryptedStrings++;
                        }
                    }
                    catch (InvalidCastException) { };
                }

                counter++;
            }

            return decryptedStrings;
        }
        public int DecryptStrings()
        {
            if (module != null)
            {
                int totalDecryptedStrings = 0;

                for (int i = 0; i < module.Types.Count; i++)
                {
                    TypeDef moduleType = module.Types[i];
                    if (IsTypeAnalyzable(moduleType))
                    {
                        for (int i2 = 0; i2 < moduleType.Methods.Count; i2++)
                        {
                            MethodDef method = moduleType.Methods[i2];
                            totalDecryptedStrings += AnalyzeMethodBody(method);
                            if (method.MDToken.ToString() == methodToken) moduleType.Methods.Remove(method);
                        }
                    }
                }

                return totalDecryptedStrings;
            } else
            {
                throw new NullReferenceException();
            }
        }

        public void SaveModule()
        {
            if (module != null)
            {
                string savePath = Path.ChangeExtension(modulePath, null);
                var opts = new ModuleWriterOptions(module);
                opts.MetadataOptions.Flags = MetadataFlags.KeepOldMaxStack;
                opts.MetadataLogger = DummyLogger.NoThrowInstance;
                module.Write(savePath + "_decrypted.exe", opts);
            } else
            {
                throw new NullReferenceException();
            }
        }

    }
}