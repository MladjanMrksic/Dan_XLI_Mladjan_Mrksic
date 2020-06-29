using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Printer
{
    public partial class MainWindow : Window
    {

        string TextToPrint = null;
        int NumberOfCopies = 0;
        static StreamWriter sw;
        static BackgroundWorker bw = new BackgroundWorker
        {
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        public MainWindow()
        {
            Thread t = new Thread(PrintEnable);
            t.IsBackground = true;
            t.Start();
            bw.DoWork += DoWork;
            bw.ProgressChanged += ProgressChanged;
            bw.RunWorkerCompleted += WorkCompleted;
            InitializeComponent();
        }
        void DoWork(object sender, DoWorkEventArgs e)
        {
            int increment = 100 / NumberOfCopies;
            for (int i = 1; i <= NumberOfCopies; i++)
            {
                if (bw.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                string fileName = string.Format(i + "." + DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute);
                string filePath = string.Format(".../.../" + fileName + ".txt");
                File.Create(filePath).Close();
                sw = new StreamWriter(filePath);
                sw.WriteLine(filePath);
                bw.ReportProgress(increment * i);
                Thread.Sleep(1000);
            }
            sw.Close();
        }
        void WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Cancelled!");
            }
            else if (e.Error != null)
            {
                MessageBox.Show("Error " + e.Error.ToString());
            }
            else
            {
                MessageBox.Show("Completed");
            }
            pb.Value = 0;
            Cancel.IsEnabled = false;
        }
        void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pb.Value = e.ProgressPercentage;
        }
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (bw.IsBusy==true)
            {
                MessageBox.Show("System is busy printing. Please wait until it finishes, or press Cancel to stop printing");
            }
            else
            {
                Cancel.IsEnabled = true;
                bw.RunWorkerAsync();
            }            
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            bw.CancelAsync();            
        }

        private void Copies_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(copies.Text, "[^0-9]"))
            {
                MessageBox.Show("Please enter only numbers.");
                copies.Text = copies.Text.Remove(copies.Text.Length - 1);
            }
            else if (int.TryParse(copies.Text, out NumberOfCopies) == true)
            {
                NumberOfCopies = Convert.ToInt32(copies.Text);
            }
        }
        private void Text_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextToPrint = text.Text;
        }
        private void PrintEnable()
        {
            while (true)
            {
                if (text == null && copies == null)
                {
                    Print.IsEnabled = false;
                }
                else
                {
                    Print.IsEnabled = true;
                }
            }
        }
    }
}
