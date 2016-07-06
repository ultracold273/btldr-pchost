using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;

namespace FirmUpdater
{
    public class DataTransferrer
    {
        SerialPort s;

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

        private static DataTransferrer Instance;
        public static DataTransferrer GetSingleton()
        {
            return Instance ?? (Instance = new DataTransferrer());
        }

        private DataTransferrer()
        {
            
        }

        public void CreateNewPort(string portname, int bandrate)
        {
            s = new SerialPort(portname, bandrate);
            s.Open();
            s.Encoding = Encoding.ASCII;
        //    s.DataReceived += s_DataReceived;
        }

        public string[] GetPorts()
        {
            return SerialPort.GetPortNames();
        }
    }
}
