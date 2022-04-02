using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.Zip
{
    /// <summary>
    /// A <see cref="PartialInputStream"/> is an <see cref="InflaterInputStream"/>
    /// whose data is only a part or subsection of a file.
    /// </summary>
    internal class PartialInputStream : Stream
    {
        Cry3File zipFile_;
        Stream baseStream_;
        readonly long start_;
        readonly long length_;
        long readPos_;
        readonly long end_;

        /// <summary>
        /// Initialise a new instance of the <see cref="PartialInputStream"/> class.
        /// </summary>
        /// <param name="zipFile">The <see cref="ZipFile"/> containing the underlying stream to use for IO.</param>
        /// <param name="start">The start of the partial data.</param>
        /// <param name="length">The length of the partial data.</param>
        public PartialInputStream(Cry3File zipFile, long start, long length)
        {
            start_ = start;
            length_ = length;

            // Although this is the only time the zipfile is used
            // keeping a reference here prevents premature closure of
            // this zip file and thus the baseStream_.

            // Code like this will cause apparently random failures depending
            // on the size of the files and when garbage is collected.
            //
            // ZipFile z = new ZipFile (stream);
            // Stream reader = z.GetInputStream(0);
            // uses reader here....
            zipFile_ = zipFile;
            baseStream_ = zipFile_._baseStream;
            readPos_ = start;
            end_ = start + length;
        }

        /// <summary>
        /// Read a byte from this stream.
        /// </summary>
        /// <returns>Returns the byte read or -1 on end of stream.</returns>
        public override int ReadByte()
        {
            // -1 is the correct value at end of stream.
            if (readPos_ >= end_) return -1;

            lock (baseStream_)
            {
                baseStream_.Seek(readPos_++, SeekOrigin.Begin);
                return baseStream_.ReadByte();
            }
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="System.ArgumentException">The sum of offset and count is larger than the buffer length. </exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="System.ArgumentNullException">buffer is null. </exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">offset or count is negative. </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (baseStream_)
            {
                if (count > end_ - readPos_)
                {
                    count = (int)(end_ - readPos_);
                    if (count == 0) return 0;
                }
                // Protect against Stream implementations that throw away their buffer on every Seek (for example, Mono FileStream)
                if (baseStream_.Position != readPos_) baseStream_.Seek(readPos_, SeekOrigin.Begin);
                var readCount = baseStream_.Read(buffer, offset, count);
                if (readCount > 0) readPos_ += readCount;
                return readCount;
            }
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="System.NotSupportedException">The stream does not support writing. </exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="System.ArgumentNullException">buffer is null. </exception>
        /// <exception cref="System.ArgumentException">The sum of offset and count is greater than the buffer length. </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">offset or count is negative. </exception>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type <see cref="System.IO.SeekOrigin"></see> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            var newPos = readPos_;
            switch (origin)
            {
                case SeekOrigin.Begin: newPos = start_ + offset; break;
                case SeekOrigin.Current: newPos = readPos_ + offset; break;
                case SeekOrigin.End: newPos = end_ + offset; break;
            }

            if (newPos < start_) throw new ArgumentException("Negative position is invalid");
            if (newPos > end_) throw new IOException("Cannot seek past end");
            readPos_ = newPos;
            return readPos_;
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="System.IO.IOException">An I/O error occurs. </exception>
        public override void Flush() { }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <value></value>
        /// <returns>The current position within the stream.</returns>
        /// <exception cref="System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="System.NotSupportedException">The stream does not support seeking. </exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get => readPos_ - start_;
            set
            {
                var newPos = start_ + value;
                if (newPos < start_) throw new ArgumentException("Negative position is invalid");
                if (newPos > end_) throw new InvalidOperationException("Cannot seek past end");
                readPos_ = newPos;
            }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <value></value>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        /// <exception cref="System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Length => length_;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value>false</value>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite => false;

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value>true</value>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek => true;

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value>true.</value>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead => true;

        /// <summary>
        /// Gets a value that determines whether the current stream can time out.
        /// </summary>
        /// <value></value>
        /// <returns>A value that determines whether the current stream can time out.</returns>
        public override bool CanTimeout => baseStream_.CanTimeout;
    }
}
