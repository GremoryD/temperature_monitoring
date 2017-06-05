using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms.DataVisualization.Charting; 
using OpenHardwareMonitor;
using OpenHardwareMonitor.Collections;
using OpenHardwareMonitor.Hardware;
using System.IO;
using System.Diagnostics; 

namespace Dyplom
{
   
    public partial class Form1 : Form
    { 
        struct Data
        {
           public Point point;
           public DateTime time;
        }
        List<Data> CPUdata;
        List<Data> GPUdata;



        Series mySeriesOfPoint;
        Series mySeriesOfPoint2;
        private DateTime now;
        protected readonly ListSet<ISensor> active = new ListSet<ISensor>();
        public event SensorEventHandler SensorAdded;
        public event SensorEventHandler SensorRemoved;

        protected virtual void ActivateSensor(ISensor sensor)
        {
            if (active.Add(sensor))
                if (SensorAdded != null)
                    SensorAdded(sensor);
        }

        public Form1()
        {
            InitializeComponent();

            CPUdata = new List<Data>();
            GPUdata = new List<Data>();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(new object[] {
            "Win32_1394Controller",
            "Win32_1394ControllerDevice",
            "Win32_BaseBoard",
            "Win32_Battery",
            "Win32_BIOS",
            "Win32_Bus",
            "Win32_CDROMDrive",
            "Win32_CIMLogicalDeviceCIMDataFile",
            "Win32_DeviceBus",
            "Win32_DeviceMemoryAddress",
            "Win32_DeviceSettings",
            "Win32_DisplayConfiguration",
            "Win32_DisplayControllerConfiguration",
            "Win32_DMAChannel",
            "Win32_DriverVXD",
            "Win32_FloppyController",
            "Win32_FloppyDrive",
            "Win32_HeatPipe",
            "Win32_IDEController",
            "Win32_IDEControllerDevice",
            "Win32_InfraredDevice",
            "Win32_IRQResource",
            "Win32_Keyboard",
            "Win32_MotherboardDevice",
            "Win32_OnBoardDevice",
            "Win32_PCMCIAController",
            "Win32_PNPAllocatedResource",
            "Win32_PnPDevice",
            "Win32_PnPEntity",
            "Win32_PointingDevice",
            "Win32_PortableBattery",
            "Win32_PortConnector",
            "Win32_PortResource",
            "Win32_POTSModem",
            "Win32_POTSModemToSerialPort",
            "Win32_PowerManagementEvent",
            "Win32_Printer",
            "Win32_PrinterConfiguration",
            "Win32_PrinterController",
            "Win32_PrinterDriverDll",
            "Win32_PrinterSetting",
            "Win32_PrinterShare",
            "Win32_PrintJob",
            "Win32_Processor",
            "Win32_SCSIController",
            "Win32_SCSIControllerDevice",
            "Win32_SerialPort",
            "Win32_SerialPortConfiguration",
            "Win32_SerialPortSetting",
            "Win32_SMBIOSMemory",
            "Win32_SoundDevice",
            "Win32_TemperatureProbe",
            "Win32_USBController",
            "Win32_USBControllerDevice",
            "Win32_VideoConfiguration",
            "Win32_VideoController",
            "Win32_VideoSettings",
            "Win32_VoltageProbe"});
            comboBox1.SelectedItem = "Win32_Processor";
            ColumnHeader columnHeader1 = new ColumnHeader();
            ColumnHeader columnHeader2 = new ColumnHeader();
            columnHeader1.Text = "Название";
            columnHeader2.Text = "Значение";
            listView1.Columns.AddRange(new ColumnHeader[] {
            columnHeader1,
            columnHeader2});

            chart1.ChartAreas.Add(new ChartArea("Math functions"));
            mySeriesOfPoint = new Series("Temprecher CPU");
            mySeriesOfPoint.ChartType = SeriesChartType.Line;
            mySeriesOfPoint2 = new Series("Temprecher GPU");
            mySeriesOfPoint2.ChartType = SeriesChartType.Line;

            chart1.Series.Add(mySeriesOfPoint);
            chart2.Series.Add(mySeriesOfPoint2);




        }

        private void InsertInfo(string Key, ref ListView lst, bool DontInsertNull)
        {
            lst.Items.Clear();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + Key);

            try
            {
                foreach (ManagementObject share in searcher.Get())
                {

                    ListViewGroup grp;
                    try
                    {
                        grp = lst.Groups.Add(share["Name"].ToString(), share["Name"].ToString());
                    }
                    catch
                    {
                        grp = lst.Groups.Add(share.ToString(), share.ToString());
                    }

                    if (share.Properties.Count <= 0)
                    {
                        MessageBox.Show("No Information Available", "No Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    foreach (PropertyData PC in share.Properties)
                    {

                        ListViewItem item = new ListViewItem(grp);
                        if (lst.Items.Count % 2 != 0)
                            item.BackColor = Color.White;
                        else
                            item.BackColor = Color.WhiteSmoke;

                        item.Text = PC.Name;

                        if (PC.Value != null && PC.Value.ToString() != "")
                        {
                            switch (PC.Value.GetType().ToString())
                            {
                                case "System.String[]":
                                    string[] str = (string[])PC.Value;

                                    string str2 = "";
                                    foreach (string st in str)
                                        str2 += st + " ";

                                    item.SubItems.Add(str2);

                                    break;
                                case "System.UInt16[]":
                                    ushort[] shortData = (ushort[])PC.Value;


                                    string tstr2 = "";
                                    foreach (ushort st in shortData)
                                        tstr2 += st.ToString() + " ";

                                    item.SubItems.Add(tstr2);

                                    break;

                                default:
                                    item.SubItems.Add(PC.Value.ToString());
                                    break;
                            }
                        }
                        else
                        {
                            if (!DontInsertNull)
                                item.SubItems.Add("No Information available");
                            else
                                continue;
                        }
                        lst.Items.Add(item);
                    }
                }
            }


            catch (Exception exp)
            {
                MessageBox.Show("can't get data because of the followeing error \n" + exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


        }

        private void RemoveNullValue(ref ListView lst)
        {
            foreach (ListViewItem item in lst.Items)
                if (item.SubItems[1].Text == "No Information available")
                    item.Remove();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                RemoveNullValue(ref listView1);
            else
                InsertInfo(comboBox1.SelectedItem.ToString(), ref listView1, checkBox1.Checked);
        }
        int i = 0;

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                List<byte> ls = GetDriveTemp();
                int temperature1 = Convert.ToInt32(ls[0]);
                int temperature2 = Convert.ToInt32(ls[1]);
                mySeriesOfPoint.Points.AddXY(i, temperature1);
                mySeriesOfPoint2.Points.AddXY(i, temperature2);
                i += (int)numericUpDown1.Value;

                Data temp = new Data();
                temp.time = DateTime.Now;
                temp.point =  new Point(i, temperature1);
                CPUdata.Add(temp);
                temp = new Data();
                temp.time = DateTime.Now;
                temp.point = new Point(i, temperature2);
                GPUdata.Add(temp);
            }
            catch(Exception exp)
            {
                MessageBox.Show(exp.ToString());
                timer1.Stop(); 
            }
        } 

        const byte TEMPERATURE_HDD = 194;
        static public List<byte> GetDriveTemp()
        {
            List<byte> retval = new List<byte>();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MSStorageDriver_ATAPISmartData");
                //loop through all the hard disks
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    byte[] arrVendorSpecific = (byte[])queryObj.GetPropertyValue("VendorSpecific");
                    //Find the temperature attribute
                    int tempIndex = Array.IndexOf(arrVendorSpecific, TEMPERATURE_HDD);
                    retval.Add(arrVendorSpecific[tempIndex + 5]);
                }
            }
            catch (ManagementException err)
            {
                Console.WriteLine("An error occurred while querying for WMI data: " + err.Message);
            }
            return retval;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)numericUpDown1.Value*1000;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }

      

}
