using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinProtector.Protections
{
    internal class Watermark
    {
        //not a quote

        public static void Execute(ModuleDef mod)
        {
            mod.Name = "❀ Rin Protector | lexy#8726";
            mod.Assembly.Name = "❀ Rin Protector | lexy#8726"; 
        }
    }
}
