using System.IO;

namespace StandardDot.TestClasses
{
    public class CheckDisposeStream : Stream
    {
        public CheckDisposeStream(Stream backingStream) {
            BackingStream = backingStream;
        }

        public override bool CanRead => BackingStream.CanRead;

        public override bool CanSeek => BackingStream.CanSeek;

        public override bool CanWrite => BackingStream.CanWrite;

        public override long Length => BackingStream.Length;

        public override long Position { get => BackingStream.Position; set => BackingStream.Position = value; }
        
        protected virtual Stream BackingStream { get; }

        public virtual bool HasBeenDisposed { get; protected set; }

        public override void Flush()
        {
            BackingStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BackingStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BackingStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BackingStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BackingStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            this.HasBeenDisposed = true;
        }
    }
}