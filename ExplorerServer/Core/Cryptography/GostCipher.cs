using System;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerServer.Core.Cryptography
{
    public class GostCipher
    {

        #region Private

        private readonly byte[][] _substitutionBox =
        {
              new byte[] { 0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xA,0xB,0xC,0xD,0xE,0xF },
              new byte[] { 0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xA,0xB,0xC,0xD,0xE,0xF },
              new byte[] { 0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xA,0xB,0xC,0xD,0xE,0xF },
              new byte[] { 0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xA,0xB,0xC,0xD,0xE,0xF },
              new byte[] { 0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xA,0xB,0xC,0xD,0xE,0xF },
              new byte[] { 0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xA,0xB,0xC,0xD,0xE,0xF },
              new byte[] { 0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xA,0xB,0xC,0xD,0xE,0xF },
              new byte[] { 0x0,0x1,0x2,0x3,0x4,0x5,0x6,0x7,0x8,0x9,0xA,0xB,0xC,0xD,0xE,0xF }
        };

        private uint[] GenerateKeys(byte[] key)
        {
            if (key.Length != 32)
            {
                throw new Exception("Wrong key.");
            }

            var subkeys = new uint[8];

            for (int i = 0; i < 8; i++)
            {
                subkeys[i] = BitConverter.ToUInt32(key, 4 * i);
            }

            return subkeys;
        }

        private uint Substitution(uint value)
        {
            uint output = 0;

            for (int i = 0; i < 8; i++)
            {
                var temp = (byte)((value >> (4 * i)) & 0x0f);
                temp = _substitutionBox[i][temp];
                output |= (UInt32)temp << (4 * i);
            }
            return output;
        }


        private byte[] EncodeBlock(byte[] block, uint[] keys)
        {
            // separate on 2 blocks.
            uint n1 = BitConverter.ToUInt32(block, 0);
            uint n2 = BitConverter.ToUInt32(block, 4);

            for (int i = 0; i < 32; i++)
            {
                int keyIndex = i < 24 ? (i % 8) : (7 - i % 8); // to 24th cycle : 0 to 7; after - 7 to 0;
                var s = (n1 + keys[keyIndex]) % uint.MaxValue; // (N1 + X[i]) mod 2^32
                s = Substitution(s); // substitute from box
                s = (s << 11) | (s >> 21);
                s = s ^ n2; // ( s + N2 ) mod 2
                //N2 = N1;
                //N1 = s;
                if (i < 31) // last cycle : N1 don't change; N2 = s;
                {
                    n2 = n1;
                    n1 = s;
                }
                else
                {
                    n2 = s;
                }
            }

            var output = new byte[8];
            var n1Buff = BitConverter.GetBytes(n1);
            var n2Buff = BitConverter.GetBytes(n2);

            for (int i = 0; i < 4; i++)
            {
                output[i] = n1Buff[i];
                output[4 + i] = n2Buff[i];
            }

            return output;
        }

        private byte[] DecodeBlock(byte[] block, uint[] keys)
        {
            // separate on 2 blocks.
            uint n1 = BitConverter.ToUInt32(block, 0);
            uint n2 = BitConverter.ToUInt32(block, 4);

            for (int i = 0; i < 32; i++)
            {
                int keyIndex = i < 8 ? (i % 8) : (7 - i % 8); // to 24th cycle : 0 to 7; after - 7 to 0;
                var s = (n1 + keys[keyIndex]) % uint.MaxValue; // (N1 + X[i]) mod 2^32
                s = Substitution(s); // substitute from box
                s = (s << 11) | (s >> 21);
                s = s ^ n2;
                if (i < 31) // last cycle : N1 don't change; N2 = s;
                {
                    n2 = n1;
                    n1 = s;
                }
                else
                {
                    n2 = s;
                }
            }

            var output = new byte[8];
            var n1Buff = BitConverter.GetBytes(n1);
            var n2Buff = BitConverter.GetBytes(n2);

            for (int i = 0; i < 4; i++)
            {
                output[i] = n1Buff[i];
                output[4 + i] = n2Buff[i];
            }

            return output;
        }

        #endregion

        #region Public

        public byte[] Encode(byte[] data, byte[] key, bool isParallel = false)
        {
            if (key.Length != 32)
            {
                throw new ArgumentException("Invalid key length");
            }
            if (data.Length%8 != 0 || data.Length == 0)
            {
                throw new ArgumentException("Invalid data length");
            }
            var subkeys = GenerateKeys(key);
            var result = new byte[data.Length];
            var block = new byte[8];

            if (isParallel)
            {
                Parallel.For(0, data.Length / 8, i =>
                {
                    Array.Copy(data, 8 * i, block, 0, 8);
                    Array.Copy(EncodeBlock(block, subkeys), 0, result, 8 * i, 8);
                });
            }
            else
            {
                for (int i = 0; i < data.Length / 8; i++) // N blocks 64bits length.
                {
                    Array.Copy(data, 8 * i, block, 0, 8);
                    Array.Copy(EncodeBlock(block, subkeys), 0, result, 8 * i, 8);
                }
            }
            return result;
        }

        public byte[] Decode(byte[] data, byte[] key, bool isParallel = false)
        {
            if (key.Length != 32)
            {
                throw new ArgumentException("Invalid key length");
            }
            if (data.Length % 8 != 0 || data.Length == 0)
            {
                throw new ArgumentException("Invalid data length");
            }
            var subkeys = GenerateKeys(key);
            var result = new byte[data.Length];
            var block = new byte[8];

            if (isParallel)
            {
                Parallel.For(0, data.Length / 8, i =>
                {
                    Array.Copy(data, 8 * i, block, 0, 8);
                    Array.Copy(DecodeBlock(block, subkeys), 0, result, 8 * i, 8);
                });
            }
            else
            {
                for (int i = 0; i < data.Length / 8; i++) // N blocks 64bits length.
                {
                    Array.Copy(data, 8 * i, block, 0, 8);
                    Array.Copy(DecodeBlock(block, subkeys), 0, result, 8 * i, 8);
                }
            }
            return result;
        }
        
        #endregion
    }
}