using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FirmUpdater
{

    class SerialPortSim
    {
        SerialPort s;
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        public bool IsOpen
        {
            get
            {
                return s.IsOpen;
            }
        }

        public SerialPortSim(string portname, int baudrate)
        {
            s = new SerialPort(portname, baudrate);
        }

        public SerialPortSim()
        {
            s = new SerialPort();
            //IsOpen = false;
        }

        public static string[] GetPortNames()
        {
            //string[] names = new string[] {"COM1", "COM2", "COM6" };
            //return names;
            return SerialPort.GetPortNames();
        }

        public void Open()
        {
            AllocConsole();
            s.Open();
            //IsOpen = true;
        }

        public void Close()
        {
            FreeConsole();
            s.Close();
            //IsOpen = false;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            Console.WriteLine("Write: ");
            for(int i = 0;i < count;i++)
            {
                Console.Write("0x{0:X} ", buffer[offset + i]);
            }
            Console.WriteLine();
            s.Write(buffer, offset, count);
        }

        public void Read(byte[] buffer, int offset, int count)
        {
            s.Read(buffer, offset, count);
            Console.WriteLine("Read {0} bytes: ", count);
            for(int i = 0; i < count;i++)
            {
                //buffer[offset + i] = Convert.ToByte(Console.ReadLine(), 16);
                Console.WriteLine("0x{0:X} ", buffer[offset + i]);
            }
        }

        public void WriteByte(byte a)
        {
            Console.WriteLine("WriteByte: ");
            Console.WriteLine("0x{0:X}", a);
        }

        public byte ReadByte()
        {
            Console.WriteLine("ReadByte: ");
            byte a = (byte) s.ReadByte();
            Console.WriteLine("0x{0:X}", a);
            return a;
        }
    }
}
