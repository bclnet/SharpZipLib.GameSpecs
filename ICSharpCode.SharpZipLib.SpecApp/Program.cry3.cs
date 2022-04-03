using ICSharpCode.SharpZipLib.Zip;
using System.Globalization;
using System.Text;

//:ref https://github.com/neoeinstein/bouncycastle/blob/master/crypto/src/security/CipherUtilities.cs

//None(OK)
//const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\Wolcen\Game\Textures_decals.pak";
//const string AesKey = null; // @"aes:30818902818100E2725EF9BB168871C238D91B64CFB8B1332F1BBCF105F40F252FB93F3A609D524CF8F5EE09BC554FD918DB8BB3531D6F88BEFEA4BFBDF51CB1E1DF5E5DFA83FD6584D37E279924224FC4F8BB6C98ED50D27002E8BA21F35F0155A08D9ED276714032AEECDA066C17FA54F1C33E5DAF8B332B3CC0771490A15261B2DD908F53F10203010001";

// TEA, comments:TEA (ERROR)
//const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\Warface\13_2000076\Game\GameInfo.pak";
//const string AesKey = null;

// comments:NEWHUNT|NEWHUNT (ERROR)
//const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\Hunt Showdown\game_hunt\gamedata.pak";
//const string AesKey = null;

// comments:STREAMCIPHER_KEYTABLE|CDR_SIGNED (OK)
const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\SNOW\Assets\GameData.pak";
const string AesKey = "aes:30818902818100D51E1D3810C4A112B2F2504B83E2F124009C0AC9CD1661913421D4E94623AD7014599DAFB0DC9F8366D164AD072B3DC5AA3D4CD24542D5F684E6A4F7473102DE2ACA11F6524015ECBD564248FC712B3A69B15B78EFAA06748259DDE77A75757E513F7AC21A0151F53C78FF45ABCC45C3F54BC6305F420981F7119AF03E6438D70203010001";

// TEA (OK)
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
    try
    {
        using var input = pak.GetInputStream(ent);
        var s = new MemoryStream();
        input.CopyTo(s);
        s.Position = 0;
        Console.WriteLine(Encoding.ASCII.GetString(s.ToArray()));
    }
    catch (Exception e) { Console.WriteLine(e.Message); }
}


//const string PakPath = @"D:\Program Files (x86)\Steam\steamapps\common\ArcheAge\game_pak";
//const string AesKey = @"aes:321F2AEEAA584AB49A6C9E09D59E9C6F";
