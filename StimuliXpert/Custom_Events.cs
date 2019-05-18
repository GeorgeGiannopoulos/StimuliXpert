using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StimuliXpert
{
    public class FloatEventArgs : EventArgs
    {
        private float number;

        public FloatEventArgs(float number)
        {
            this.number = number;
        }

        public float Number
        {
            get { return number; }
        }
    }

    public class IntEventArgs : EventArgs
    {
        private int number;

        public IntEventArgs(int number)
        {
            this.number = number;
        }

        public int Number
        {
            get { return number; }
        }
    }

    public class BoolEventArgs : EventArgs
    {
        private bool state;

        public BoolEventArgs(bool state)
        {
            this.state = state;
        }

        public bool State
        {
            get { return state; }
        }
    }

    public class TextEventArgs : EventArgs
    {
        private string text;

        public TextEventArgs(string text)
        {
            this.text = text;
        }

        public string Text
        {
            get { return text; }
        }
    }
}
