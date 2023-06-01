using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.IO.Compression;

namespace KoiVM.Runtime.Data {
    internal unsafe class VMDataInitializer
    {
        internal static VMData GetData(Module module)
        {
            if (!Platform.LittleEndian)
                throw new PlatformNotSupportedException();

            return new VMData(module, GetKoiStreamMapped());
        }
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
        private static byte[] Key = Encoding.ASCII.GetBytes(@"qwr{@^h`h&_`50/ja9!'dcmh3!uw<&=?");
        private static byte[] IV = Encoding.ASCII.GetBytes(@"9/\~V).A,lY&=t2b");

        

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
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
        #endregion

        public static byte[] EncryptOrDecrypt(byte[] text, byte[] key)
        {
            byte[] xor = new byte[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                xor[i] = (byte)(text[i] ^ key[i % key.Length]);
            }
            return xor;
        }



        static void* GetKoiStreamMapped()
        {
            var res = typeof(VMDataInitializer).Assembly.GetManifestResourceStream("RinVM");
            byte[] bytes = Encoding.ASCII.GetBytes("lexy#8726");

            var res_nonencrypt = EncryptOrDecrypt(ReadFully(res), bytes);
            Stream stream = new MemoryStream(res_nonencrypt);

            var reader = new System.IO.BinaryReader(stream);   
            byte[] data = reader.ReadBytes((int)stream.Length);

            void* ptr = (void*)Marshal.AllocHGlobal((int)stream.Length);
            Marshal.Copy(data, 0, (IntPtr)ptr, data.Length);
            return ptr;

        }

    }

}