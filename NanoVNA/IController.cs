using System.Text;

namespace NanoVNA
{
	public class IController
	{
		private ComModel comModel = new ComModel();

		private IView view;

		public IController(IView view)
		{
			this.view = view;
			view.SetController(this);
			comModel.comCloseEvent += view.CloseComEvent;
			comModel.comOpenEvent += view.OpenComEvent;
			comModel.comReceiveDataEvent += view.ComReceiveDataEvent;
		}

		public bool SendDataToCom(byte[] data)
		{
			return comModel.Send(data);
		}

		public bool SendDataToCom(string str)
		{
			if (str != null && str != "")
			{
				return comModel.Send(Encoding.Default.GetBytes(str));
			}
			return true;
		}

		public void OpenSerialPort(string portName, string baudRate, string dataBits, string stopBits, string parity, string handshake)
		{
			if (portName != null && portName != "")
			{
				comModel.Open(portName, baudRate, dataBits, stopBits, parity, handshake);
			}
		}

		public void CloseSerialPort()
		{
			comModel.Close();
		}
	}
}
