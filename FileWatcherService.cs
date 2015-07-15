using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace WindowsService1
{
    public partial class FileWatcherService : ServiceBase
    {
        private FileSystemWatcher inputFileWatcher;
        private ILogger logger;

        private List<BackgroundWorker> workers;
        private Timer timer;
        private Queue<string> queue;
        private Random random;

        public FileWatcherService()
        {
            InitializeComponent();

            if (!System.Diagnostics.EventLog.SourceExists("FileWatcherServiceSource"))
            {
                EventLog.CreateEventSource("FileWatcherServiceSource", "FileWatcherServiceLog");
            }

            this.ServiceName = "FileWatcherService";
            eventLog1.Source = "FileWatcherServiceSource";
            eventLog1.Log = "FileWatcherServiceLog";
            timer = new Timer(5000);
            timer.Elapsed += timer_Elapsed;
            logger = new Logger(eventLog1);
            queue =new Queue<string>();
            workers = new List<BackgroundWorker>(Environment.ProcessorCount);
            random = new Random();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var worker = workers.FirstOrDefault(x => !x.IsBusy);
            if (worker != null && queue.Any())
            {
                lock (workers)
                {
                    workers.Remove(worker);
                    worker = new BackgroundWorker();
                    workers.Add(worker);
                }
                //logger.Log(string.Format("Queue before dequeue: {0}", string.Join(", ", queue.ToArray())));
                var next = queue.Dequeue();
                //logger.Log(string.Format("Queue after dequeue: {0}", string.Join(", ", queue.ToArray())));
                CreateNewProcess(next, worker);
            }
            else
            {
                if (worker == null)
                {
                    logger.Log("No available background workers");
                }
            }
        }

        private void CreateNewProcess(string file, BackgroundWorker worker)
        {
            //LongRunningProcess process = new LongRunningProcess(file, random.Next(50), logger);
            SchematronProcess schematronProcess = new SchematronProcess(file, @"c:\xbrl\output", logger);

            worker.RunWorkerAsync(schematronProcess);
            worker.DoWork += (o, args) => schematronProcess.Start();
            worker.RunWorkerCompleted += (o, args) =>
            {
                var bw = o as BackgroundWorker;
            };
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            eventLog1.WriteEntry("Started");
            timer.Enabled = true;
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                workers.Add(new BackgroundWorker());
            }

            inputFileWatcher = new FileSystemWatcher(@"C:\xbrl\schematronpickup");

            inputFileWatcher.Changed += (sender, a) =>
                {
                    logger.Log(string.Format("{0} changed", a.FullPath));
                    // check this file is not already in the queue
                    if (!queue.Any(x=> x == a.FullPath))
                    {
                        queue.Enqueue(a.FullPath);
                        logger.Log(string.Format("Queued {0}", a.FullPath));
                    }
                };
            inputFileWatcher.EnableRaisingEvents = true;

        }

        protected override void OnContinue()
        {
            base.OnContinue();
            eventLog1.WriteEntry("Continued");
            inputFileWatcher.EnableRaisingEvents = true;
            timer.Enabled = true;
        }

        protected override void OnStop()
        {
            base.OnStop();
            eventLog1.WriteEntry("Stopping");
            inputFileWatcher.EnableRaisingEvents = false;
            timer.Enabled = false;

            foreach (var backgroundWorker in workers)
            {
                if (backgroundWorker.IsBusy)
                {
                    backgroundWorker.CancelAsync();    
                }
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            eventLog1.WriteEntry("Paused");
            timer.Enabled = false;
            inputFileWatcher.EnableRaisingEvents = false;
        }
    }
}
