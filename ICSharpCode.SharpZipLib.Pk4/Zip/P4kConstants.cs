using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace ICSharpCode.SharpZipLib.Zip
{
	/// <summary>
	/// P4kConstants
	/// </summary>
	internal static class P4kConstants
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

		/// <summary>
		/// Signature for local entry header
		/// </summary>
		public const int LocalHeaderSignatureEncrypted = 'P' | ('K' << 8) | (3 << 16) | (20 << 24);

		/// <summary>
		/// ZStd compression.
		/// </summary>
		public const CompressionMethod CompressionMethod_ZStd = (CompressionMethod)100;

		//0xFD2FB528 LE
		public static bool IsZstdStream(byte[] bytes, long length) => bytes.Length > 3 && bytes[0] == 0x28 && bytes[1] == 0xB5 && bytes[2] == 0x2F && bytes[3] == 0xFD;

		// Extensions: Encoding
		public static bool IsZipUnicode(this Encoding e) => e.Equals(StringCodec.UnicodeZipEncoding);
	}
}
