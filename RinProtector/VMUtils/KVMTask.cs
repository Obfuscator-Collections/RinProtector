using dnlib.DotNet;
using dnlib.DotNet.Writer;
using RinProtector.Protections;
using System.IO;

namespace KVM
{
    public class KVMTask
    {
        public void VMAndOtherProtections(ModuleDef module, string outPath, string snPath, string snPass)
        {
            assemblyReferencesAdder(module);

            Utils.ModuleWriterListener = new ModuleWriterListener();
            Utils.ModuleWriterOptions = new ModuleWriterOptions(module);

            Utils.ModuleWriterOptions.Listener = Utils.ModuleWriterListener;
            Utils.ModuleWriterOptions.Logger = DummyLogger.NoThrowInstance;

            if (File.Exists(snPath))
            {
                StrongNameKey signatureKey = Utils.LoadSNKey(snPath, snPass);
                Utils.ModuleWriterOptions.InitializeStrongNameSigning(module, signatureKey);
            }
            new InitializePhase().InitializeP(module);
            //EXECUTING OTHER PROTECTIONS

          
            Renamer.Execute(module);
            Junk.Execute(module); 
            Watermark.Execute(module);
            MethodMark.Execute(module);

            //EXECUTING OTHER PROTECTIONS
            MemoryStream output = new MemoryStream();
            module.Write(output, Utils.ModuleWriterOptions);

            File.WriteAllBytes(outPath, output.ToArray());
        }

        private void assemblyReferencesAdder(ModuleDef moduleDefMD)
        {
            var asmResolver = new AssemblyResolver { EnableTypeDefCache = true };
            var modCtx = new ModuleContext(asmResolver);
            asmResolver.DefaultModuleContext = modCtx;
            var asmRefs = moduleDefMD.GetAssemblyRefs();
            moduleDefMD.Context = modCtx;
            foreach (var asmRef in asmRefs)
            {
                try
                {
                    if (asmRef == null) continue;
                    var asm = asmResolver.Resolve(asmRef.FullName, moduleDefMD);
                    if (asm == null) continue;
                    moduleDefMD.Context.AssemblyResolver.AddToCache(asm);
                }
                catch{}
            }
        }
    }
}
