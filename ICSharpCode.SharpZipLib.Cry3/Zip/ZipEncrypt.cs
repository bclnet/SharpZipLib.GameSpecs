using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Runtime.InteropServices;
using static ICSharpCode.SharpZipLib.Zip.Cry3File;

namespace ICSharpCode.SharpZipLib.Zip
{
    /// <summary>
    /// ZipEncrypt
    /// </summary>
    internal unsafe static class ZipEncrypt
    {
        public static bool DecryptBufferWithStreamCipher(char engineId, ref byte[] data, int size, byte[] key, byte[] iv)
        {
            try
            {
                var cipher = new BufferedBlockCipher(new SicRevBlockCipher(engineId == 'A' ? (IBlockCipher)new AesEngine() : new TwofishEngine()));
                cipher.Init(false, new ParametersWithIV(new KeyParameter(key), iv));
                data = cipher.DoFinal(data, 0, size);
            }
            catch (CryptoException ex) { Console.WriteLine(ex.Message); return false; }
            return true;
        }

        public static int GetEncryptionKeyIndex(ZipEntry fileEntry)
            => (int)unchecked(~(fileEntry.Crc >> 2) & 0xF);

        public static void GetEncryptionInitialVector(ZipEntry fileEntry, out byte[] iv)
        {
            unchecked
            {
                var intIV = new[] {
                    (uint)(fileEntry.Size ^ (fileEntry.CompressedSize << 12)),
                    (uint)(fileEntry.CompressedSize != 0 ? 0 : 1),
                    (uint)(fileEntry.Crc ^ (fileEntry.CompressedSize << 12)),
                    (uint)((fileEntry.Size != 0 ? 0 : 1) ^ fileEntry.CompressedSize),
                };
                iv = new byte[16];
                fixed (uint* ptr = intIV) Marshal.Copy((IntPtr)ptr, iv, 0, 16);
            }
        }

        public static bool RsaVerifyData(byte[][] data, int[] sizes, int numBuffers, byte[] signedHash, int signedHashSize, byte[] publicKey)
        {
            // TODO
            return true;
        }

        #region RSA KEY

        // cry
        static byte[] _RsaKey = {
            0x30, 0x81, 0x9F, 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01,
            0x05, 0x00, 0x03, 0x81, 0x8D, 0x00, 0x30, 0x81, 0x89, 0x02, 0x81, 0x81, 0x00, 0xA9, 0xD5, 0x90,
            0xA4, 0xBC, 0x92, 0xDB, 0x8C, 0xF1, 0xFC, 0x5A, 0xD5, 0x8F, 0x46, 0x05, 0x52, 0x16, 0xEE, 0xF3,
            0xC3, 0xBE, 0x86, 0xDE, 0x70, 0x1F, 0x4E, 0x2D, 0x18, 0xD3, 0x01, 0x92, 0x46, 0xBE, 0xFA, 0xAD,
            0x66, 0x04, 0x7B, 0x8C, 0xDD, 0x0D, 0x24, 0x8D, 0xA7, 0x23, 0xCA, 0x52, 0xC8, 0xE5, 0x01, 0xE0,
            0xB7, 0x2B, 0xEB, 0x55, 0xCF, 0x0D, 0xF7, 0x97, 0x77, 0xDC, 0x11, 0xE8, 0x7B, 0x18, 0xCC, 0xDB,
            0x90, 0x07, 0x2D, 0x9D, 0xC4, 0xAD, 0x80, 0x7C, 0x50, 0x23, 0x85, 0x46, 0xF3, 0xE9, 0x2C, 0x54,
            0x81, 0x11, 0x7B, 0x6D, 0xE2, 0x57, 0x87, 0x8E, 0x65, 0xE1, 0xD3, 0x16, 0xC4, 0x54, 0xED, 0x29,
            0xED, 0x51, 0xFD, 0xB1, 0xEF, 0xE4, 0x95, 0x01, 0x24, 0xAE, 0xC0, 0x6A, 0xFA, 0xE0, 0x5B, 0x19,
            0xD2, 0xE6, 0xF0, 0x22, 0x3B, 0xC3, 0xE7, 0xDD, 0x17, 0x1A, 0x8C, 0xF8, 0xE1, 0x02, 0x03, 0x01,
            0x00, 0x01 };

        static AsymmetricKeyParameter GetPublicKey(byte[] keyInfoData)
        {
            if (!(new Asn1InputStream(keyInfoData).ReadObject() is DerSequence sequence)) throw new Exception("Invalid PrivateKey Data");

            AlgorithmIdentifier algId = null; DerBitString keyData = null;
            foreach (var value in sequence)
            {
                if (value is AlgorithmIdentifier || value is DerSequence) algId = AlgorithmIdentifier.GetInstance(value);
                else if (value is DerBitString || value is byte[]) keyData = DerBitString.GetInstance(value);
                else if (value is DerInteger && keyData == null) keyData = new DerBitString(sequence);
            }
            if (keyData == null) throw new Exception("Invalid PrivateKey Data");

            return PublicKeyFactory.CreateKey(new SubjectPublicKeyInfo(algId ?? new AlgorithmIdentifier(PkcsObjectIdentifiers.RsaEncryption), keyData.GetBytes()));
        }

        public static bool DecryptKeysTable(byte[] aesKey, byte[] CDR_IV, byte[] keys_table, int digestSize, out byte[] cdrIV, out byte[][] keysTable)
        {
            var digest = digestSize == 257 ? new Blake2bDigest()
                : digestSize == 256 ? (IDigest)new Sha256Digest()
                : new Sha1Digest();
            try
            {
                var publicKey = GetPublicKey(aesKey ?? _RsaKey);
                var cipher = new OaepEncoding(new RsaEngine(), digest);

                // cdr iv
                cipher.Init(false, publicKey);
                cdrIV = cipher.ProcessBlock(CDR_IV, 0, RSA_KEY_MESSAGE_LENGTH);

                // Decrypt the table of cipher keys.
                keysTable = new byte[BLOCK_CIPHER_NUM_KEYS][];
                for (int i = 0, offset = 0; i < BLOCK_CIPHER_NUM_KEYS; i++, offset += RSA_KEY_MESSAGE_LENGTH)
                {
                    cipher.Init(false, publicKey);
                    keysTable[i] = cipher.ProcessBlock(keys_table, offset, RSA_KEY_MESSAGE_LENGTH);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                cdrIV = default;
                keysTable = default;
                return false;
            }
        }

        #endregion
    }
}