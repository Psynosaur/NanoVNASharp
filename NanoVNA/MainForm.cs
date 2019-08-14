using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace NanoVNA
{
	public class MainForm : Form, IView
	{
        private IController controller;
        private int receiveBytesCount;
        private double[] mag11 = Enumerable.Repeat(1.0, 101).ToArray();
        private double[] ang11 = Enumerable.Repeat(0.0, 101).ToArray();
        private double[] mag21 = Enumerable.Repeat(1.0, 101).ToArray();
        private double[] ang21 = Enumerable.Repeat(0.0, 101).ToArray();
        private double[] freqs = Enumerable.Repeat(1.0, 101).ToArray();
        private string receiveDate;
        private HitTestResult myTestResult;
        private IContainer components;
        private ComboBox comListCbx;
        private Label label3;
        private Button openCloseSpbtn;
        private System.Windows.Forms.Timer statustimer;
        private System.Windows.Forms.Timer autoRefreshtimer;
        private Button refreshbtn;
        private GroupBox comGroupBox;
        private Chart chart1;
        private Label label8;
        private GroupBox chartGroupBox;
        private CheckBox autoRefreshcbx;
        private GroupBox stimulusGroupBox;
        private Label StartLabel;
        private Label StopLabel;
        private NumericUpDown StopNumericUpDown;
        private NumericUpDown StartNumericUpDown;
        private NumericUpDown SpanNumericUpDown;
        private NumericUpDown CenterNumericUpDown;
        private Label SpanLabel;
        private Label CenterLabel;
        private Label label4;
        private Label label5;
        private Label label2;
        private Label label1;
        private ComboBox formatComboBox;
        private Label label6;
        private GroupBox DataGroupBox;
        private Button getDataButton;
        private NumericUpDown autoRefreshtimerNumericUpDown;
        private Button saveS2pButton;
        private Button saveS1pButton;
        private Button loadSnpFileButton;
        private GroupBox stateGroupBox;
        private Button recallButton;
        private Button saveButton;
        private GroupBox responseGroupBox;
        private Button calButton;
        private GroupBox saveGroupBox;
        private Button save4Button;
        private Button save3Button;
        private Button save2Button;
        private Button save1Button;
        private Button save0Button;
        private GroupBox recallGroupBox;
        private Button Recall4Button;
        private Button Recall3Button;
        private Button Recall2Button;
        private Button Recall1Button;
        private Button Recall0Button;
        private GroupBox calGroupBox;
        private Button throughButton;
        private Button isolationButton;
        private Button loadButton;
        private Button shortButton;
        private Button openButton;
        private Button doneButton;
        private Button recallBackButton;
        private Button saveBackButton;
        private ToolTip chartToolTip;
        private CheckBox YMinCheckBox;
        private CheckBox YMaxCheckBox;
        private NumericUpDown YMinNumericUpDown;
        private NumericUpDown YMaxNumericUpDown;
        private Label YMinLabel;
        private Label YMaxLabel;
        private GroupBox languageGroupBox;
        private Button languageButton;
        private GroupBox aboutBox;
        private Button infoButton;
        private Button AboutButton;

        public MainForm()
		{
			InitializeComponent();
			InitializeCOMCombox();
			base.MaximizeBox = false;
		}

		public void SetController(IController controller)
		{
			this.controller = controller;
		}

		private void InitializeCOMCombox()
		{
			string[] portNames = SerialPort.GetPortNames();
			if (portNames.Length == 0)
			{
				openCloseSpbtn.Enabled = false;
				return;
			}
			Array.Sort(portNames);
			for (int i = 0; i < portNames.Length; i++)
			{
				comListCbx.Items.Add(portNames[i]);
			}
			comListCbx.Text = portNames[portNames.Length - 1];
			openCloseSpbtn.Enabled = true;
		}

		public void OpenComEvent(object sender, SerialPortEventArgs e)
		{
			if (base.InvokeRequired)
			{
				Invoke(new Action<object, SerialPortEventArgs>(OpenComEvent), sender, e);
			}
			else if (e.isOpend)
			{
				if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CN")
				{
					openCloseSpbtn.Text = "断开";
				}
				else
				{
					openCloseSpbtn.Text = "Disconnect";
				}
				autoRefreshcbx.Enabled = true;
				comListCbx.Enabled = false;
				refreshbtn.Enabled = false;
				if (!autoRefreshcbx.Checked)
				{
					getDataButton.Enabled = true;
				}
				saveButton.Enabled = true;
				recallButton.Enabled = true;
				calButton.Enabled = true;
				infoButton.Enabled = true;
				if (autoRefreshcbx.Checked)
				{
					autoRefreshtimer.Start();
					loadSnpFileButton.Enabled = false;
				}
			}
			else
			{
				autoRefreshcbx.Enabled = false;
				getDataButton.Enabled = false;
				saveButton.Enabled = false;
				recallButton.Enabled = false;
				calButton.Enabled = false;
				loadSnpFileButton.Enabled = true;
				infoButton.Enabled = false;
			}
		}

		public void CloseComEvent(object sender, SerialPortEventArgs e)
		{
			if (base.InvokeRequired)
			{
				Invoke(new Action<object, SerialPortEventArgs>(CloseComEvent), sender, e);
			}
			else if (!e.isOpend)
			{
				if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CN")
				{
					openCloseSpbtn.Text = "连接";
				}
				else
				{
					openCloseSpbtn.Text = "Connect";
				}
				autoRefreshcbx.Enabled = false;
				autoRefreshtimer.Stop();
				comListCbx.Enabled = true;
				refreshbtn.Enabled = true;
				getDataButton.Enabled = false;
				saveButton.Enabled = false;
				recallButton.Enabled = false;
				calButton.Enabled = false;
				loadSnpFileButton.Enabled = true;
				infoButton.Enabled = false;
			}
		}

		public void ComReceiveDataEvent(object sender, SerialPortEventArgs e)
		{
			if (base.InvokeRequired)
			{
				try
				{
					Invoke(new Action<object, SerialPortEventArgs>(ComReceiveDataEvent), sender, e);
				}
				catch (Exception)
				{
				}
				return;
			}
			receiveBytesCount += e.receivedBytes.Length;
			receiveDate += Encoding.Default.GetString(e.receivedBytes);
			while (receiveDate.Contains("ch>"))
			{
				if (receiveDate.Contains("frequencies"))
				{
					GetFreqs(receiveDate);
					int startIndex = receiveDate.IndexOf("ch>", receiveDate.IndexOf("frequencies")) + "ch>".Length;
					receiveDate = receiveDate.Substring(startIndex);
				}
				else if (receiveDate.Contains("data"))
				{
					GetData(receiveDate);
					int startIndex2 = receiveDate.IndexOf("ch>", receiveDate.IndexOf("data")) + "ch>".Length;
					receiveDate = receiveDate.Substring(startIndex2);
				}
				else if (receiveDate.Contains("info"))
				{
					GetInfo(receiveDate);
					int startIndex3 = receiveDate.IndexOf("ch>", receiveDate.IndexOf("info")) + "ch>".Length;
					receiveDate = receiveDate.Substring(startIndex3);
				}
				else
				{
					int startIndex4 = receiveDate.IndexOf("ch>") + "ch>".Length;
					receiveDate = receiveDate.Substring(startIndex4);
				}
			}
		}

		private void statustimer_Tick(object sender, EventArgs e)
		{
		}

		private void openCloseSpbtn_Click(object sender, EventArgs e)
		{
			if (openCloseSpbtn.Text == "Connect" || openCloseSpbtn.Text == "连接")
			{
				controller.OpenSerialPort(comListCbx.Text, "921600", "8", "One", "None", "None");
			}
			else
			{
				controller.CloseSerialPort();
			}
		}

		private void refreshbtn_Click(object sender, EventArgs e)
		{
			comListCbx.Items.Clear();
			string[] portNames = SerialPort.GetPortNames();
			if (portNames.Length == 0)
			{
				openCloseSpbtn.Enabled = false;
				return;
			}
			Array.Sort(portNames);
			for (int i = 0; i < portNames.Length; i++)
			{
				comListCbx.Items.Add(portNames[i]);
			}
			comListCbx.Text = portNames[portNames.Length - 1];
			openCloseSpbtn.Enabled = true;
		}

		private void autoRefreshcbx_CheckedChanged(object sender, EventArgs e)
		{
			if (autoRefreshcbx.Checked)
			{
				autoRefreshtimer.Enabled = true;
				autoRefreshtimer.Interval = (int)autoRefreshtimerNumericUpDown.Value;
				autoRefreshtimer.Start();
				autoRefreshtimerNumericUpDown.Enabled = false;
				getDataButton.Enabled = false;
				loadSnpFileButton.Enabled = false;
			}
			else
			{
				autoRefreshtimer.Enabled = false;
				autoRefreshtimer.Stop();
				autoRefreshtimerNumericUpDown.Enabled = true;
				getDataButton.Enabled = true;
				loadSnpFileButton.Enabled = true;
			}
		}

		private void autoSendtimer_Tick(object sender, EventArgs e)
		{
			controller.SendDataToCom("frequencies\r\ndata 0\r\ndata 1\r\n");
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
		}

		private void GetData(string data)
		{
			double num = 1.0;
			double num2 = 0.0;
			double num3 = 1.0;
			double num4 = 0.0;
			int num5 = data.IndexOf("data ") + "data ".Length;
			int num6 = data.IndexOf("ch>", num5);
			if (num6 - num5 > 500)
			{
				data = data.Substring(num5, num6 - num5);
				string[] array = data.Split(' ', '\n');
				if (array.Length == 204)
				{
					try
					{
						int num7 = Convert.ToInt32(array[0]);
						for (int i = 0; i < 101; i++)
						{
							try
							{
								num = Convert.ToDouble(array[i * 2 + 1], CultureInfo.InvariantCulture);
							}
							catch (Exception)
							{
							}
							try
							{
								num2 = Convert.ToDouble(array[i * 2 + 2], CultureInfo.InvariantCulture);
							}
							catch (Exception)
							{
							}
							if (Math.Abs(num) < 100.0)
							{
								num3 = num;
							}
							else
							{
								num = num3;
							}
							if (Math.Abs(num2) < 100.0)
							{
								num4 = num2;
							}
							else
							{
								num2 = num4;
							}
							switch (num7)
							{
							default:
								return;
							case 0:
								mag11[i] = Math.Sqrt(num * num + num2 * num2);
								ang11[i] = 180.0 / Math.PI * Math.Atan2(num2, num);
								break;
							case 1:
								mag21[i] = Math.Sqrt(num * num + num2 * num2);
								ang21[i] = 180.0 / Math.PI * Math.Atan2(num2, num);
								break;
							}
						}
						UpdatePlot();
						saveS1pButton.Enabled = true;
						saveS2pButton.Enabled = true;
					}
					catch (Exception)
					{
					}
				}
			}
		}

		private void GetFreqs(string data)
		{
			double num = 50000.0;
			int num2 = data.IndexOf("frequencies\r\n") + "frequencies\r\n".Length;
			int num3 = data.IndexOf("ch>", num2);
			if (num3 - num2 > 500)
			{
				data = data.Substring(num2, num3 - num2);
				string[] array = data.Split(' ', '\n');
				if (array.Length == 102)
				{
					try
					{
						for (int i = 0; i < 101; i++)
						{
							try
							{
								num = Convert.ToDouble(array[i], CultureInfo.InvariantCulture);
							}
							catch (Exception)
							{
							}
							if (i > 0 && num >= 50000.0)
							{
								freqs[i] = num;
							}
							else
							{
								if (i != 0)
								{
									return;
								}
								if (num > 50000.0)
								{
									freqs[i] = num;
								}
								else
								{
									freqs[i] = 50000.0;
								}
							}
						}
						decimal num4 = (decimal)freqs[0] / 1000000m;
						decimal num5 = (decimal)freqs[100] / 1000000m;
						decimal num6 = ((decimal)freqs[0] + (decimal)freqs[100]) / 2m / 1000000m;
						decimal num7 = ((decimal)freqs[100] - (decimal)freqs[0]) / 1000000m;
						if (Math.Abs(CenterNumericUpDown.Value - num6) > decimal.Zero && !CenterNumericUpDown.Focused)
						{
							CenterNumericUpDown.Value = num6;
						}
						if (Math.Abs(SpanNumericUpDown.Value - num7) > decimal.Zero && !SpanNumericUpDown.Focused)
						{
							SpanNumericUpDown.Value = num7;
						}
						if (Math.Abs(StartNumericUpDown.Value - num4) > decimal.Zero && !StartNumericUpDown.Focused)
						{
							StartNumericUpDown.Value = num4;
						}
						if (Math.Abs(StopNumericUpDown.Value - num5) > decimal.Zero && !StopNumericUpDown.Focused)
						{
							StopNumericUpDown.Value = num5;
						}
					}
					catch (Exception)
					{
					}
				}
			}
		}

		private void GetInfo(string data)
		{
			int num = data.IndexOf("info\r\n") + "info\r\n".Length;
			int num2 = data.IndexOf("ch>", num);
			data = data.Substring(num, num2 - num);
			MessageBox.Show(data, "Firmware information");
		}

		private void StartNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			controller.SendDataToCom("sweep start " + (StartNumericUpDown.Value * 1000000m).ToString("f0", CultureInfo.InvariantCulture) + "\r\n");
		}

		private void StopNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			controller.SendDataToCom("sweep stop " + (StopNumericUpDown.Value * 1000000m).ToString("f0", CultureInfo.InvariantCulture) + "\r\n");
		}

		private void CenterNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			controller.SendDataToCom("sweep center " + (CenterNumericUpDown.Value * 1000000m).ToString("f0", CultureInfo.InvariantCulture) + "\r\n");
		}

		private void SpanNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			controller.SendDataToCom("sweep span " + (SpanNumericUpDown.Value * 1000000m).ToString("f0", CultureInfo.InvariantCulture) + "\r\n");
		}

		private void formatComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			YMaxNumericUpDown.Enabled = true;
			YMinNumericUpDown.Enabled = true;
			YMaxNumericUpDown.Maximum = decimal.MaxValue;
			YMaxNumericUpDown.Minimum = decimal.MinValue;
			YMinNumericUpDown.Maximum = decimal.MaxValue;
			YMinNumericUpDown.Minimum = decimal.MinValue;
			string text = formatComboBox.Text;
			if (text == null)
			{
				goto IL_0fda;
			}
			switch (text)
			{
			case "Linear S11":
			case "Linear S21":
			case "Linear S11&S21":
				break;
			case "Phase S11":
			case "Phase S21":
			case "Phase S11&S21":
				goto IL_047b;
			case "SWR S11":
				goto IL_06ab;
			case "Smith S11":
			case "Admittance S11":
				goto IL_08cc;
			case "Polar S11":
				goto IL_0b5f;
			case "GroupDelay S11":
			case "GroupDelay S21":
			case "GroupDelay S11&S21":
				goto IL_0d7d;
			default:
				goto IL_0fda;
			}
			YMaxCheckBox.Enabled = true;
			YMinCheckBox.Enabled = true;
			YMaxCheckBox.Checked = false;
			YMinCheckBox.Checked = false;
			chart1.ChartAreas[0].AxisY.Title = "Magnitude |S|";
			chart1.Series[0].ChartType = SeriesChartType.Line;
			chart1.Series[1].ChartType = SeriesChartType.Line;
			chart1.ChartAreas[0].AxisY.Minimum = 0.0;
			chart1.ChartAreas[0].AxisY.Maximum = 1.0;
			YMaxNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Maximum;
			YMinNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Minimum;
			YMaxNumericUpDown.Maximum = 1000m;
			YMaxNumericUpDown.Minimum = decimal.One;
			YMinNumericUpDown.Maximum = 500m;
			YMinNumericUpDown.Minimum = decimal.Zero;
			chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = true;
			chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
			chart1.ChartAreas[0].AxisY.LineWidth = 1;
			chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = true;
			chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
			chart1.ChartAreas[0].BackImage = "";
			goto IL_11fc;
			IL_11fc:
			UpdatePlot();
			return;
			IL_0d7d:
			YMaxCheckBox.Enabled = true;
			YMinCheckBox.Enabled = true;
			YMaxCheckBox.Checked = true;
			YMinCheckBox.Checked = false;
			YMaxNumericUpDown.Enabled = false;
			chart1.ChartAreas[0].AxisY.Title = "GroupDelay:ns";
			chart1.Series[0].ChartType = SeriesChartType.Line;
			chart1.Series[1].ChartType = SeriesChartType.Line;
			chart1.ChartAreas[0].AxisY.Minimum = 0.0;
			chart1.ChartAreas[0].AxisY.Maximum = 100.0;
			YMaxNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Maximum;
			YMinNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Minimum;
			chart1.ChartAreas[0].AxisY.Maximum = double.NaN;
			YMaxNumericUpDown.Maximum = 100000m;
			YMaxNumericUpDown.Minimum = -100m;
			YMinNumericUpDown.Maximum = 90000m;
			YMinNumericUpDown.Minimum = -180m;
			chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = true;
			chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
			chart1.ChartAreas[0].AxisY.LineWidth = 1;
			chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = true;
			chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
			chart1.ChartAreas[0].BackImage = "";
			goto IL_11fc;
			IL_0b5f:
			chart1.ChartAreas[0].AxisY.Title = "Polar";
			chart1.Series[0].ChartType = SeriesChartType.Polar;
			chart1.Series[1].ChartType = SeriesChartType.Polar;
			chart1.ChartAreas[0].AxisY.Minimum = 0.0;
			chart1.ChartAreas[0].AxisY.Maximum = 1.0;
			YMaxNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Maximum;
			YMinNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Minimum;
			YMaxNumericUpDown.Maximum = decimal.One;
			YMaxNumericUpDown.Minimum = decimal.One;
			YMinNumericUpDown.Maximum = decimal.Zero;
			YMinNumericUpDown.Minimum = decimal.Zero;
			chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
			chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
			chart1.ChartAreas[0].AxisY.LineWidth = 0;
			chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
			chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
			chart1.ChartAreas[0].BackImage = "";
			YMaxCheckBox.Enabled = false;
			YMinCheckBox.Enabled = false;
			YMaxNumericUpDown.Enabled = false;
			YMinNumericUpDown.Enabled = false;
			goto IL_11fc;
			IL_08cc:
			chart1.Series[0].ChartType = SeriesChartType.Polar;
			chart1.Series[1].ChartType = SeriesChartType.Polar;
			chart1.ChartAreas[0].AxisY.Minimum = 0.0;
			chart1.ChartAreas[0].AxisY.Maximum = 1.0;
			YMaxNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Maximum;
			YMinNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Minimum;
			YMaxNumericUpDown.Maximum = decimal.One;
			YMaxNumericUpDown.Minimum = decimal.One;
			YMinNumericUpDown.Maximum = decimal.Zero;
			YMinNumericUpDown.Minimum = decimal.Zero;
			chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
			chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
			chart1.ChartAreas[0].AxisY.LineWidth = 0;
			chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = false;
			chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
			YMaxCheckBox.Enabled = false;
			YMinCheckBox.Enabled = false;
			YMaxNumericUpDown.Enabled = false;
			YMinNumericUpDown.Enabled = false;
			if (formatComboBox.Text == "Admittance S11")
			{
				chart1.ChartAreas[0].AxisY.Title = "Admittance Smith";
				if (File.Exists("Admittance.png"))
				{
					chart1.ChartAreas[0].BackImage = "Admittance.png";
				}
			}
			else
			{
				chart1.ChartAreas[0].AxisY.Title = "Impedance Smith";
				if (File.Exists("Smith.png"))
				{
					chart1.ChartAreas[0].BackImage = "Smith.png";
				}
			}
			goto IL_11fc;
			IL_047b:
			YMaxCheckBox.Enabled = true;
			YMinCheckBox.Enabled = true;
			YMaxCheckBox.Checked = false;
			YMinCheckBox.Checked = false;
			chart1.ChartAreas[0].AxisY.Title = "Phase:deg";
			chart1.Series[0].ChartType = SeriesChartType.Line;
			chart1.Series[1].ChartType = SeriesChartType.Line;
			chart1.ChartAreas[0].AxisY.Minimum = -180.0;
			chart1.ChartAreas[0].AxisY.Maximum = 180.0;
			YMaxNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Maximum;
			YMinNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Minimum;
			YMaxNumericUpDown.Maximum = 180m;
			YMaxNumericUpDown.Minimum = -179m;
			YMinNumericUpDown.Maximum = 179m;
			YMinNumericUpDown.Minimum = -180m;
			chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = true;
			chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
			chart1.ChartAreas[0].AxisY.LineWidth = 1;
			chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = true;
			chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
			chart1.ChartAreas[0].BackImage = "";
			goto IL_11fc;
			IL_0fda:
			YMaxCheckBox.Enabled = true;
			YMinCheckBox.Enabled = true;
			YMaxCheckBox.Checked = false;
			YMinCheckBox.Checked = false;
			chart1.ChartAreas[0].AxisY.Title = "Magnitude:dB";
			chart1.Series[0].ChartType = SeriesChartType.Line;
			chart1.Series[1].ChartType = SeriesChartType.Line;
			chart1.ChartAreas[0].AxisY.Minimum = -70.0;
			chart1.ChartAreas[0].AxisY.Maximum = 0.0;
			YMaxNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Maximum;
			YMinNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Minimum;
			YMaxNumericUpDown.Maximum = 80m;
			YMaxNumericUpDown.Minimum = -100m;
			YMinNumericUpDown.Maximum = 60m;
			YMinNumericUpDown.Minimum = -180m;
			chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = true;
			chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
			chart1.ChartAreas[0].AxisY.LineWidth = 1;
			chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = true;
			chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
			chart1.ChartAreas[0].BackImage = "";
			goto IL_11fc;
			IL_06ab:
			YMaxCheckBox.Enabled = true;
			YMinCheckBox.Enabled = true;
			YMaxCheckBox.Checked = false;
			YMinCheckBox.Checked = false;
			chart1.ChartAreas[0].AxisY.Title = "SWR";
			chart1.Series[0].ChartType = SeriesChartType.Line;
			chart1.Series[1].ChartType = SeriesChartType.Line;
			chart1.ChartAreas[0].AxisY.Minimum = 1.0;
			chart1.ChartAreas[0].AxisY.Maximum = 20.0;
			YMaxNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Maximum;
			YMinNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Minimum;
			YMaxNumericUpDown.Maximum = 100m;
			YMaxNumericUpDown.Minimum = 2m;
			YMinNumericUpDown.Maximum = 20m;
			YMinNumericUpDown.Minimum = decimal.One;
			chart1.ChartAreas[0].AxisX.LabelStyle.Enabled = true;
			chart1.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
			chart1.ChartAreas[0].AxisY.LineWidth = 1;
			chart1.ChartAreas[0].AxisY.LabelStyle.Enabled = true;
			chart1.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
			chart1.ChartAreas[0].BackImage = "";
			goto IL_11fc;
		}

		private void getDataButton_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("frequencies\r\ndata 0\r\ndata 1\r\n");
		}

		public void UpdatePlot()
		{
			chart1.Series[0].Points.Clear();
			chart1.Series[1].Points.Clear();
			if (!(freqs[0] > 5000.0))
			{
				return;
			}
			string text = formatComboBox.Text;
			if (text != null)
			{
				switch (text)
				{
				case "Logmag S11":
					for (int num8 = 0; num8 < 101; num8++)
					{
						chart1.Series[0].Points.AddXY(freqs[num8] / 1000000.0, 20.0 * Math.Log10(mag11[num8]));
					}
					return;
				case "Logmag S21":
					for (int j = 0; j < 101; j++)
					{
						chart1.Series[1].Points.AddXY(freqs[j] / 1000000.0, 20.0 * Math.Log10(mag21[j]));
					}
					return;
				case "Linear S11":
					for (int n = 0; n < 101; n++)
					{
						chart1.Series[0].Points.AddXY(freqs[n] / 1000000.0, mag11[n]);
					}
					return;
				case "Linear S21":
					for (int num10 = 0; num10 < 101; num10++)
					{
						chart1.Series[1].Points.AddXY(freqs[num10] / 1000000.0, mag21[num10]);
					}
					return;
				case "Linear S11&S21":
					for (int num6 = 0; num6 < 101; num6++)
					{
						chart1.Series[0].Points.AddXY(freqs[num6] / 1000000.0, mag11[num6]);
						chart1.Series[1].Points.AddXY(freqs[num6] / 1000000.0, mag21[num6]);
					}
					return;
				case "Phase S11":
					for (int l = 0; l < 101; l++)
					{
						chart1.Series[0].Points.AddXY(freqs[l] / 1000000.0, ang11[l]);
					}
					return;
				case "Phase S21":
					for (int num11 = 0; num11 < 101; num11++)
					{
						chart1.Series[1].Points.AddXY(freqs[num11] / 1000000.0, ang21[num11]);
					}
					return;
				case "Phase S11&S21":
					for (int num9 = 0; num9 < 101; num9++)
					{
						chart1.Series[0].Points.AddXY(freqs[num9] / 1000000.0, ang11[num9]);
						chart1.Series[1].Points.AddXY(freqs[num9] / 1000000.0, ang21[num9]);
					}
					return;
				case "SWR S11":
					for (int num7 = 0; num7 < 101; num7++)
					{
						if (mag11[num7] > 0.99)
						{
							chart1.Series[0].Points.AddXY(freqs[num7] / 1000000.0, 198.99999999999983);
						}
						else
						{
							chart1.Series[0].Points.AddXY(freqs[num7] / 1000000.0, (1.0 + mag11[num7]) / (1.0 - mag11[num7]));
						}
					}
					return;
				case "Smith S11":
				case "Admittance S11":
				case "Polar S11":
					for (int num5 = 0; num5 < 101; num5++)
					{
						chart1.Series[0].Points.AddXY(90.0 - ang11[num5], mag11[num5]);
					}
					return;
				case "GroupDelay S11":
					for (int m = 1; m < 101; m++)
					{
						double num4 = ang11[m - 1] - ang11[m];
						if (num4 >= 180.0)
						{
							num4 -= 360.0;
						}
						else if (num4 < -180.0)
						{
							num4 += 360.0;
						}
						num4 /= (freqs[m] - freqs[m - 1]) / 1000000000.0 * 360.0;
						chart1.Series[0].Points.AddXY(freqs[m] / 1000000.0, num4);
					}
					return;
				case "GroupDelay S21":
					for (int k = 1; k < 101; k++)
					{
						double num3 = ang21[k - 1] - ang21[k];
						if (num3 >= 180.0)
						{
							num3 -= 360.0;
						}
						else if (num3 < -180.0)
						{
							num3 += 360.0;
						}
						num3 /= (freqs[k] - freqs[k - 1]) / 1000000000.0 * 360.0;
						chart1.Series[1].Points.AddXY(freqs[k] / 1000000.0, num3);
					}
					return;
				case "GroupDelay S11&S21":
					for (int i = 1; i < 101; i++)
					{
						if (freqs[i] - freqs[i - 1] > 0.0)
						{
							double num = ang11[i - 1] - ang11[i];
							double num2 = ang21[i - 1] - ang21[i];
							if (num >= 180.0)
							{
								num -= 360.0;
							}
							else if (num < -180.0)
							{
								num += 360.0;
							}
							num /= (freqs[i] - freqs[i - 1]) / 1000000000.0 * 360.0;
							chart1.Series[0].Points.AddXY(freqs[i] / 1000000.0, num);
							if (num2 >= 180.0)
							{
								num2 -= 360.0;
							}
							else if (num2 < -180.0)
							{
								num2 += 360.0;
							}
							num2 /= (freqs[i] - freqs[i - 1]) / 1000000000.0 * 360.0;
							chart1.Series[1].Points.AddXY(freqs[i] / 1000000.0, num2);
						}
					}
					return;
				}
			}
			for (int num12 = 0; num12 < 101; num12++)
			{
				chart1.Series[0].Points.AddXY(freqs[num12] / 1000000.0, 20.0 * Math.Log10(mag11[num12]));
				chart1.Series[1].Points.AddXY(freqs[num12] / 1000000.0, 20.0 * Math.Log10(mag21[num12]));
			}
		}

		private void saveS1pButton_Click(object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "s1p files (*.s1p)|*.s1p";
			saveFileDialog.Title = "";
			saveFileDialog.ShowDialog();
			string fileName = saveFileDialog.FileName;
			if (!(fileName == ""))
			{
				try
				{
					StreamWriter streamWriter;
					using (streamWriter = new StreamWriter(fileName))
					{
						streamWriter.WriteLine("! File created by NanoVNA (https://gen111.taobao.com/)");
						streamWriter.WriteLine("! Start " + freqs[0] + "Hz  Stop " + freqs[100] + "Hz   Points 101");
						streamWriter.WriteLine("# HZ S MA R 50");
						streamWriter.WriteLine("!S - Parameter data, F S11");
						int num = 0;
						double[] array = freqs;
						for (int i = 0; i < array.Length; i++)
						{
							double num2 = array[i];
							streamWriter.WriteLine(freqs[num].ToString("F0", CultureInfo.InvariantCulture) + "\t" + mag11[num].ToString("F9", CultureInfo.InvariantCulture) + "\t" + ang11[num].ToString("F9", CultureInfo.InvariantCulture));
							num++;
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}

		private void saveS2pButton_Click(object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "s2p files (*.s2p)|*.s2p";
			saveFileDialog.Title = "";
			saveFileDialog.ShowDialog();
			string fileName = saveFileDialog.FileName;
			if (!(fileName == ""))
			{
				try
				{
					StreamWriter streamWriter;
					using (streamWriter = new StreamWriter(fileName))
					{
						streamWriter.WriteLine("! File created by NanoVNA (https://gen111.taobao.com/)");
						streamWriter.WriteLine("! Start " + freqs[0] + "Hz  Stop " + freqs[100] + "Hz   Points 101");
						streamWriter.WriteLine("# HZ S MA R 50");
						streamWriter.WriteLine("! S - Parameter data, F S11 S21 (S12 and S22 are invalid)");
						int num = 0;
						double[] array = freqs;
						for (int i = 0; i < array.Length; i++)
						{
							double num2 = array[i];
							streamWriter.WriteLine(freqs[num].ToString("F0", CultureInfo.InvariantCulture) + "\t" + mag11[num].ToString("F9", CultureInfo.InvariantCulture) + "\t" + ang11[num].ToString("F9", CultureInfo.InvariantCulture) + "\t" + mag21[num].ToString("F9", CultureInfo.InvariantCulture) + "\t" + ang21[num].ToString("F9", CultureInfo.InvariantCulture) + "\t1.0\t0.0\t1.0\t0.0");
							num++;
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}

		private void loadSnpFileButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "snp files (*.s1p;*.s2p)|*.s1p;*.s2p"
			};
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					using (StreamReader streamReader = new StreamReader(openFileDialog.FileName))
					{
						string a = "RI";
						double num = 1000000000.0;
						double num2 = 0.0;
						List<string> list = new List<string>();
						string text;
						string[] value;
						while ((text = streamReader.ReadLine()) != null)
						{
							value = text.ToUpper().Split(new char[2]
							{
								' ',
								'\t'
							}, StringSplitOptions.RemoveEmptyEntries);
							text = string.Join(" ", value);
							if (!text.StartsWith("#"))
							{
								if (!text.StartsWith("!") && text.Length > 3)
								{
									list.Add(text);
								}
								continue;
							}
							text = text.Substring(1);
							text = text.TrimStart();
							value = text.Split(' ');
							string text2 = value[0];
							if (text2 == null)
							{
								goto IL_0261;
							}
							switch (text2)
							{
							case "HZ":
							case "S":
								break;
							case "KHZ":
							case "MS":
								goto IL_022d;
							case "MHZ":
							case "US":
								goto IL_023a;
							case "GHZ":
							case "NS":
								goto IL_0247;
							case "THZ":
							case "PS":
								goto IL_0254;
							default:
								goto IL_0261;
							}
							num = 1.0;
							goto IL_026c;
							IL_022d:
							num = 1000.0;
							goto IL_026c;
							IL_0261:
							num = 1000000000.0;
							goto IL_026c;
							IL_026c:
							if (value[1] == "S" && value[2] == "RI" && value[3] == "R" && value[4] == "50")
							{
								a = "RI";
							}
							else if (value[1] == "S" && value[2] == "MA" && value[3] == "R" && value[4] == "50")
							{
								a = "MA";
							}
							else
							{
								if (!(value[1] == "S") || !(value[2] == "DB") || !(value[3] == "R") || !(value[4] == "50"))
								{
									MessageBox.Show("Unsupported files!", "ERROR");
									return;
								}
								a = "DB";
							}
							continue;
							IL_0254:
							num = 1000000000000.0;
							goto IL_026c;
							IL_0247:
							num = 1000000000.0;
							goto IL_026c;
							IL_023a:
							num = 1000000.0;
							goto IL_026c;
						}
						num2 = list.Count;
						value = list[0].Split(' ', '\t');
						if (value.Length < 4)
						{
							for (int i = 0; i < 101; i++)
							{
								value = list[(int)((double)i * (num2 - 1.0) / 100.0)].Split(' ', '\t');
								freqs[i] = Convert.ToDouble(value[0], CultureInfo.InvariantCulture) * num;
								if (a == "MA")
								{
									mag11[i] = Convert.ToDouble(value[1], CultureInfo.InvariantCulture);
									ang11[i] = Convert.ToDouble(value[2], CultureInfo.InvariantCulture);
								}
								else if (a == "DB")
								{
									mag11[i] = Math.Pow(10.0, Convert.ToDouble(value[1], CultureInfo.InvariantCulture) / 20.0);
									ang11[i] = Convert.ToDouble(value[2], CultureInfo.InvariantCulture);
								}
								else
								{
									mag11[i] = Math.Sqrt(Convert.ToDouble(value[1], CultureInfo.InvariantCulture) * Convert.ToDouble(value[1], CultureInfo.InvariantCulture) + Convert.ToDouble(value[2], CultureInfo.InvariantCulture) * Convert.ToDouble(value[2], CultureInfo.InvariantCulture));
									ang11[i] = 180.0 / Math.PI * Math.Atan2(Convert.ToDouble(value[2], CultureInfo.InvariantCulture), Convert.ToDouble(value[1], CultureInfo.InvariantCulture));
								}
								mag21[i] = 1.0;
								ang21[i] = 0.0;
							}
						}
						else
						{
							for (int j = 0; j < 101; j++)
							{
								value = list[(int)((double)j * (num2 - 1.0) / 100.0)].Split(' ', '\t');
								freqs[j] = Convert.ToDouble(value[0], CultureInfo.InvariantCulture) * num;
								if (a == "MA")
								{
									mag11[j] = Convert.ToDouble(value[1], CultureInfo.InvariantCulture);
									ang11[j] = Convert.ToDouble(value[2], CultureInfo.InvariantCulture);
									mag21[j] = Convert.ToDouble(value[3], CultureInfo.InvariantCulture);
									ang21[j] = Convert.ToDouble(value[4], CultureInfo.InvariantCulture);
								}
								else if (a == "DB")
								{
									mag11[j] = Math.Pow(10.0, Convert.ToDouble(value[1], CultureInfo.InvariantCulture) / 20.0);
									ang11[j] = Convert.ToDouble(value[2], CultureInfo.InvariantCulture);
									mag21[j] = Math.Pow(10.0, Convert.ToDouble(value[3], CultureInfo.InvariantCulture) / 20.0);
									ang21[j] = Convert.ToDouble(value[4], CultureInfo.InvariantCulture);
								}
								else
								{
									mag11[j] = Math.Sqrt(Convert.ToDouble(value[1], CultureInfo.InvariantCulture) * Convert.ToDouble(value[1], CultureInfo.InvariantCulture) + Convert.ToDouble(value[2], CultureInfo.InvariantCulture) * Convert.ToDouble(value[2], CultureInfo.InvariantCulture));
									ang11[j] = 180.0 / Math.PI * Math.Atan2(Convert.ToDouble(value[2], CultureInfo.InvariantCulture), Convert.ToDouble(value[1], CultureInfo.InvariantCulture));
									mag21[j] = Math.Sqrt(Convert.ToDouble(value[3], CultureInfo.InvariantCulture) * Convert.ToDouble(value[3], CultureInfo.InvariantCulture) + Convert.ToDouble(value[4]) * Convert.ToDouble(value[4], CultureInfo.InvariantCulture));
									ang21[j] = 180.0 / Math.PI * Math.Atan2(Convert.ToDouble(value[4], CultureInfo.InvariantCulture), Convert.ToDouble(value[3], CultureInfo.InvariantCulture));
								}
							}
						}
					}
					UpdatePlot();
				}
				catch (Exception)
				{
					throw;
				}
			}
		}

		private void saveButton_Click(object sender, EventArgs e)
		{
			saveGroupBox.Location = stateGroupBox.Location;
			stateGroupBox.Visible = false;
			responseGroupBox.Visible = false;
			languageGroupBox.Visible = false;
			saveGroupBox.Visible = true;
		}

		private void recallButton_Click(object sender, EventArgs e)
		{
			recallGroupBox.Location = stateGroupBox.Location;
			stateGroupBox.Visible = false;
			responseGroupBox.Visible = false;
			languageGroupBox.Visible = false;
			recallGroupBox.Visible = true;
		}

		private void calButton_Click(object sender, EventArgs e)
		{
			calGroupBox.Location = stateGroupBox.Location;
			stateGroupBox.Visible = false;
			responseGroupBox.Visible = false;
			languageGroupBox.Visible = false;
			calGroupBox.Visible = true;
		}

		private void save0Button_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("save 0\r\n");
			saveGroupBox.Visible = false;
			stateGroupBox.Visible = true;
			responseGroupBox.Visible = true;
			languageGroupBox.Visible = true;
		}

		private void save1Button_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("save 1\r\n");
			saveGroupBox.Visible = false;
			stateGroupBox.Visible = true;
			responseGroupBox.Visible = true;
			languageGroupBox.Visible = true;
		}

		private void save2Button_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("save 2\r\n");
			saveGroupBox.Visible = false;
			stateGroupBox.Visible = true;
			responseGroupBox.Visible = true;
			languageGroupBox.Visible = true;
		}

		private void save3Button_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("save 3\r\n");
			saveGroupBox.Visible = false;
			stateGroupBox.Visible = true;
			responseGroupBox.Visible = true;
			languageGroupBox.Visible = true;
		}

		private void save4Button_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("save 4\r\n");
			saveGroupBox.Visible = false;
			stateGroupBox.Visible = true;
			responseGroupBox.Visible = true;
			languageGroupBox.Visible = true;
		}

		private void saveBackButton_Click(object sender, EventArgs e)
		{
			saveGroupBox.Visible = false;
			stateGroupBox.Visible = true;
			responseGroupBox.Visible = true;
			languageGroupBox.Visible = true;
		}

		private void Recall0Button_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("recall 0\r\n");
			recallGroupBox.Visible = false;
			stateGroupBox.Visible = true;
			responseGroupBox.Visible = true;
			languageGroupBox.Visible = true;
		}

		private void Recall1Button_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("recall 1\r\n");
			recallGroupBox.Visible = false;
			stateGroupBox.Visible = true;
			responseGroupBox.Visible = true;
			languageGroupBox.Visible = true;
		}

		private void Recall2Button_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("recall 2\r\n");
			recallGroupBox.Visible = false;
			stateGroupBox.Visible = true;
			responseGroupBox.Visible = true;
			languageGroupBox.Visible = true;
		}

		private void Recall3Button_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("recall 3\r\n");
			recallGroupBox.Visible = false;
			stateGroupBox.Visible = true;
			responseGroupBox.Visible = true;
			languageGroupBox.Visible = true;
		}

		private void Recall4Button_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("recall 4\r\n");
			recallGroupBox.Visible = false;
			stateGroupBox.Visible = true;
			responseGroupBox.Visible = true;
			languageGroupBox.Visible = true;
		}

		private void recallBackButton_Click(object sender, EventArgs e)
		{
			recallGroupBox.Visible = false;
			stateGroupBox.Visible = true;
			responseGroupBox.Visible = true;
			languageGroupBox.Visible = true;
		}

		private void openButton_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("cal open\r\n");
			openButton.Font = new Font(openButton.Font, FontStyle.Underline);
		}

		private void shortButton_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("cal reset\r\n");
			controller.SendDataToCom("cal short\r\n");
			shortButton.Font = new Font(shortButton.Font, FontStyle.Underline);
		}

		private void loadButton_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("cal load\r\n");
			loadButton.Font = new Font(loadButton.Font, FontStyle.Underline);
		}

		private void isolationButton_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("cal isoln\r\n");
			isolationButton.Font = new Font(isolationButton.Font, FontStyle.Underline);
		}

		private void throughButton_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("cal thru\r\n");
			throughButton.Font = new Font(throughButton.Font, FontStyle.Underline);
		}

		private void doneButton_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("cal done\r\n");
			openButton.Font = new Font("宋体", 9f);
			shortButton.Font = new Font("宋体", 9f);
			loadButton.Font = new Font("宋体", 9f);
			isolationButton.Font = new Font("宋体", 9f);
			throughButton.Font = new Font("宋体", 9f);
			saveGroupBox.Location = stateGroupBox.Location;
			calGroupBox.Visible = false;
			saveGroupBox.Visible = true;
		}

		private void chart1_MouseClick(object sender, MouseEventArgs e)
		{
			chart1.ChartAreas[0].AxisX.PixelPositionToValue(e.X);
			Series series = chart1.Series[0];
		}

		private void chart1_MouseMove(object sender, MouseEventArgs e)
		{
			if (myTestResult == chart1.HitTest(e.X, e.Y))
			{
				return;
			}
			myTestResult = chart1.HitTest(e.X, e.Y);
			if (myTestResult.ChartElementType == ChartElementType.DataPoint)
			{
				string text = formatComboBox.Text;
				if (text == null)
				{
					return;
				}
				switch (text)
				{
				case "Smith S11":
				case "Admittance S11":
				{
					double num = (1.0 - Math.Pow(mag11[myTestResult.PointIndex] * Math.Cos(ang11[myTestResult.PointIndex] / 180.0 * Math.PI), 2.0) - Math.Pow(mag11[myTestResult.PointIndex] * Math.Sin(ang11[myTestResult.PointIndex] / 180.0 * Math.PI), 2.0)) / (Math.Pow(1.0 - mag11[myTestResult.PointIndex] * Math.Cos(ang11[myTestResult.PointIndex] / 180.0 * Math.PI), 2.0) + Math.Pow(mag11[myTestResult.PointIndex] * Math.Sin(ang11[myTestResult.PointIndex] / 180.0 * Math.PI), 2.0)) * 50.0;
					double num2 = 2.0 * mag11[myTestResult.PointIndex] * Math.Sin(ang11[myTestResult.PointIndex] / 180.0 * Math.PI) / (Math.Pow(1.0 - mag11[myTestResult.PointIndex] * Math.Cos(ang11[myTestResult.PointIndex] / 180.0 * Math.PI), 2.0) + Math.Pow(mag11[myTestResult.PointIndex] * Math.Sin(ang11[myTestResult.PointIndex] / 180.0 * Math.PI), 2.0)) * 50.0;
					double num3 = Math.Sqrt(Math.Pow(num, 2.0) + Math.Pow(num2, 2.0));
					double num4 = freqs[myTestResult.PointIndex] / 1000000.0;
					if (formatComboBox.Text == "Admittance S11")
					{
						double num5 = num / (num * num + num2 * num2) * 1000.0;
						double num6 = (0.0 - num2 / (num * num + num2 * num2)) * 1000.0;
						double num7 = 1.0 / num3 * 1000.0;
						chartToolTip.Show("Frequency：" + num4 + " MHz\r\nConductance：" + num5.ToString("F4", CultureInfo.InvariantCulture) + " millisiemens\r\nSusceptance：" + num6.ToString("F4", CultureInfo.InvariantCulture) + " millisiemens\r\nAdmittance：" + num7.ToString("F4", CultureInfo.InvariantCulture) + " millisiemens", chart1, e.X + 10, e.Y + 10);
						break;
					}
					string text2;
					string text3;
					double num8;
					if (num2 >= 0.0)
					{
						text2 = "Inductive: ";
						text3 = " nH";
						num8 = 1000.0 * num2 / (Math.PI * 2.0 * num4);
					}
					else
					{
						text2 = "Capacitive: ";
						text3 = " pF";
						num8 = 1000000.0 / (Math.PI * 2.0 * num4 * Math.Abs(num2));
					}
					chartToolTip.Show("Frequency：" + num4 + " MHz\r\nResistance：" + num.ToString("F4", CultureInfo.InvariantCulture) + " ohms\r\nReactance：" + num2.ToString("F4", CultureInfo.InvariantCulture) + " ohms\r\nImpedance：" + num3.ToString("F4", CultureInfo.InvariantCulture) + " ohms\r\n" + text2 + num8.ToString("F4", CultureInfo.InvariantCulture) + text3, chart1, e.X + 10, e.Y + 10);
					break;
				}
				case "Polar S11":
					chartToolTip.Show("Frequency：" + freqs[myTestResult.PointIndex] / 1000000.0 + " MHz\r\nLinMag：" + myTestResult.Series.Points[myTestResult.PointIndex].YValues[0].ToString("F4", CultureInfo.InvariantCulture) + "\r\nPhase：" + (90.0 - myTestResult.Series.Points[myTestResult.PointIndex].XValue).ToString("F4", CultureInfo.InvariantCulture) + " deg", chart1, e.X + 10, e.Y + 10);
					break;
				case "Logmag S11":
				case "Logmag S21":
				case "Logmag S11&S21":
					chartToolTip.Show("Frequency：" + freqs[myTestResult.PointIndex] / 1000000.0 + " MHz\r\nLogMag：" + myTestResult.Series.Points[myTestResult.PointIndex].YValues[0].ToString("F4", CultureInfo.InvariantCulture) + " dB", chart1, e.X + 10, e.Y + 10);
					break;
				case "Linear S11":
				case "Linear S21":
				case "Linear S11&S21":
					chartToolTip.Show("Frequency：" + freqs[myTestResult.PointIndex] / 1000000.0 + " MHz\r\nLinMag：" + myTestResult.Series.Points[myTestResult.PointIndex].YValues[0].ToString("F4", CultureInfo.InvariantCulture), chart1, e.X + 10, e.Y + 10);
					break;
				case "Phase S11":
				case "Phase S21":
				case "Phase S11&S21":
					chartToolTip.Show("Frequency：" + freqs[myTestResult.PointIndex] / 1000000.0 + " MHz\r\nPhase：" + myTestResult.Series.Points[myTestResult.PointIndex].YValues[0].ToString("F4", CultureInfo.InvariantCulture) + " deg", chart1, e.X + 10, e.Y + 10);
					break;
				case "GroupDelay S11":
				case "GroupDelay S21":
				case "GroupDelay S11&S21":
					chartToolTip.Show("Frequency：" + freqs[myTestResult.PointIndex] / 1000000.0 + " MHz\r\nGroupDelay：" + myTestResult.Series.Points[myTestResult.PointIndex].YValues[0].ToString("F4", CultureInfo.InvariantCulture) + " ns", chart1, e.X + 10, e.Y + 10);
					break;
				case "SWR S11":
					chartToolTip.Show("Frequency：" + freqs[myTestResult.PointIndex] / 1000000.0 + " MHz\r\nSWR：" + myTestResult.Series.Points[myTestResult.PointIndex].YValues[0].ToString("F4", CultureInfo.InvariantCulture), chart1, e.X + 10, e.Y + 10);
					break;
				}
			}
			else
			{
				chartToolTip.Hide(chart1);
			}
		}

		private void YMaxCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (YMaxCheckBox.Checked)
			{
				YMaxNumericUpDown.Enabled = false;
				chart1.ChartAreas[0].AxisY.Maximum = double.NaN;
			}
			else
			{
				YMaxNumericUpDown.Enabled = true;
				try
				{
					YMaxNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Maximum;
				}
				catch (Exception)
				{
					YMaxNumericUpDown.Value = YMaxNumericUpDown.Maximum;
				}
			}
		}

		private void YMinCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (YMinCheckBox.Checked)
			{
				YMinNumericUpDown.Enabled = false;
				chart1.ChartAreas[0].AxisY.Minimum = double.NaN;
			}
			else
			{
				YMinNumericUpDown.Enabled = true;
				try
				{
					YMinNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Minimum;
				}
				catch (Exception)
				{
					YMinNumericUpDown.Value = YMinNumericUpDown.Minimum;
				}
			}
		}

		private void YMaxNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if ((double)YMaxNumericUpDown.Value > chart1.ChartAreas[0].AxisY.Minimum)
			{
				chart1.ChartAreas[0].AxisY.Maximum = (double)YMaxNumericUpDown.Value;
			}
			else
			{
				YMaxNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Minimum + decimal.One;
			}
		}

		private void YMinNumericUpDown_ValueChanged(object sender, EventArgs e)
		{
			if ((double)YMinNumericUpDown.Value < chart1.ChartAreas[0].AxisY.Maximum)
			{
				chart1.ChartAreas[0].AxisY.Minimum = (double)YMinNumericUpDown.Value;
			}
			else
			{
				YMinNumericUpDown.Value = (decimal)chart1.ChartAreas[0].AxisY.Maximum - decimal.One;
			}
		}

		private void languageButton_Click(object sender, EventArgs e)
		{
			if (openCloseSpbtn.Text == "Disconnect" || openCloseSpbtn.Text == "断开")
			{
				controller.CloseSerialPort();
			}
			if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CN")
			{
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
			}
			else
			{
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("zh-CN");
			}
			LoadLanguage(Form.ActiveForm, typeof(MainForm));
		}

		public static void LoadLanguage(Form form, Type formType)
		{
			if (form != null)
			{
				ComponentResourceManager componentResourceManager = new ComponentResourceManager(formType);
				componentResourceManager.ApplyResources(form, "$this");
				Loading(form, componentResourceManager);
			}
		}

		private static void Loading(Control control, ComponentResourceManager resources)
		{
			if (control is MenuStrip)
			{
				resources.ApplyResources(control, control.Name);
				MenuStrip menuStrip = (MenuStrip)control;
				if (menuStrip.Items.Count > 0)
				{
					foreach (ToolStripMenuItem item in menuStrip.Items)
					{
						Loading(item, resources);
					}
				}
			}
			foreach (Control control2 in control.Controls)
			{
				resources.ApplyResources(control2, control2.Name);
				Loading(control2, resources);
			}
		}

		private static void Loading(ToolStripMenuItem item, ComponentResourceManager resources)
		{
			if (item != null)
			{
				resources.ApplyResources(item, item.Name);
				if (item.DropDownItems.Count > 0)
				{
					foreach (ToolStripMenuItem dropDownItem in item.DropDownItems)
					{
						Loading(dropDownItem, resources);
					}
				}
			}
		}

		private void InfoButton_Click(object sender, EventArgs e)
		{
			controller.SendDataToCom("info\r\n");
		}

		private void AboutButton_Click(object sender, EventArgs e)
		{
			if (Thread.CurrentThread.CurrentUICulture.Name == "zh-CN")
			{
				MessageBox.Show("NanoVNA 是日本edy555(https://twitter.com/edy555)设计的开源硬件项目。\r\n我修改了部分电路，增加了电池管理电路，重新设计了PCB，并在淘宝 https://gen111.taobao.com/ 接近成本价出售。\r\n本软件设计用于控制NanoVNA并导出 Touchstone(snp)文件。", "关于 NanoVNA", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0, "https://gen111.taobao.com/");
			}
			else
			{
				MessageBox.Show("NanoVNA is an open source hardware project designed by edy555(https://twitter.com/edy555).\r\nI remade NanoVNA based on edy555 and modified some circuits, while selling at the cost of the online store https://gen111.taobao.com/.\r\nThis PC software is designed for control NanoVNA and export Touchstone(snp) files.", "About NanoVNA", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0, "https://gen111.taobao.com/");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager componentResourceManager = new System.ComponentModel.ComponentResourceManager(typeof(NanoVNA.MainForm));
			System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
			System.Windows.Forms.DataVisualization.Charting.Legend legend = new System.Windows.Forms.DataVisualization.Charting.Legend();
			System.Windows.Forms.DataVisualization.Charting.Series series = new System.Windows.Forms.DataVisualization.Charting.Series();
			System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
			System.Windows.Forms.DataVisualization.Charting.Title title = new System.Windows.Forms.DataVisualization.Charting.Title();
			System.Windows.Forms.DataVisualization.Charting.Title title2 = new System.Windows.Forms.DataVisualization.Charting.Title();
			comListCbx = new System.Windows.Forms.ComboBox();
			label3 = new System.Windows.Forms.Label();
			openCloseSpbtn = new System.Windows.Forms.Button();
			statustimer = new System.Windows.Forms.Timer(components);
			autoRefreshtimer = new System.Windows.Forms.Timer(components);
			refreshbtn = new System.Windows.Forms.Button();
			comGroupBox = new System.Windows.Forms.GroupBox();
			chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
			label8 = new System.Windows.Forms.Label();
			chartGroupBox = new System.Windows.Forms.GroupBox();
			YMinCheckBox = new System.Windows.Forms.CheckBox();
			YMaxCheckBox = new System.Windows.Forms.CheckBox();
			YMinNumericUpDown = new System.Windows.Forms.NumericUpDown();
			YMaxNumericUpDown = new System.Windows.Forms.NumericUpDown();
			YMinLabel = new System.Windows.Forms.Label();
			YMaxLabel = new System.Windows.Forms.Label();
			label6 = new System.Windows.Forms.Label();
			formatComboBox = new System.Windows.Forms.ComboBox();
			recallGroupBox = new System.Windows.Forms.GroupBox();
			recallBackButton = new System.Windows.Forms.Button();
			Recall4Button = new System.Windows.Forms.Button();
			Recall3Button = new System.Windows.Forms.Button();
			Recall2Button = new System.Windows.Forms.Button();
			Recall1Button = new System.Windows.Forms.Button();
			Recall0Button = new System.Windows.Forms.Button();
			calGroupBox = new System.Windows.Forms.GroupBox();
			doneButton = new System.Windows.Forms.Button();
			throughButton = new System.Windows.Forms.Button();
			isolationButton = new System.Windows.Forms.Button();
			loadButton = new System.Windows.Forms.Button();
			shortButton = new System.Windows.Forms.Button();
			openButton = new System.Windows.Forms.Button();
			saveGroupBox = new System.Windows.Forms.GroupBox();
			saveBackButton = new System.Windows.Forms.Button();
			save4Button = new System.Windows.Forms.Button();
			save3Button = new System.Windows.Forms.Button();
			save2Button = new System.Windows.Forms.Button();
			save1Button = new System.Windows.Forms.Button();
			save0Button = new System.Windows.Forms.Button();
			autoRefreshcbx = new System.Windows.Forms.CheckBox();
			stimulusGroupBox = new System.Windows.Forms.GroupBox();
			label4 = new System.Windows.Forms.Label();
			label5 = new System.Windows.Forms.Label();
			label2 = new System.Windows.Forms.Label();
			label1 = new System.Windows.Forms.Label();
			SpanNumericUpDown = new System.Windows.Forms.NumericUpDown();
			CenterNumericUpDown = new System.Windows.Forms.NumericUpDown();
			SpanLabel = new System.Windows.Forms.Label();
			CenterLabel = new System.Windows.Forms.Label();
			StopNumericUpDown = new System.Windows.Forms.NumericUpDown();
			StartNumericUpDown = new System.Windows.Forms.NumericUpDown();
			StopLabel = new System.Windows.Forms.Label();
			StartLabel = new System.Windows.Forms.Label();
			DataGroupBox = new System.Windows.Forms.GroupBox();
			loadSnpFileButton = new System.Windows.Forms.Button();
			saveS2pButton = new System.Windows.Forms.Button();
			saveS1pButton = new System.Windows.Forms.Button();
			autoRefreshtimerNumericUpDown = new System.Windows.Forms.NumericUpDown();
			getDataButton = new System.Windows.Forms.Button();
			stateGroupBox = new System.Windows.Forms.GroupBox();
			recallButton = new System.Windows.Forms.Button();
			saveButton = new System.Windows.Forms.Button();
			responseGroupBox = new System.Windows.Forms.GroupBox();
			calButton = new System.Windows.Forms.Button();
			chartToolTip = new System.Windows.Forms.ToolTip(components);
			languageGroupBox = new System.Windows.Forms.GroupBox();
			languageButton = new System.Windows.Forms.Button();
			aboutBox = new System.Windows.Forms.GroupBox();
			infoButton = new System.Windows.Forms.Button();
			AboutButton = new System.Windows.Forms.Button();
			comGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)chart1).BeginInit();
			chartGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)YMinNumericUpDown).BeginInit();
			((System.ComponentModel.ISupportInitialize)YMaxNumericUpDown).BeginInit();
			recallGroupBox.SuspendLayout();
			calGroupBox.SuspendLayout();
			saveGroupBox.SuspendLayout();
			stimulusGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)SpanNumericUpDown).BeginInit();
			((System.ComponentModel.ISupportInitialize)CenterNumericUpDown).BeginInit();
			((System.ComponentModel.ISupportInitialize)StopNumericUpDown).BeginInit();
			((System.ComponentModel.ISupportInitialize)StartNumericUpDown).BeginInit();
			DataGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)autoRefreshtimerNumericUpDown).BeginInit();
			stateGroupBox.SuspendLayout();
			responseGroupBox.SuspendLayout();
			languageGroupBox.SuspendLayout();
			aboutBox.SuspendLayout();
			SuspendLayout();
			comListCbx.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			comListCbx.FormattingEnabled = true;
			componentResourceManager.ApplyResources(comListCbx, "comListCbx");
			comListCbx.Name = "comListCbx";
			componentResourceManager.ApplyResources(label3, "label3");
			label3.Name = "label3";
			componentResourceManager.ApplyResources(openCloseSpbtn, "openCloseSpbtn");
			openCloseSpbtn.Name = "openCloseSpbtn";
			openCloseSpbtn.UseVisualStyleBackColor = true;
			openCloseSpbtn.Click += new System.EventHandler(openCloseSpbtn_Click);
			statustimer.Enabled = true;
			statustimer.Interval = 1000;
			statustimer.Tick += new System.EventHandler(statustimer_Tick);
			autoRefreshtimer.Interval = 1000;
			autoRefreshtimer.Tick += new System.EventHandler(autoSendtimer_Tick);
			componentResourceManager.ApplyResources(refreshbtn, "refreshbtn");
			refreshbtn.Name = "refreshbtn";
			refreshbtn.UseVisualStyleBackColor = true;
			refreshbtn.Click += new System.EventHandler(refreshbtn_Click);
			comGroupBox.Controls.Add(refreshbtn);
			comGroupBox.Controls.Add(comListCbx);
			comGroupBox.Controls.Add(label3);
			comGroupBox.Controls.Add(openCloseSpbtn);
			componentResourceManager.ApplyResources(comGroupBox, "comGroupBox");
			comGroupBox.Name = "comGroupBox";
			comGroupBox.TabStop = false;
			chartArea.AxisX.InterlacedColor = System.Drawing.Color.White;
			chartArea.AxisX.IntervalAutoMode = System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.VariableCount;
			chartArea.AxisX.IsLabelAutoFit = false;
			chartArea.AxisX.IsMarginVisible = false;
			chartArea.AxisX.MajorGrid.Interval = 0.0;
			chartArea.AxisX.MajorGrid.LineColor = System.Drawing.Color.DarkGray;
			chartArea.AxisX.MinorGrid.LineColor = System.Drawing.Color.Silver;
			chartArea.AxisX.MinorTickMark.Size = 5f;
			chartArea.AxisX.Title = "Frequencies:MHz";
			chartArea.AxisX.TitleAlignment = System.Drawing.StringAlignment.Far;
			chartArea.AxisY.IntervalAutoMode = System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.VariableCount;
			chartArea.AxisY.IsStartedFromZero = false;
			chartArea.AxisY.MajorGrid.LineColor = System.Drawing.Color.DarkGray;
			chartArea.AxisY.MajorTickMark.Enabled = false;
			chartArea.AxisY.MajorTickMark.LineColor = System.Drawing.Color.LightGray;
			chartArea.AxisY.Maximum = 0.0;
			chartArea.AxisY.Minimum = -70.0;
			chartArea.AxisY.MinorGrid.LineColor = System.Drawing.Color.LightGray;
			chartArea.AxisY.MinorTickMark.LineColor = System.Drawing.Color.LightGray;
			chartArea.AxisY.Title = "Magnitude:dB";
			chartArea.BackColor = System.Drawing.Color.Transparent;
			chartArea.BackImageAlignment = System.Windows.Forms.DataVisualization.Charting.ChartImageAlignmentStyle.Center;
			chartArea.BackImageTransparentColor = System.Drawing.Color.Transparent;
			chartArea.BackImageWrapMode = System.Windows.Forms.DataVisualization.Charting.ChartImageWrapMode.Scaled;
			chartArea.CursorX.IsUserEnabled = true;
			chartArea.CursorX.IsUserSelectionEnabled = true;
			chartArea.Name = "ChartArea1";
			chart1.ChartAreas.Add(chartArea);
			legend.Enabled = false;
			legend.Name = "Legend1";
			chart1.Legends.Add(legend);
			componentResourceManager.ApplyResources(chart1, "chart1");
			chart1.Name = "chart1";
			series.BorderWidth = 2;
			series.ChartArea = "ChartArea1";
			series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
			series.Color = System.Drawing.Color.FromArgb(255, 128, 0);
			series.Legend = "Legend1";
			series.Name = "Series1";
			series2.BorderWidth = 2;
			series2.ChartArea = "ChartArea1";
			series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
			series2.Color = System.Drawing.Color.Blue;
			series2.Legend = "Legend1";
			series2.Name = "Series2";
			chart1.Series.Add(series);
			chart1.Series.Add(series2);
			title.Alignment = System.Drawing.ContentAlignment.TopLeft;
			title.DockedToChartArea = "ChartArea1";
			title.ForeColor = System.Drawing.Color.FromArgb(255, 128, 0);
			title.Name = "Title1";
			title.Text = "S11";
			title2.Alignment = System.Drawing.ContentAlignment.TopLeft;
			title2.DockedToChartArea = "ChartArea1";
			title2.ForeColor = System.Drawing.Color.Blue;
			title2.Name = "Title2";
			title2.Text = "S21";
			chart1.Titles.Add(title);
			chart1.Titles.Add(title2);
			chart1.MouseClick += new System.Windows.Forms.MouseEventHandler(chart1_MouseClick);
			chart1.MouseMove += new System.Windows.Forms.MouseEventHandler(chart1_MouseMove);
			componentResourceManager.ApplyResources(label8, "label8");
			label8.Name = "label8";
			chartGroupBox.Controls.Add(YMinCheckBox);
			chartGroupBox.Controls.Add(YMaxCheckBox);
			chartGroupBox.Controls.Add(YMinNumericUpDown);
			chartGroupBox.Controls.Add(YMaxNumericUpDown);
			chartGroupBox.Controls.Add(YMinLabel);
			chartGroupBox.Controls.Add(YMaxLabel);
			chartGroupBox.Controls.Add(label6);
			chartGroupBox.Controls.Add(formatComboBox);
			chartGroupBox.Controls.Add(chart1);
			componentResourceManager.ApplyResources(chartGroupBox, "chartGroupBox");
			chartGroupBox.Name = "chartGroupBox";
			chartGroupBox.TabStop = false;
			componentResourceManager.ApplyResources(YMinCheckBox, "YMinCheckBox");
			YMinCheckBox.BackColor = System.Drawing.Color.White;
			YMinCheckBox.Name = "YMinCheckBox";
			YMinCheckBox.UseVisualStyleBackColor = false;
			YMinCheckBox.CheckedChanged += new System.EventHandler(YMinCheckBox_CheckedChanged);
			componentResourceManager.ApplyResources(YMaxCheckBox, "YMaxCheckBox");
			YMaxCheckBox.BackColor = System.Drawing.Color.White;
			YMaxCheckBox.Name = "YMaxCheckBox";
			YMaxCheckBox.UseVisualStyleBackColor = false;
			YMaxCheckBox.CheckedChanged += new System.EventHandler(YMaxCheckBox_CheckedChanged);
			YMinNumericUpDown.DecimalPlaces = 1;
			componentResourceManager.ApplyResources(YMinNumericUpDown, "YMinNumericUpDown");
			YMinNumericUpDown.Minimum = new decimal(new int[4]
			{
				180,
				0,
				0,
				-2147483648
			});
			YMinNumericUpDown.Name = "YMinNumericUpDown";
			YMinNumericUpDown.Value = new decimal(new int[4]
			{
				70,
				0,
				0,
				-2147483648
			});
			YMinNumericUpDown.ValueChanged += new System.EventHandler(YMinNumericUpDown_ValueChanged);
			YMaxNumericUpDown.DecimalPlaces = 1;
			componentResourceManager.ApplyResources(YMaxNumericUpDown, "YMaxNumericUpDown");
			YMaxNumericUpDown.Minimum = new decimal(new int[4]
			{
				180,
				0,
				0,
				-2147483648
			});
			YMaxNumericUpDown.Name = "YMaxNumericUpDown";
			YMaxNumericUpDown.ValueChanged += new System.EventHandler(YMaxNumericUpDown_ValueChanged);
			componentResourceManager.ApplyResources(YMinLabel, "YMinLabel");
			YMinLabel.BackColor = System.Drawing.Color.White;
			YMinLabel.Name = "YMinLabel";
			componentResourceManager.ApplyResources(YMaxLabel, "YMaxLabel");
			YMaxLabel.BackColor = System.Drawing.Color.White;
			YMaxLabel.Name = "YMaxLabel";
			componentResourceManager.ApplyResources(label6, "label6");
			label6.BackColor = System.Drawing.Color.White;
			label6.Name = "label6";
			formatComboBox.FormattingEnabled = true;
			formatComboBox.Items.AddRange(new object[16]
			{
				componentResourceManager.GetString("formatComboBox.Items"),
				componentResourceManager.GetString("formatComboBox.Items1"),
				componentResourceManager.GetString("formatComboBox.Items2"),
				componentResourceManager.GetString("formatComboBox.Items3"),
				componentResourceManager.GetString("formatComboBox.Items4"),
				componentResourceManager.GetString("formatComboBox.Items5"),
				componentResourceManager.GetString("formatComboBox.Items6"),
				componentResourceManager.GetString("formatComboBox.Items7"),
				componentResourceManager.GetString("formatComboBox.Items8"),
				componentResourceManager.GetString("formatComboBox.Items9"),
				componentResourceManager.GetString("formatComboBox.Items10"),
				componentResourceManager.GetString("formatComboBox.Items11"),
				componentResourceManager.GetString("formatComboBox.Items12"),
				componentResourceManager.GetString("formatComboBox.Items13"),
				componentResourceManager.GetString("formatComboBox.Items14"),
				componentResourceManager.GetString("formatComboBox.Items15")
			});
			componentResourceManager.ApplyResources(formatComboBox, "formatComboBox");
			formatComboBox.Name = "formatComboBox";
			formatComboBox.SelectedIndexChanged += new System.EventHandler(formatComboBox_SelectedIndexChanged);
			recallGroupBox.Controls.Add(recallBackButton);
			recallGroupBox.Controls.Add(Recall4Button);
			recallGroupBox.Controls.Add(Recall3Button);
			recallGroupBox.Controls.Add(Recall2Button);
			recallGroupBox.Controls.Add(Recall1Button);
			recallGroupBox.Controls.Add(Recall0Button);
			componentResourceManager.ApplyResources(recallGroupBox, "recallGroupBox");
			recallGroupBox.Name = "recallGroupBox";
			recallGroupBox.TabStop = false;
			componentResourceManager.ApplyResources(recallBackButton, "recallBackButton");
			recallBackButton.Name = "recallBackButton";
			recallBackButton.UseVisualStyleBackColor = true;
			recallBackButton.Click += new System.EventHandler(recallBackButton_Click);
			componentResourceManager.ApplyResources(Recall4Button, "Recall4Button");
			Recall4Button.Name = "Recall4Button";
			Recall4Button.UseVisualStyleBackColor = true;
			Recall4Button.Click += new System.EventHandler(Recall4Button_Click);
			componentResourceManager.ApplyResources(Recall3Button, "Recall3Button");
			Recall3Button.Name = "Recall3Button";
			Recall3Button.UseVisualStyleBackColor = true;
			Recall3Button.Click += new System.EventHandler(Recall3Button_Click);
			componentResourceManager.ApplyResources(Recall2Button, "Recall2Button");
			Recall2Button.Name = "Recall2Button";
			Recall2Button.UseVisualStyleBackColor = true;
			Recall2Button.Click += new System.EventHandler(Recall2Button_Click);
			componentResourceManager.ApplyResources(Recall1Button, "Recall1Button");
			Recall1Button.Name = "Recall1Button";
			Recall1Button.UseVisualStyleBackColor = true;
			Recall1Button.Click += new System.EventHandler(Recall1Button_Click);
			componentResourceManager.ApplyResources(Recall0Button, "Recall0Button");
			Recall0Button.Name = "Recall0Button";
			Recall0Button.UseVisualStyleBackColor = true;
			Recall0Button.Click += new System.EventHandler(Recall0Button_Click);
			calGroupBox.Controls.Add(doneButton);
			calGroupBox.Controls.Add(throughButton);
			calGroupBox.Controls.Add(isolationButton);
			calGroupBox.Controls.Add(loadButton);
			calGroupBox.Controls.Add(shortButton);
			calGroupBox.Controls.Add(openButton);
			componentResourceManager.ApplyResources(calGroupBox, "calGroupBox");
			calGroupBox.Name = "calGroupBox";
			calGroupBox.TabStop = false;
			componentResourceManager.ApplyResources(doneButton, "doneButton");
			doneButton.Name = "doneButton";
			doneButton.UseVisualStyleBackColor = true;
			doneButton.Click += new System.EventHandler(doneButton_Click);
			componentResourceManager.ApplyResources(throughButton, "throughButton");
			throughButton.Name = "throughButton";
			throughButton.UseVisualStyleBackColor = true;
			throughButton.Click += new System.EventHandler(throughButton_Click);
			componentResourceManager.ApplyResources(isolationButton, "isolationButton");
			isolationButton.Name = "isolationButton";
			isolationButton.UseVisualStyleBackColor = true;
			isolationButton.Click += new System.EventHandler(isolationButton_Click);
			componentResourceManager.ApplyResources(loadButton, "loadButton");
			loadButton.Name = "loadButton";
			loadButton.UseVisualStyleBackColor = true;
			loadButton.Click += new System.EventHandler(loadButton_Click);
			componentResourceManager.ApplyResources(shortButton, "shortButton");
			shortButton.Name = "shortButton";
			shortButton.UseVisualStyleBackColor = true;
			shortButton.Click += new System.EventHandler(shortButton_Click);
			componentResourceManager.ApplyResources(openButton, "openButton");
			openButton.Name = "openButton";
			openButton.UseVisualStyleBackColor = true;
			openButton.Click += new System.EventHandler(openButton_Click);
			saveGroupBox.Controls.Add(saveBackButton);
			saveGroupBox.Controls.Add(save4Button);
			saveGroupBox.Controls.Add(save3Button);
			saveGroupBox.Controls.Add(save2Button);
			saveGroupBox.Controls.Add(save1Button);
			saveGroupBox.Controls.Add(save0Button);
			componentResourceManager.ApplyResources(saveGroupBox, "saveGroupBox");
			saveGroupBox.Name = "saveGroupBox";
			saveGroupBox.TabStop = false;
			componentResourceManager.ApplyResources(saveBackButton, "saveBackButton");
			saveBackButton.Name = "saveBackButton";
			saveBackButton.UseVisualStyleBackColor = true;
			saveBackButton.Click += new System.EventHandler(saveBackButton_Click);
			componentResourceManager.ApplyResources(save4Button, "save4Button");
			save4Button.Name = "save4Button";
			save4Button.UseVisualStyleBackColor = true;
			save4Button.Click += new System.EventHandler(save4Button_Click);
			componentResourceManager.ApplyResources(save3Button, "save3Button");
			save3Button.Name = "save3Button";
			save3Button.UseVisualStyleBackColor = true;
			save3Button.Click += new System.EventHandler(save3Button_Click);
			componentResourceManager.ApplyResources(save2Button, "save2Button");
			save2Button.Name = "save2Button";
			save2Button.UseVisualStyleBackColor = true;
			save2Button.Click += new System.EventHandler(save2Button_Click);
			componentResourceManager.ApplyResources(save1Button, "save1Button");
			save1Button.Name = "save1Button";
			save1Button.UseVisualStyleBackColor = true;
			save1Button.Click += new System.EventHandler(save1Button_Click);
			componentResourceManager.ApplyResources(save0Button, "save0Button");
			save0Button.Name = "save0Button";
			save0Button.UseVisualStyleBackColor = true;
			save0Button.Click += new System.EventHandler(save0Button_Click);
			componentResourceManager.ApplyResources(autoRefreshcbx, "autoRefreshcbx");
			autoRefreshcbx.Name = "autoRefreshcbx";
			autoRefreshcbx.UseVisualStyleBackColor = true;
			autoRefreshcbx.CheckedChanged += new System.EventHandler(autoRefreshcbx_CheckedChanged);
			stimulusGroupBox.Controls.Add(label4);
			stimulusGroupBox.Controls.Add(label5);
			stimulusGroupBox.Controls.Add(label2);
			stimulusGroupBox.Controls.Add(label1);
			stimulusGroupBox.Controls.Add(SpanNumericUpDown);
			stimulusGroupBox.Controls.Add(CenterNumericUpDown);
			stimulusGroupBox.Controls.Add(SpanLabel);
			stimulusGroupBox.Controls.Add(CenterLabel);
			stimulusGroupBox.Controls.Add(StopNumericUpDown);
			stimulusGroupBox.Controls.Add(StartNumericUpDown);
			stimulusGroupBox.Controls.Add(StopLabel);
			stimulusGroupBox.Controls.Add(StartLabel);
			componentResourceManager.ApplyResources(stimulusGroupBox, "stimulusGroupBox");
			stimulusGroupBox.Name = "stimulusGroupBox";
			stimulusGroupBox.TabStop = false;
			componentResourceManager.ApplyResources(label4, "label4");
			label4.Name = "label4";
			componentResourceManager.ApplyResources(label5, "label5");
			label5.Name = "label5";
			componentResourceManager.ApplyResources(label2, "label2");
			label2.Name = "label2";
			componentResourceManager.ApplyResources(label1, "label1");
			label1.Name = "label1";
			SpanNumericUpDown.DecimalPlaces = 6;
			componentResourceManager.ApplyResources(SpanNumericUpDown, "SpanNumericUpDown");
			SpanNumericUpDown.Maximum = new decimal(new int[4]
			{
				900,
				0,
				0,
				0
			});
			SpanNumericUpDown.Name = "SpanNumericUpDown";
			SpanNumericUpDown.Value = new decimal(new int[4]
			{
				300,
				0,
				0,
				0
			});
			SpanNumericUpDown.ValueChanged += new System.EventHandler(SpanNumericUpDown_ValueChanged);
			CenterNumericUpDown.DecimalPlaces = 6;
			componentResourceManager.ApplyResources(CenterNumericUpDown, "CenterNumericUpDown");
			CenterNumericUpDown.Maximum = new decimal(new int[4]
			{
				900,
				0,
				0,
				0
			});
			CenterNumericUpDown.Minimum = new decimal(new int[4]
			{
				5,
				0,
				0,
				131072
			});
			CenterNumericUpDown.Name = "CenterNumericUpDown";
			CenterNumericUpDown.Value = new decimal(new int[4]
			{
				150,
				0,
				0,
				0
			});
			CenterNumericUpDown.ValueChanged += new System.EventHandler(CenterNumericUpDown_ValueChanged);
			componentResourceManager.ApplyResources(SpanLabel, "SpanLabel");
			SpanLabel.Name = "SpanLabel";
			componentResourceManager.ApplyResources(CenterLabel, "CenterLabel");
			CenterLabel.Name = "CenterLabel";
			StopNumericUpDown.DecimalPlaces = 6;
			componentResourceManager.ApplyResources(StopNumericUpDown, "StopNumericUpDown");
			StopNumericUpDown.Maximum = new decimal(new int[4]
			{
				900,
				0,
				0,
				0
			});
			StopNumericUpDown.Minimum = new decimal(new int[4]
			{
				5,
				0,
				0,
				131072
			});
			StopNumericUpDown.Name = "StopNumericUpDown";
			StopNumericUpDown.Value = new decimal(new int[4]
			{
				300,
				0,
				0,
				0
			});
			StopNumericUpDown.ValueChanged += new System.EventHandler(StopNumericUpDown_ValueChanged);
			StartNumericUpDown.DecimalPlaces = 6;
			componentResourceManager.ApplyResources(StartNumericUpDown, "StartNumericUpDown");
			StartNumericUpDown.Maximum = new decimal(new int[4]
			{
				900,
				0,
				0,
				0
			});
			StartNumericUpDown.Minimum = new decimal(new int[4]
			{
				5,
				0,
				0,
				131072
			});
			StartNumericUpDown.Name = "StartNumericUpDown";
			StartNumericUpDown.Value = new decimal(new int[4]
			{
				5,
				0,
				0,
				131072
			});
			StartNumericUpDown.ValueChanged += new System.EventHandler(StartNumericUpDown_ValueChanged);
			componentResourceManager.ApplyResources(StopLabel, "StopLabel");
			StopLabel.Name = "StopLabel";
			componentResourceManager.ApplyResources(StartLabel, "StartLabel");
			StartLabel.Name = "StartLabel";
			DataGroupBox.Controls.Add(loadSnpFileButton);
			DataGroupBox.Controls.Add(saveS2pButton);
			DataGroupBox.Controls.Add(saveS1pButton);
			DataGroupBox.Controls.Add(autoRefreshtimerNumericUpDown);
			DataGroupBox.Controls.Add(getDataButton);
			DataGroupBox.Controls.Add(label8);
			DataGroupBox.Controls.Add(autoRefreshcbx);
			componentResourceManager.ApplyResources(DataGroupBox, "DataGroupBox");
			DataGroupBox.Name = "DataGroupBox";
			DataGroupBox.TabStop = false;
			componentResourceManager.ApplyResources(loadSnpFileButton, "loadSnpFileButton");
			loadSnpFileButton.Name = "loadSnpFileButton";
			loadSnpFileButton.UseVisualStyleBackColor = true;
			loadSnpFileButton.Click += new System.EventHandler(loadSnpFileButton_Click);
			componentResourceManager.ApplyResources(saveS2pButton, "saveS2pButton");
			saveS2pButton.Name = "saveS2pButton";
			saveS2pButton.UseVisualStyleBackColor = true;
			saveS2pButton.Click += new System.EventHandler(saveS2pButton_Click);
			componentResourceManager.ApplyResources(saveS1pButton, "saveS1pButton");
			saveS1pButton.Name = "saveS1pButton";
			saveS1pButton.UseVisualStyleBackColor = true;
			saveS1pButton.Click += new System.EventHandler(saveS1pButton_Click);
			autoRefreshtimerNumericUpDown.Increment = new decimal(new int[4]
			{
				100,
				0,
				0,
				0
			});
			componentResourceManager.ApplyResources(autoRefreshtimerNumericUpDown, "autoRefreshtimerNumericUpDown");
			autoRefreshtimerNumericUpDown.Maximum = new decimal(new int[4]
			{
				5000,
				0,
				0,
				0
			});
			autoRefreshtimerNumericUpDown.Minimum = new decimal(new int[4]
			{
				800,
				0,
				0,
				0
			});
			autoRefreshtimerNumericUpDown.Name = "autoRefreshtimerNumericUpDown";
			autoRefreshtimerNumericUpDown.Value = new decimal(new int[4]
			{
				1200,
				0,
				0,
				0
			});
			componentResourceManager.ApplyResources(getDataButton, "getDataButton");
			getDataButton.Name = "getDataButton";
			getDataButton.UseVisualStyleBackColor = true;
			getDataButton.Click += new System.EventHandler(getDataButton_Click);
			stateGroupBox.Controls.Add(recallButton);
			stateGroupBox.Controls.Add(saveButton);
			componentResourceManager.ApplyResources(stateGroupBox, "stateGroupBox");
			stateGroupBox.Name = "stateGroupBox";
			stateGroupBox.TabStop = false;
			componentResourceManager.ApplyResources(recallButton, "recallButton");
			recallButton.Name = "recallButton";
			recallButton.UseVisualStyleBackColor = true;
			recallButton.Click += new System.EventHandler(recallButton_Click);
			componentResourceManager.ApplyResources(saveButton, "saveButton");
			saveButton.Name = "saveButton";
			saveButton.UseVisualStyleBackColor = true;
			saveButton.Click += new System.EventHandler(saveButton_Click);
			responseGroupBox.Controls.Add(calButton);
			componentResourceManager.ApplyResources(responseGroupBox, "responseGroupBox");
			responseGroupBox.Name = "responseGroupBox";
			responseGroupBox.TabStop = false;
			componentResourceManager.ApplyResources(calButton, "calButton");
			calButton.Name = "calButton";
			calButton.UseVisualStyleBackColor = true;
			calButton.Click += new System.EventHandler(calButton_Click);
			languageGroupBox.Controls.Add(languageButton);
			componentResourceManager.ApplyResources(languageGroupBox, "languageGroupBox");
			languageGroupBox.Name = "languageGroupBox";
			languageGroupBox.TabStop = false;
			componentResourceManager.ApplyResources(languageButton, "languageButton");
			languageButton.Name = "languageButton";
			languageButton.UseVisualStyleBackColor = true;
			languageButton.Click += new System.EventHandler(languageButton_Click);
			aboutBox.Controls.Add(infoButton);
			aboutBox.Controls.Add(AboutButton);
			componentResourceManager.ApplyResources(aboutBox, "aboutBox");
			aboutBox.Name = "aboutBox";
			aboutBox.TabStop = false;
			componentResourceManager.ApplyResources(infoButton, "infoButton");
			infoButton.Name = "infoButton";
			infoButton.UseVisualStyleBackColor = true;
			infoButton.Click += new System.EventHandler(InfoButton_Click);
			componentResourceManager.ApplyResources(AboutButton, "AboutButton");
			AboutButton.Name = "AboutButton";
			AboutButton.UseVisualStyleBackColor = true;
			AboutButton.Click += new System.EventHandler(AboutButton_Click);
			componentResourceManager.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			base.Controls.Add(aboutBox);
			base.Controls.Add(languageGroupBox);
			base.Controls.Add(calGroupBox);
			base.Controls.Add(recallGroupBox);
			base.Controls.Add(stateGroupBox);
			base.Controls.Add(DataGroupBox);
			base.Controls.Add(saveGroupBox);
			base.Controls.Add(stimulusGroupBox);
			base.Controls.Add(chartGroupBox);
			base.Controls.Add(comGroupBox);
			base.Controls.Add(responseGroupBox);
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			base.Name = "MainForm";
			base.Load += new System.EventHandler(MainForm_Load);
			comGroupBox.ResumeLayout(performLayout: false);
			comGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)chart1).EndInit();
			chartGroupBox.ResumeLayout(performLayout: false);
			chartGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)YMinNumericUpDown).EndInit();
			((System.ComponentModel.ISupportInitialize)YMaxNumericUpDown).EndInit();
			recallGroupBox.ResumeLayout(performLayout: false);
			calGroupBox.ResumeLayout(performLayout: false);
			saveGroupBox.ResumeLayout(performLayout: false);
			stimulusGroupBox.ResumeLayout(performLayout: false);
			stimulusGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)SpanNumericUpDown).EndInit();
			((System.ComponentModel.ISupportInitialize)CenterNumericUpDown).EndInit();
			((System.ComponentModel.ISupportInitialize)StopNumericUpDown).EndInit();
			((System.ComponentModel.ISupportInitialize)StartNumericUpDown).EndInit();
			DataGroupBox.ResumeLayout(performLayout: false);
			DataGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)autoRefreshtimerNumericUpDown).EndInit();
			stateGroupBox.ResumeLayout(performLayout: false);
			responseGroupBox.ResumeLayout(performLayout: false);
			languageGroupBox.ResumeLayout(performLayout: false);
			aboutBox.ResumeLayout(performLayout: false);
			ResumeLayout(performLayout: false);
		}
	}
}
