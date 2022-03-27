using System;
using System.IO;
using System.Reflection;
using static ICSharpCode.SharpZipLib.Zip.P4kConstants;

namespace ICSharpCode.SharpZipLib.Zip
{
	/// <summary>
	/// P4kOutputStream
	/// </summary>
	/// <seealso cref="ZipOutputStream" />
	public class P4kOutputStream : ZipOutputStream
	{
		#region base

		static readonly FieldInfo curMethodField = typeof(ZipOutputStream).GetField("curMethod", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly MethodInfo PutNextEntryMethod = typeof(ZipOutputStream).GetMethod("PutNextEntry", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly MethodInfo CloseEntryMethod = typeof(ZipOutputStream).GetMethod("CloseEntry", BindingFlags.NonPublic | BindingFlags.Instance);

		CompressionMethod curMethod
		{
			get => (CompressionMethod)curMethodField.GetValue(this);
			set => curMethodField.SetValue(this, value);
		}

		void basePutNextEntry(ZipEntry entry) => PutNextEntryMethod.Invoke(this, new object[] { entry });
		void baseCloseEntry() => CloseEntryMethod.Invoke(this, null);

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new Zip output stream, writing a zip archive.
		/// </summary>
		/// <param name="baseOutputStream">
		/// The output stream to which the archive contents are written.
		/// </param>
		public P4kOutputStream(Stream baseOutputStream)
			: base(baseOutputStream) { }

		/// <summary>
		/// Creates a new Zip output stream, writing a zip archive.
		/// </summary>
		/// <param name="baseOutputStream">The output stream to which the archive contents are written.</param>
		/// <param name="bufferSize">Size of the buffer to use.</param>
		public P4kOutputStream(Stream baseOutputStream, int bufferSize)
			: base(baseOutputStream, bufferSize) { }

		#endregion Constructors

		/// <summary>
		/// Starts a new Zip entry. It automatically closes the previous
		/// entry if present.
		/// All entry elements bar name are optional, but must be correct if present.
		/// If the compression method is stored and the output is not patchable
		/// the compression for that entry is automatically changed to deflate level 0
		/// </summary>
		/// <param name="entry">
		/// the entry.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// if entry passed is null.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// if an I/O error occurred.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// if stream was finished
		/// </exception>
		/// <exception cref="ZipException">
		/// Too many entries in the Zip file<br/>
		/// Entry name is too long<br/>
		/// Finish has already been called<br/>
		/// </exception>
		/// <exception cref="System.NotImplementedException">
		/// The Compression method specified for the entry is unsupported.
		/// </exception>
		public new void PutNextEntry(ZipEntry entry)
		{
			basePutNextEntry(entry);

			if (curMethod == CompressionMethod_ZStd)
			{
				throw new NotImplementedException("ZStd not implemented");
			}
		}

		/// <summary>
		/// Closes the current entry, updating header and footer information as required
		/// </summary>
		/// <exception cref="ZipException">
		/// Invalid entry field values.
		/// </exception>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// No entry is active.
		/// </exception>
		public new void CloseEntry()
		{
			baseCloseEntry();

			if (curMethod == CompressionMethod_ZStd)
			{
				throw new NotImplementedException("ZStd not implemented");
			}
		}

		/// <summary>
		/// Writes the given buffer to the current entry.
		/// </summary>
		/// <param name="buffer">The buffer containing data to write.</param>
		/// <param name="offset">The offset of the first byte to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		/// <exception cref="ZipException">Archive size is invalid</exception>
		/// <exception cref="System.InvalidOperationException">No entry is active.</exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
			base.Write(buffer, offset, count);

			if (curMethod == CompressionMethod_ZStd)
			{
				throw new NotImplementedException("ZStd not implemented");
			}
		}
	}
}
