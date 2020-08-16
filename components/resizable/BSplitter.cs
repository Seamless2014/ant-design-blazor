using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace AntDesign
{
    internal class BSplitter
    {
        internal Action _propertyChanged;

        internal BsSettings BsbSettings { get; set; }

        internal int PreviousPosition { get; set; } = 0;
        internal int PreviousPosition2 { get; set; } = 0;

        internal int Position { get; set; } = 0;

        internal int Step { get; set; } = 0;

        //protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}

        internal void InvokePropertyChanged()
        {
            _propertyChanged?.Invoke();
        }
    }
}
