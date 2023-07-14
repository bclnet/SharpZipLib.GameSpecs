#if true
using ICSharpCode.SharpZipLib.Zip;

const string PakPath = @"D:\Roberts Space Industries\StarCitizen\LIVE\Data.p4k";
const string AesKey = "aes:5E7A2002302EEB1A3BB617C30FDE1E47";

using var pakStream = File.Open(PakPath, FileMode.Open, FileAccess.Read, FileShare.Read);
var pak = new P4kFile(pakStream, Utils.ParseKey(AesKey));
//foreach (ZipEntry ent in pak) Console.WriteLine(ent.Name);

var entry = pak.FindEntry(@"Data\dedicated.cfg", true);
//var entry = pak.FindEntry(@"Data\Scripts\Entities\Vehicles\Implementations\Xml\DRAK_Cutlass.xml", true);
if (entry == -1) throw new FileNotFoundException();

using var input = pak.GetInputStream(entry);
if (!input.CanRead) throw new Exception();
var body = new StreamReader(input).ReadToEnd();
Console.WriteLine(body);
#endif