//using ICSharpCode.SharpZipLib.Zip;
//using System.Globalization;

////:ref https://github.com/neoeinstein/bouncycastle/blob/master/crypto/src/security/CipherUtilities.cs

//const string PakPath1 = @"D:\Program Files (x86)\Steam\steamapps\common\ArcheAge\game_pak";
//const string AesKey1 = @"aes:321F2AEEAA584AB49A6C9E09D59E9C6F";
//const string AesAlgo1 = @"AES/CBC/PKCS7PADDING";

//const string PakPath2 = @"D:\Program Files (x86)\Steam\steamapps\common\Warface\13_2000076\Game\GameInfo.pak";
//const string AesKey2 = @"aes:30818902818100E2725EF9BB168871C238D91B64CFB8B1332F1BBCF105F40F252FB93F3A609D524CF8F5EE09BC554FD918DB8BB3531D6F88BEFEA4BFBDF51CB1E1DF5E5DFA83FD6584D37E279924224FC4F8BB6C98ED50D27002E8BA21F35F0155A08D9ED276714032AEECDA066C17FA54F1C33E5DAF8B332B3CC0771490A15261B2DD908F53F10203010001";
//const string AesAlgo2 = @"TWOFISH/CBC/NoPadding";

//using var pakStream = File.Open(PakPath1, FileMode.Open, FileAccess.Read, FileShare.Read);
//var pak = new AesFile(pakStream, AesAlgo1, ParseKey(AesKey1));
//foreach (ZipEntry ent in pak) Console.WriteLine(ent.Name);
