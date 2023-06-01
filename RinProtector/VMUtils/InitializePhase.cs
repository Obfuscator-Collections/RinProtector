using System;
using System.Collections.Generic;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using KoiVM;
using MethodAttributes = dnlib.DotNet.MethodAttributes;
using System.IO;
using KoiVM.RT.Mutation;
using System.Linq;
using System.Runtime.CompilerServices;
using KoiVM.Protections;

namespace KVM
{
    public class InitializePhase
    {
        // fish.cs
        public static object VirtualizerKey = new object();
        public static object MergeKey = new object();
        public static HashSet<MethodDef> methods = new HashSet<MethodDef>();

        public void InitializeP(ModuleDef module)
        {
          
            foreach (var typeDef in module.Types)
                foreach (MethodDef methodDef in typeDef.Methods)
                {
                    methods.Add(methodDef);
                }

            var seed = new Random().Next(1, int.MaxValue);
            ModuleDef merge = module;

            var vr = new Virtualizer(seed, false);
            vr.ExportDbgInfo = false;
            vr.DoStackWalk = true;
           
            vr.Initialize();

            VirtualizerKey = vr;
            MergeKey = merge;

            vr.CommitRuntime(merge);

         
            var ctor = typeof(InternalsVisibleToAttribute).GetConstructor(new[] { typeof(string) });
            if (methods.Count > 0)
            {
                var ca = new CustomAttribute((ICustomAttributeType)module.Import(ctor));
                ca.ConstructorArguments.Add(new CAArgument(module.CorLibTypes.String, "ッ Protected by RinProtector ッ"));
                module.Assembly.CustomAttributes.Add(ca);
                module.CustomAttributes.Add(ca);
                var ctor2 = typeof(Attribute).GetConstructor(new[] { typeof(string) });

                var ca2 = new CustomAttribute((ICustomAttributeType)module.Import(ctor2));
                ca2.ConstructorArguments.Add(new CAArgument(module.CorLibTypes.String, "❃ Protected by RinProtector ❃"));
                module.CustomAttributes.Add(ca2);

                var ctor3 = typeof(CompilerGeneratedAttribute).GetConstructor(new[] { typeof(string) });

                var ca3 = new CustomAttribute((ICustomAttributeType)module.Import(ctor3));
                ca3.ConstructorArguments.Add(new CAArgument(module.CorLibTypes.String, "✺ lexy#8726 ✺"));
                module.CustomAttributes.Add(ca3);

            }



            MarkPhase(module);
        }

        public void MarkPhase(ModuleDef module)
        {
            var vr = (Virtualizer)VirtualizerKey;

            var refRepl = new Dictionary<IMemberRef, IMemberRef>();

            var oldType = module.GlobalType;
            //var newType = new TypeDefUser("Protected-By-Rin");
        
            oldType.BaseType = module.CorLibTypes.GetTypeRef("System", "Object");
            //module.Types.Insert(0, newType);


            var cctor = oldType.FindOrCreateStaticConstructor();


            cctor.Body = new CilBody(true, new List<Instruction>
            {
                ///*   Instruction.Create(OpCodes.Call, old_cctor)*/,

                Instruction.Create(OpCodes.Ret)


            }, new List<ExceptionHandler>(), new List<Local>()) ;         
            for (int i = 0; i < oldType.Methods.Count; i++)
            {
                var nativeMethod = oldType.Methods[i];
                if (nativeMethod.IsNative)
                {
                    var methodStub = new MethodDefUser(nativeMethod.Name, nativeMethod.MethodSig.Clone());
                    methodStub.Attributes = MethodAttributes.Assembly | MethodAttributes.Static;
                    methodStub.Body = new CilBody();
                    methodStub.Body.Instructions.Add(new Instruction(OpCodes.Jmp, nativeMethod));
                    methodStub.Body.Instructions.Add(new Instruction(OpCodes.Ret));

                    oldType.Methods[i] = methodStub;
                   oldType.Methods.Add(nativeMethod);
                    refRepl[nativeMethod] = methodStub;
                }
            }

        

            var toProcess = new Dictionary<ModuleDef, List<MethodDef>>();
            foreach (var entry in new Scanner(module, methods).Scan())
            {

                vr.AddMethod(entry.Item1, entry.Item2);
                toProcess.AddListEntry(entry.Item1.Module, entry.Item1);
            }

            Utils.ModuleWriterListener.OnWriterEvent += new Listener
            {
                vr = vr,
                methods = toProcess,
                refRepl = refRepl,
                module = module
            }.OnWriterEvent;
        }

        class Listener
        {
            public Virtualizer vr;
            public Dictionary<ModuleDef, List<MethodDef>> methods;
            public Dictionary<IMemberRef, IMemberRef> refRepl;
            IModuleWriterListener commitListener = null;
            public ModuleDef module;
            public void OnWriterEvent(object sender, ModuleWriterListenerEventArgs e)
            {
                var writer = (ModuleWriter)sender;
                if (commitListener != null)
                    commitListener.OnWriterEvent(writer, e.WriterEvent);

                if (e.WriterEvent == ModuleWriterEvent.MDBeginWriteMethodBodies && methods.ContainsKey(writer.Module))
                {

                    vr.ProcessMethods(writer.Module);

                    foreach (var repl in refRepl)
                        vr.Runtime.Descriptor.Data.ReplaceReference(repl.Key, repl.Value);

                    commitListener = vr.CommitModule((ModuleDefMD)module);
                }
            }
        }

    }

}