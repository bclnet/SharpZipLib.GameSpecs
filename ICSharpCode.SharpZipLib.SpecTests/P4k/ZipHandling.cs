using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.Tests.P4k
{
    public class ZipHandling
    {
        [Test]
        [Category("P4k")]
        //[TestCase(@"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", "aes:5E7A2002302EEB1A3BB617C30FDE1E47", @"Data\dedicated.cfg")]
        [TestCase(@"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", "aes:5E7A2002302EEB1A3BB617C30FDE1E47", @"Data\Scripts\Entities\Vehicles\Implementations\Xml\DRAK_Cutlass.xml")]
        public void OpenFile(string path, string aesKey, string entryPath)
        {
            using var pakStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var pak = new P4kFile(pakStream, Utils.ParseKey(aesKey));

            // test find entry
            var entry = pak.FindEntry(entryPath, true);
            if (entry == -1) throw new FileNotFoundException();

            // test walk names
            foreach (ZipEntry ent in pak) Assert.IsTrue(!string.IsNullOrEmpty(ent.Name));
        }

        [Test]
        [Category("P4k")]
        //[TestCase(@"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", "aes:5E7A2002302EEB1A3BB617C30FDE1E47", @"Data\dedicated.cfg")]
        [TestCase(@"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", "aes:5E7A2002302EEB1A3BB617C30FDE1E47", @"Data\Scripts\Entities\Vehicles\Implementations\Xml\DRAK_Cutlass.xml")]
        public void OpenFileEntry(string path, string aesKey, string entryPath)
        {
            using var pakStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var pak = new P4kFile(pakStream, Utils.ParseKey(aesKey));

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