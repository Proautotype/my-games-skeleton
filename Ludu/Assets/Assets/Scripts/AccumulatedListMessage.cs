using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class AccumulatedListMessage
    {
        bool clear = false;
        bool polarity = false;
        int number { get; set; } = 0;
        public AccumulatedListMessage(bool clear, bool polarity, int number)
        {
            this.clear = clear;
            this.polarity = polarity;
            this.number = number;
        }

        public AccumulatedListMessage(int number, bool polarity)
        {
            this.number = number;
            this.polarity = polarity;
        }

        public AccumulatedListMessage(bool clear)
        {
            this.clear = clear;
        }

        public int Number
        {
            get => this.number;
            set => this.number  = (int)value;
        }

        public bool Polarity
        {
            get => this.polarity;
        }

        public override string ToString()
        {
            return $"clear {clear} + polarity {polarity} + number {number} ";
        }
    }
}
