using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace ICSharpCode.SharpZipLib.Zip
{
	/// <summary>
	/// Cry3Constants
	/// </summary>
	internal static class Cry3Constants
	{
		#region base

		static readonly Type ZipFormatType = typeof(ZipEntry).Assembly.GetType("ICSharpCode.SharpZipLib.Zip.ZipFormat");
		static readonly Type ZipHelperStreamType = typeof(ZipEntry).Assembly.GetType("ICSharpCode.SharpZipLib.Zip.ZipHelperStream");
		static readonly MethodInfo ZipFormat_ReadDataDescriptorMethod =
			ZipFormatType?.GetMethod("ReadDataDescriptor", BindingFlags.NonPublic | BindingFlags.Static) ??
			ZipHelperStreamType?.GetMethod("ReadDataDescriptor", BindingFlags.Public | BindingFlags.Instance);
		static readonly MethodInfo TestStatus_AddErrorMethod = typeof(TestStatus).GetMethod("AddError", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly MethodInfo TestStatus_SetOperationMethod = typeof(TestStatus).GetMethod("SetOperation", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly MethodInfo TestStatus_SetEntryMethod = typeof(TestStatus).GetMethod("SetEntry", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly MethodInfo TestStatus_SetBytesTestedMethod = typeof(TestStatus).GetMethod("SetBytesTested", BindingFlags.NonPublic | BindingFlags.Instance);

		internal static void ZipFormat_ReadDataDescriptor(Stream stream, bool zip64, DescriptorData data)
		{
			if (ZipFormatType != null) ZipFormat_ReadDataDescriptorMethod.Invoke(null, new object[] { stream, zip64, data });
			else if (ZipHelperStreamType != null) ZipFormat_ReadDataDescriptorMethod.Invoke(Activator.CreateInstance(ZipHelperStreamType, stream), new object[] { zip64, data });
			else throw new InvalidOperationException();
		}

		// Extensions: TestStatus
		public static void AddError(this TestStatus source) => TestStatus_AddErrorMethod.Invoke(source, null);
		public static void SetOperation(this TestStatus source, TestOperation operation) => TestStatus_SetOperationMethod.Invoke(source, new object[] { operation });
		public static void SetEntry(this TestStatus source, ZipEntry entry) => TestStatus_SetEntryMethod.Invoke(source, new object[] { entry });
		public static void SetBytesTested(this TestStatus source, long value) => TestStatus_SetBytesTestedMethod.Invoke(source, new object[] { value });

		#endregion

		// Extensions: Encoding
		public static bool IsZipUnicode(this Encoding e) => e.Equals(StringCodec.UnicodeZipEncoding);
	}
}
