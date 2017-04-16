using System;

namespace Ham2.viewmodel
{
    public class DispatchEventArgs : EventArgs
    {
#pragma warning disable RECS0154 // Parameter is never used

        public DispatchEventArgs(Action command)
#pragma warning restore RECS0154 // Parameter is never used
                => Command = command;

        public Action Command
        {
            get;
        }
    }
}