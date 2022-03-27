using System;
using System.Reflection;
using static ICSharpCode.SharpZipLib.Zip.P4kConstants;

namespace ICSharpCode.SharpZipLib.Zip
{
	/// <summary>
	/// P4kEntry
	/// </summary>
	/// <seealso cref="ZipEntry" />
	public class P4kEntry : ZipEntry
	{
		#region base

		static readonly FieldInfo nameField = typeof(ZipEntry).GetField("name", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly FieldInfo versionMadeByField = typeof(ZipEntry).GetField("versionMadeBy", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly FieldInfo versionToExtractField = typeof(ZipEntry).GetField("versionToExtract", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly FieldInfo methodField = typeof(ZipEntry).GetField("method", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly PropertyInfo CompressionMethodForHeaderProperty = typeof(ZipEntry).GetProperty("CompressionMethodForHeader", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly PropertyInfo EncryptionOverheadSizeProperty = typeof(ZipEntry).GetProperty("EncryptionOverheadSize", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly PropertyInfo CryptoCheckValueProperty = typeof(ZipEntry).GetProperty("CryptoCheckValue", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly MethodInfo ProcessExtraDataMethod = typeof(ZipEntry).GetMethod("ProcessExtraData", BindingFlags.NonPublic | BindingFlags.Instance);

		string name
		{
			get => (string)nameField.GetValue(this);
			set => nameField.SetValue(this, value);
		}
		ushort versionMadeBy
		{
			get => (ushort)versionMadeByField.GetValue(this);
			set => versionMadeByField.SetValue(this, value);
		}
		ushort versionToExtract
		{
			get => (ushort)versionToExtractField.GetValue(this);
			set => versionToExtractField.SetValue(this, value);
		}
		CompressionMethod method
		{
			get => (CompressionMethod)methodField.GetValue(this);
			set => methodField.SetValue(this, value);
		}
		internal CompressionMethod CompressionMethodForHeader => (CompressionMethod)CompressionMethodForHeaderProperty.GetValue(this);
		internal int EncryptionOverheadSize => (int)EncryptionOverheadSizeProperty.GetValue(this);
		internal byte CryptoCheckValue
		{
			get => (byte)CryptoCheckValueProperty.GetValue(this);
			set => CryptoCheckValueProperty.SetValue(this, value);
		}
		internal void ProcessExtraData(bool localHeader)
			=> ProcessExtraDataMethod.Invoke(this, new object[] { localHeader });

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a zip entry with the given name.
		/// </summary>
		/// <param name="name">
		/// The name for this entry. Can include directory components.
		/// The convention for names is 'unix' style paths with relative names only.
		/// There are with no device names and path elements are separated by '/' characters.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// The name passed is null
		/// </exception>
		public P4kEntry(string name)
			: this(name, 0, ZipConstants.VersionMadeBy, CompressionMethod.Deflated, true) { }

		/// <summary>
		/// Creates a zip entry with the given name and version required to extract
		/// </summary>
		/// <param name="name">
		/// The name for this entry. Can include directory components.
		/// The convention for names is 'unix'  style paths with no device names and
		/// path elements separated by '/' characters.  This is not enforced see <see cref="CleanName(string)">CleanName</see>
		/// on how to ensure names are valid if this is desired.
		/// </param>
		/// <param name="versionRequiredToExtract">
		/// The minimum 'feature version' required this entry
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// The name passed is null
		/// </exception>
		internal P4kEntry(string name, int versionRequiredToExtract)
			: this(name, versionRequiredToExtract, ZipConstants.VersionMadeBy, CompressionMethod.Deflated, true) { }

		/// <summary>
		/// Initializes an entry with the given name and made by information
		/// </summary>
		/// <param name="name">Name for this entry</param>
		/// <param name="madeByInfo">Version and HostSystem Information</param>
		/// <param name="versionRequiredToExtract">Minimum required zip feature version required to extract this entry</param>
		/// <param name="method">Compression method for this entry.</param>
		/// <param name="unicode">Whether the entry uses unicode for name and comment</param>
		/// <exception cref="ArgumentNullException">
		/// The name passed is null
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// versionRequiredToExtract should be 0 (auto-calculate) or > 10
		/// </exception>
		/// <remarks>
		/// This constructor is used by the ZipFile class when reading from the central header
		/// It is not generally useful, use the constructor specifying the name only.
		/// </remarks>
		internal P4kEntry(string name, int versionRequiredToExtract, int madeByInfo, CompressionMethod method, bool unicode)
			: base(name)
		{
			this.name = name;
			this.versionMadeBy = (ushort)madeByInfo;
			this.versionToExtract = (ushort)versionRequiredToExtract;
			this.method = method;

			IsUnicodeText = unicode;
		}

		/// <summary>
		/// Creates a deep copy of the given zip entry.
		/// </summary>
		/// <param name="entry">
		/// The entry to copy.
		/// </param>
		[Obsolete("Use Clone instead")]
		public P4kEntry(ZipEntry entry)
			: base(entry) { }

		#endregion Constructors

		/// <summary>
		/// Determines whether [is aes crypted].
		/// </summary>
		/// <returns>
		///   <c>true</c> if [is aes crypted] [the specified source]; otherwise, <c>false</c>.
		/// </returns>
		public bool IsAesCrypted
			=> this.ExtraData.Length >= 168 && this.ExtraData[168] > 0x00;

		/// <summary>
		/// Get a value indicating whether this entry can be decompressed by the library.
		/// </summary>
		/// <remarks>This is based on the <see cref="ZipEntry.Version"></see> and
		/// whether the <see cref="P4kEntry.IsCompressionMethodSupported()">compression method</see> is supported.</remarks>
		public new bool CanDecompress
			=> Version <= ZipConstants.VersionMadeBy
			&& (Version == 10 || Version == 11 || Version == 20 || Version == 45 || Version == 46 || Version == 51)
			// TODO: Add support for ZStd
			&& IsCompressionMethodSupported();

		/// <summary>
		/// Test entry to see if data can be extracted.
		/// </summary>
		/// <returns>Returns true if data can be extracted for this entry; false otherwise.</returns>
		public new bool IsCompressionMethodSupported()
			=> IsCompressionMethodSupported(CompressionMethod);

		/// <summary>
		/// Test a <see cref="CompressionMethod">compression method</see> to see if this library
		/// supports extracting data compressed with that method
		/// </summary>
		/// <param name="method">The compression method to test.</param>
		/// <returns>Returns true if the compression method is supported; false otherwise</returns>
		public new static bool IsCompressionMethodSupported(CompressionMethod method)
			=> method == CompressionMethod.Deflated
			|| method == CompressionMethod.Stored
			|| method == CompressionMethod.BZip2
			|| method == CompressionMethod_ZStd;
	}
}
