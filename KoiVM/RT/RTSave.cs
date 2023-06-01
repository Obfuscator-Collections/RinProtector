using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace KoiVM.RT
{
    public static class SaveRuntime
    {
        public static ModuleDef TargetModule;

        public static string RTName { get; private set; } = "RinRT";

        #region encryption
        public static byte[] Compress(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (GZipStream dstream = new GZipStream(output, CompressionMode.Compress))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            MemoryStream input = new MemoryStream(data);
            MemoryStream output = new MemoryStream();
            using (GZipStream dstream = new GZipStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

  


        public static byte[] EncryptOrDecrypt(byte[] text, byte[] key)
        {
            byte[] xor = new byte[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                xor[i] = (byte)(text[i] ^ key[i % key.Length]);
            }
            return xor;
        }



        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var ms = new MemoryStream())
            using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();

                return ms.ToArray();
            }
        }
        #endregion

        public static void Save(ModuleDef rtmd, byte[] buffer)
        {
            Console.WriteLine("Preparing VM Runtime..");
            var stream = new MemoryStream();
            var options = new ModuleWriterOptions(rtmd);
            byte[] bytes = Encoding.ASCII.GetBytes("lexy#8726");

            var buffer_enc = EncryptOrDecrypt(buffer, bytes);
            rtmd.Resources.Add(new EmbeddedResource("RinVM", buffer_enc, ManifestResourceAttributes.Public));

            rtmd.Write(stream, options);

            var RuntimeLib = stream.ToArray();

            if (File.Exists(Path.Combine(Path.GetDirectoryName(TargetModule.Location), RTName)))
                File.Delete(Path.Combine(Path.GetDirectoryName(TargetModule.Location), RTName));

            if (!Directory.Exists(Path.GetDirectoryName(TargetModule.Location)))
                Directory.CreateDirectory(Path.GetDirectoryName(TargetModule.Location));



            File.WriteAllBytes(Path.Combine(Path.GetDirectoryName(TargetModule.Location) + "//Rin//", RTName + ".dll"), RuntimeLib);
          
        }
    }
}
