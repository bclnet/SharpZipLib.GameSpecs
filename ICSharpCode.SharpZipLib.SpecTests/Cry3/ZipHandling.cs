using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.Tests.Cry3
{
    public class ZipHandling
    {
        [Test]
        [Category("Cry3")]
        // None (OK)
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Wolcen\Game\Textures_decals.pak", "aes:30818902818100E2725EF9BB168871C238D91B64CFB8B1332F1BBCF105F40F252FB93F3A609D524CF8F5EE09BC554FD918DB8BB3531D6F88BEFEA4BFBDF51CB1E1DF5E5DFA83FD6584D37E279924224FC4F8BB6C98ED50D27002E8BA21F35F0155A08D9ED276714032AEECDA066C17FA54F1C33E5DAF8B332B3CC0771490A15261B2DD908F53F10203010001", "")]
        // TEA, comments:TEA (ERROR)
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Warface\13_2000076\Game\GameInfo.pak", null, "")]
        // comments:NEWHUNT|NEWHUNT (ERROR)
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Hunt Showdown\game_hunt\gamedata.pak", null, "")]
        // comments:STREAMCIPHER_KEYTABLE|CDR_SIGNED (INDEV)
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\SNOW\Assets\GameData.pak", null, "")]
        // TEA (FILE)
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Ryse Son of Rome\GameRyse\GameData.pak", null, "")]
        // comments:CDR_SIGNED (OK)
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Robinson The Journey\game_robinson\gamedata.pak", null, "")]
        // None (OK)
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Crysis Remastered\Game\gamedata.pak", null, "")]
        public void OpenFile(string path, string aesKey, string entryPath)
        {
            using var pakStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var pak = new Cry3File(pakStream, Utils.ParseKey(aesKey));

            var entry = pak.FindEntry(entryPath, true);
            if (entry == -1) throw new FileNotFoundException();

            // test walk names
            foreach (ZipEntry ent in pak) Assert.IsTrue(!string.IsNullOrEmpty(ent.Name));
        }

        [Test]
        [Category("Cry3")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Wolcen\Game\Textures_decals.pak", "aes:30818902818100E2725EF9BB168871C238D91B64CFB8B1332F1BBCF105F40F252FB93F3A609D524CF8F5EE09BC554FD918DB8BB3531D6F88BEFEA4BFBDF51CB1E1DF5E5DFA83FD6584D37E279924224FC4F8BB6C98ED50D27002E8BA21F35F0155A08D9ED276714032AEECDA066C17FA54F1C33E5DAF8B332B3CC0771490A15261B2DD908F53F10203010001", "")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Warface\13_2000076\Game\GameInfo.pak", null, "")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Hunt Showdown\game_hunt\gamedata.pak", null, "")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\SNOW\Assets\GameData.pak", null, "")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Ryse Son of Rome\GameRyse\GameData.pak", null, "")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Robinson The Journey\game_robinson\gamedata.pak", null, "")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Crysis Remastered\Game\gamedata.pak", null, "")]
        public void OpenFileEntry(string path, string aesKey, string entryPath)
        {
            using var pakStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var pak = new Cry3File(pakStream, Utils.ParseKey(aesKey));

            // test export entry
            var entry = pak.FindEntry(entryPath, true);
            if (entry == -1) throw new FileNotFoundException();
            using var input = pak.GetInputStream(entry);
            if (!input.CanRead) throw new Exception();
            var body = new StreamReader(input).ReadToEnd();
            Console.WriteLine(body);
        }
    }
}