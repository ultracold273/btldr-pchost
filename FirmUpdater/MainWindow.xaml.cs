using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.ComponentModel;

namespace FirmUpdater
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private DataTransferrer dtTrans;
        private string srcBinFilePath;
        private BackgroundWorker worker;
        private bool isDownload;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void window_loaded(object sender, RoutedEventArgs e)
        {
            dtTrans = DataTransferrer.GetSingleton();
            int[] item = { 9600, 115200, 230400 };
            foreach (int a in item)
            {
                spp_rate_comboBox.Items.Add(a.ToString());
            }
            spp_rate_comboBox.SelectedItem = spp_rate_comboBox.Items[2];

            string[] ports = dtTrans.GetPorts();
            ports.ToList().ForEach(n => spp_name_comboBox.Items.Add(n));
            spp_name_comboBox.SelectedItem = spp_name_comboBox.Items[spp_name_comboBox.Items.Count - 1];

            isDownload = false;

            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += new DoWorkEventHandler(bgWorker_DoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
            worker.ProgressChanged += new ProgressChangedEventHandler(process_bar_Update);
        }

        private void sel_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openHexFileDialog = new OpenFileDialog();
                openHexFileDialog.Filter = "HEX files (*.hex)|*.hex|BIN files (*.bin)|*.bin|All files (*.*)|*.*";
                if (openHexFileDialog.ShowDialog() == true)
                {
                    hex_file_textBlock.Text = openHexFileDialog.FileName;
                    if (openHexFileDialog.FileName.EndsWith(".hex"))
                    {
                        HexFileReader fileReader = new HexFileReader(openHexFileDialog.FileName);
                        fileReader.ToBinary();
                        srcBinFilePath = fileReader.bFileName;
                    }else
                    {
                        srcBinFilePath = openHexFileDialog.FileName;
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }

        private void dw_button_Click(object sender, RoutedEventArgs e)
        {
            if (srcBinFilePath == null)
            {
                MessageBox.Show("No File Selected!");
                return;
            }
            if (!dtTrans.IsOpen)
            {
                MessageBox.Show("Serial Port does not Open");
                return;
            }
            // Whether there are current download 
            if (isDownload)
            {
                return;
            }

            isDownload = true;
            // Open a background thread doing the communication
            dtTrans.AssignBinFile(srcBinFilePath);
            //dw_process.Value = 50;
            worker.RunWorkerAsync(); //dtTrans.ProgramFirmware();
            //dw_process.Value = 60;
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            e.Result = dtTrans.ProgramFirmware(worker, e);

        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }


        private void port_button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!dtTrans.IsOpen)
                {
                    var PortName = spp_name_comboBox.SelectedItem.ToString();
                    var BaudRate = Convert.ToInt32(spp_rate_comboBox.SelectedItem.ToString());
                    dtTrans.CreateNewPort(PortName, BaudRate);
                    port_button.Content = "Close";
                }
                else
                {
                    dtTrans.Close();
                    port_button.Content = "Open";
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }

        public void process_bar_Update(object sender, ProgressChangedEventArgs val)
        {
            dw_process.Value = val.ProgressPercentage;
        }
    }
}
