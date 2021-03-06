﻿using System;
using System.Collections.Generic;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.ComponentModel;


namespace A001
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string filename = "YourXaml.xaml";
        private List<Thread> openThreads = new List<Thread>();

        public MainWindow()
        {
            InitializeComponent();

            Thread.Sleep(1000); // make sure splash screen shows for at least 1 second
        }


        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // see if the user really wants to shut down this window
            string msg = "Do you really want to close?";
            MessageBoxResult result = MessageBox.Show(msg, "Closing Window", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.No)
            {   // user does not want to close
                e.Cancel = true;
            }

            foreach (var t in openThreads)
            {
                t.Abort();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            File.WriteAllText(filename, txtXamlData.Text);
            Application.Current.Shutdown();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            this.Title = e.GetPosition(this).ToString();
        }

        private void btnViewXaml_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(filename, txtXamlData.Text);

            Window myWindow = null; // This is the dynamic window
            try
            {
                using (Stream sr = File.Open(filename, FileMode.Open))
                {
                    myWindow = (Window)XamlReader.Load(sr);
                }

                myWindow.ShowDialog();
                myWindow.Close();
                myWindow = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists(filename))
            {
                txtXamlData.Text = File.ReadAllText(filename);
            }
            else
            {
                txtXamlData.Text =
                        "<Window xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"\n"
                        + "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"\n"
                        + "Height =\"400\" Width =\"500\" WindowStartupLocation=\"CenterScreen\">\n"
                        + "<StackPanel>\n"
                        + "</StackPanel>\n"
                        + "</Window>";
            }
        }



        # region // ========= multithread ========

        private void multithread_gettime_Click(object sender, RoutedEventArgs e)
        {
            Update_Multithread_gettime_enable(false);
            Update_Multithread_txtBox("Getting time now...");

            Thread thread = new Thread(Multithread_GetTimeThread);
            thread.Name = "Getting Time - Multithread";
            thread.Start();
        }

        private void Multithread_GetTimeThread()
        {
            for (int i = 0; i < 5; i++)
            {
                // Time consuming wait
                Thread.Sleep(TimeSpan.FromSeconds(2));

                // Update the time on UI.  Delegate must be short and fast.
                Update_Multithread_txtBox(DateTime.Now.ToString());
            }
            Update_Multithread_gettime_enable(true);
        }

        public void Update_Multithread_txtBox(string val)
        {   // Update text on UI.  Delegate must be short and fast.
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { multithread_txtBox.Text = val; });
        }

        public void Update_Multithread_gettime_enable(bool val)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (ThreadStart)delegate() { multithread_gettime.IsEnabled = val; });
        }

        #endregion

        #region // ========= multithread with async & await ========

        private async void multithread_gettime_Click2(object sender, RoutedEventArgs e)
        {
            multithread_gettime2.IsEnabled = false;
            multithread_gettime2.Content = "Wait for it...";
            multithread_txtBox2.Text = "Getting time using async & await";

            Task<string> t = new Task<string>(new Func<object, string>(GetTimeAsync), 5);
            t.Start();

            string result = await t;    // Wait for task to complete.

            multithread_txtBox2.Text = result;
            multithread_gettime2.IsEnabled = true;
            multithread_gettime2.Content = "Get Time";
        }

        private string GetTimeAsync(object waitTime)
        {
            int waitTimeInt = (int)waitTime;
            Thread.Sleep(TimeSpan.FromSeconds(waitTimeInt));
            return DateTime.Now.ToString();
        }
        #endregion

        #region // ========= multithread Background Worker ========

        string multithread3_string = "Nothing yet";
        private readonly BackgroundWorker multithread3_BGWorker = new BackgroundWorker();
        private bool bgInitDone = false;

        private void multithread_gettime_Click3(object sender, RoutedEventArgs e)
        {
            if (!bgInitDone)
            {
                bgInitDone = true;
                multithread3_BGWorker.WorkerReportsProgress = true;
                multithread3_BGWorker.WorkerSupportsCancellation = true;
                multithread3_BGWorker.DoWork += multithread3_DoWork;
                multithread3_BGWorker.RunWorkerCompleted += multithread3_RunWorkerCompleted;
                multithread3_BGWorker.ProgressChanged += multithread3_ProgressChanged;
            }

            multithread_gettime3.IsEnabled = false;
            multithread_txtBox3.Text = "Wait for it...";


            multithread3_BGWorker.RunWorkerAsync();
            multithread3_cancel.IsEnabled = true;
        }


        private void multithread3_cancel_Click(object sender, RoutedEventArgs e)
        {
            if (multithread3_BGWorker.WorkerSupportsCancellation == true)
            {   // Cancel the asynchronous operation.
                multithread3_BGWorker.CancelAsync();
            }
        }


        private void multithread3_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int i = 0; i < 10; i++)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }

                e.Result = DateTime.Now.ToString();
                worker.ReportProgress(i * 10);
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }


        private void multithread3_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            multithread_txtBox3.Text = (e.ProgressPercentage.ToString() + "%");
            multithread3_progress.Value = e.ProgressPercentage;
        }


        private void multithread3_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == true)
            {
                multithread_txtBox3.Text = "Canceled";
            }
            else if (e.Error != null)
            {
                multithread_txtBox3.Text = "Error: " + e.Error.Message;
            }
            else
            {
                multithread3_string = e.Result as string;
                multithread_txtBox3.Text = multithread3_string;
            }

            multithread_gettime3.IsEnabled = true;
            multithread3_cancel.IsEnabled = false;
            multithread3_progress.Value = 0;
        }

        #endregion


        #region // ========= multithread TPL ========

        // Ref MSDN: Task-based Asynchronous Pattern (TAP)

        CancellationTokenSource ts;
        Task<String> multithread4_tsk;
        Progress<double> progRep = null;

        private void multithread4_gettime_Click(object sender, RoutedEventArgs e)
        {
            progRep = new Progress<double>((val) => { multithread4_progress.Value = val; });
            ts = new CancellationTokenSource();

            multithread4_gettime.IsEnabled = false;
            multithread4_txtBox.Text = "Wait for it...";
            multithread4_cancel.IsEnabled = true;

            multithread4_tsk = Task<String>.Factory.StartNew(() => multithread4_worker(), ts.Token);

            var canceled = multithread4_tsk.ContinueWith(canceledTask => 
                {
                    multithread4_txtBox.Text = "Canceled";
                    multithread4_gettime.IsEnabled = true;
                    multithread4_cancel.IsEnabled = false;
                    multithread4_progress.Value = 0;
                },
                                               CancellationToken.None,
                                               TaskContinuationOptions.OnlyOnCanceled,
                                               TaskScheduler.FromCurrentSynchronizationContext());

            var completed = multithread4_tsk.ContinueWith(completedTask =>
            {
                multithread4_txtBox.Text = completedTask.Result;
                multithread4_gettime.IsEnabled = true;
                multithread4_cancel.IsEnabled = false;
                multithread4_progress.Value = 0;
            },
                                               CancellationToken.None,
                                               TaskContinuationOptions.OnlyOnRanToCompletion,
                                               TaskScheduler.FromCurrentSynchronizationContext());

        }

        private void multithread4_cancel_Click(object sender, RoutedEventArgs e)
        {
            ts.Cancel();
        }

        private string multithread4_worker()
        {
            for (int i = 0; i < 7; i++)
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));

                ((IProgress<double>)progRep).Report( 100.0 * (i+1.0)/7.0 );
                try
                {
                    ts.Token.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
            }

            return DateTime.Now.ToString(); ;
        }

        #endregion



        #region // ========= Data Binding ========
        private void bindFill_Click(object sender, RoutedEventArgs e)
        {
            bindPanel.DataContext = BindExample.GetExample();
        }
        #endregion


        #region // ========= Data Binding with Notify ========
        private void btnBindNotify_Click(object sender, RoutedEventArgs e)
        {
            pnlBindNotify.DataContext = new NotifyingString();

            Thread thread = new Thread(new ParameterizedThreadStart((obj) =>
            {
                NotifyingString v = (NotifyingString)obj;

                for (; ; )
                {
                    v.Val = DateTime.Now.ToString();
                    Thread.Sleep(1000);
                }
            }));
            thread.Start(pnlBindNotify.DataContext);

            openThreads.Add(thread); // so that we can kill the thread when the program closes
        }

        #endregion

        // ========= ========

    }
}
