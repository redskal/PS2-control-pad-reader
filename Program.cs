/*
 * PS2controller reader
 * by Dorian Warboys / Red Skål
 * https://github.com/redskal/
 * 
 * A crude way of monitoring the signals from a PS2 gamepad rigged
 * up to USB as a HID device.
 * 
 * We use HIDSharp to find the device, then exploit the fact that
 * Linux treats everything as a file. Using BinaryReader we can
 * extrapolate the data from the file.
 * 
 * Reading 8 bytes from the file will give us all the data needed.
 * Below I've listed the read-outs I have seen with the 'analog'
 * button on. If this is off the values are different and sticks
 * do not send any signals.
 * 
 * Using BitArray we can decode the data and act upon it.
 * 
 * 8 bytes:
 *  byte 0 = 1h idle
 *  byte 1 = right stick Y axis (00h = up, 80h = center, FFh = down)
 *  byte 2 = right stick X axis (00h = left, 80h = center, FFh = right)
 *  byte 3 = left stick X axis
 *  byte 4 = left stick Y axis
 *  byte 5 = Fh idle, DpadL = 6h, DpadR = 2h, DpadU = 0h, DpadD = 4h
 *           1Fh = Tri, 2Fh = Circ, 4Fh = X, 8Fh = Sqr
 *  byte 6 = 0h idle, L1 = 4h, L2 = 1h, R1 = 8h, R2 = 2h, Select = 10h, Start = 20h, LSbutton = 40h, RSbutton = 80h
 *  byte 7 = F0h idle
 */
using System;
using System.IO;
using System.Linq;
using System.Collections;
using HidSharp;

namespace DroneClient
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var list = DeviceList.Local;
            var hidDeviceList = list.GetHidDevices().ToArray();
            int _vid, _pid = 0;
            string filename;
            int thrustPWM, thrust = 0;
            int directionPWM, direction = 0;
            string dirTemp;
            bool padsOff = true;

            foreach (HidDevice d in hidDeviceList)
            {
                _vid = d.VendorID;
                _pid = d.ProductID;

                if (_vid == 0x0810 && _pid == 0x0003)
                {
                    filename = d.GetFileSystemName().ToString();
                    Console.WriteLine("Found:\t" + _vid.ToString() + ":" + _pid.ToString());
                    Console.WriteLine("\t" + filename + "\n\n");

                    DateTime delay = DateTime.Now.AddMilliseconds(1500);
                    while (DateTime.Now < delay) { }

                    if (File.Exists(filename))
                    {
                        Console.WriteLine(" Found file.");
                        Console.WriteLine(" Reading now...\n");
                        while (true)
                        {
                            // Read 8 bytes from controller file
                            byte[] data = ReadController(filename);

                            // begin display
                            Console.Clear();
                            Console.WriteLine("Data:\t{0:X2} {1:X2} {2:X2} {3:X2}", data[0], data[1], data[2], data[3]);
                            Console.WriteLine("\t{0:X2} {1:X2} {2:X2} {3:X2}", data[4], data[5], data[6], data[7]);
                            Console.WriteLine();

                            BitArray bits = new BitArray(data);

                            // display as useable data
                            if (data[4] <= 128) // forwards
                            {
                                thrustPWM = RRemapPWM(data[4]);
                                thrust = 0;
                            }
                            else // reverse
                            {
                                thrustPWM = RemapPWM(data[4]);
                                thrust = 1;
                            }


                            if (data[2] < 128) // left
                            {
                                directionPWM = RRemapPWM(data[2]);
                                direction = 1;
                                dirTemp = "LEFT";
                            }
                            else if (data[2] > 128) //right
                            {
                                directionPWM = RemapPWM(data[2]);
                                direction = 2;
                                dirTemp = "RIGHT";
                            }
                            else // straight ahead, cap'n
                            {
                                directionPWM = direction = 0;
                                dirTemp = "STRAIGHT";
                            }

                            Console.WriteLine("RS val: {0}\t\tThrust:\t\t{1}\t{2}", data[4], thrustPWM, (thrust < 1) ? "FORWARD" : "REVERSE");
                            Console.WriteLine("LS val: {0}\t\tDirection:\t{1}\t{2}", data[2], directionPWM, dirTemp);
                            Console.WriteLine();

                            Console.WriteLine("L1: {0}\tL2: {1}\nR1: {2}\tR2: {3}", bits[50] ? "ON" : "OFF", bits[48] ? "ON" : "OFF", bits[51] ? "ON" : "OFF", bits[49] ? "ON" : "OFF");
                            Console.WriteLine("Triangle: {0}\tCircle: {1}\tCross: {2}\tSquare: {3}", bits[44] ? "ON " : "OFF", bits[45] ? "ON " : "OFF", bits[46] ? "ON " : "OFF", bits[47] ? "ON " : "OFF");
                            Console.WriteLine("Sel: {0}\tStart: {1}\tLSbut: {2}\tRSbut: {3}", bits[52] ? "ON " : "OFF", bits[53] ? "ON" : "OFF", bits[54] ? "ON" : "OFF", bits[55] ? "ON" : "OFF");
                            Console.WriteLine();


                            // delay to avoid butchering the CPU
                            delay = DateTime.Now.AddMilliseconds(50);
                            while (DateTime.Now < delay) { }
                        }
                    }
                    else
                    {
                        Console.WriteLine(" No file found.");
                    }
                }

            }
        }

        public static byte[] ReadController(string filename)
        {
            byte[] data = { 1 };

            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                // read 8 bytes from controller
                using (BinaryReader r = new BinaryReader(fs))
                {
                    data = r.ReadBytes(8);
                    r.Close();
                }
            }
            return data;
        }

        private static int RRemapPWM(int val)
        {
            int inMin = 128;
            int inMax = 0;

            return (255 - ((inMax - val) * (0xFF - 0x00) / (inMax - inMin)));
        }

        private static int RemapPWM(int val)
        {
            int inMin = 129;
            int inMax = 255;

            return ((val - inMin) * (0xFF - 0x00) / (inMax - inMin));
        }
    }
}
