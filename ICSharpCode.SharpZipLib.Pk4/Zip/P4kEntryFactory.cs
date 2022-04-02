using ICSharpCode.SharpZipLib.Core;
using System;
using System.IO;
using static ICSharpCode.SharpZipLib.Zip.ZipEntryFactory;

namespace ICSharpCode.SharpZipLib.Zip
{
    /// <summary>
    /// P4kEntryFactory. Basic implementation of <see cref="IEntryFactory"></see>
    /// </summary>
    public class P4kEntryFactory : IEntryFactory
    {
        private INameTransform nameTransform_;
        private DateTime fixedDateTime_ = DateTime.Now;
        private TimeSetting timeSetting_ = TimeSetting.LastWriteTime;
        private bool isUnicodeText_;

        private int getAttributes_ = -1;
        private int setAttributes_;

        /// <summary>
        /// Initialise a new instance of the <see cref="P4kEntryFactory"/> class.
        /// </summary>
        /// <remarks>A default <see cref="INameTransform"/>, and the LastWriteTime for files is used.</remarks>
        public P4kEntryFactory()
        {
            nameTransform_ = new ZipNameTransform();
            isUnicodeText_ = true;
        }

        /// <summary>
        /// Initialise a new instance of <see cref="P4kEntryFactory"/> using the specified <see cref="TimeSetting"/>
        /// </summary>
        /// <param name="timeSetting">The <see cref="TimeSetting">time setting</see> to use when creating <see cref="ZipEntry">Zip entries</see>.</param>
        public P4kEntryFactory(TimeSetting timeSetting) : this()
            => timeSetting_ = timeSetting;

        /// <summary>
        /// Initialise a new instance of <see cref="P4kEntryFactory"/> using the specified <see cref="DateTime"/>
        /// </summary>
        /// <param name="time">The time to set all <see cref="ZipEntry.DateTime"/> values to.</param>
        public P4kEntryFactory(DateTime time) : this()
        {
            timeSetting_ = TimeSetting.Fixed;
            FixedDateTime = time;
        }

        /// <summary>
        /// Get / set the <see cref="INameTransform"/> to be used when creating new <see cref="ZipEntry"/> values.
        /// </summary>
        /// <remarks>
        /// Setting this property to null will cause a default <see cref="ZipNameTransform">name transform</see> to be used.
        /// </remarks>
        public INameTransform NameTransform
        {
            get => nameTransform_;
            set => nameTransform_ = value ?? new ZipNameTransform();
        }

        /// <summary>
        /// Get / set the <see cref="TimeSetting"/> in use.
        /// </summary>
        public TimeSetting Setting
        {
            get => timeSetting_;
            set => timeSetting_ = value;
        }

        /// <summary>
        /// Get / set the <see cref="DateTime"/> value to use when <see cref="Setting"/> is set to <see cref="TimeSetting.Fixed"/>
        /// </summary>
        public DateTime FixedDateTime
        {
            get => fixedDateTime_;
            set
            {
                if (value.Year < 1970) throw new ArgumentException("Value is too old to be valid", nameof(value));
                fixedDateTime_ = value;
            }
        }

        /// <summary>
        /// A bitmask defining the attributes to be retrieved from the actual file.
        /// </summary>
        /// <remarks>The default is to get all possible attributes from the actual file.</remarks>
        public int GetAttributes
        {
            get => getAttributes_;
            set => getAttributes_ = value;
        }

        /// <summary>
        /// A bitmask defining which attributes are to be set on.
        /// </summary>
        /// <remarks>By default no attributes are set on.</remarks>
        public int SetAttributes
        {
            get => setAttributes_;
            set => setAttributes_ = value;
        }

        /// <summary>
        /// Get set a value indicating whether unicode text should be set on.
        /// </summary>
        public bool IsUnicodeText
        {
            get => isUnicodeText_;
            set => isUnicodeText_ = value;
        }

        /// <summary>
        /// Make a new <see cref="ZipEntry"/> for a file.
        /// </summary>
        /// <param name="fileName">The name of the file to create a new entry for.</param>
        /// <returns>Returns a new <see cref="ZipEntry"/> based on the <paramref name="fileName"/>.</returns>
        public ZipEntry MakeFileEntry(string fileName)
            => MakeFileEntry(fileName, null, true);

        /// <summary>
        /// Make a new <see cref="ZipEntry"/> for a file.
        /// </summary>
        /// <param name="fileName">The name of the file to create a new entry for.</param>
        /// <param name="useFileSystem">If true entry detail is retrieved from the file system if the file exists.</param>
        /// <returns>Returns a new <see cref="ZipEntry"/> based on the <paramref name="fileName"/>.</returns>
        public ZipEntry MakeFileEntry(string fileName, bool useFileSystem)
            => MakeFileEntry(fileName, null, useFileSystem);

        /// <summary>
        /// Make a new <see cref="ZipEntry"/> from a name.
        /// </summary>
        /// <param name="fileName">The name of the file to create a new entry for.</param>
        /// <param name="entryName">An alternative name to be used for the new entry. Null if not applicable.</param>
        /// <param name="useFileSystem">If true entry detail is retrieved from the file system if the file exists.</param>
        /// <returns>Returns a new <see cref="ZipEntry"/> based on the <paramref name="fileName"/>.</returns>
        public ZipEntry MakeFileEntry(string fileName, string entryName, bool useFileSystem)
        {
            var result = new P4kEntry(nameTransform_.TransformFile(!string.IsNullOrEmpty(entryName) ? entryName : fileName));
            result.IsUnicodeText = isUnicodeText_;

            var externalAttributes = 0;
            var useAttributes = setAttributes_ != 0;

            var fi = useFileSystem ? new FileInfo(fileName) : null;
            if (fi != null && fi.Exists)
            {
                switch (timeSetting_)
                {
                    case TimeSetting.CreateTime: result.DateTime = fi.CreationTime; break;
                    case TimeSetting.CreateTimeUtc: result.DateTime = fi.CreationTimeUtc; break;
                    case TimeSetting.LastAccessTime: result.DateTime = fi.LastAccessTime; break;
                    case TimeSetting.LastAccessTimeUtc: result.DateTime = fi.LastAccessTimeUtc; break;
                    case TimeSetting.LastWriteTime: result.DateTime = fi.LastWriteTime; break;
                    case TimeSetting.LastWriteTimeUtc: result.DateTime = fi.LastWriteTimeUtc; break;
                    case TimeSetting.Fixed: result.DateTime = fixedDateTime_; break;
                    default: throw new ZipException("Unhandled time setting in MakeFileEntry");
                }

                result.Size = fi.Length;

                useAttributes = true;
                externalAttributes = ((int)fi.Attributes & getAttributes_);
            }
            else
            {
                if (timeSetting_ == TimeSetting.Fixed) result.DateTime = fixedDateTime_;
            }

            if (useAttributes)
            {
                externalAttributes |= setAttributes_;
                result.ExternalFileAttributes = externalAttributes;
            }

            return result;
        }

        /// <summary>
        /// Make a new <see cref="ZipEntry"></see> for a directory.
        /// </summary>
        /// <param name="directoryName">The raw untransformed name for the new directory</param>
        /// <returns>Returns a new <see cref="ZipEntry"></see> representing a directory.</returns>
        public ZipEntry MakeDirectoryEntry(string directoryName)
            => MakeDirectoryEntry(directoryName, true);

        /// <summary>
        /// Make a new <see cref="ZipEntry"></see> for a directory.
        /// </summary>
        /// <param name="directoryName">The raw untransformed name for the new directory</param>
        /// <param name="useFileSystem">If true entry detail is retrieved from the file system if the file exists.</param>
        /// <returns>Returns a new <see cref="ZipEntry"></see> representing a directory.</returns>
        public ZipEntry MakeDirectoryEntry(string directoryName, bool useFileSystem)
        {
            var result = new P4kEntry(nameTransform_.TransformDirectory(directoryName));
            result.IsUnicodeText = isUnicodeText_;
            result.Size = 0;

            var externalAttributes = 0;
            var di = useFileSystem ? new DirectoryInfo(directoryName) : null;
            if (di != null && di.Exists)
            {
                switch (timeSetting_)
                {
                    case TimeSetting.CreateTime: result.DateTime = di.CreationTime; break;
                    case TimeSetting.CreateTimeUtc: result.DateTime = di.CreationTimeUtc; break;
                    case TimeSetting.LastAccessTime: result.DateTime = di.LastAccessTime; break;
                    case TimeSetting.LastAccessTimeUtc: result.DateTime = di.LastAccessTimeUtc; break;
                    case TimeSetting.LastWriteTime: result.DateTime = di.LastWriteTime; break;
                    case TimeSetting.LastWriteTimeUtc: result.DateTime = di.LastWriteTimeUtc; break;
                    case TimeSetting.Fixed: result.DateTime = fixedDateTime_; break;
                    default: throw new ZipException("Unhandled time setting in MakeDirectoryEntry");
                }
                externalAttributes = ((int)di.Attributes & getAttributes_);
            }
            else
            {
                if (timeSetting_ == TimeSetting.Fixed) result.DateTime = fixedDateTime_;
            }

            // Always set directory attribute on.
            externalAttributes |= (setAttributes_ | 16);
            result.ExternalFileAttributes = externalAttributes;

            return result;
        }
    }
}
