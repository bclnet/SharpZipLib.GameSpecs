﻿namespace ICSharpCode.SharpZipLib.Zip
{
    /// <summary>
    /// ZipDirStructures
    /// </summary>
    internal unsafe static class ZipDir
    {
        static readonly uint[] TEA_DEFAULTKEY = { 0xc968fb67, 0x8f9b4267, 0x85399e84, 0xf9b99dc4 };
        const uint TEA_DELTA = 0x9e3779b9;
        static void btea(uint* v, int n, uint[] k)
        {
            uint y, z, sum;
            uint p, rounds, e;
            uint TEA_MX() => ((z >> 5 ^ y << 2) + (y >> 3 ^ z << 4)) ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));

            if (n > 1) // Coding Part
            {
                rounds = (uint)(6 + 52 / n);
                sum = 0;
                z = v[n - 1];
                do
                {
                    sum += TEA_DELTA;
                    e = (sum >> 2) & 3;
                    for (p = 0; p < (uint)(n - 1); p++)
                    {
                        y = v[p + 1];
                        z = v[p] += TEA_MX();
                    }
                    y = v[0];
                    z = v[n - 1] += TEA_MX();
                } while (--rounds != 0);
            }
            else if (n < -1) // Decoding Part
            {
                n = -n;
                rounds = (uint)(6 + 52 / n);
                sum = rounds * TEA_DELTA;
                y = v[0];
                do
                {
                    e = (sum >> 2) & 3;
                    for (p = (uint)(n - 1); p > 0; p--)
                    {
                        z = v[p - 1];
                        y = v[p] -= TEA_MX();
                    }
                    z = v[n - 1];
                    y = v[0] -= TEA_MX();
                } while ((sum -= TEA_DELTA) != 0);
            }
        }

        static void SwapByteOrder(uint* values, int count)
        {
            for (uint* w = values, e = values + count; w != e; ++w)
                *w = (*w >> 24) + ((*w >> 8) & 0xff00) + ((*w & 0xff00) << 8) + (*w << 24);
        }

        internal static void TeaEncrypt(ref byte[] data, int size)
        {
            fixed (byte* dataPtr = data)
            {
                var intBuffer = (uint*)dataPtr;
                var encryptedLen = size >> 2;

                SwapByteOrder(intBuffer, encryptedLen);
                btea(intBuffer, encryptedLen, TEA_DEFAULTKEY);
                SwapByteOrder(intBuffer, encryptedLen);
            }
        }

        internal static void TeaDecrypt(ref byte[] data, int size, Cry3File.CryCustomTeaEncryptionHeader header)
        {
            fixed (byte* dataPtr = data)
            {
                var intBuffer = (uint*)dataPtr;
                var encryptedLen = size >> 2;

                SwapByteOrder(intBuffer, encryptedLen);
                btea(intBuffer, -encryptedLen, TEA_DEFAULTKEY);
                SwapByteOrder(intBuffer, encryptedLen);
            }
        }

        internal static void StreamCipher(ref byte[] data, int size, uint inKey = 0)
        {
            //    StreamCipherState cipher;
            //    gEnv->pSystem->GetCrypto()->GetStreamCipher()->Init(cipher, (const uint8*)&inKey, sizeof(inKey));
            //    gEnv->pSystem->GetCrypto()->GetStreamCipher()->EncryptStream(cipher, (uint8*)buffer, size, (uint8*)buffer);
        }
    }
}
