using System.IO;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Globalization;

public class FtpHelper
{
    public FtpHelper(ICakeContext context) {
        this._context = context;
    }

    private ICakeContext _context;

    public string Host { get; set; }

    public string RemoteDir { get; set; }
    
    public string Username { get; set; }

    public string SecurePasswordLocation { get; set; }

    public byte[] AesKey => System.Convert.FromBase64String(_context.EnvironmentVariable("FTPAesKey"));

    public int DeleteRetryAttempts { get; set; }

    public int UploadRetryAttempts { get; set; }

    public int SocketPollInterval { get; set; }

    public int ConnectTimeout { get; set; }

    public int ReadTimeout { get; set; }

    public int DataConnectionConnectTimeout { get; set; }

    public int DataConnectionReadTimeout { get; set; }

    // psProtectedString - this is the output from
    //   powershell> $psProtectedString = ConvertFrom-SecureString -SecureString $aSecureString -key (1..16)
    // key - make sure you add size checking 
    // notes: this will throw an cryptographic invalid padding exception if it cannot decrypt correctly (wrong key)
    public SecureString ConvertToSecureString(string psProtectedString, byte[] key)
    {
        string trimmedPsProtectedString = psProtectedString.Trim();
        // '|' is indeed the separater
        byte[] asBytes = Convert.FromBase64String( trimmedPsProtectedString );
        string[] strArray = Encoding.Unicode.GetString(asBytes).Split(new[] { '|' });

        if (strArray.Length != 3) throw new InvalidDataException("input had incorrect format");

        // strArray[0] is a static/magic header or signature (different passwords produce
        //    the same header)  It unused in our case, looks like 16 bytes as hex-string
        // you know strArray[1] is a base64 string by the '=' at the end
        //    the IV is shorter than the body, and you can verify that it is the IV, 
        //    because it is exactly 16bytes=128bits and it decrypts the password correctly
        // you know strArray[2] is a hex-string because it is [0-9a-f]
        //byte[] magicHeader = HexStringToByteArray(encrypted.Substring(0, 32));
        byte[] rgbIV = Convert.FromBase64String(strArray[1]);
        byte[] cipherBytes = HexStringToByteArray(strArray[2]);

        // setup the decrypter
        SecureString str = new SecureString();
        var algorithm = Aes.Create();
        ICryptoTransform transform = algorithm.CreateDecryptor(key, rgbIV);
        using (var stream = new CryptoStream(new MemoryStream(cipherBytes), transform, CryptoStreamMode.Read))
        {
            // using this silly loop format to loop one char at a time
            // so we never store the entire password naked in memory
            int numRed = 0;
            byte[] buffer = new byte[2]; // two bytes per unicode char
            while( (numRed = stream.Read(buffer, 0, buffer.Length)) > 0 )
            {
                str.AppendChar(Encoding.Unicode.GetString(buffer).ToCharArray()[0]);
            }
        }

        return str;
    }

    // from http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
    public byte[] HexStringToByteArray(String hex)
    {
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }

    public SecureString DecryptPassword( string psPasswordFile, byte[] key )
    {
        if( ! System.IO.File.Exists(psPasswordFile))
        {
            throw new ArgumentException("file does not exist: " + psPasswordFile);
        }

        string formattedCipherText = System.IO.File.ReadAllText( psPasswordFile );

        return ConvertToSecureString(formattedCipherText, key);

    }
}