namespace NanoVNA
{
	public interface IView
	{
		void SetController(IController controller);

		void OpenComEvent(object sender, SerialPortEventArgs e);

		void CloseComEvent(object sender, SerialPortEventArgs e);

		void ComReceiveDataEvent(object sender, SerialPortEventArgs e);
	}
}
