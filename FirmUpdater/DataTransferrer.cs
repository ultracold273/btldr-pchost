using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.ComponentModel;

namespace FirmUpdater
{
    public class DataTransferrer
    {
        private SerialPortSim s;
        private FileStream fs;
        private uint startAddress;
        private uint endAddress;

        private const byte HDR_START = 0xB0;
        private const byte HDR_RESP = 0xB1;
        private const byte HDR_PAYLOAD = 0xB2;
        private const byte HDR_RESET = 0xB3;
        private const byte HDR_FIN = 0xB4;
        private const byte HDR_HASH = 0xBA;

        private const byte RES_STT_OK = 0x00;
        private const byte RES_STT_CRC_ERR = 0x01;
        private const byte RES_STT_TIMEOUT = 0x02;
        private const byte RES_STT_FLASH_BUSY = 0x03;
        private const byte RES_STT_FLASH_ERR = 0x04;
        private const byte RES_STT_ADDR_ERR = 0x05;
        private const byte RES_STT_CRYPTO_ERR = 0x06;

        private const byte RET_SYNC_FAIL = 0xFF;
        private const byte RET_HDR_ERR = 0xFE;
        private const byte RET_OK = 0x00;
        private static DataTransferrer Instance;
        public static DataTransferrer GetSingleton()
        {
            return Instance ?? (Instance = new DataTransferrer());
        }

        private DataTransferrer()
        {
            s = new SerialPortSim();
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
            s = new SerialPortSim(portname, bandrate);
            s.Open();
        }

        public void AssignBinFile(string path)
        {
            byte[] temp = new byte[4];
            fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            fs.Position = fs.Length - 8;
            fs.Read(temp, 0, 4);
            startAddress = BitConverter.ToUInt32(temp, 0);
            fs.Read(temp, 0, 4);
            endAddress = BitConverter.ToUInt32(temp, 0);
            fs.Position = 0;
        }

        public string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }

        public void Close()
        {
            s.Close();
        }

        public byte ProgramFirmware(object sender, DoWorkEventArgs e)
        {
            byte[] packet = new byte[64];
            packet[0] = 0x55;
            s.Write(packet, 0, 1);
            if (s.ReadByte() != 0xAA)
            {
                return RET_SYNC_FAIL;
            }
            int sendByte = ConstructStartPacket(ref packet, startAddress, endAddress);
            if (0 != SendAndGetResponse(ref packet, sendByte)) return 1;
            uint programSize = endAddress - startAddress;
            uint blocks = (programSize % 16 == 0)?programSize / 16: programSize / 16 + 1;
            for(uint i = 0;i < blocks;i++)
            {
                sendByte = ConstructPayloadPacket(ref packet, i);
                if (0 != SendAndGetResponse(ref packet, sendByte)) return 1;
                //int percentage = (int) ((float) i / (float)blocks);
                int percentage = (int)((float)i); // Only For testing / (float)blocks);
                (sender as BackgroundWorker).ReportProgress(percentage);
            }
            sendByte = ConstructFinishPacket(ref packet);
            if (0 != SendAndGetResponse(ref packet, sendByte)) return 1;
            packet[0] = 0xAA;
            s.Write(packet, 0, 1);
            if (s.ReadByte() != 0x55)
            {
                return RET_SYNC_FAIL;
            }

            return 0;
        }

        private byte SendAndGetResponse(ref byte[] packet, int sendByte)
        {
            byte[] response = new byte[10];
            byte iRetry = 1;
            s.Write(packet, 0, sendByte);
            s.Read(response, 0, 4);
            while (true)
            {
                if (response[0] != HDR_RESP)
                {
                    sendByte = ConstructResetPacket(ref packet);
                    s.Write(packet, 0, sendByte);
                    return RET_HDR_ERR;
                }
                else if (response[1] == RES_STT_OK) return RET_OK;
                else if ((response[1] == RES_STT_CRC_ERR || response[1] == RES_STT_TIMEOUT) && iRetry <= 3)
                {
                    ModifyPayloadPacketRetry(ref packet, iRetry);
                    s.Write(packet, 0, sendByte);
                    s.Read(response, 0, 4);
                    iRetry++;
                }
                else
                {
                    sendByte = ConstructResetPacket(ref packet);
                    s.Write(packet, 0, sendByte);
                    return response[1];
                }
            }
        }

        /*
         *  Start Packet contains two 32-bit unsigned word, program start address and end address 
         */
        private int ConstructStartPacket(ref byte[] packet, uint startAddress, uint endAddress)
        {
            packet[0] = HDR_START; packet[1] = 8; packet[2] = 8; packet[3] = 0;
            BitConverter.GetBytes(startAddress).CopyTo(packet, 4);
            BitConverter.GetBytes(endAddress).CopyTo(packet, 8);
            Crc32.Calculate(packet, 0, 12).CopyTo(packet, 12);
            return 16;     
        }

        private int ConstructPayloadPacket(ref byte[] packet, uint block)
        {
            packet[0] = HDR_PAYLOAD; packet[1] = 20; packet[2] = 20; packet[3] = 0;
            // Fill in the start address
            fs.Read(packet, 4, 4);
            fs.Read(packet, 8, 16);
            Crc32.Calculate(packet, 0, 24).CopyTo(packet, 24);
            return 28;
        }

        private void ModifyPayloadPacketRetry(ref byte[] packet, byte iRetry)
        {
            packet[3] = iRetry;
            return;
        }

        private int ConstructFinishPacket(ref byte[] packet)
        {
            packet[0] = HDR_FIN; packet[1] = 0;packet[2] = 0; packet[3] = 0;
            Crc32.Calculate(packet, 0, 4).CopyTo(packet, 4);
            return 8;
        }

        private int ConstructResetPacket(ref byte[] packet)
        {
            packet[0] = HDR_RESET; packet[1] = 0;packet[2] = 0; packet[3] = 0;
            Crc32.Calculate(packet, 0, 4).CopyTo(packet, 4);
            return 8;
        }
    }
}
