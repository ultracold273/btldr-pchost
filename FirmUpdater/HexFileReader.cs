using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FirmUpdater
{
    class HexFileReader
    {
        private StreamReader file;
        private FileStream binFile;
        private string hexFileName;
        private string binFileName;
        private UInt32 Addr;
        public HexFileReader(string path)
        {
            hexFileName = path;
            binFileName = Path.GetFileName(path).Replace(".hex", ".bin");
            Addr = 0xFFFFFFFF;
        }

        private byte HexParseLine(string line, out byte[] data)
        {
            if (line.Length < 1 || line[0] != ':')
            {
                data = new byte[0];
                return 255;
            }
            byte crc_verify = 0x00;
            byte recLen = Convert.ToByte(line.Substring(1, 2), 16);
            byte addr_h = Convert.ToByte(line.Substring(3, 2), 16);
            byte addr_l = Convert.ToByte(line.Substring(5, 2), 16);
            uint addr = (uint) ((addr_h << 8) | addr_l);
            if (addr < (Addr & 0x0000ffff))
            {
                Addr &= 0xffff0000;
                Addr |= addr;
            }
            byte recType = Convert.ToByte(line.Substring(7, 2), 16);
            crc_verify = (byte) (recLen + addr_h + addr_l + recType);
            data = new byte[recLen];
            for (int i = 0;i < recLen;i++)
            {
                data[i] = Convert.ToByte(line.Substring(9 + i * 2, 2), 16);
                crc_verify += data[i];
            }
            byte crc = Convert.ToByte(line.Substring(line.Length - 2, 2), 16);
            if (crc != (byte) (0x01 + ~crc_verify))
            {
                throw new Exception("CRCError");
            }
            return recType;
        }

        public void ToBinary()
        {
            file = new StreamReader(hexFileName);
            binFile = new FileStream(binFileName, FileMode.Create);
            string line;
            while((line = file.ReadLine()) != null)
            {
                byte[] data;
                switch (HexParseLine(line, out data))
                {
                    case 0:
                        binFile.Write(data, 0, data.Length);
                        break;
                    case 1:
                        break;
                    case 2: break;
                    case 4:
                        Addr &= 0x0000ffff;
                        Addr |= (uint) ((data[1] << 24) | (data[0] << 16));
                        break;
                    case 5: break;
                }
            }
            file.Close();
            binFile.Close();
        }

        public UInt32 startAddr
        {
            get
            {
                return Addr;
            }
        }

        public string bFileName
        {
            get
            {
                return binFileName;
            }
        }
    }
}
