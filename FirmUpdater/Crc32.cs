using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FirmUpdater
{
    public class Crc32
    {
        public const UInt32 DefaultPolynomial = 0x04C11DB7;
        public const UInt32 DefaultSeed = 0xffffffffu;

        public static byte[] Calculate(byte[] inArray, int offset, int count)
        {
            /*
            Initialize();
            HashCore(inArray, offset, count);
            //if (count % 4 != 0) throw new Exception("Length shall be multiple of 4");
            return HashFinal();
            */
            if (count % 4 != 0) throw new Exception("Length shall be multiple of 4");
            UInt32 crc = DefaultSeed;
            for (int i = 0; i < count / 4; i++)
            {
                UInt32 data = BitConverter.ToUInt32(inArray, i * 4);
                crc = crc ^ data;
                for (int j = 0; j < 32; j++)
                {
                    if ((crc & 0x80000000) != 0) crc = (crc << 1) ^ DefaultPolynomial;
                    else crc = (crc << 1);
                }
            }
            return BitConverter.GetBytes(crc);
        }
    }   
}
