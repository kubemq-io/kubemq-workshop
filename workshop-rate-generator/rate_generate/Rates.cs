using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace rate_generate
{
    /// <summary>
    /// Rates: A class that contain the rate detail and generate is value
    /// </summary>
    internal class Rates
    {
        internal bool isActive;
        internal string rateName;
        internal double buy;
        internal double sell;
        internal int id;
        private static System.Timers.Timer rateChanger;
        public Rates(string pName, int pId)
        {
            isActive = true;
            rateName = pName;
            buy = GetRateInitialValue();
            sell = GetRateInitialValue();
            id = pId;
            SetRateChangeTimer();
        }
        /// <summary>
        /// Set a timer changing the rate.
        /// </summary>
        private void SetRateChangeTimer()
        {
            rateChanger = new System.Timers.Timer(Manager.rnd.Next(550, 880));

            rateChanger.Elapsed += OnRateChangeEvent;
            rateChanger.AutoReset = true;
            rateChanger.Enabled = true;
        }

        /// <summary>
        /// Rate Change event.
        /// </summary>
        private void OnRateChangeEvent(object sender, ElapsedEventArgs e)
        {
            buy = GetDoubleRandomNumber(buy - 0.05, buy + 0.05, buy);
            sell = GetDoubleRandomNumber(sell - 0.05, sell + 0.05, sell);
        }

        /// <summary>
        /// Generate random relative positive double.
        /// </summary>
        /// <param name="minimum">Minimum Change</param>
        /// <param name="maximum">Maximum Change</param>
        /// <param name="currentValue">The current Value</param>
        /// <returns>A relative value that is not under 1 and does not exceed 2</returns>
        private double GetDoubleRandomNumber(double minimum, double maximum, double currentValue)
        {
            var next = Manager.rnd.NextDouble();

            currentValue = Manager.rnd.NextDouble() * Math.Abs(maximum - minimum) + minimum;
            if (currentValue < 1)
            {
                currentValue = Manager.rnd.NextDouble() * Math.Abs(maximum - (minimum)) + minimum + 0.05;
            }
            else if (currentValue > 2)
            {
                currentValue = Manager.rnd.NextDouble() * Math.Abs((maximum - 0.05) - minimum) + minimum;
            }
            return currentValue;
        }

        /// <summary>
        /// Get Initial value for rate.
        /// </summary>
        /// <returns> int between 1-2</returns>
        private int GetRateInitialValue()
        {
            return Manager.rnd.Next(1, 2);
        }
    }
}
