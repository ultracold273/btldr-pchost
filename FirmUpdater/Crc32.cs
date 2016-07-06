using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirmUpdater
{
    public class Crc32
    {
        public static byte[] Calculate(byte[] inArray, int offset, int count)
        {
            byte[] res = new byte[4];
            if (count % 4 != 0) throw new Exception("Length shall be multiple of 4");
            return res;
        }
    }
}
