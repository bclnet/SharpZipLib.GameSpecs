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
        [TestCase(@"Data\dedicated.cfg")]
        public void OpenFile(string entryPath)
        {
            using var pakStream = File.Open(@"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", FileMode.Open, FileAccess.Read, FileShare.Read);
            var pak = new P4kFile(pakStream);

            // test find entry
            var entry = pak.FindEntry(entryPath, true);
            if (entry == -1) throw new FileNotFoundException();

            // test walk names
            foreach (ZipEntry ent in pak) Assert.IsTrue(!string.IsNullOrEmpty(ent.Name));
        }

        [Test]
        [Category("P4k")]
        [TestCase(@"Data\dedicated.cfg")]
        public void OpenFileEntry(string entryPath)
        {
            using var pakStream = File.Open(@"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", FileMode.Open, FileAccess.Read, FileShare.Read);
            var pak = new P4kFile(pakStream);

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