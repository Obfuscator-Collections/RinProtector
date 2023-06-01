#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using KoiVM.AST;
using KoiVM.AST.IL;
using KoiVM.CFG;
using KoiVM.RT.Mutation;
using KoiVM.VM;

#endregion

namespace KoiVM.RT
{
    public class VMRuntime
    {
        private List<Tuple<MethodDef, ILBlock>> basicBlocks;

        internal DbgWriter dbgWriter;

        private List<IKoiChunk> extraChunks;
        private List<IKoiChunk> finalChunks;
        internal Dictionary<MethodDef, Tuple<ScopeBlock, ILBlock>> methodMap;

        private RuntimeMutator rtMutator;
        internal BasicBlockSerializer serializer;
        private readonly IVMSettings settings;

        public VMRuntime(IVMSettings settings, ModuleDef rt)
        {
            this.settings = settings;
            Init(rt);
        }

        public ModuleDef Module => rtMutator.RTModule;

        public VMDescriptor Descriptor
        {
            get;
            private set;
        }

        public byte[] RuntimeLibrary
        {
            get;
            private set;
        }

        public byte[] RuntimeSymbols
        {
            get;
            private set;
        }

        public byte[] DebugInfo => dbgWriter.GetDbgInfo();

        private void Init(ModuleDef rt)
        {
            Descriptor = new VMDescriptor(settings);
            methodMap = new Dictionary<MethodDef, Tuple<ScopeBlock, ILBlock>>();
            basicBlocks = new List<Tuple<MethodDef, ILBlock>>();

            extraChunks = new List<IKoiChunk>();
            finalChunks = new List<IKoiChunk>();
            serializer = new BasicBlockSerializer(this);

            rtMutator = new RuntimeMutator(rt, this);
        }

        public void AddMethod(MethodDef method, ScopeBlock rootScope)
        {
            ILBlock entry = null;
            foreach(ILBlock block in rootScope.GetBasicBlocks())
            {
                if(block.Id == 0)
                    entry = block;
                basicBlocks.Add(Tuple.Create(method, block));
            }
            Debug.Assert(entry != null);
            methodMap[method] = Tuple.Create(rootScope, entry);
        }

        internal void AddHelper(MethodDef method, ScopeBlock rootScope, ILBlock entry)
        {
            methodMap[method] = Tuple.Create(rootScope, entry);
        }

        public void AddBlock(MethodDef method, ILBlock block)
        {
            basicBlocks.Add(Tuple.Create(method, block));
        }

        public ScopeBlock LookupMethod(MethodDef method)
        {
            var m = methodMap[method];
            return m.Item1;
        }

        public ScopeBlock LookupMethod(MethodDef method, out ILBlock entry)
        {
            var m = methodMap[method];
            entry = m.Item2;
            return m.Item1;
        }

        public void AddChunk(IKoiChunk chunk)
        {
            extraChunks.Add(chunk);
        }

        public void ExportMethod(MethodDef method)
        {
            rtMutator.ReplaceMethodStub(method);
        }

        public IModuleWriterListener CommitModule(ModuleDefMD module)
        {
            return rtMutator.CommitModule(module);
        }

        public void CommitRuntime(ModuleDef targetModule = null)
        {
            rtMutator.CommitRuntime(targetModule);
            SaveRuntime.TargetModule = targetModule;
        }
     
        public void OnKoiRequested()
        {
            var header = new HeaderChunk(this);

            foreach(var block in basicBlocks) finalChunks.Add(block.Item2.CreateChunk(this, block.Item1));
            finalChunks.AddRange(extraChunks);
            //finalChunks.Add(new BinaryChunk(Watermark.GenerateWatermark((uint) settings.Seed)));
            Descriptor.Random.Shuffle(finalChunks);
            finalChunks.Insert(0, header);

            ComputeOffsets();
            FixupReferences();
            header.WriteData(this);
            
            SaveRuntime.Save(Module, ChunkData());
        }

        public byte[] ChunkData()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    foreach (var data in finalChunks)
                    {
                        writer.Write(data.GetData());

                    }
                }
                return stream.ToArray();
            }
          
        }
        private void ComputeOffsets()
        {
            uint offset = 0;
            foreach(var chunk in finalChunks)
            {
                chunk.OnOffsetComputed(offset);
                offset += chunk.Length;
            }
        }

        private void FixupReferences()
        {
            foreach(var block in basicBlocks)
            foreach(var instr in block.Item2.Content)
                if(instr.Operand is ILRelReference)
                {
                    var reference = (ILRelReference) instr.Operand;
                    instr.Operand = ILImmediate.Create(reference.Resolve(this), ASTType.I4);
                }
        }

        public void ResetData()
        {
            methodMap = new Dictionary<MethodDef, Tuple<ScopeBlock, ILBlock>>();
            basicBlocks = new List<Tuple<MethodDef, ILBlock>>();

            extraChunks = new List<IKoiChunk>();
            finalChunks = new List<IKoiChunk>();
            Descriptor.ResetData();

            rtMutator.InitHelpers();
        }
    }
}