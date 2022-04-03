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
    public unsafe static class ZipEncrypt
    {
        static bool StartStreamCipher(byte[] key, byte[] iv, out IBufferedCipher cipher)
        {
            try
            {
                cipher = new BufferedBlockCipher(new SicRevBlockCipher(new TwofishEngine()));
                cipher.Init(false, new ParametersWithIV(new KeyParameter(key), iv));
            }
            catch (CryptoException ex) { Console.WriteLine(ex.Message); cipher = default; return false; }
            return true;
        }

        static bool DecryptBufferWithStreamCipher(ref byte[] data, int size, IBufferedCipher cipher)
        {
            try
            {
                data = cipher.DoFinal(data, 0, size);
                return true;
            }
            catch (CryptoException ex) { Console.WriteLine(ex.Message); return false; }
        }

        public static bool DecryptBufferWithStreamCipher(ref byte[] data, int size, byte[] key, byte[] iv)
        {
            if (!StartStreamCipher(key, iv, out var cipher)) return false;
            if (!DecryptBufferWithStreamCipher(ref data, size, cipher)) return false;
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

        internal static bool RsaVerifyData(byte[][] data, int[] sizes, int numBuffers, byte[] signedHash, int signedHashSize, byte[] publicKey)
        {
            // TODO
            return true;
        }

        #region RSA KEY

        // cry
        public static byte[] _RsaKey = {
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
            0x00, 0x01
        };

        // snow
        //public static byte[] _RsaKey = {
        //    0x30, 0x81, 0x89, 0x02, 0x81, 0x81, 0x00, 0xD5, 0x1E, 0x1D, 0x38, 0x10, 0xC4, 0xA1, 0x12, 0xB2, 0xF2, 0x50, 0x4B, 0x83, 0xE2, 0xF1, 0x24, 0x00, 0x9C, 0x0A, 0xC9, 0xCD, 0x16, 0x61, 0x91, 0x34, 
        //    0x21, 0xD4, 0xE9, 0x46, 0x23, 0xAD, 0x70, 0x14, 0x59, 0x9D, 0xAF, 0xB0, 0xDC, 0x9F, 0x83, 0x66, 0xD1, 0x64, 0xAD, 0x07, 0x2B, 0x3D, 0xC5, 0xAA, 0x3D, 0x4C, 0xD2, 0x45, 0x42, 0xD5, 0xF6, 0x84, 
        //    0xE6, 0xA4, 0xF7, 0x47, 0x31, 0x02, 0xDE, 0x2A, 0xCA, 0x11, 0xF6, 0x52, 0x40, 0x15, 0xEC, 0xBD, 0x56, 0x42, 0x48, 0xFC, 0x71, 0x2B, 0x3A, 0x69, 0xB1, 0x5B, 0x78, 0xEF, 0xAA, 0x06, 0x74, 0x82, 
        //    0x59, 0xDD, 0xE7, 0x7A, 0x75, 0x75, 0x7E, 0x51, 0x3F, 0x7A, 0xC2, 0x1A, 0x01, 0x51, 0xF5, 0x3C, 0x78, 0xFF, 0x45, 0xAB, 0xCC, 0x45, 0xC3, 0xF5, 0x4B, 0xC6, 0x30, 0x5F, 0x42, 0x09, 0x81, 0xF7, 
        //    0x11, 0x9A, 0xF0, 0x3E, 0x64, 0x38, 0xD7, 0x02, 0x03, 0x01, 0x00, 0x01
        //};

        // wolcen
        //30818902818100E2725EF9BB168871C238D91B64CFB8B1332F1BBCF105F40F252FB93F3A609D524CF8F5EE09BC554FD918DB8BB3531D6F88BEFEA4BFBDF51CB1E1DF5E5DFA83FD6584D37E279924224FC4F8BB6C98ED50D27002E8BA21F35F0155A08D9ED276714032AEECDA066C17FA54F1C33E5DAF8B332B3CC0771490A15261B2DD908F53F10203010001

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

        internal static bool DecryptKeysTable(byte[] aesKey, ref CryCustomEncryptionHeader headerEncryption, out byte[] cdrIV, out byte[][] keysTable)
        {
            try
            {
                var publicKey = GetPublicKey(aesKey ?? _RsaKey);
                var cipher = new OaepEncoding(new RsaEngine(), new Sha256Digest());
                cipher.Init(false, publicKey);
                cdrIV = cipher.ProcessBlock(headerEncryption.CDR_IV, 0, RSA_KEY_MESSAGE_LENGTH);

                // Decrypt the table of cipher keys.
                keysTable = new byte[BLOCK_CIPHER_NUM_KEYS][];
                for (int i = 0, offset = 0; i < BLOCK_CIPHER_NUM_KEYS; i++, offset += RSA_KEY_MESSAGE_LENGTH)
                {
                    cipher.Init(false, publicKey);
                    keysTable[i] = cipher.ProcessBlock(headerEncryption.keys_table, offset, RSA_KEY_MESSAGE_LENGTH);
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