using System;

namespace NanoVNA
{
	public class SerialPortEventArgs : EventArgs
	{
		public bool isOpend;

		public byte[] receivedBytes;
	}
}
