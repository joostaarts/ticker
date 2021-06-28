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
        private SoundPlayer soundPlayer;
        public bool Enabled { get; set; } = true;
        public double Threshold { get; set; }

        public Alarm(double threshold)
        {
            soundPlayer = new SoundPlayer(@"C:\Windows\Media\Alarm01.wav");
            Threshold = threshold;
        }

        public void TriggerAlarm(double value)
        {
            if (Enabled && value >= Threshold)
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
