using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO;
using System.Timers;

namespace MyNewService
{
    public partial class S : ServiceBase
    {
        private TimeLogger logger;
        static int countText = 0;
        static int countLinks = 0;
        static int countMedia = 0;
        public S()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "MySource", "MyNewLog");
            }
            eventLog1.Source = "MySource";
            eventLog1.Log = "MyNewLog";
        }

        protected override void OnStart(string[] args)
        {
            //timer1.Tick += timer1_Tick;
            //timer1.Start();

            System.Timers.Timer timer = new System.Timers.Timer(8000);
            timer.Elapsed += timer1_Tick;
            timer.Start();
            //{
            //    logger = new TimeLogger();
            //    Thread loggerThread = new Thread(new ThreadStart(logger.Start));
            //    loggerThread.Start();
            //}       // basic example      
            eventLog1.WriteEntry("Сработало + In OnStart.");
        }

        protected override void OnStop()
        {
            
            //using (System.IO.StreamWriter file = new System.IO.StreamWriter("T:\\TextOnStop.txt", true))
            //{
            //    file.WriteLine("Всего таблиц: " + db.Table.Count().ToString());
            //}
            //if (db.Table.Count() > 0)
            //{
            //    int i = 0;
            //    Authors[] authors = new Authors[db.Table.Count()];
            //    Texts[] text = new Texts[db.Table.Count()];
            //    PicVids[] picVids = new PicVids[db.Table.Count()];
            //    foreach (Models.Table table in tables)
            //    {
            //        //Authors auth = new Authors() { Author = table.Author, Data_Post_Id = table.Data_Post_ID }; authors[i] = auth;
            //        Texts txt = new Texts() { Text = table.Text, Data_Post_Id = table.Data_Post_ID }; text[i] = txt;
            //        List<string> a = new List<string>(); a.Add(table.Media);
            //        PicVids pv = new PicVids() { Data_Post_Id = table.Data_Post_ID, MainContent = a }; picVids[i] = pv;
            //        i++;
            //    }
            //    //DataContractJsonSerializer jsonFormatter1 = new DataContractJsonSerializer(typeof(Authors[]));
            //    //using (FileStream fs = new FileStream("T:\\posts4.json", FileMode.OpenOrCreate))
            //    //{
            //    //    jsonFormatter1.WriteObject(fs, authors);
            //    //}
            //    DataContractJsonSerializer jsonFormatter2 = new DataContractJsonSerializer(typeof(Texts[]));
            //    using (FileStream fs = new FileStream("T:\\posts5.json", FileMode.OpenOrCreate))
            //    {
            //        jsonFormatter2.WriteObject(fs, text);
            //    }
            //    DataContractJsonSerializer jsonFormatter3 = new DataContractJsonSerializer(typeof(PicVids[]));
            //    using (FileStream fs = new FileStream("T:\\posts6.json", FileMode.OpenOrCreate))
            //    {
            //        jsonFormatter3.WriteObject(fs, picVids);
            //    }
            //    using (System.IO.StreamWriter file = new System.IO.StreamWriter("T:\\Finally.txt", true))
            //    {
            //        tables = (from t in db.Table select t).ToList();
            //        foreach (Models.Table table in tables)
            //        {
            //            file.WriteLine(table.Id);
            //            file.WriteLine(table.Author);
            //            file.WriteLine(table.Text);
            //            file.WriteLine(table.Media);
            //            file.WriteLine(table.Data_Post_ID);
            //            file.WriteLine("/-----------------");
            //        }                    
            //    }
            //}
            //logger.Stop();
            eventLog1.WriteEntry("In OnStop.");
        }

        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e)
        {
            
        }

        private void eventLog1_EntryWritten_1(object sender, EntryWrittenEventArgs e)
        {

        }
        private void JSON_Read(int file_number)
        {
            Models.MyNewDatabaseTables db = new Models.MyNewDatabaseTables();
      
            using (FileStream fs = new FileStream("T:\\post" + file_number.ToString() + ".json", FileMode.OpenOrCreate))
            {
                switch (file_number)
                {
                    case 1:
                        {
                            List<Models.TableText> table = (from tab in db.TableText select tab).ToList();
                            Models.TableText t = new Models.TableText();
                            int id = 0;
                            if (table.Any())
                                id = table[table.Count - 1].Id + 1;
                            t.Id = id;
                            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Texts[]));
                            Texts[] text = (Texts[])jsonFormatter.ReadObject(fs);
                            if (table.Count < text.Length)   //Если ещё есть элементы, которые мы не загнали в БД, то продолжаем это делать
                            {
                                if (text[table.Count].Text != "")
                                    t.Text = text[table.Count].Text;
                                else
                                    t.Text = "Нет текста";
                                db.TableText.Add(t);
                                db.SaveChanges();
                                using (System.IO.StreamWriter file = new System.IO.StreamWriter("T:\\Status" + file_number.ToString() + ".txt", true))
                                {
                                    file.WriteLine("Считан " + file_number + " json файл и добавлена " + t.Id + " таблица");
                                    file.WriteLine(t.Id);
                                    file.WriteLine(t.Text);
                                    file.WriteLine();
                                }
                            }
                            else             //Если новых элементов нет, то мы ничего не делаем
                                using (System.IO.StreamWriter file = new System.IO.StreamWriter("T:\\Status" + file_number.ToString() + ".txt", true))                                
                                    file.WriteLine("Нет новых элементов для считывания!");
                            
                            //using (System.IO.StreamWriter file = new System.IO.StreamWriter("T:\\Status.txt", true))
                            //{
                            //    file.WriteLine("Считан " + file_number + " и добавлена " + t.Id + " таблица");
                            //}
                            
                            break;
                        }

                    case 2:
                        {
                            List<Models.TableLinks> table = (from tab in db.TableLinks select tab).ToList();
                            Models.TableLinks t = new Models.TableLinks();
                            int id = 0;
                            if (table.Any())
                                id = table[table.Count - 1].Id + 1;
                            t.Id = id;
                            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Link[]));
                            Link[] links = (Link[])jsonFormatter.ReadObject(fs);
                            if (table.Count < links.Length)
                            {
                                if (links[table.Count].Links.Any())
                                    t.Links = links[table.Count].Links[0];
                                else
                                    t.Links = "Нет элементов";
                                db.TableLinks.Add(t);
                                db.SaveChanges();
                                using (System.IO.StreamWriter file = new System.IO.StreamWriter("T:\\Status" + file_number.ToString() + ".txt", true))
                                {
                                    file.WriteLine("Считан " + file_number + " json файл и добавлена " + t.Id + " таблица");
                                    file.WriteLine(t.Id);
                                    file.WriteLine(t.Links);
                                    file.WriteLine();
                                }
                            }
                            else             
                                using (System.IO.StreamWriter file = new System.IO.StreamWriter("T:\\Status" + file_number.ToString() + ".txt", true))
                                    file.WriteLine("Нет новых элементов для считывания!");
                            
                            break;
                        }

                    case 3:
                        {
                            List<Models.TableLinks> table = (from tab in db.TableLinks select tab).ToList();
                            Models.TableMedia t = new Models.TableMedia();
                            int id = 0;
                            if (table.Any())
                                id = table[table.Count - 1].Id + 1;
                            t.Id = id;
                            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(PicVids[]));
                            PicVids[] pv = (PicVids[])jsonFormatter.ReadObject(fs);
                            if (table.Count < pv.Length)
                            {
                                if (pv[table.Count].MainContent.Any())
                                    t.Media = pv[table.Count].MainContent[0];
                                else
                                    t.Media = "Нет элементов";
                                db.TableMedia.Add(t);
                                db.SaveChanges();

                                using (System.IO.StreamWriter file = new System.IO.StreamWriter("T:\\Status" + file_number.ToString() + ".txt", true))
                                {
                                    file.WriteLine("Считан " + file_number + " json файл и добавлена " + t.Id + " таблица");
                                    file.WriteLine(t.Id);
                                    file.WriteLine(t.Media);
                                    file.WriteLine();
                                }
                            }
                            else             
                                using (System.IO.StreamWriter file = new System.IO.StreamWriter("T:\\Status" + file_number.ToString() + ".txt", true))
                                    file.WriteLine("Нет новых элементов для считывания!");
                            break;
                        }

                    case 4:
                        {
                            //DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Authors[]));
                            //Authors[] auth = (Authors[])jsonFormatter.ReadObject(fs);
                            //if (count <= auth.Length)
                            //    t.Author = auth[count].Author;
                            break;
                        }
                }

            }            
           
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            //using (StreamWriter file = new StreamWriter("T:\\Test.txt", true))            
            //    file.WriteLine("Таймер тикает");            
            Thread T1 = new Thread(() => JSON_Read(1));
            Thread T2 = new Thread(() => JSON_Read(2));
            Thread T3 = new Thread(() => JSON_Read(3));
            string CanWork = "";
            while (CanWork != "2")
            {
                using (StreamReader sr = new StreamReader(@"T:\Process.txt"))
                    CanWork = sr.ReadLine();
                if (CanWork != "2") Thread.Sleep(5000);
            }
            T1.Start();
            T2.Start();
            T3.Start();
            while (T1.IsAlive || T2.IsAlive || T3.IsAlive) { }
            using (StreamWriter sw = new StreamWriter(@"T:\Process.txt", false))
                sw.WriteLine(1);
            //Thread.Sleep(6000);
            //JSON_Read(1);
            //JSON_Read(2);    
            //JSON_Read(3);
        }
    }
}
