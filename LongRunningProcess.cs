using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

namespace WindowsService1
{
    public class LongRunningProcess : IProcess
    {
        private ILogger logger;
        private int runTime;
        private string file;

        public LongRunningProcess(string file, int runTime, ILogger logger)
        {
            this.logger = logger;
            this.runTime = runTime;
            this.file = file;
        }

        public void Start()
        {
            logger.Log(string.Format("Starting {0} for {1} seconds", file, runTime));
            Thread.Sleep(runTime * 1000);
            logger.Log(string.Format("Finished {0}", file));
        }
    }
}
