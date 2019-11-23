using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace MyNewService
{
    class TimeLogger
    {
        Timer timer;
        // Запуск процесса записи
        static int count = 0;
        public void Start()
        {
            timer = new Timer(1000);
            timer.Elapsed += this.Log;
            timer.Start();
        }
        // Завершение записи
        public void Stop()
        {
            timer.Stop();
        }
        // Собственно запись
        private void Log(Object source, ElapsedEventArgs e)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter("T:\\Text.txt", true))
            {
                file.WriteLine(DateTime.Now.ToString());
            }
        }
       
    }
}
