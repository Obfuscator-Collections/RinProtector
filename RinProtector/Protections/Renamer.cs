using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using Utils.Analyzer;

namespace RinProtector.Protections
{
    internal class Renamer
    {
        private static int MethodAmount { get; set; }

        private static int ParameterAmount { get; set; }

        private static int PropertyAmount { get; set; }

        private static int FieldAmount { get; set; }

        private static int EventAmount { get; set; }
        public static string GenerateRandomString(int leng)
        {
            int length = leng;

         
            StringBuilder str_build = new StringBuilder();
            Random random = new Random();

            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                str_build.Append(letter);
            }
            return str_build.ToString();
        }
        public static void Execute(ModuleDef mod)
        {
            foreach (TypeDef type in mod.Types)
            {
                if (!type.IsSpecialName)
                {
                    type.Name = "ツ-Rin-Protector-" + Randomizer.GenerateRandomString(30);
                }
                type.Namespace = "ツ-Rin-Protector-" + Randomizer.GenerateRandomString(30);
                foreach (MethodDef m in type.Methods)
                {
                    if (CanRename(m))
                    {
                        m.Name = "ツ-Rin-Protector-" + Randomizer.GenerateRandomString(30);
                        ++MethodAmount;
                    }

                    foreach (Parameter para in m.Parameters)
                        if (CanRename(para))
                        {
                            para.Name = "ツ-Rin-Protector-" + Randomizer.GenerateRandomString(30);
                            ++ParameterAmount;
                        }
                }

                foreach (PropertyDef p in type.Properties)
                    if (CanRename(p))
                    {
                        p.Name = "ツ-Rin-Protector-" + Randomizer.GenerateRandomString(30);
                        ++PropertyAmount;
                    }

                foreach (FieldDef field in type.Fields)
                    if (CanRename(field))
                    {
                        field.Name = "ツ-Rin-Protector-" + Randomizer.GenerateRandomString(30);
                        ++FieldAmount;
                    }

                foreach (EventDef e in type.Events)
                    if (CanRename(e))
                    {
                        e.Name = "ツ-Rin-Protector-" + Randomizer.GenerateRandomString(30);
                        ++EventAmount;
                    }
            }




        }


        public static bool CanRename(object obj)
        {
            DefAnalyzer analyze;
            if (obj is MethodDef) analyze = new MethodDefAnalyzer();
            else if (obj is PropertyDef) analyze = new PropertyDefAnalyzer();
            else if (obj is EventDef) analyze = new EventDefAnalyzer();
            else if (obj is FieldDef) analyze = new FieldDefAnalyzer();
            else if (obj is Parameter) analyze = new ParameterAnalyzer();
            else return false;
            return analyze.Execute(obj);
        }
    }
}
