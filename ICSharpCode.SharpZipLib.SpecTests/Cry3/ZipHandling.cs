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
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Wolcen\Game\Textures_decals.pak", null, "Textures/decals/blood/blood_beach_diluted.dds")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Warface\13_2000076\Game\GameInfo.pak", "aes:308189028181009B606931DCF7027A4DC0E5263B4AD0D8F4A492A16E4B5EC0850F074B4C3DA627FF96676D2379F89062DE6C917F268CBD822404D26D9D79BCB0182D4C96EEAF2B918A0300BFB81619622D1556B4E02D16FE0C7ED72C01EE429C4C849C6A786BCEC44D6C50CB914648BB662D0BA235680002D4605058D1C30DA11237822A01F2EF0203010001", "paklist.txt")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Hunt Showdown\game_hunt\gamedata.pak", "aes:30818902818100affd71ca741c1aa5895becf596e8732d290453d275cf6ff0bb214324ebab7eedd7f39deebc2708d88b6d536a58da5683137fafec478e41e6f8b0882e5eba236b9d2a150ee513ae562ce56b6aaf982c27a8c317281afa0f84f546ecb825ccf2217519c84ed0ceab179ee5ccdab0cb40a95d5442120f25a61e7da79d30c7d7d8a70203010001", "difficulty/delta.cfg")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\SNOW\Assets\GameData.pak", "aes:30818902818100D51E1D3810C4A112B2F2504B83E2F124009C0AC9CD1661913421D4E94623AD7014599DAFB0DC9F8366D164AD072B3DC5AA3D4CD24542D5F684E6A4F7473102DE2ACA11F6524015ECBD564248FC712B3A69B15B78EFAA06748259DDE77A75757E513F7AC21A0151F53C78FF45ABCC45C3F54BC6305F420981F7119AF03E6438D70203010001", "Libs/ActionGraphs/mm_controls_inair.xml")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Ryse Son of Rome\GameRyse\GameData.pak", null, "Difficulty/easy.cfg")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Robinson The Journey\game_robinson\gamedata.pak", null, "libs/ai/AIInterestTypes.xml")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Crysis Remastered\Game\gamedata.pak", null, "entities/AdvancedDoor.ent")]
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
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Wolcen\Game\Textures_decals.pak", null, "Textures/decals/blood/blood_beach_diluted.dds")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Warface\13_2000076\Game\GameInfo.pak", "aes:308189028181009B606931DCF7027A4DC0E5263B4AD0D8F4A492A16E4B5EC0850F074B4C3DA627FF96676D2379F89062DE6C917F268CBD822404D26D9D79BCB0182D4C96EEAF2B918A0300BFB81619622D1556B4E02D16FE0C7ED72C01EE429C4C849C6A786BCEC44D6C50CB914648BB662D0BA235680002D4605058D1C30DA11237822A01F2EF0203010001", "paklist.txt")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Hunt Showdown\game_hunt\gamedata.pak", "aes:30818902818100affd71ca741c1aa5895becf596e8732d290453d275cf6ff0bb214324ebab7eedd7f39deebc2708d88b6d536a58da5683137fafec478e41e6f8b0882e5eba236b9d2a150ee513ae562ce56b6aaf982c27a8c317281afa0f84f546ecb825ccf2217519c84ed0ceab179ee5ccdab0cb40a95d5442120f25a61e7da79d30c7d7d8a70203010001", "difficulty/delta.cfg")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\SNOW\Assets\GameData.pak", "aes:30818902818100D51E1D3810C4A112B2F2504B83E2F124009C0AC9CD1661913421D4E94623AD7014599DAFB0DC9F8366D164AD072B3DC5AA3D4CD24542D5F684E6A4F7473102DE2ACA11F6524015ECBD564248FC712B3A69B15B78EFAA06748259DDE77A75757E513F7AC21A0151F53C78FF45ABCC45C3F54BC6305F420981F7119AF03E6438D70203010001", "Libs/ActionGraphs/mm_controls_inair.xml")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Ryse Son of Rome\GameRyse\GameData.pak", null, "Difficulty/easy.cfg")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Robinson The Journey\game_robinson\gamedata.pak", null, "libs/ai/AIInterestTypes.xml")]
        [TestCase(@"D:\Program Files (x86)\Steam\steamapps\common\Crysis Remastered\Game\gamedata.pak", null, "entities/AdvancedDoor.ent")]
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