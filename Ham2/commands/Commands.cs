using System;
using System.Linq;
using System.Windows.Input;

namespace Ham2.commands
{
    public static class Commands
    {
        public static readonly ICommand PlaceHolderCommand = new RoutedCommand("Place Marker", typeof(Commands));
    }
}
