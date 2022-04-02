using ICSharpCode.SharpZipLib.Zip;
using System.Globalization;
using System.Text;

//:ref https://github.com/neoeinstein/bouncycastle/blob/master/crypto/src/security/CipherUtilities.cs

// None (OK)
//const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\Wolcen\Game\Textures_decals.pak";
//const string AesKey = null; // @"aes:30818902818100E2725EF9BB168871C238D91B64CFB8B1332F1BBCF105F40F252FB93F3A609D524CF8F5EE09BC554FD918DB8BB3531D6F88BEFEA4BFBDF51CB1E1DF5E5DFA83FD6584D37E279924224FC4F8BB6C98ED50D27002E8BA21F35F0155A08D9ED276714032AEECDA066C17FA54F1C33E5DAF8B332B3CC0771490A15261B2DD908F53F10203010001";

// TEA, comments:TEA (ERROR)
//const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\Warface\13_2000076\Game\GameInfo.pak";
//const string AesKey = null;

// comments:NEWHUNT|NEWHUNT (ERROR)
//const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\Hunt Showdown\game_hunt\gamedata.pak";
//const string AesKey = null;

// comments:STREAMCIPHER_KEYTABLE|CDR_SIGNED (FILE)
const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\SNOW\Assets\GameData.pak";
const string AesKey = null;

// TEA (FILE)
//const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\Ryse Son of Rome\GameRyse\GameData.pak";
//const string AesKey = null;

// comments:CDR_SIGNED (OK)
//const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\Robinson The Journey\game_robinson\gamedata.pak";
//const string AesKey = null;

// None (OK)
//const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\Crysis Remastered\Game\gamedata.pak";
//const string AesKey = null;

using var pakStream = File.Open(PakPath, FileMode.Open, FileAccess.Read, FileShare.Read);
var pak = new Cry3File(pakStream, Utils.ParseKey(AesKey));
foreach (ZipEntry ent in pak)
{
	Console.WriteLine(ent.Name);

    // read
    using var input = pak.GetInputStream(ent);
    var s = new MemoryStream();
    input.CopyTo(s);
    s.Position = 0;
    Console.WriteLine(Encoding.ASCII.GetString(s.ToArray()));
}


//const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\ArcheAge\game_pak";
//const string AesKey = @"aes:321F2AEEAA584AB49A6C9E09D59E9C6F";
