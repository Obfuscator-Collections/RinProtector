using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace RinProtector.Protections
{
    internal class Junk
    {

        //not a quote

        public static void Execute(ModuleDef module)
        {
            foreach (var item31 in module.GetTypes())
            {
                item31.Namespace = "";
            }

            for (int i = 0; i < 100; i++)
            {

                var junk = new TypeDefUser("", "ツ-Rin-Protector-" + Randomizer.GenerateRandomString(30));
                module.Types.Add(junk);
            }





        }
    }
}
