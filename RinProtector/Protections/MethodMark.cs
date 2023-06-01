using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RinProtector.Protections
{
    internal class MethodMark
    {

        //not a quote

      public static void Execute(ModuleDef mod)
        {
            foreach (TypeDef typeDef in mod.GetTypes())
            {
                foreach (MethodDef methodDef in typeDef.Methods)
                {

                    TypeRef attrRef = mod.CorLibTypes.GetTypeRef("System", "ObsoleteAttribute");
                   
                    var ctorRef = new MemberRefUser(mod, ".ctor", MethodSig.CreateInstance(mod.CorLibTypes.Void, mod.CorLibTypes.String,mod.CorLibTypes.Int32), attrRef);

                    var attr = new CustomAttribute(ctorRef);
                    attr.ConstructorArguments.Add(new CAArgument(mod.CorLibTypes.String, "RinVM"));
                    attr.ConstructorArguments.Add(new CAArgument(mod.CorLibTypes.Int32, 8726));
                    methodDef.CustomAttributes.Add(attr);
        
                  

                  
                   

                }
            }
        }
    }
}
