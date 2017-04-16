using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ham2.utility
{
    public class ProcessHelper
    {
        private string _path;
        private Process _runningProc;
        private List<string> _standardError = new List<string>();
        private List<string> _standardOut = new List<string>();

        public ProcessHelper(string path) => this._path = path;

        public event EventHandler<ErrorDataReceivedEventArgs> ErrorDataReceived;
        public event EventHandler<FFmpegProcessExitedEventArgs> FFmpegProcessExited;
        public event EventHandler FFmpegProcessStarted;
        public event EventHandler<OutputDataReceivedEventArgs> OutputDataReceived;

        private void ProcErrorDataReceived(object sender, DataReceivedEventArgs e) =>
            OnErrorDataReceived(e.Data);

        private void ProcExited(object sender, EventArgs e)
        {
            var exitcode = this._runningProc.ExitCode;
            var endTime = this._runningProc.ExitTime;
            var startTime = this._runningProc.StartTime;
            var totalProcTime = this._runningProc.TotalProcessorTime;
            var userTime = this._runningProc.UserProcessorTime;

            OnFFmpegProcessExited(exitcode, endTime, startTime, totalProcTime, userTime);
        }

        private void ProcOutputDataReceived(object sender, DataReceivedEventArgs e) =>
            OnOutputDataReceived(e.Data);

        private async Task<int> WaitForProcessAsync(Process proc)
        {
            await Task.Run(new Action(proc.WaitForExit));
            return proc.ExitCode;
        }

        internal void KillProcess()
        {
            if (this._runningProc == null)
            {
                return;
            }

            this._runningProc.Kill();
        }

        internal void StandardIn(string v)
        {
            this._runningProc.StandardInput.Write(v);
            this._runningProc.StandardInput.Flush();
        }

        public void OnErrorDataReceived(string errorReceived)
        {
            EventHandler<ErrorDataReceivedEventArgs> t = ErrorDataReceived;
            t?.Invoke(this, new ErrorDataReceivedEventArgs(errorReceived));
        }

        public void OnFFmpegProcessExited(int exitCode, DateTime endTime, DateTime startTime, TimeSpan totalProcTime, TimeSpan userTime)
        {
            EventHandler<FFmpegProcessExitedEventArgs> t = FFmpegProcessExited;
            t?.Invoke(this, new FFmpegProcessExitedEventArgs(exitCode, endTime, startTime, totalProcTime, userTime));
        }

        public void OnFFmpegProcessStarted()
        {
            EventHandler t = FFmpegProcessStarted;
            t?.Invoke(this, new EventArgs());
        }

        public void OnOutputDataReceived(string outputReceived)
        {
            EventHandler<OutputDataReceivedEventArgs> t = OutputDataReceived;
            t?.Invoke(this, new OutputDataReceivedEventArgs(outputReceived));
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            var si = new ProcessStartInfo(this._path)
            {
                Arguments = Arguments,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var proc = Process.Start(si);
            string se = string.Empty;
            var so = string.Empty;

            while (proc.StandardOutput.EndOfStream == false)
            {
                so += proc.StandardOutput.ReadToEnd();
            }

            while (proc.StandardError.EndOfStream == false)
            {
                se += proc.StandardError.ReadToEnd();
            }

            proc.Close();

            var lines = se.Split('\r', '\n');
            StandardError.Clear();
            foreach (var item in lines)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                StandardError.Add(item);
            }

            lines = so.Split('\r', '\n');
            StandardOut.Clear();
            foreach (var item in lines)
            {
                if (string.IsNullOrEmpty(item))
                {
                    continue;
                }
                StandardOut.Add(item);
            }
        }

        /// <summary>
        /// Starts asynchronously.
        /// </summary>
        /// <returns></returns>
        public async Task<int> StartAsync()
        {
            var si = new ProcessStartInfo(this._path)
            {
                Arguments = Arguments
            };

            this._runningProc = new Process()
            {
                StartInfo = si
            };

            this._runningProc.StartInfo.UseShellExecute = false;
            this._runningProc.StartInfo.CreateNoWindow = true;

            this._runningProc.StartInfo.RedirectStandardError = true;
            this._runningProc.StartInfo.RedirectStandardOutput = true;
            this._runningProc.StartInfo.RedirectStandardInput = true;

            this._runningProc.EnableRaisingEvents = true;
            this._runningProc.ErrorDataReceived += ProcErrorDataReceived;
            this._runningProc.OutputDataReceived += ProcOutputDataReceived;

            this._runningProc.Exited += ProcExited;

            this._runningProc.Start();
            OnFFmpegProcessStarted();

            this._runningProc.BeginErrorReadLine();
            this._runningProc.BeginOutputReadLine();

            Task<int> result = WaitForProcessAsync(this._runningProc);
            var exitcode = await result;

            this._runningProc.ErrorDataReceived -= ProcErrorDataReceived;
            this._runningProc.Exited -= ProcExited;
            this._runningProc.OutputDataReceived -= ProcOutputDataReceived;

            return exitcode;
        }

        public string Arguments
        {
            get; set;
        }

        public bool IsAsyncProcessRunning => this._runningProc?.HasExited == false;

        public List<string> StandardError => this._standardError;

        public List<string> StandardOut => this._standardOut;

        public class ErrorDataReceivedEventArgs : EventArgs
        {
#pragma warning disable RECS0154 // Parameter is never used

            public ErrorDataReceivedEventArgs(string errorDataReceived) =>
#pragma warning restore RECS0154 // Parameter is never used
                ErrorData = errorDataReceived;

            public string ErrorData
            {
                get;
            }
        }
        public class FFmpegProcessExitedEventArgs : EventArgs
        {
            public FFmpegProcessExitedEventArgs(int exitCode, DateTime endTime, DateTime startTime, TimeSpan totalProcTime, TimeSpan userTime)
            {
                ExitCode = exitCode;
                EndTime = endTime;
                StartTime = startTime;
                TotalProcTime = totalProcTime;
                UserTime = userTime;
            }

            public DateTime EndTime
            {
                get; private set;
            }

            public int ExitCode
            {
                get; private set;
            }

            public DateTime StartTime
            {
                get; private set;
            }

            public TimeSpan TotalProcTime
            {
                get; private set;
            }

            public TimeSpan UserTime
            {
                get; private set;
            }
        }
        public class OutputDataReceivedEventArgs : EventArgs
        {
#pragma warning disable RECS0154 // Parameter is never used

            public OutputDataReceivedEventArgs(string outputReceived) =>
#pragma warning restore RECS0154 // Parameter is never used
                OutputData = outputReceived;

            public string OutputData
            {
                get;
            }
        }
    }
}