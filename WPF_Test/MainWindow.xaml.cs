using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;


namespace WPF_Test
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        public delegate void Method(int i);
        public MainWindow()
        {
            InitializeComponent();
            
        }
        
        private List<string> GetPostLinks(IWebElement element)
        {
            IWebElement div = element;
            List<string> links = new List<string>();
            List<IWebElement> post_text = div.FindElements(By.ClassName("wall_post_text")).ToList();
            if (post_text.Any())
            {
                List<IWebElement> post_links = (div.FindElement(By.ClassName("wall_post_text")).FindElements(By.TagName("a"))).ToList();
                //List<IWebElement> post_links = post_text.FindElements(By.TagName("a")).ToList();
                foreach (IWebElement link in post_links)
                {

                    var a = link.GetAttribute("href");
                    if (a == null)
                        links.Add("");
                    else
                        links.Add(link.GetAttribute("href"));
                }
                return links;
                
            }
            else
                return links;
        }
        private void LogIn(string username, string password, ChromeDriver cd)
        {
            cd.Navigate().GoToUrl("https://vk.com/feed");
            List<IWebElement> webElements = (from item in cd.FindElementsById("email") where item.Displayed select item).ToList();
            foreach (IWebElement element in webElements)
            {
                element.Click();
                element.SendKeys(username);
            }
            webElements = (from item in cd.FindElementsById("pass") where item.Displayed select item).ToList();
            foreach (IWebElement element in webElements)
            {
                element.Click();
                element.SendKeys(password);
            }
            webElements = cd.FindElementsById("login_button").ToList();
            webElements[0].Click();
        }
        private bool AdLock(IWebElement element, string classname)  //Проверка на рекламную запись
        {
            IWebElement advertisement;
            try                                                                 //Если рассматриваемый блок рекламный
            {
                advertisement = element.FindElement(By.ClassName(classname));   
                if (advertisement != null) return advertisement.Text.Contains("Рекламная запись");
                else return false;
            }
            catch(OpenQA.Selenium.NoSuchElementException)  //Если это не рекламная запись, но и не новостной блок, то мы его тоже пропускаем
            {
                return true;
            }           
        }
        private List<string> GetPostMainContent(IWebElement element)
        {
            IWebElement div = element;
            List<string> pictures = new List<string>();
            try
            {
                //div = element.FindElement(By.ClassName("page_post_sized_thumbs"));   //Находим блок с рисунками, видео или гифками
                List<IWebElement> post_pics_elements = div.FindElements(By.ClassName("page_post_thumb_wrap")).ToList(); //Переключаемся непосредственно на ссылку
                foreach(IWebElement picture in post_pics_elements)
                {
                    var anchor = picture.GetAttribute("href");
                    if(anchor == null) pictures.Add(picture.GetCssValue("background-image").Split('"')[1]);
                    else pictures.Add(anchor);
                }
            }
            catch(OpenQA.Selenium.NoSuchElementException)                 //Если такой блок не нашли
            {
                div = element.FindElement(By.ClassName("page_doc_photo"));
                pictures.Add(div.GetCssValue("background-image").Split('"')[1]);
            }
            return pictures;
                       
        }   
        private void Write_Info_ToFile(IWebElement element, string path)
        {
            List<string> pictures = GetPostMainContent(element);
            List<string> links = GetPostLinks(element);
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
            {
                sw.WriteLine(GetPostAuthor(element) + " :");
                sw.WriteLine();
                sw.WriteLine(GetPostText(element));
                sw.WriteLine();
                for (int i = 0; i < links.Count; i++) sw.WriteLine(links[i]);
                sw.WriteLine();
                for (int i = 0; i < pictures.Count; i++) sw.WriteLine(pictures[i]);
                sw.WriteLine();
                sw.WriteLine("---------------------------------------\n");
                sw.Close();
            }
        }

        private string GetPostText(IWebElement element)
        {            
            List<IWebElement> post_text_elements = element.FindElements(By.ClassName("wall_post_text")).ToList(); //Находим блок с текстом
            if (post_text_elements.Count != 0)    //Если такой блок найден (текста может и не быть), то:
            { 
                if (post_text_elements[0].Text.Contains("Показать полностью…"))   //Если есть скрытая часть текста, то мы её открываем:
                {
                    post_text_elements[0].FindElement(By.ClassName("wall_post_more")).Click();
                    return post_text_elements[0].Text;
                }
                else return post_text_elements[0].Text;  
            }
            else return "";
        }
        private string GetPostAuthor(IWebElement element)
        {
            return element.FindElement(By.ClassName("author")).Text;
        }
        private string GetPostId(IWebElement element)  //возвращает уникальный ID поста
        {
            string str = "";
            List<IWebElement> post = element.FindElements(By.ClassName("_post")).ToList();
            if (post.Count > 0)
                str = post[0].GetAttribute("data-post-id");
            else str = "";
            return str;
        }
        private bool RepeatLock(string data_post_id)
        {
            Post[] posts = PostsContainer.ToArray();
            foreach (Post post in posts)
                if (post.Data_Post_Id == data_post_id) return true;
            return false;
        }
        private void Parsing_Click(object sender, RoutedEventArgs e)
        {
            string WriteFilePath = @"output.txt";
            ChromeDriver chromeDriver = new ChromeDriver();
            chromeDriver.Navigate().GoToUrl("https://vk.com/feed");
            string login;
            string password;
            using (StreamReader sw = new StreamReader("login.txt"))
            {
                login = sw.ReadLine();
                password = sw.ReadLine();
                sw.Close();
            }            
            LogIn(login, password, chromeDriver); //регистрация vk.com      
            while(Semaphore.iterator != 5)  //АЛГОРИТМ ПЛАНИРОВАНИЯ ПОТОКОВ И РЕСУРСОВ         !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            {
                Thread.Sleep(5000);
                var posts = (from item in chromeDriver.FindElementsByClassName("feed_row") where item.Displayed select item).ToList(); //новостные блоки            
                foreach (IWebElement post in posts)
                {
                    if (post.Text == "") continue;    //Если новость "существует", бывает пустой блок, а это от него защита
                    else
                    {
                        if (AdLock(post, "post_date")) continue;  //Если это рекламные пост или пост с возвожными друзьями, то мы его пропускаем
                        else
                        {
                            if (RepeatLock(GetPostId(post)))
                                continue;
                            else
                            {
                                Post p = new Post()
                                {
                                    Author = GetPostAuthor(post),
                                    Text = GetPostText(post),
                                    Links = GetPostLinks(post),
                                    MainContent = GetPostMainContent(post),
                                    Data_Post_Id = GetPostId(post)
                                };
                                PostsContainer.Add(p);
                            }
                            Write_Info_ToFile(post, WriteFilePath);
                        }
                    }
                }
                //MessageBox.Show("Парсинг завершён!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                int[] captions = new int[Semaphore.table.GetUpperBound(1) + 1]; //Определем количество элементов в строке данного table[iterator][];
                for (int j = 0; j < captions.Length; j++)
                    captions[j] = Semaphore.table[Semaphore.iterator, j];
                Thread T1 = new Thread(() => { });
                Thread T2 = new Thread(() => { }); 
                Thread T3 = new Thread(() => { });
                Thread T4 = new Thread(() => { });
                for(int i = 0; i < captions.Length; i++)
                {
                    switch (captions[i])
                    {
                        case 1:
                            T1 = new Thread(() => JSON_Write(captions[i]));
                            T1.Start();
                           // T1.Join();
                            break;     
                        case 2:
                            T2 = new Thread(() => JSON_Write(captions[i]));
                            T2.Start();
                            //T2.Join();
                            break;
                        case 3:
                            T3 = new Thread(() => JSON_Write(captions[i]));
                            T3.Start();
                          //  T3.Join();
                            //Thread.Sleep(100);
                            break;                                                   
                        case 4:
                            T4 = new Thread(() => JSON_Read(i + 1));  //Если читаем файл, то читаем i+1 (то есть важна позиция T4 в таблице);
                            T4.Start();
                           // T4.Join();
                            break;
                    }
                    while (T1.IsAlive || T2.IsAlive || T3.IsAlive || T4.IsAlive) 
                    { }
                }
                //T1.Start();
                //T2.Start();
                //T3.Start();
                //T4.Start();
                bool[] readiness = new bool[] {true, true, true, true};
                while(readiness.Contains(true))
                {
                    if (T1.IsAlive) continue;
                    else readiness[0] = false;
                    if (T2.IsAlive) continue;
                    else readiness[1] = false;
                    if (T3.IsAlive) continue;
                    else readiness[2] = false;
                    if (T4.IsAlive) continue;
                    else readiness[3] = false;
                }
                //Потоки завершили работу;
                Semaphore.iterator++;
                chromeDriver.Navigate().Refresh();
            }                              
        }
        private void Nothing(int i)
        {

        }
        private void JSON_Write(int file_number)
        {
            
            Post[] PostsToSerialize = PostsContainer.ToArray();
            int _size = PostsToSerialize.Length;
            Authors[] auth = new Authors[_size];
            Texts[] text = new Texts[_size];
            Link[] links = new Link[_size];
            PicVids[] pv = new PicVids[_size];
            for (int i = 0; i < _size; i++)
            {
                auth[i] = new Authors() { Author = PostsToSerialize[i].Author, Data_Post_Id = PostsToSerialize[i].Data_Post_Id };
                text[i] = new Texts() { Text = PostsToSerialize[i].Text, Data_Post_Id = PostsToSerialize[i].Data_Post_Id};
                links[i] = new Link() { Links = PostsToSerialize[i].Links, Data_Post_Id = PostsToSerialize[i].Data_Post_Id };
                pv[i] = new PicVids() { MainContent = PostsToSerialize[i].MainContent, Data_Post_Id = PostsToSerialize[i].Data_Post_Id };
            }
            //using (FileStream fs = new FileStream("posts" + file_number.ToString() + ".json", FileMode.Append, FileAccess.Write))
            using (FileStream fs = new FileStream("T:\\post" + file_number.ToString() + ".json", FileMode.OpenOrCreate))
            {
                switch (file_number)
                {
                    
                    case 1:
                        {
                            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Texts[]));
                            jsonFormatter.WriteObject(fs, text);
                            break;
                        }

                    case 2:
                        {
                            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Link[]));
                            jsonFormatter.WriteObject(fs, links);
                            break;
                        }

                    case 3:
                        {
                            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(PicVids[]));
                            jsonFormatter.WriteObject(fs, pv);
                            break;
                        }
                    
                    case 4:
                        {
                            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Authors[]));
                            jsonFormatter.WriteObject(fs, auth);
                            break;
                        }
                }
            }
          //  MessageBox.Show("Поток записи в файл " + file_number.ToString() + " отработал!");
        }
        private void JSON_Read(int file_number)
        {           
            using (FileStream fs = new FileStream("T:\\post" + file_number.ToString() + ".json", FileMode.OpenOrCreate))
            {
                switch (file_number)
                {
                    case 1:
                        {
                            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Texts[]));
                            Texts[] text = (Texts[])jsonFormatter.ReadObject(fs);
                            using (StreamWriter sw = new StreamWriter("T:\\output" + file_number + ".txt", false, System.Text.Encoding.Default))
                            {
                                for (int i = 0; i < text.Length; i++)
                                {
                                    sw.WriteLine(text[i].Text);
                                    sw.WriteLine(text[i].Data_Post_Id);
                                    sw.WriteLine("------------------------------");
                                    sw.WriteLine();
                                }
                                sw.Close();
                            }
                            break;
                        }

                    case 2:
                        {
                            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Link[]));
                            Link[] links = (Link[])jsonFormatter.ReadObject(fs);
                            using (StreamWriter sw = new StreamWriter("T:\\output" + file_number + ".txt", false, System.Text.Encoding.Default))
                            {
                                for (int i = 0; i < links.Length; i++)
                                {
                                    for (int j = 0; j < links[i].Links.Count; j++)
                                        sw.WriteLine(links[i].Links[j]);
                                    sw.WriteLine(links[i].Data_Post_Id);
                                    sw.WriteLine("------------------------------");
                                    sw.WriteLine();
                                }
                                sw.Close();
                            }
                            break;
                        }

                    case 3:
                        {
                            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(PicVids[]));
                            PicVids[] pv = (PicVids[])jsonFormatter.ReadObject(fs);
                            using (StreamWriter sw = new StreamWriter("T:\\output" + file_number + ".txt", false, System.Text.Encoding.Default))
                            {
                                for (int i = 0; i < pv.Length; i++)
                                {
                                    for(int j =0; j < pv[i].MainContent.Count; j++)
                                        sw.WriteLine(pv[i].MainContent[j]);       
                                    sw.WriteLine(pv[i].Data_Post_Id);
                                    sw.WriteLine("------------------------------");
                                    sw.WriteLine();
                                }
                                sw.Close();
                            }
                            break;
                        }
                    
                    case 4:
                        {
                            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Authors[]));
                            Authors[] auth = (Authors[])jsonFormatter.ReadObject(fs);
                            using (StreamWriter sw = new StreamWriter("T:\\output" + file_number + ".txt", false, System.Text.Encoding.Default))
                            {
                                for (int i = 0; i < auth.Length; i++)
                                {
                                    sw.WriteLine(auth[i].Author);
                                    sw.WriteLine(auth[i].Data_Post_Id);
                                    sw.WriteLine("------------------------------");
                                    sw.WriteLine();
                                }
                                sw.Close();
                            }
                            break;
                        }
                }
            }
            
            //MessageBox.Show("Поток чтения " + file_number.ToString() + " отработал!");
        }
        private void Thread_Ready(Thread T)
        {
            bool ready = true;
            while(T.IsAlive)
            {
                ready = false;
            }
            ready = true;
        }
        private void Threads_Click(object sender, RoutedEventArgs e)
        {
            Thread T1 = new Thread(() => JSON_Write(1));                  
            Thread T2 = new Thread(() => JSON_Write(2));            
            Thread T3 = new Thread(() => JSON_Write(3));           
            Thread T4 = new Thread(() => JSON_Read(1));
            bool flag = true; bool ready = false;
            string[] readiness = new string[4];
            while (flag)
            {
                if (readiness[0] != "Поток 1 запущен")
                {
                    T1.Start();
                    readiness[0] = "Поток 1 запущен";
                    continue;
                }
                if (T1.IsAlive == true)
                {
                    T1.Join();
                    T4.Start();
                    continue;
                }
                else
                {

                    if (readiness[1] != "Поток 2 запущен")
                    {
                        T2.Start();
                        readiness[1] = "Поток 2 запущен";
                        continue;
                    }
                    if (T2.IsAlive || T4.IsAlive)
                        continue;
                    else  //Когда записался 2 файл и прочитался 1
                    {
                        if (readiness[2] != "Поток 3 запущен")
                        {
                            (T4 = new Thread(() => JSON_Read(2))).Start();  //Читает 2 файл  
                            T3.Start();
                            readiness[2] = "Поток 3 запущен";
                            readiness[3] = "Поток 4 запущен";
                            continue;
                        }
                        if (T3.IsAlive || T4.IsAlive)
                            continue;
                        else  //Когда записался 3 файл и прочитался 2 файл
                        {
                            if (readiness[3] == "Поток 4 запущен")
                            {
                                (T4 = new Thread(() => JSON_Read(3))).Start();  //Читает 3 файл
                                readiness[3] = "Поток 4 отработал";
                            }
                            if (T4.IsAlive)
                                continue;
                            else
                                flag = false;
                        }
                    }
                }
            }         //1 Вариант
            //string[] captions = new string[3];
            //int count = 0;
            //while (!ready)
            //{
            //    for (int i = 0; i < captions.Length; i++)
            //    {
            //        if (captions[i] == "Ready" && !T4.IsAlive)
            //        {
            //            T4 = new Thread(() => JSON_Read(i));
            //            captions[i] = "Finished";
            //        }
            //    }
            //    if (T1.IsAlive)
            //        captions[0] = "Not ready";
            //    else
            //        captions[0] = "Ready";
            //    if (T2.IsAlive)
            //        captions[1] = "Not ready";
            //    else
            //        captions[1] = "Ready";
            //    if (T3.IsAlive)
            //        captions[2] = "Not ready";
            //    else
            //        captions[2] = "Ready";
            //    for (int i = 0; i < captions.Length; i++)
            //    {
            //        if (captions[i] == "Finished")
            //        {
            //            captions[i] = "Exit";
            //            count++;
            //        }
            //    }
            //    if (count == 3) ready = true;
            //}
        }
        
    }
}
