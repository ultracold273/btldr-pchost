using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;

namespace FirmUpdater
{
    public class DataTransferrer
    {
        private SerialPort s;
        private FileStream fs;
        private uint startAddress;
        private uint programSize;

        private const byte HDR_START = 0xA0;
        private const byte HDR_RESP = 0xA1;
        private const byte HDR_PAYLOAD = 0xA2;
        private const byte HDR_RESET = 0xA3;
        private const byte HDR_FIN = 0xA4;
        private const byte HDR_HASH = 0xAA;

        private const byte RES_STT_OK = 0x00;
        private const byte RES_STT_CRC_ERR = 0x01;
        private const byte RES_STT_TIMEOUT = 0x02;
        private const byte RES_STT_FLASH_BUSY = 0x03;
        private const byte RES_STT_FLASH_ERR = 0x04;
        private const byte RES_STT_ADDR_ERR = 0x05;
        private const byte RES_STT_CRYPTO_ERR = 0x06;

        private const byte RET_SYNC_FAIL = 0xFF;
        private static DataTransferrer Instance;
        public static DataTransferrer GetSingleton()
        {
            return Instance ?? (Instance = new DataTransferrer());
        }

        private DataTransferrer()
        {
            
        }

        public bool IsOpen
        {
            get
            {
                return s.IsOpen;
            }
        }

        public void CreateNewPort(string portname, int bandrate)
        {
            s = new SerialPort(portname, bandrate);
            s.Open();
        }

        public void AssignBinFile(string path)
        {
            byte[] temp = new byte[4];
            fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Read(temp, 0, 4);
            startAddress = BitConverter.ToUInt32(temp, 0);
            fs.Read(temp, 0, 4);
            programSize = BitConverter.ToUInt32(temp, 0);
        }

        public string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }

        public void Close()
        {
            s.Close();
        }

        public byte ProgramFirmware()
        {
            byte[] packet = new byte[64];
            packet[0] = 0x55;
            s.Write(packet, 0, 1);
            if (s.ReadByte() != 0xAA)
            {
                return RET_SYNC_FAIL;
            }
            int sendByte = ConstructStartPacket(ref packet, startAddress, programSize);
            s.Write(packet, 0, sendByte);
            uint blocks = (programSize % 128 == 0)?programSize / 128: programSize / 128 + 1;
            for(uint i = 0;i < blocks;i++)
            {
                sendByte = ConstructPayloadPacket(ref packet, i);
            }
            return 0;
        }

        /*
         *  Start Packet contains two 32-bit unsigned word, program start address and end address 
         */
        private int ConstructStartPacket(ref byte[] packet, uint startAddress, uint programSize)
        {
            packet[0] = HDR_START; packet[1] = 8; packet[2] = 8; packet[3] = 0;
            BitConverter.GetBytes(startAddress).CopyTo(packet, 4);
            BitConverter.GetBytes(startAddress + programSize).CopyTo(packet, 8);
            Crc32.Calculate(packet, 0, 12).CopyTo(packet, 12);
            return 16;
        }

        private int ConstructPayloadPacket(ref byte[] packet, uint block)
        {
            packet[0] = HDR_PAYLOAD; packet[1] = 20; packet[2] = 20; packet[3] = 0;
            BitConverter.GetBytes(block).CopyTo(packet, 4);
            fs.Read(packet, 8, 16);
            Crc32.Calculate(packet, 0, 24).CopyTo(packet, 24);
            return 28;
        }

        private void ModifyPayloadPacketRetry(ref byte[] packet, byte iRetry)
        {
            packet[3] = iRetry;
            return;
        }
    }
}
