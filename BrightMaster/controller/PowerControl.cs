using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrightMaster
{
    class PowerControl
    {
        SerialPort serialPort;
        bool closing = false;
        bool listening = false;
        string globalStr = "";
        string remoteMode = "[:]SYSTem:REM\r\n";
        string powerOn = "[:]OUTP 1\r\n";
        string powerOff = "[:]OUTP 0\r\n";
        string setVoltage = "[:]VOLTage";
        string setCurrent = "[:]CURR";
        bool hasCOM = true;
        public PowerControl()
        {
            string comport = ConfigurationManager.AppSettings["SerialPort"];
            if(comport == "")
            {
                hasCOM = false;
                return;
            }
            serialPort = new SerialPort(comport, 9600);
            //serialPort.RtsEnable = 
            serialPort.RtsEnable = true;
            serialPort.DtrEnable = true;
            serialPort.Open();
            serialPort.WriteLine(remoteMode);
        }



        public void PowerOn()
        {
            SetCurrent(GlobalVars.Instance.PowerSettings.Current);
            SetVoltage(GlobalVars.Instance.PowerSettings.Voltage);
            Send(powerOn);
        }

        private void Send(string s)
        {
            if (!hasCOM)
                return;
            Send(Encoding.Default.GetBytes(s));
        }
        private void Send(byte[] bytes)
        {
            serialPort.Write(bytes, 0, bytes.Length);
        }


        public void PowerOff()
        {
           
            Send(powerOff);
        }

        public void SetVoltage(float voltage)
        {
            string cmd = string.Format("{0} {1}\r\n", setVoltage, voltage);
            //serialPort.WriteLine(cmd);
            Send(cmd);
        }

        public void SetCurrent(float current)
        {
            string cmd = string.Format("{0} {1}\r\n", setCurrent, current);
            Send(cmd);
        }

        public void Close()
        {
            if (!hasCOM)
                return;

            closing = true;
            if (serialPort != null && serialPort.IsOpen)
            {
                while (listening)
                    System.Windows.Forms.Application.DoEvents();
                serialPort.Close();
            }
        }
    }
}
