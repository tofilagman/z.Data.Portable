using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace z.Data
{

    public static class NickEncryption
    {
        public static string Encrypt(string Data, string Key)
        {
            using (var n = new Enigma())
            {
                var cr = new LZ().compressToEncodedURIComponent(Data);
                return n.Encrypt(cr, Key);
            }
        }

        public static string Decrypt(string Data, string Key)
        {
            using (var n = new Enigma())
            {
                var lk = n.Decrypt(Data, Key);
                return new LZ().decompressFromEncodedURIComponent(lk);
            }
        }
    }

    internal sealed class Rotor : IDisposable
    {
        protected byte[] brotor = new byte[] {
            0x4e, 0x75, 0x33, 0x34, 0x35, 0x50, 0x51, 0x53, 0x76, 0x77, 0x57, 0x79, 0x7a,
            0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49, 0x4a, 0x4b, 0x4c, 0x4d,
            0x61, 0x62, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69, 0x6a, 0x6b, 0x54, 0x58,
            0x30, 0x4f, 0x72, 0x73, 0x74, 0x59, 0x5a, 0x6c, 0x6d, 0x24,0x78,  0x55, 0x56,
            0x6e, 0x6f, 0x70, 0x71, 0x36, 0x31, 0x32, 0x37, 0x38, 0x39, 0x2b, 0x2d, 0x52,
        };
        public int turns { get; protected set; } = 0;

        public int indexOf(byte c)
        {

            for (int i = 0; i < brotor.Length; i++)
                if (brotor[i] == c)
                    return i;

            return -1;
        }

        public int Length => brotor.Length;

        public void turn(int Step)
        {
            Step = Step + turns;

            int Counter = (Step == 0 || Step == Length) ? 1 : (Step > Length) ? (Step - Length) + 1 : (Length - Step) + 1;

            while (Counter > Length)
                Counter = Counter - Length;

            byte b;
            for (int i = 0; i < Step; i++)
            {
                b = brotor[0];
                Array.Copy(brotor, 1, brotor, 0, brotor.Length - 1);
                brotor[brotor.Length - 1] = b;
            }

            turns++;
        }

        public byte charAt(int i)
        {
            return brotor[i];
        }

        ~Rotor() => Dispose();

        public void Dispose()
        {
            GC.Collect();
            GC.SuppressFinalize(this);
        }
    }

    internal sealed class Enigma : IDisposable
    {
        private Rotor lRotor;
        private Rotor mRotor;
        private Rotor sRotor;

        private void Initiate()
        {
            lRotor = new Rotor();
            mRotor = new Rotor();
            sRotor = new Rotor();
        }


        public void Sha256()
        {
            byte[] data = KeyGen.ASCIIEncoder("LJ Gomez");
            var gg = SHA256.MessageSHA256(data);

            var k = Convert.ToBase64String(gg);
;        }

        public string Encrypt(string Data, string Key)
        {
            this.Initiate();
            char[] cypher = new char[Data.Length];

            var keyIndex = GetKey(Key);
            for (var i = 0; i < Data.Length; i++)
            {
                cypher[i] = EncryptChar(Data[i], keyIndex);
            }

            return new string(cypher);
        }

        public string Decrypt(string Data, string Key)
        {
            this.Initiate();
            char[] plain = new char[Data.Length];

            var keyIndex = GetKey(Key);

            for (int i = 0; i < Data.Length; i++)
                plain[i] = DecryptChar(Data[i], keyIndex);

            return new string(plain);
        }

        protected int GetKey(string Key)
        {
            return KeyGen.DeriveKey(Key, 32).Sum(x => Convert.ToInt32(x));
        }

        protected char EncryptChar(char c, int keyCode)
        {
            sRotor.turn(keyCode);
            byte ch;
            try
            {
                ch = lRotor.charAt(sRotor.indexOf((byte)c));
                ch = lRotor.charAt(mRotor.indexOf(ch));
            }
            catch
            {
                return c;
            }

            // sRotor.turn(keyCode);

            if (sRotor.turns % sRotor.Length == 0)
                mRotor.turn(keyCode);

            if (mRotor.turns % mRotor.Length == 0)
                lRotor.turn(keyCode);

            return (char)ch;
        }

        protected char DecryptChar(char c, int keyCode)
        {
            sRotor.turn(keyCode);
            byte ch;
            try
            {
                ch = mRotor.charAt(lRotor.indexOf((byte)c));
                ch = sRotor.charAt(lRotor.indexOf(ch));
            }
            catch
            {
                return c;
            }

            if (sRotor.turns % sRotor.Length == 0)
                mRotor.turn(keyCode);

            if (mRotor.turns % mRotor.Length == 0)
                lRotor.turn(keyCode);

            return (char)ch;
        }

        ~Enigma() => Dispose();

        public void Dispose()
        {
            lRotor?.Dispose();
            mRotor?.Dispose();
            sRotor?.Dispose();
            GC.Collect();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// make a key out of a password
        /// </summary>
        internal sealed class KeyGen
        {
            private KeyGen()
            { }

            /// <summary>
            /// safe method to generate a key
            /// see the next override for more info
            /// </summary>
            public static byte[] DeriveKey(string password, int keySize, byte[] salt = null)
            {
                return DeriveKey(password, keySize, salt, 1024);
            }

            /// <summary>
            /// make a key out of a string password
            /// based on PBKDF1 (PKCS #5 v1.5)
            /// see http://www.faqs.org/rfcs/rfc2898.html
            /// if you do not want a salt set it to null
            /// recomended salt length must be between 8 and 16 bytes
            /// This implementation support keySize up to 32 bytes
            /// use salt = null, iterationCount = 1 for minimal strength
            /// </summary>
            public static byte[] DeriveKey(string password, int keySize, byte[] salt, int iterationCount)
            {
                if (keySize > 32) keySize = 32;
                byte[] data = ASCIIEncoder(password);
                if (salt != null)
                {
                    byte[] temp = new byte[data.Length + salt.Length];
                    Array.Copy(data, 0, temp, 0, data.Length);
                    Array.Copy(salt, 0, temp, data.Length, salt.Length);
                    data = temp;
                }
                if (iterationCount <= 0) iterationCount = 1;
                for (int i = 0; i < iterationCount; i++)
                {
                    data = SHA256.MessageSHA256(data);
                }
                byte[] key = new byte[keySize];
                Array.Copy(data, 0, key, 0, keySize);

                return key;
            }

            /// <summary>
            /// helper function not to rely on System.Text.ASCIIEncoder
            /// </summary>
            public static byte[] ASCIIEncoder(string s)
            {
                byte[] ascii = new byte[s.Length];
                for (int i = 0; i < s.Length; i++)
                {
                    ascii[i] = (byte)s[i];
                }
                return ascii;
            }

        }//EPC

        /// <summary>
        /// SHA-256 implementation
        /// </summary>
        internal sealed class SHA256
        {
            //SHA-256 constants
            private const uint _H00 = 0x6a09e667;
            private const uint _H10 = 0xbb67ae85;
            private const uint _H20 = 0x3c6ef372;
            private const uint _H30 = 0xa54ff53a;
            private const uint _H40 = 0x510e527f;
            private const uint _H50 = 0x9b05688c;
            private const uint _H60 = 0x1f83d9ab;
            private const uint _H70 = 0x5be0cd19;

            /// <summary>
            /// This method take a byte array, pads it according to the 
            /// specifications in FIPS 180-2 and passes to the SHA-256
            /// hash function. It returns a byte array containing 
            /// the hash of the given message.
            /// </summary>
            /// <param name="message">Message to be hashed as byte array</param>
            /// <returns>Message hash value as byte array</returns>
            public static byte[] MessageSHA256(byte[] message)
            {
                int n, pBase;
                long l;
                byte[] m;
                byte[] pad;
                byte[] swap;
                byte[] temp;

                // Determine length of message in 512-bit blocks
                n = (int)(message.Length >> 6);

                // Determine space required for padding
                if ((message.Length & 0x3f) < 56)
                {
                    n++;
                    pBase = 56;
                }
                else
                {
                    n += 2;
                    pBase = 120;
                }

                // Determine total message length
                l = message.Length << 3;

                // Create message padding 
                pad = new byte[pBase - (message.Length & 0x3f)];
                pad[0] = 0x80;
                for (int i = 1; i < pad.Length; i++)
                    pad[i] = 0;
                m = new byte[n * 64];

                // Assemble padded message
                Array.Copy(message, 0, m, 0, message.Length);
                Array.Copy(pad, 0, m, message.Length, pad.Length);
                temp = BitConverter.GetBytes((long)l);
                swap = new byte[8];
                for (int i = 0; i < 8; i++)
                    swap[i] = temp[7 - i];
                Array.Copy(swap, 0, m, message.Length + pad.Length, 8);

                // Call Hash1 to hash message
                return Hash256(n, m);
            }

            /// <summary>
            /// SHA-1 Hash function - This method should only be used
            /// with properly padded messages.  To hash an unpadded 
            /// message use one of the other methods.  This method is
            /// called by other methods once the message is padded
            /// and loaded as a byte array.  For internal use only!
            /// </summary>
            /// <param name="n">Number of 512-bit message segments</param>
            /// <param name="m">Message to be hashed as byte array</param>
            /// <returns>Hash value as byte array</returns>
            private static byte[] Hash256(int n, byte[] m)
            {
                // K Constants			
                uint[] K = { 0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
                           0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
                           0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
                           0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
                           0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
                           0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
                           0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
                           0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
                           0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
                           0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
                           0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
                           0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
                           0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
                           0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
                           0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
                           0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2 };
                // Calculation variables
                uint a, b, c, d, e, f, g, h, temp1, temp2;
                // Intermediate hash values
                uint[] interHash = new uint[8];
                // Scheduled W values
                uint[] w = new uint[64];
                // Final hash byte array
                byte[] hash = new byte[32];
                // Used to correct for endian
                byte[] swap = new byte[4];
                byte[] swap2 = new byte[4];

                // Initial hash values
                interHash[0] = _H00;
                interHash[1] = _H10;
                interHash[2] = _H20;
                interHash[3] = _H30;
                interHash[4] = _H40;
                interHash[5] = _H50;
                interHash[6] = _H60;
                interHash[7] = _H70;

                // Perform hash operation
                for (int i = 0; i < n; i++)
                {
                    // Prepare the message schedule
                    for (int t = 0; t < 64; t++)
                    {
                        if (t < 16)
                        {
                            for (int j = 0; j < 4; j++)
                                swap[j] = m[i * 64 + t * 4 + 3 - j];
                            w[t] = BitConverter.ToUInt32(swap, 0);
                        }
                        else
                        {
                            w[t] = (uint)(Sigma(3, w[t - 2]) + w[t - 7]
                                + Sigma(2, w[t - 15]) + w[t - 16]);

                        }
                    }

                    //Initialize the five working variables
                    a = interHash[0];
                    b = interHash[1];
                    c = interHash[2];
                    d = interHash[3];
                    e = interHash[4];
                    f = interHash[5];
                    g = interHash[6];
                    h = interHash[7];

                    //Perform main hash loop
                    for (int t = 0; t < 64; t++)
                    {

                        temp1 = (uint)(h + Sigma(1, e) + Func(true, e, f, g)
                            + K[t] + w[t]);
                        temp2 = (uint)(Sigma(0, a) + Func(false, a, b, c));
                        h = g;
                        g = f;
                        f = e;
                        e = (uint)(d + temp1);
                        d = c;
                        c = b;
                        b = a;
                        a = (uint)(temp1 + temp2);
                    }

                    //Compute the intermediate hash values
                    interHash[0] = (uint)((a + interHash[0]));
                    interHash[1] = (uint)((b + interHash[1]));
                    interHash[2] = (uint)((c + interHash[2]));
                    interHash[3] = (uint)((d + interHash[3]));
                    interHash[4] = (uint)((e + interHash[4]));
                    interHash[5] = (uint)((f + interHash[5]));
                    interHash[6] = (uint)((g + interHash[6]));
                    interHash[7] = (uint)((h + interHash[7]));
                }

                // Copy Intermediate results to final hash array
                for (int i = 0; i < 8; i++)
                {
                    swap2 = BitConverter.GetBytes((uint)interHash[i]);
                    for (int j = 0; j < 4; j++)
                    {
                        swap[j] = swap2[3 - j];
                    }
                    Array.Copy(swap, 0, hash, i * 4, 4);
                }

                return hash;
            }

            /// <summary>
            /// Performs SHA-256 logical functions.  See FIPS 180-2 for
            /// complete description.  For internal use only!
            /// </summary>
            /// <param name="b">
            /// Boolean value indicating whether the desired function is Ch
            /// </param>
            /// <param name="x">First arguement</param>
            /// <param name="y">Second arguement</param>
            /// <param name="z">Third arguement</param>
            /// <returns>Function results</returns>
            private static uint Func(bool b, uint x, uint y, uint z)
            {
                // Ch function
                if (b)
                    return (uint)((x & y) ^ ((~x) & z));
                // Maj function
                return (uint)((x & y) ^ (x & z) ^ (y & z));

            }

            /// <summary>
            /// Performs the SHA-256 shifting and rotating functions.
            /// See FIPS 180-2 for complete details.  For internal
            /// use only!
            /// </summary>
            /// <param name="i">Integer indicating which function to perform</param>
            /// <param name="x">Operand</param>
            /// <returns>Result of rotation/shift</returns>
            private static uint Sigma(int i, uint x)
            {
                uint temp;

                switch (i)
                {
                    case 0:
                        temp = (uint)((x >> 2) | (x << 30));
                        temp = temp ^ (uint)((x >> 13) | (x << 19));
                        temp = temp ^ (uint)((x >> 22) | (x << 10));
                        break;
                    case 1:
                        temp = (uint)((x >> 6) | (x << 26));
                        temp = temp ^ (uint)((x >> 11) | (x << 21));
                        temp = temp ^ (uint)((x >> 25) | (x << 7));
                        break;
                    case 2:
                        temp = (uint)((x >> 7) | (x << 25));
                        temp = temp ^ (uint)((x >> 18) | (x << 14));
                        temp = temp ^ (uint)(x >> 3);
                        break;
                    case 3:
                        temp = (uint)((x >> 17) | (x << 15));
                        temp = temp ^ (uint)((x >> 19) | (x << 13));
                        temp = temp ^ (uint)(x >> 10);
                        break;
                    default:
                        temp = x;
                        break;
                }
                return temp;
            }
        }
    }
}
