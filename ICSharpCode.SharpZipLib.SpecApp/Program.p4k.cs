//using ICSharpCode.SharpZipLib.Zip;

//using var pakStream = File.Open(@"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k", FileMode.Open, FileAccess.Read, FileShare.Read);
//var pak = new P4kFile(pakStream);
////foreach (ZipEntry ent in pak) Console.WriteLine(ent.Name);
//var entry = pak.FindEntry(@"Data\dedicated.cfg", true);
//if (entry == -1) throw new FileNotFoundException();
//using var input = pak.GetInputStream(entry);
//if (!input.CanRead) throw new Exception();
//var body = new StreamReader(input).ReadToEnd();
//Console.WriteLine(body);
