using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FirmUpdater
{

    class SerialPortSim
    {
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public bool IsOpen
        {
            get;set;
        }

        public SerialPortSim(string portname, int baudrate)
        {
            IsOpen = false;
        }

        public SerialPortSim()
        {
            IsOpen = false;
        }

        public static string[] GetPortNames()
        {
            string[] names = new string[] {"COM1", "COM2", "COM6" };
            return names;
        }

        public void Open()
        {
            AllocConsole();
            IsOpen = true;
        }

        public void Close()
        {
            FreeConsole();
            IsOpen = false;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            Console.WriteLine("Write: ");
            for(int i = 0;i < count;i++)
            {
                Console.Write("0x{0:X} ", buffer[offset + i]);
            }
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            for(int i = 0; i < count;i++)
            {
                buffer[offset + i] = Convert.ToByte(Console.ReadLine(), 16);
            }
        }

        public void WriteByte(byte a)
        {
            Console.WriteLine("WriteByte: ");
            Console.WriteLine("0x{0:X}", a);
        }

        public byte ReadByte()
        {
            return Convert.ToByte(Console.ReadLine(), 16);
        }
    }
}
