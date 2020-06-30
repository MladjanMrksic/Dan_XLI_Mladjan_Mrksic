using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Printer
{
    public partial class MainWindow : Window
    {
        string TextToPrint;
        int NumberOfCopies;
        static StreamWriter sw;
        /// <summary>
        /// Creating and instance of BackgroundWorker to be used to write to file while keeping the program responsive
        /// </summary>
        static BackgroundWorker bw = new BackgroundWorker
        {
            //Setting these properties to true enables us to monitor progress and cancel the process at any time
            WorkerReportsProgress = true,
            WorkerSupportsCancellation = true
        };
        /// <summary>
        /// Constructor for MainWidow Class
        /// </summary>
        public MainWindow()
        {
            //Subscribing to specific methods for specific jobs
            bw.DoWork += DoWork;
            bw.ProgressChanged += ProgressChanged;
            bw.RunWorkerCompleted += WorkCompleted;
            InitializeComponent();
        }

       /// <summary>
       /// This method specifies what job the BackgroundWorker will do
       /// </summary>
       /// <param name="sender">Information about the object that raised the event</param>
       /// <param name="e">Data provided for Background Worker.DoWork event</param>
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
                using (sw)
                {
                    sw.WriteLine(TextToPrint);
                }
                bw.ReportProgress(increment * i);
                Thread.Sleep(1000);
            }
        }
        /// <summary>
        /// This method specifies what should be done when the DoWork method is done or interupted for any reason
        /// </summary>
        /// <param name="sender">Information about the object that raised the event</param>
        /// <param name="e">Data provided for Background Worker.Completed event</param>
        void WorkCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Printing cancelled!", "Cancel", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else if (e.Error != null)
            {
                MessageBox.Show("Error " + e.Error.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Printing completed!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            pb.Value = 0;
            Cancel.IsEnabled = false;
        }
        /// <summary>
        /// This method handles how information is presented when ReportProgress is called
        /// </summary>
        /// <param name="sender">Information about the object that raised the event</param>
        /// <param name="e">Data provided for Background Worker.ProgressChanged event</param>
        void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pb.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// This method handles button Print click
        /// </summary>
        /// <param name="sender">Information about the object that raised the event</param>
        /// <param name="e">Data provided for the event</param>
        private void Print_Click(object sender, RoutedEventArgs e)
        {
            //If the process is already running, display message
            if (bw.IsBusy==true)
                MessageBox.Show("System is busy printing. Please wait until it finishes, or press Cancel to stop printing", "Printing in progress", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                //Enable button "Cancel" and start work
                Cancel.IsEnabled = true;
                bw.RunWorkerAsync();
            }            
        }
        /// <summary>
        /// This method handles button Cancel click
        /// </summary>
        /// <param name="sender">Information about the object that raised the event</param>
        /// <param name="e">Data provided for the event</param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            bw.CancelAsync();            
        }

        /// <summary>
        /// This method handles changes in the "Copies" textbox
        /// </summary>
        /// <param name="sender">Information about the object that raised the event</param>
        /// <param name="e">Data provided for the event</param>
        private void Copies_TextChanged(object sender, TextChangedEventArgs e)
        {
            //If user enters a non numeric, a message will be displayed and textbox cleared
            if (System.Text.RegularExpressions.Regex.IsMatch(copies.Text, "[^0-9]"))
            {
                MessageBox.Show("Please enter only numbers.", "Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                copies.Text = copies.Text.Remove(copies.Text.Length - 1);
            }
            //Else if number is a parsable number, it's value is assigned to a local variable
            else if (int.TryParse(copies.Text, out NumberOfCopies) == true)
                NumberOfCopies = Convert.ToInt32(copies.Text);
            //Logic behind Enabling/Disabling Print button
            if (TextToPrint != null && NumberOfCopies > 0)
                Print.IsEnabled = true;
            else if (string.IsNullOrWhiteSpace(TextToPrint) || NumberOfCopies <= 0)
                Print.IsEnabled = false;
        }
        /// <summary>
        /// This method handles changes in the "Text" textbox
        /// </summary>
        /// <param name="sender">Information about the object that raised the event</param>
        /// <param name="e">Data provided for the event</param>
        private void Text_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Asigning Text textbox value to local variable
            TextToPrint = text.Text;
            //Logic behind Enabling/Disabling Print button
            if (string.IsNullOrWhiteSpace(TextToPrint) || NumberOfCopies <= 0)
                Print.IsEnabled = false;
            else if (TextToPrint != null && NumberOfCopies > 0)
                Print.IsEnabled = true;
        }       
    }
}
