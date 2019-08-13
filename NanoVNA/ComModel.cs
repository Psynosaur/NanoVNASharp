using System;
using System.IO.Ports;
using System.Threading;

namespace NanoVNA
{
	public class ComModel
	{
		private SerialPort sp = new SerialPort();

		private object thisLock = new object();

		public event SerialPortEventHandler comReceiveDataEvent;

		public event SerialPortEventHandler comOpenEvent;

		public event SerialPortEventHandler comCloseEvent;

		private void DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			if (sp.BytesToRead > 0)
			{
				lock (thisLock)
				{
					int bytesToRead = sp.BytesToRead;
					byte[] array = new byte[bytesToRead];
					try
					{
						sp.Read(array, 0, bytesToRead);
					}
					catch (Exception)
					{
					}
					SerialPortEventArgs serialPortEventArgs = new SerialPortEventArgs();
					serialPortEventArgs.receivedBytes = array;
					if (this.comReceiveDataEvent != null)
					{
						this.comReceiveDataEvent(this, serialPortEventArgs);
					}
				}
			}
		}

		public bool Send(byte[] bytes)
		{
			if (!sp.IsOpen)
			{
				return false;
			}
			try
			{
				sp.Write(bytes, 0, bytes.Length);
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}

		public void Open(string portName, string baudRate, string dataBits, string stopBits, string parity, string handshake)
		{
			if (sp.IsOpen)
			{
				Close();
			}
			sp.PortName = portName;
			sp.BaudRate = Convert.ToInt32(baudRate);
			sp.DataBits = Convert.ToInt16(dataBits);
			if (handshake == "None")
			{
				sp.RtsEnable = true;
				sp.DtrEnable = true;
			}
			SerialPortEventArgs serialPortEventArgs = new SerialPortEventArgs();
			try
			{
				sp.StopBits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
				sp.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
				sp.Handshake = (Handshake)Enum.Parse(typeof(Handshake), handshake);
				sp.WriteTimeout = 1000;
				sp.Open();
				sp.DataReceived += DataReceived;
				serialPortEventArgs.isOpend = true;
			}
			catch (Exception)
			{
				serialPortEventArgs.isOpend = false;
			}
			if (this.comOpenEvent != null)
			{
				this.comOpenEvent(this, serialPortEventArgs);
			}
		}

		public void Close()
		{
			new Thread(CloseSpThread).Start();
		}

		private void CloseSpThread()
		{
			SerialPortEventArgs serialPortEventArgs = new SerialPortEventArgs();
			serialPortEventArgs.isOpend = false;
			try
			{
				sp.Close();
				sp.DataReceived -= DataReceived;
			}
			catch (Exception)
			{
				serialPortEventArgs.isOpend = true;
			}
			if (this.comCloseEvent != null)
			{
				this.comCloseEvent(this, serialPortEventArgs);
			}
		}
	}
}
