using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ticker
{
    class Alarm
    {
        private readonly Mutex mutex = new Mutex();
        private readonly double threshold;
        private SoundPlayer soundPlayer;
        public bool Enabled { get; set; } = true;

        public Alarm(double threshold)
        {
            soundPlayer = new SoundPlayer(@"C:\Windows\Media\Alarm01.wav");
            this.threshold = threshold;
        }

        public void TriggerAlarm(double value)
        {
            if (Enabled && value >= threshold)
            {
                Task.Run(() =>
                {
                    if (mutex.WaitOne(0))
                    {
                        soundPlayer.PlaySync();
                        mutex.ReleaseMutex();
                    }
                });
            }
        }
    }
}
