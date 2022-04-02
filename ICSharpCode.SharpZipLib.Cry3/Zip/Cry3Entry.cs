using System;
using System.Reflection;

namespace ICSharpCode.SharpZipLib.Zip
{
	/// <summary>
	/// Cry3Entry
	/// </summary>
	/// <seealso cref="ZipEntry" />
	public class Cry3Entry : ZipEntry
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
		internal void ProcessExtraData(bool localHeader) => ProcessExtraDataMethod.Invoke(this, new object[] { localHeader });

		#endregion

		internal long OffsetAfterNameLen;

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
		public Cry3Entry(string name)
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
		internal Cry3Entry(string name, int versionRequiredToExtract)
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
		internal Cry3Entry(string name, int versionRequiredToExtract, int madeByInfo, CompressionMethod method, bool unicode)
			: base(name)
		{
			this.name = name;
			this.versionMadeBy = (ushort)madeByInfo;
			this.versionToExtract = (ushort)versionRequiredToExtract;
			this.method = method;

			IsUnicodeText = unicode;
		}
	}
}
