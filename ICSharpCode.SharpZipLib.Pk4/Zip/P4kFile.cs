using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using static ICSharpCode.SharpZipLib.Zip.P4kConstants;

namespace ICSharpCode.SharpZipLib.Zip
{
    /// <summary>
    /// P4kFile
    /// </summary>
    /// <seealso cref="ICSharpCode.SharpZipLib.Zip.ZipFile" />
    public class P4kFile : ZipFile
    {
        static readonly StringCodec CompatCodec = new StringCodec();
        //StringCodec _stringCodecLocal = CompatCodec;
        internal Stream _baseStream;
        ZipEntry[] _entries;
        StringCodec __stringCodec = CompatCodec;
        long _offsetOfFirstEntry;

        #region base

        static readonly FieldInfo isDisposed_Field = typeof(ZipFile).GetField("isDisposed_", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo name_Field = typeof(ZipFile).GetField("name_", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo comment_Field = typeof(ZipFile).GetField("comment_", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo baseStream_Field = typeof(ZipFile).GetField("baseStream_", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo isStreamOwnerField = typeof(ZipFile).GetField("isStreamOwner", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo offsetOfFirstEntryField = typeof(ZipFile).GetField("offsetOfFirstEntry", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo entries_Field = typeof(ZipFile).GetField("entries_", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo keyField = typeof(ZipFile).GetField("key", BindingFlags.NonPublic | BindingFlags.Instance);
        //static readonly FieldInfo _stringCodecField = typeof(ZipFile).GetField("_stringCodec", BindingFlags.NonPublic | BindingFlags.Instance) ??
        //    typeof(P4kFile).GetField("_stringCodecLocal", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo isNewArchive_Field = typeof(ZipFile).GetField("isNewArchive_", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo DisposeInternalMethod = typeof(ZipFile).GetMethod("DisposeInternal", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo CreateAndInitDecryptionStreamMethod = typeof(ZipFile).GetMethod("CreateAndInitDecryptionStream", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly MethodInfo LocateBlockWithSignatureMethod = typeof(ZipFile).GetMethod("LocateBlockWithSignature", BindingFlags.NonPublic | BindingFlags.Instance);

        bool isDisposed_ => (bool)isDisposed_Field.GetValue(this);
        string name_
        {
            get => (string)name_Field.GetValue(this);
            set => name_Field.SetValue(this, value);
        }
        string comment_
        {
            get => (string)comment_Field.GetValue(this);
            set => comment_Field.SetValue(this, value);
        }
        Stream baseStream_
        {
            get => (Stream)baseStream_Field.GetValue(this);
            set => baseStream_Field.SetValue(this, value);
        }
        bool isStreamOwner
        {
            get => (bool)isStreamOwnerField.GetValue(this);
            set => isStreamOwnerField.SetValue(this, value);
        }
        long offsetOfFirstEntry
        {
            get => (long)offsetOfFirstEntryField.GetValue(this);
            set => offsetOfFirstEntryField.SetValue(this, value);
        }
        ZipEntry[] entries_
        {
            get => (ZipEntry[])entries_Field.GetValue(this);
            set => entries_Field.SetValue(this, value);
        }
        byte[] key
        {
            get => (byte[])keyField.GetValue(this);
            set => keyField.SetValue(this, value);
        }
        //StringCodec _stringCodec
        //{
        //    get => (StringCodec)_stringCodecField.GetValue(this);
        //    set => _stringCodecField.SetValue(this, value);
        //}
        bool isNewArchive_
        {
            get => (bool)isNewArchive_Field.GetValue(this);
            set => isNewArchive_Field.SetValue(this, value);
        }

        void DisposeInternal(bool disposing) => DisposeInternalMethod.Invoke(this, new object[] { disposing });
        Stream CreateAndInitDecryptionStream(Stream baseStream, ZipEntry entry) => (Stream)CreateAndInitDecryptionStreamMethod.Invoke(this, new object[] { baseStream, entry });
        long LocateBlockWithSignature(int signature, long endLocation, int minimumBlockSize, int maximumVariableData) => (long)LocateBlockWithSignatureMethod.Invoke(this, new object[] { signature, endLocation, minimumBlockSize, maximumVariableData });

        #endregion

        #region Constructors

        static Stream EmptyStreamHack => new MemoryStream();

        /// <summary>
        /// Opens a Zip file with the given name for reading.
        /// </summary>
        /// <param name="name">The name of the file to open.</param>
        /// <param name="aesKey">The <see cref="byte[]"/> to use as the key.</param>
        /// <param name="stringCodec"></param>
        /// <exception cref="ArgumentNullException">The argument supplied is null.</exception>
        /// <exception cref="IOException">
        /// An i/o error occurs
        /// </exception>
        /// <exception cref="ZipException">
        /// The file doesn't contain a valid zip archive.
        /// </exception>
        internal P4kFile(string name, byte[] aesKey, StringCodec stringCodec = null)
            : base(EmptyStreamHack, false)
        {
            isNewArchive_ = false;
            name_ = name ?? throw new ArgumentNullException(nameof(name));
            key = aesKey;

            EntryFactory = new P4kEntryFactory();
            _baseStream = baseStream_ = File.Open(name, FileMode.Open, FileAccess.Read, FileShare.Read);
            //__stringCodec = _stringCodec;
            //if (stringCodec != null) __stringCodec = _stringCodec = stringCodec;
            isStreamOwner = true;

            try
            {
                ReadEntries();
            }
            catch { DisposeInternal(true); throw; }
        }

        /// <summary>
        /// Opens a Zip file reading the given <see cref="FileStream"/>.
        /// </summary>
        /// <param name="file">The <see cref="FileStream"/> to read archive data from.</param>
        /// <param name="aesKey">The <see cref="byte[]"/> to use as the key.</param>
        /// <exception cref="ArgumentNullException">The supplied argument is null.</exception>
        /// <exception cref="IOException">
        /// An i/o error occurs.
        /// </exception>
        /// <exception cref="ZipException">
        /// The file doesn't contain a valid zip archive.
        /// </exception>
        public P4kFile(FileStream file, byte[] aesKey)
            : this(file, aesKey, false) { }

        /// <summary>
        /// Opens a Zip file reading the given <see cref="FileStream"/>.
        /// </summary>
        /// <param name="file">The <see cref="FileStream"/> to read archive data from.</param>
        /// <param name="aesKey">The <see cref="byte[]"/> to use as the key.</param>
        /// <param name="leaveOpen">true to leave the <see cref="FileStream">file</see> open when the ZipFile is disposed, false to dispose of it</param>
        /// <exception cref="ArgumentNullException">The supplied argument is null.</exception>
        /// <exception cref="IOException">
        /// An i/o error occurs.
        /// </exception>
        /// <exception cref="ZipException">
        /// The file doesn't contain a valid zip archive.
        /// </exception>
        public P4kFile(FileStream file, byte[] aesKey, bool leaveOpen)
            : base(EmptyStreamHack, false)
        {
            isNewArchive_ = false;
            if (file == null) throw new ArgumentNullException(nameof(file));
            key = aesKey;
            if (!file.CanSeek) throw new ArgumentException("Stream is not seekable", nameof(file));

            EntryFactory = new P4kEntryFactory();
            _baseStream = baseStream_ = file;
            //__stringCodec = _stringCodec;
            name_ = file.Name;
            isStreamOwner = !leaveOpen;

            try
            {
                ReadEntries();
            }
            catch { DisposeInternal(true); throw; }
        }

        /// <summary>
        /// Opens a Zip file reading the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read archive data from.</param>
        /// <param name="aesKey">The <see cref="byte[]"/> to use as the key.</param>
        /// <exception cref="IOException">
        /// An i/o error occurs
        /// </exception>
        /// <exception cref="ZipException">
        /// The stream doesn't contain a valid zip archive.<br/>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Stream">stream</see> doesnt support seeking.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <see cref="Stream">stream</see> argument is null.
        /// </exception>
        public P4kFile(Stream stream, byte[] aesKey)
            : this(stream, aesKey, false) { }

        /// <summary>
        /// Opens a Zip file reading the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read archive data from.</param>
        /// <param name="aesKey">The <see cref="byte[]"/> to use as the key.</param>
        /// <param name="leaveOpen">true to leave the <see cref="Stream">stream</see> open when the ZipFile is disposed, false to dispose of it</param>
        /// <exception cref="IOException">
        /// An i/o error occurs
        /// </exception>
        /// <exception cref="ZipException">
        /// The stream doesn't contain a valid zip archive.<br/>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The <see cref="Stream">stream</see> doesnt support seeking.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The <see cref="Stream">stream</see> argument is null.
        /// </exception>
        public P4kFile(Stream stream, byte[] aesKey, bool leaveOpen)
            : base(EmptyStreamHack, false)
        {
            isNewArchive_ = false;
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            key = aesKey;
            if (!stream.CanSeek) throw new ArgumentException("Stream is not seekable", nameof(stream));

            EntryFactory = new P4kEntryFactory();
            _baseStream = baseStream_ = stream;
            //__stringCodec = _stringCodec;
            isStreamOwner = !leaveOpen;

            if (_baseStream.Length > 0)
            {
                try
                {
                    ReadEntries();
                }
                catch { DisposeInternal(true); throw; }
            }
            else
            {
                _entries = entries_ = Array.Empty<ZipEntry>();
                isNewArchive_ = true;
            }
        }

        /// <summary>
        /// Initialises a default <see cref="ZipFile"/> instance with no entries and no file storage.
        /// </summary>
        internal P4kFile()
            : base(EmptyStreamHack, false) { }

        #endregion Constructors

        #region Creators

        /// <summary>
        /// Create a new <see cref="ZipFile"/> whose data will be stored in a file.
        /// </summary>
        /// <param name="fileName">The name of the archive to create.</param>
        /// <returns>Returns the newly created <see cref="ZipFile"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="fileName"></paramref> is null</exception>
        public new static ZipFile Create(string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            var fs = File.Create(fileName);
            return new P4kFile
            {
                name_ = fileName,
                baseStream_ = fs,
                isStreamOwner = true
            };
        }

        /// <summary>
        /// Create a new <see cref="ZipFile"/> whose data will be stored on a stream.
        /// </summary>
        /// <param name="outStream">The stream providing data storage.</param>
        /// <returns>Returns the newly created <see cref="ZipFile"/></returns>
        /// <exception cref="ArgumentNullException"><paramref name="outStream"> is null</paramref></exception>
        /// <exception cref="ArgumentException"><paramref name="outStream"> doesnt support writing.</paramref></exception>
        public new static ZipFile Create(Stream outStream)
        {
            if (outStream == null) throw new ArgumentNullException(nameof(outStream));
            if (!outStream.CanWrite) throw new ArgumentException("Stream is not writeable", nameof(outStream));
            if (!outStream.CanSeek) throw new ArgumentException("Stream is not seekable", nameof(outStream));

            var result = new P4kFile
            {
                baseStream_ = outStream
            };
            return result;
        }

        #endregion Creators

        #region KeyHandling

        /// <summary>
        /// Get/set the encryption key value.
        /// </summary>
        public byte[] Key
        {
            get => key;
            set => key = value;
        }

        #endregion KeyHandling

        #region Input Handling

        /// <summary>
        /// Gets an input stream for reading the given zip entry data in an uncompressed form.
        /// Normally the <see cref="ZipEntry"/> should be an entry returned by GetEntry().
        /// </summary>
        /// <param name="entry">The <see cref="ZipEntry"/> to obtain a data <see cref="Stream"/> for</param>
        /// <returns>An input <see cref="Stream"/> containing data for this <see cref="ZipEntry"/></returns>
        /// <exception cref="ObjectDisposedException">
        /// The ZipFile has already been closed
        /// </exception>
        /// <exception cref="ICSharpCode.SharpZipLib.Zip.ZipException">
        /// The compression method for the entry is unknown
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// The entry is not found in the ZipFile
        /// </exception>
        public new Stream GetInputStream(ZipEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            if (isDisposed_) throw new ObjectDisposedException("ZipFile");

            var index = entry.ZipFileIndex;
            if (index < 0 || index >= _entries.Length || _entries[index].Name != entry.Name)
            {
                index = FindEntry(entry.Name, true);
                if (index < 0) throw new ZipException("Entry cannot be found");
            }
            return GetInputStream(index);
        }

        /// <summary>
        /// Creates an input stream reading a zip entry
        /// </summary>
        /// <param name="entryIndex">The index of the entry to obtain an input stream for.</param>
        /// <returns>
        /// An input <see cref="Stream"/> containing data for this <paramref name="entryIndex"/>
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// The ZipFile has already been closed
        /// </exception>
        /// <exception cref="ICSharpCode.SharpZipLib.Zip.ZipException">
        /// The compression method for the entry is unknown
        /// </exception>
        /// <exception cref="IndexOutOfRangeException">
        /// The entry is not found in the ZipFile
        /// </exception>
        public new Stream GetInputStream(long entryIndex)
        {
            if (isDisposed_) throw new ObjectDisposedException("Pk4File");

            var start = LocateEntry(_entries[entryIndex]);
            var method = _entries[entryIndex].CompressionMethod;
            Stream result = new PartialInputStream(this, start, entries_[entryIndex].CompressedSize);

            if (_entries[entryIndex].IsCrypted == true)
            {
                result = CreateAndInitDecryptionStream(result, _entries[entryIndex]);
                if (result == null) throw new ZipException("Unable to decrypt this entry");
            }

            if (_entries[entryIndex] is P4kEntry entry && entry.IsAesCrypted == true)
            {
                result = CreateAndInitAesDecryptionStream(result, _entries[entryIndex]);
                if (result == null) throw new ZipException("Unable to decrypt this entry");
            }

            switch (method)
            {
                // read as is.
                case CompressionMethod.Stored: break;
                // No need to worry about ownership and closing as underlying stream close does nothing.
                case CompressionMethod.Deflated: result = new InflaterInputStream(result, new Inflater(true)); break;
                case CompressionMethod.BZip2: result = new BZip2.BZip2InputStream(result); break;
                case CompressionMethod_ZStd:
                    var buffBytes = new byte[4];

                    if (result.CanSeek && result.Read(buffBytes, 0, 4) > 0)
                    {
                        if (IsZstdStream(buffBytes, result.Length))
                        {
                            result.Seek(-4, SeekOrigin.Current);
                            result = new ZstdNet.DecompressionStream(result);
                        }
                        else result.Seek(-4, SeekOrigin.Current);
                    }
                    else result = new ZstdNet.DecompressionStream(result);
                    break;

                default: throw new ZipException("Unsupported compression method " + method);
            }

            return result;
        }

        #endregion Input Handling

        #region Archive Testing

        /// <summary>
        /// Test an archive for integrity/validity
        /// </summary>
        /// <param name="testData">Perform low level data Crc check</param>
        /// <returns>true if all tests pass, false otherwise</returns>
        /// <remarks>Testing will terminate on the first error found.</remarks>
        public new bool TestArchive(bool testData)
            => TestArchive(testData, TestStrategy.FindFirstError, null);

        /// <summary>
        /// Test an archive for integrity/validity
        /// </summary>
        /// <param name="testData">Perform low level data Crc check</param>
        /// <param name="strategy">The <see cref="TestStrategy"></see> to apply.</param>
        /// <param name="resultHandler">The <see cref="ZipTestResultHandler"></see> handler to call during testing.</param>
        /// <returns>true if all tests pass, false otherwise</returns>
        /// <exception cref="ObjectDisposedException">The object has already been closed.</exception>
        public new bool TestArchive(bool testData, TestStrategy strategy, ZipTestResultHandler resultHandler)
        {
            if (isDisposed_) throw new ObjectDisposedException("ZipFile");

            var status = new TestStatus(this);
            resultHandler?.Invoke(status, null);
            var test = testData ? (HeaderTest.Header | HeaderTest.Extract) : HeaderTest.Header;
            var testing = true;

            try
            {
                var entryIndex = 0;
                while (testing && (entryIndex < Count))
                {
                    if (resultHandler != null)
                    {
                        status.SetEntry(this[entryIndex]);
                        status.SetOperation(TestOperation.EntryHeader);
                        resultHandler(status, null);
                    }

                    try
                    {
                        TestLocalHeader((P4kEntry)this[entryIndex], test);
                    }
                    catch (ZipException ex)
                    {
                        status.AddError();

                        resultHandler?.Invoke(status, $"Exception during test - '{ex.Message}'");

                        testing &= strategy != TestStrategy.FindFirstError;
                    }

                    if (testing && testData && this[entryIndex].IsFile)
                    {
                        // Don't check CRC for AES encrypted archives
                        var checkCRC = this[entryIndex].AESKeySize == 0;

                        if (resultHandler != null) { status.SetOperation(TestOperation.EntryData); resultHandler(status, null); }

                        var crc = new Crc32();

                        using (Stream entryStream = this.GetInputStream(this[entryIndex]))
                        {
                            var buffer = new byte[4096];
                            var totalBytes = 0L;
                            int bytesRead;
                            while ((bytesRead = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                if (checkCRC) crc.Update(new ArraySegment<byte>(buffer, 0, bytesRead));
                                if (resultHandler != null) { totalBytes += bytesRead; status.SetBytesTested(totalBytes); resultHandler(status, null); }
                            }
                        }

                        if (checkCRC && this[entryIndex].Crc != crc.Value) { status.AddError(); resultHandler?.Invoke(status, "CRC mismatch"); testing &= strategy != TestStrategy.FindFirstError; }

                        if ((this[entryIndex].Flags & (int)GeneralBitFlags.Descriptor) != 0)
                        {
                            var data = new DescriptorData();
                            ZipFormat_ReadDataDescriptor(_baseStream, this[entryIndex].LocalHeaderRequiresZip64, data);
                            if (checkCRC && this[entryIndex].Crc != data.Crc) { status.AddError(); resultHandler?.Invoke(status, "Descriptor CRC mismatch"); }
                            if (this[entryIndex].CompressedSize != data.CompressedSize) { status.AddError(); resultHandler?.Invoke(status, "Descriptor compressed size mismatch"); }
                            if (this[entryIndex].Size != data.Size) { status.AddError(); resultHandler?.Invoke(status, "Descriptor size mismatch"); }
                        }
                    }

                    if (resultHandler != null) { status.SetOperation(TestOperation.EntryComplete); resultHandler(status, null); }

                    entryIndex += 1;
                }

                if (resultHandler != null)
                {
                    status.SetOperation(TestOperation.MiscellaneousTests);
                    resultHandler(status, null);
                }

                // TODO: the 'Corrina Johns' test where local headers are missing from
                // the central directory.  They are therefore invisible to many archivers.
            }
            catch (Exception ex) { status.AddError(); resultHandler?.Invoke(status, $"Exception during test - '{ex.Message}'"); }

            if (resultHandler != null) { status.SetOperation(TestOperation.Complete); status.SetEntry(null); resultHandler(status, null); }

            return (status.ErrorCount == 0);
        }

        [Flags]
        private enum HeaderTest
        {
            Extract = 0x01,     // Check that this header represents an entry whose data can be extracted
            Header = 0x02,     // Check that this header contents are valid
        }

        /// <summary>
        /// Test a local header against that provided from the central directory
        /// </summary>
        /// <param name="entry">
        /// The entry to test against
        /// </param>
        /// <param name="tests">The type of <see cref="HeaderTest">tests</see> to carry out.</param>
        /// <returns>The offset of the entries data in the file</returns>
        private long TestLocalHeader(P4kEntry entry, HeaderTest tests)
        {
            lock (_baseStream)
            {
                var testHeader = (tests & HeaderTest.Header) != 0;
                var testData = (tests & HeaderTest.Extract) != 0;

                var entryAbsOffset = _offsetOfFirstEntry + entry.Offset;

                _baseStream.Seek(entryAbsOffset, SeekOrigin.Begin);
                var signature = (int)ReadLEUint();

                if (signature != ZipConstants.LocalHeaderSignature && signature != P4kConstants.LocalHeaderSignatureEncrypted)
                    throw new ZipException($"Wrong local header signature at 0x{entryAbsOffset:x}, expected 0x{ZipConstants.LocalHeaderSignature:x8}, actual 0x{signature:x8}");

                var extractVersion = (short)(ReadLEUshort() & 0x00ff);
                var localFlags = (short)ReadLEUshort();
                var compressionMethod = (short)ReadLEUshort();
                var fileTime = (short)ReadLEUshort();
                var fileDate = (short)ReadLEUshort();
                var crcValue = ReadLEUint();
                long compressedSize = ReadLEUint();
                long size = ReadLEUint();
                int storedNameLength = ReadLEUshort();
                int extraDataLength = ReadLEUshort();

                var nameData = new byte[storedNameLength];
                StreamUtils.ReadFully(_baseStream, nameData);

                var extraData = new byte[extraDataLength];
                StreamUtils.ReadFully(_baseStream, extraData);

                var localExtraData = new ZipExtraData(extraData);

                // Extra data / zip64 checks
                if (localExtraData.Find(1))
                {
                    // 2010-03-04 Forum 10512: removed checks for version >= ZipConstants.VersionZip64
                    // and size or compressedSize = MaxValue, due to rogue creators.

                    size = localExtraData.ReadLong();
                    compressedSize = localExtraData.ReadLong();

                    if ((localFlags & (int)GeneralBitFlags.Descriptor) != 0)
                    {
                        // These may be valid if patched later
                        if (size != -1 && size != entry.Size) throw new ZipException("Size invalid for descriptor");
                        if (compressedSize != -1 && compressedSize != entry.CompressedSize) throw new ZipException("Compressed size invalid for descriptor");
                    }
                }
                else
                {
                    // No zip64 extra data but entry requires it.
                    if (extractVersion >= ZipConstants.VersionZip64 && ((uint)size == uint.MaxValue || (uint)compressedSize == uint.MaxValue)) throw new ZipException("Required Zip64 extended information missing");
                }

                if (testData)
                {
                    if (entry.IsFile)
                    {
                        if (!entry.IsCompressionMethodSupported()) throw new ZipException("Compression method not supported");

                        if (extractVersion > ZipConstants.VersionMadeBy || (extractVersion > 20 && extractVersion < ZipConstants.VersionZip64)) throw new ZipException($"Version required to extract this entry not supported ({extractVersion})");

                        if ((localFlags & (int)(GeneralBitFlags.Patched | GeneralBitFlags.StrongEncryption | GeneralBitFlags.EnhancedCompress | GeneralBitFlags.HeaderMasked)) != 0)
                            throw new ZipException("The library does not support the zip version required to extract this entry");
                    }
                }

                if (testHeader)
                {
                    if ((extractVersion <= 63) &&   // Ignore later versions as we dont know about them..
                        (extractVersion != 10) &&
                        (extractVersion != 11) &&
                        (extractVersion != 20) &&
                        (extractVersion != 21) &&
                        (extractVersion != 25) &&
                        (extractVersion != 27) &&
                        (extractVersion != 45) &&
                        (extractVersion != 46) &&
                        (extractVersion != 50) &&
                        (extractVersion != 51) &&
                        (extractVersion != 52) &&
                        (extractVersion != 61) &&
                        (extractVersion != 62) &&
                        (extractVersion != 63)
                        )
                        throw new ZipException($"Version required to extract this entry is invalid ({extractVersion})");

                    var localEncoding = __stringCodec.ZipInputEncoding(localFlags);

                    // Local entry flags dont have reserved bit set on.
                    if ((localFlags & (int)(GeneralBitFlags.ReservedPKware4 | GeneralBitFlags.ReservedPkware14 | GeneralBitFlags.ReservedPkware15)) != 0)
                        throw new ZipException("Reserved bit flags cannot be set.");

                    // Encryption requires extract version >= 20
                    if ((localFlags & (int)GeneralBitFlags.Encrypted) != 0 && extractVersion < 20)
                        throw new ZipException(string.Format("Version required to extract this entry is too low for encryption ({0})", extractVersion));

                    // Strong encryption requires encryption flag to be set and extract version >= 50.
                    if ((localFlags & (int)GeneralBitFlags.StrongEncryption) != 0)
                    {
                        if ((localFlags & (int)GeneralBitFlags.Encrypted) == 0) throw new ZipException("Strong encryption flag set but encryption flag is not set");
                        if (extractVersion < 50) throw new ZipException(string.Format("Version required to extract this entry is too low for encryption ({0})", extractVersion));
                    }

                    // Patched entries require extract version >= 27
                    if ((localFlags & (int)GeneralBitFlags.Patched) != 0 && extractVersion < 27) throw new ZipException($"Patched data requires higher version than ({extractVersion})");

                    // Central header flags match local entry flags.
                    if (localFlags != entry.Flags) throw new ZipException("Central header/local header flags mismatch");

                    // Central header compression method matches local entry
                    if (entry.CompressionMethodForHeader != (CompressionMethod)compressionMethod) throw new ZipException("Central header/local header compression method mismatch");

                    if (entry.Version != extractVersion) throw new ZipException("Extract version mismatch");

                    // Strong encryption and extract version match
                    if ((localFlags & (int)GeneralBitFlags.StrongEncryption) != 0)
                    {
                        if (extractVersion < 62) throw new ZipException("Strong encryption flag set but version not high enough");
                    }

                    if ((localFlags & (int)GeneralBitFlags.HeaderMasked) != 0)
                    {
                        if (fileTime != 0 || fileDate != 0) throw new ZipException("Header masked set but date/time values non-zero");
                    }

                    if ((localFlags & (int)GeneralBitFlags.Descriptor) == 0)
                    {
                        if (crcValue != (uint)entry.Crc) throw new ZipException("Central header/local header crc mismatch");
                    }

                    // Crc valid for empty entry.
                    // This will also apply to streamed entries where size isnt known and the header cant be patched
                    if (size == 0 && compressedSize == 0)
                    {
                        if (crcValue != 0) throw new ZipException("Invalid CRC for empty entry");
                    }

                    // TODO: make test more correct...  can't compare lengths as was done originally as this can fail for MBCS strings
                    // Assuming a code page at this point is not valid?  Best is to store the name length in the ZipEntry probably
                    if (entry.Name.Length > storedNameLength) throw new ZipException("File name length mismatch");

                    // Name data has already been read convert it and compare.
                    var localName = localEncoding.GetString(nameData);

                    // Central directory and local entry name match
                    if (localName != entry.Name) throw new ZipException("Central header and local header file name mismatch");

                    // Directories have zero actual size but can have compressed size
                    if (entry.IsDirectory)
                    {
                        if (size > 0) throw new ZipException("Directory cannot have size");

                        // There may be other cases where the compressed size can be greater than this?
                        // If so until details are known we will be strict.
                        if (entry.IsCrypted)
                        {
                            if (compressedSize > entry.EncryptionOverheadSize + 2) throw new ZipException("Directory compressed size invalid");
                        }
                        else if (compressedSize > 2)
                        {
                            // When not compressed the directory size can validly be 2 bytes
                            // if the true size wasn't known when data was originally being written.
                            // NOTE: Versions of the library 0.85.4 and earlier always added 2 bytes
                            throw new ZipException("Directory compressed size invalid");
                        }
                    }

                    if (!ZipNameTransform.IsValidName(localName, true)) throw new ZipException("Name is invalid");
                }

                // Tests that apply to both data and header.

                // Size can be verified only if it is known in the local header.
                // it will always be known in the central header.
                if (((localFlags & (int)GeneralBitFlags.Descriptor) == 0) ||
                    ((size > 0 || compressedSize > 0) && entry.Size > 0))
                {
                    if (size != 0 && size != entry.Size) throw new ZipException($"Size mismatch between central header({entry.Size}) and local header({size})");

                    if ((compressedSize != 0)
                        && (compressedSize != entry.CompressedSize && compressedSize != 0xFFFFFFFF && compressedSize != -1))
                        throw new ZipException($"Compressed size mismatch between central header({entry.CompressedSize}) and local header({compressedSize})");
                }

                var extraLength = storedNameLength + extraDataLength;
                return _offsetOfFirstEntry + entry.Offset + ZipConstants.LocalHeaderBaseSize + extraLength;
            }
        }

        #endregion Archive Testing

        #region Internal routines

        #region Reading

        /// <summary>
        /// Read an unsigned short in little endian byte order.
        /// </summary>
        /// <returns>Returns the value read.</returns>
        /// <exception cref="EndOfStreamException">
        /// The stream ends prematurely
        /// </exception>
        private ushort ReadLEUshort()
        {
            int data1 = _baseStream.ReadByte();

            if (data1 < 0)
            {
                throw new EndOfStreamException("End of stream");
            }

            int data2 = _baseStream.ReadByte();

            if (data2 < 0)
            {
                throw new EndOfStreamException("End of stream");
            }

            return unchecked((ushort)((ushort)data1 | (ushort)(data2 << 8)));
        }

        /// <summary>
        /// Read a uint in little endian byte order.
        /// </summary>
        /// <returns>Returns the value read.</returns>
        /// <exception cref="IOException">
        /// An i/o error occurs.
        /// </exception>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The file ends prematurely
        /// </exception>
        private uint ReadLEUint()
        {
            return (uint)(ReadLEUshort() | (ReadLEUshort() << 16));
        }

        private ulong ReadLEUlong()
        {
            return ReadLEUint() | ((ulong)ReadLEUint() << 32);
        }

        #endregion Reading

        /// <summary>
        /// Search for and read the central directory of a zip file filling the entries array.
        /// </summary>
        /// <exception cref="System.IO.IOException">
        /// An i/o error occurs.
        /// </exception>
        /// <exception cref="ICSharpCode.SharpZipLib.Zip.ZipException">
        /// The central directory is malformed or cannot be found
        /// </exception>
        private void ReadEntries()
        {
            // Search for the End Of Central Directory.  When a zip comment is
            // present the directory will start earlier
            //
            // The search is limited to 64K which is the maximum size of a trailing comment field to aid speed.
            // This should be compatible with both SFX and ZIP files but has only been tested for Zip files
            // If a SFX file has the Zip data attached as a resource and there are other resources occurring later then
            // this could be invalid.
            // Could also speed this up by reading memory in larger blocks.

            if (_baseStream.CanSeek == false)
            {
                throw new ZipException("ZipFile stream must be seekable");
            }

            var locatedEndOfCentralDir = LocateBlockWithSignature(ZipConstants.EndOfCentralDirectorySignature,
                _baseStream.Length, ZipConstants.EndOfCentralRecordBaseSize, 0xffff);

            if (locatedEndOfCentralDir < 0) throw new ZipException("Cannot find central directory");

            // Read end of central directory record
            ushort thisDiskNumber = ReadLEUshort();
            ushort startCentralDirDisk = ReadLEUshort();
            ulong entriesForThisDisk = ReadLEUshort();
            ulong entriesForWholeCentralDir = ReadLEUshort();
            ulong centralDirSize = ReadLEUint();
            long offsetOfCentralDir = ReadLEUint();
            uint commentSize = ReadLEUshort();

            if (commentSize > 0)
            {
                var comment = new byte[commentSize];
                StreamUtils.ReadFully(_baseStream, comment);
                comment_ = __stringCodec.ZipArchiveCommentEncoding.GetString(comment);
            }
            else comment_ = string.Empty;

            var isZip64 = false;
            var requireZip64 = false;

            // Check if zip64 header information is required.
            if ((thisDiskNumber == 0xffff) ||
                (startCentralDirDisk == 0xffff) ||
                (entriesForThisDisk == 0xffff) ||
                (entriesForWholeCentralDir == 0xffff) ||
                (centralDirSize == 0xffffffff) ||
                (offsetOfCentralDir == 0xffffffff))
                requireZip64 = true;

            // #357 - always check for the existance of the Zip64 central directory.
            // #403 - Take account of the fixed size of the locator when searching.
            //    Subtract from locatedEndOfCentralDir so that the endLocation is the location of EndOfCentralDirectorySignature,
            //    rather than the data following the signature.
            var locatedZip64EndOfCentralDirLocator = LocateBlockWithSignature(
                ZipConstants.Zip64CentralDirLocatorSignature,
                locatedEndOfCentralDir - 4,
                ZipConstants.Zip64EndOfCentralDirectoryLocatorSize,
                0);

            if (locatedZip64EndOfCentralDirLocator < 0)
            {
                // This is only an error in cases where the Zip64 directory is required.
                if (requireZip64) throw new ZipException("Cannot find Zip64 locator");
            }
            else
            {
                isZip64 = true;

                // number of the disk with the start of the zip64 end of central directory 4 bytes
                // relative offset of the zip64 end of central directory record 8 bytes
                // total number of disks 4 bytes
                ReadLEUint(); // startDisk64 is not currently used
                ulong offset64 = ReadLEUlong();
                uint totalDisks = ReadLEUint();

                _baseStream.Position = (long)offset64;
                long sig64 = ReadLEUint();

                if (sig64 != ZipConstants.Zip64CentralFileHeaderSignature) throw new ZipException($"Invalid Zip64 Central directory signature at {offset64:X}");

                // NOTE: Record size = SizeOfFixedFields + SizeOfVariableData - 12.
                ulong recordSize = ReadLEUlong();
                int versionMadeBy = ReadLEUshort();
                int versionToExtract = ReadLEUshort();
                uint thisDisk = ReadLEUint();
                uint centralDirDisk = ReadLEUint();
                entriesForThisDisk = ReadLEUlong();
                entriesForWholeCentralDir = ReadLEUlong();
                centralDirSize = ReadLEUlong();
                offsetOfCentralDir = (long)ReadLEUlong();

                // NOTE: zip64 extensible data sector (variable size) is ignored.
            }

            _entries = entries_ = new ZipEntry[entriesForThisDisk];

            // SFX/embedded support, find the offset of the first entry vis the start of the stream
            // This applies to Zip files that are appended to the end of an SFX stub.
            // Or are appended as a resource to an executable.
            // Zip files created by some archivers have the offsets altered to reflect the true offsets
            // and so dont require any adjustment here...
            // TODO: Difficulty with Zip64 and SFX offset handling needs resolution - maths?
            if (!isZip64 && (offsetOfCentralDir < locatedEndOfCentralDir - (4 + (long)centralDirSize)))
            {
                _offsetOfFirstEntry = offsetOfFirstEntry = locatedEndOfCentralDir - (4 + (long)centralDirSize + offsetOfCentralDir);
                if (_offsetOfFirstEntry <= 0) throw new ZipException("Invalid embedded zip archive");
            }

            _baseStream.Seek(_offsetOfFirstEntry + offsetOfCentralDir, SeekOrigin.Begin);

            for (var i = 0UL; i < entriesForThisDisk; i++)
            {
                if (ReadLEUint() != ZipConstants.CentralHeaderSignature) throw new ZipException("Wrong Central Directory signature");

                int versionMadeBy = ReadLEUshort();
                int versionToExtract = ReadLEUshort();
                int bitFlags = ReadLEUshort();
                int method = ReadLEUshort();
                uint dostime = ReadLEUint();
                uint crc = ReadLEUint();
                var csize = (long)ReadLEUint();
                var size = (long)ReadLEUint();
                int nameLen = ReadLEUshort();
                int extraLen = ReadLEUshort();
                int commentLen = ReadLEUshort();

                int diskStartNo = ReadLEUshort();  // Not currently used
                int internalAttributes = ReadLEUshort();  // Not currently used

                uint externalAttributes = ReadLEUint();
                long offset = ReadLEUint();

                byte[] buffer = new byte[Math.Max(nameLen, commentLen)];
                var entryEncoding = __stringCodec.ZipInputEncoding(bitFlags);

                StreamUtils.ReadFully(_baseStream, buffer, 0, nameLen);
                string name = entryEncoding.GetString(buffer, 0, nameLen);
                var unicode = entryEncoding.IsZipUnicode();

                var entry = new P4kEntry(name, versionToExtract, versionMadeBy, (CompressionMethod)method, unicode)
                {
                    Crc = crc & 0xffffffffL,
                    Size = size & 0xffffffffL,
                    CompressedSize = csize & 0xffffffffL,
                    Flags = bitFlags,
                    DosTime = dostime,
                    ZipFileIndex = (long)i,
                    Offset = offset,
                    ExternalFileAttributes = (int)externalAttributes,
                    CryptoCheckValue = (bitFlags & 8) == 0
                        ? (byte)(crc >> 24)
                        : (byte)((dostime >> 8) & 0xff),
                };

                if (extraLen > 0)
                {
                    var extra = new byte[extraLen];
                    StreamUtils.ReadFully(_baseStream, extra);
                    entry.ExtraData = extra;
                }

                entry.ProcessExtraData(false);

                if (commentLen > 0)
                {
                    StreamUtils.ReadFully(_baseStream, buffer, 0, commentLen);
                    entry.Comment = entryEncoding.GetString(buffer, 0, commentLen);
                }

                _entries[i] = entry;
            }
        }

        /// <summary>
        /// Locate the data for a given entry.
        /// </summary>
        /// <returns>
        /// The start offset of the data.
        /// </returns>
        /// <exception cref="System.IO.EndOfStreamException">
        /// The stream ends prematurely
        /// </exception>
        /// <exception cref="ICSharpCode.SharpZipLib.Zip.ZipException">
        /// The local header signature is invalid, the entry and central header file name lengths are different
        /// or the local and entry compression methods dont match
        /// </exception>
        private long LocateEntry(ZipEntry entry)
            => TestLocalHeader((P4kEntry)entry, HeaderTest.Extract);

        private Stream CreateAndInitAesDecryptionStream(Stream baseStream, ZipEntry entry)
        {
            using (var aes = new AesManaged
            {
                Key = key,
                IV = new byte[16],
                Mode = CipherMode.CBC,
                Padding = PaddingMode.None
            })
            {
                var crypto = new CryptoStream(baseStream, aes.CreateDecryptor(), CryptoStreamMode.Read);

                var buffer = new MemoryStream();
                crypto.CopyTo(buffer);

                // Trim NULL off end of stream
                buffer.Seek(-1, SeekOrigin.End);
                while (buffer.Position > 1 && buffer.ReadByte() == 0) buffer.Seek(-2, SeekOrigin.Current);
                buffer.SetLength(buffer.Position);

                buffer.Seek(0, SeekOrigin.Begin);

                return buffer;
            }
        }

        #endregion Internal routines
    }
}
