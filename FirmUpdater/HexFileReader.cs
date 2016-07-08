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
        private uint startAddress;
        private uint endAddress;
        public HexFileReader(string path)
        {
            hexFileName = path;
            binFileName = Path.GetFileName(path).Replace(".hex", ".bin");
            startAddress = 0xFFFFFFFF;
            endAddress = 0x00000000;
        }

        private byte HexParseLine(string line, out byte[] data, out uint addr, out byte recLen)
        {
            if (line.Length < 1 || line[0] != ':')
            {
                data = new byte[0];
                addr = 0;
                recLen = 0;
                return 255;
            }
            byte crc_verify = 0x00;
            recLen = Convert.ToByte(line.Substring(1, 2), 16);
            byte addr_h = Convert.ToByte(line.Substring(3, 2), 16);
            byte addr_l = Convert.ToByte(line.Substring(5, 2), 16);
            addr = (uint) ((addr_h << 8) | addr_l);
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
            byte[] writeBuffer = new byte[32];
            uint writeBufferLength = 0;
            uint phyAddr = 0;
            file = new StreamReader(hexFileName);
            binFile = new FileStream(binFileName, FileMode.Create);
            string line;
            while((line = file.ReadLine()) != null)
            {
                byte[] recData;
                uint recAddr;
                byte recLen;
                switch (HexParseLine(line, out recData, out recAddr, out recLen))
                {
                    case 0:
                        phyAddr &= 0xffff0000;
                        phyAddr |= recAddr;
                        if (phyAddr < startAddress) startAddress = phyAddr;
                        if (phyAddr + recLen > endAddress) endAddress = phyAddr + recLen;
                        recData.CopyTo(writeBuffer, writeBufferLength);
                        writeBufferLength += recLen;
                        if (writeBufferLength >= 16)
                        {
                            uint prevLength = writeBufferLength - recLen;
                            binFile.Write(BitConverter.GetBytes(phyAddr - prevLength), 0, 4);
                            binFile.Write(writeBuffer, 0, 16);
                            for(int i = 0;i < writeBufferLength;i++)
                            {
                                writeBuffer[i] = writeBuffer[16 + i];
                            }
                            phyAddr += (16 - prevLength);
                            writeBufferLength -= 16;
                        }
                        break;
                    case 1:
                        break;
                    case 2: break;
                    case 4:
                        phyAddr &= 0x0000ffff;
                        phyAddr |= (uint) ((recData[0] << 24) | (recData[1] << 16));
                        break;
                    case 5: break;
                }
            }
            if (writeBufferLength > 0)
            {
                binFile.Write(BitConverter.GetBytes(phyAddr), 0, 4);
                for(uint i = writeBufferLength;i < 16;i++)
                {
                    writeBuffer[i] = 0xff;
                }
                binFile.Write(writeBuffer, 0, 16);
            }
            binFile.Write(BitConverter.GetBytes(startAddress), 0, 4);
            binFile.Write(BitConverter.GetBytes(endAddress), 0, 4);
            file.Close();
            binFile.Close();
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
