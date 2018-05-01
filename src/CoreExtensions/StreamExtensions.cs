using System.IO;
using StandardDot.CoreExtensions.Object;

namespace StandardDot.CoreExtensions
{
    /// <summary>
    /// Extensions for streams.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts a stream to a string. If a stream is readable, it goes to the beginning.
        /// </summary>
        /// <param name="target">The stream to convert.</param>
        /// <returns>The string pulled from the stream.</returns>
        public static string GetString(this Stream target)
        {
            if (target.CanSeek)
            {
                target.Position = 0;
            }
            using (StreamReader reader = new StreamReader(target))
            {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Converts a byte[] to a string. If a stream is readable, it goes to the beginning.
        /// </summary>
        /// <param name="input">The stream to convert.</param>
        /// <returns>The byte[] pulled from the stream.</returns>
        public static byte[] ToByteArray(this Stream input)
        {
            if (input.CanSeek)
            {
                input.Position = 0;
            }
            
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}