using System;
using System.IO;
using System.Reflection;
using static ICSharpCode.SharpZipLib.Zip.P4kConstants;

namespace ICSharpCode.SharpZipLib.Zip
{
	/// <summary>
	/// P4kInputStream
	/// </summary>
	/// <seealso cref="ZipInputStream" />
	public class P4kInputStream : ZipInputStream
	{
		#region base

		static readonly FieldInfo methodField = typeof(ZipInputStream).GetField("method", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly MethodInfo CompleteCloseEntryMethod = typeof(ZipInputStream).GetMethod("CompleteCloseEntry", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly MethodInfo CloseEntryMethod = typeof(ZipInputStream).GetMethod("CloseEntry", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly MethodInfo InitialReadMethod = typeof(ZipInputStream).GetMethod("InitialRead", BindingFlags.NonPublic | BindingFlags.Instance);
		static readonly MethodInfo BodyReadMethod = typeof(ZipInputStream).GetMethod("BodyRead", BindingFlags.NonPublic | BindingFlags.Instance);

		int method
		{
			get => (int)methodField.GetValue(this);
			set => methodField.SetValue(this, value);
		}

		void baseCompleteCloseEntry(bool testCrc) => CompleteCloseEntryMethod.Invoke(this, new object[] { testCrc });
		void baseCloseEntry() => CloseEntryMethod.Invoke(this, null);
		int baseInitialRead(byte[] destination, int offset, int count) => (int)InitialReadMethod.Invoke(this, new object[] { destination, offset, count });
		int baseBodyRead(byte[] buffer, int offset, int count) => (int)BodyReadMethod.Invoke(this, new object[] { buffer, offset, count });

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new Zip input stream, for reading a zip archive.
		/// </summary>
		/// <param name="baseInputStream">The underlying <see cref="Stream"/> providing data.</param>
		public P4kInputStream(Stream baseInputStream)
			: base(baseInputStream) { }

		/// <summary>
		/// Creates a new Zip input stream, for reading a zip archive.
		/// </summary>
		/// <param name="baseInputStream">The underlying <see cref="Stream"/> providing data.</param>
		/// <param name="bufferSize">Size of the buffer.</param>
		public P4kInputStream(Stream baseInputStream, int bufferSize)
			: base(baseInputStream, bufferSize) { }

		#endregion Constructors

		/// <summary>
		/// Complete cleanup as the final part of closing.
		/// </summary>
		/// <param name="testCrc">True if the crc value should be tested</param>
		private void CompleteCloseEntry(bool testCrc)
		{
			baseCompleteCloseEntry(testCrc);
			if (method == (int)CompressionMethod_ZStd)
			{
				throw new NotImplementedException("ZStd not implemented");
			}
		}

		/// <summary>
		/// Closes the current zip entry and moves to the next one.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The stream is closed
		/// </exception>
		/// <exception cref="ZipException">
		/// The Zip stream ends early
		/// </exception>
		public new void CloseEntry()
		{
			baseCloseEntry();
			if (method == (int)CompressionMethod_ZStd)
			{
				throw new NotImplementedException("ZStd not supported");
			}
		}

		/// <summary>
		/// Perform the initial read on an entry which may include 
		/// reading encryption headers and setting up inflation.
		/// </summary>
		/// <param name="destination">The destination to fill with data read.</param>
		/// <param name="offset">The offset to start reading at.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <returns>The actual number of bytes read.</returns>
		private int InitialRead(byte[] destination, int offset, int count)
		{
			var result = baseInitialRead(destination, offset, count);
			if (method == (int)CompressionMethod_ZStd)
			{
				throw new NotImplementedException("ZStd not implemented");
			}
			return result;
		}

		/// <summary>
		/// Reads a block of bytes from the current zip entry.
		/// </summary>
		/// <returns>
		/// The number of bytes read (this may be less than the length requested, even before the end of stream), or 0 on end of stream.
		/// </returns>
		/// <exception name="IOException">
		/// An i/o error occured.
		/// </exception>
		/// <exception cref="ZipException">
		/// The deflated stream is corrupted.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The stream is not open.
		/// </exception>
		private int BodyRead(byte[] buffer, int offset, int count)
		{
			var result = baseBodyRead(buffer, offset, count);
			if (method == (int)CompressionMethod_ZStd)
			{
				throw new NotImplementedException("ZStd not implemented");
			}
			return result;
		}
	}
}
