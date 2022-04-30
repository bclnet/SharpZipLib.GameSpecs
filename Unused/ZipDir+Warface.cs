using System;

namespace ICSharpCode.SharpZipLib.Zip
{
    /// <summary>
    /// ZipDirStructures
    /// </summary>
    unsafe static partial class ZipDir
    {
        // https://github.com/n1kodim/WarfaceXTEACrypt/blob/main/CryTea%D0%A1rypt.cpp
        // https://www.unknowncheats.me/forum/warface/450666-game-cfg-decrypt.html

        //static readonly uint[] WARFACETEA_DEFAULTKEY = { 0x4dd87487, 0x0c15011b0, 0x5edd6b3d, 0x43cf5892 };
        internal static readonly byte[] WARFACETEA_DEFAULTKEY = { 
            0x87, 0x74, 0xd8, 0x4d,
            0xb0, 0x11, 0x50, 0xc1,
            0x3d, 0x6b, 0xdd, 0x5e,
            0x92, 0x58, 0xcf, 0x43
        };

        // src and trg can be the same pointer (in place decryption)
        // len must be in bytes and must be multiple of 8 byts (64bits).
        // key is 128bit: int key[4] = {n1,n2,n3,n4};
        // void decipher(unsigned int *const v,unsigned int *const w,const unsigned int *const k)
        static void TEA_ENCODE(uint* src, uint* trg, int len, uint* key)
        {
            uint* v = src, w = trg; int nlen = (len) >> 3;
            uint a = key[0], b = key[1], c = key[2], d = key[3];
            while (nlen-- != 0)
            {
                uint y = v[0], z = v[1], n = 32, sum = 0;
                while (n-- > 0) { sum += TEA_DELTA; y += (z << 4) + a ^ z + sum ^ (z >> 5) + b; z += (y << 4) + c ^ y + sum ^ (y >> 5) + d; }
                w[0] = y; w[1] = z; v += 2; w += 2;
            }
        }

        // src and trg can be the same pointer (in place decryption)
        // len must be in bytes and must be multiple of 8 byts (64bits).
        // key is 128bit: int key[4] = {n1,n2,n3,n4};
        // void decipher(unsigned int *const v,unsigned int *const w,const unsigned int *const k)
        static void TEA_DECODE(uint* src, uint* trg, int len, uint* key)
        {
            uint* v = src, w = trg; int nlen = len >> 3;
            uint a = key[0], b = key[1], c = key[2], d = key[3];
            while (nlen-- != 0)
            {
                uint y = v[0], z = v[1], sum = 0xC6EF3720, n = 32;
                while (n-- > 0) { z -= (y << 4) + c ^ y + sum ^ (y >> 5) + d; y -= (z << 4) + a ^ z + sum ^ (z >> 5) + b; sum -= TEA_DELTA; }
                w[0] = y; w[1] = z; v += 2; w += 2;
            }
        }

        static void NotBytes(uint* values, int count)
        {
            for (uint* w = values, e = values + count; w != e; ++w) *w = ~*w;
        }

        internal static void TeaDecrypt(ref byte[] data, int size, byte[] key)
        {
            System.IO.File.WriteAllBytes(@"C:\T_\CDR.txt", data);
            size -= 3;
            var newData = new byte[size];
            Array.Copy(data, 3, newData, 0, newData.Length);
            data = newData;
            fixed (byte* dataPtr = data)
            fixed (byte* keyPtr = key)
            {
                var intBuffer = (uint*)dataPtr;
                var key2Ptr = (uint*)keyPtr;
                var encryptedLen = size >> 2;

                NotBytes(intBuffer, encryptedLen);
                TEA_DECODE(intBuffer, intBuffer, size, key2Ptr);
            }
        }
    }
}
