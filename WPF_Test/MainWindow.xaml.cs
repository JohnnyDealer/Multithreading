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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        [DataContract]
        public class Post      //новостной блок
        {
            [DataMember]
            public string Author { get; set; }
            [DataMember]
            public string Text { get; set; }
            [DataMember]
            public List<string> MainContent = new List<string>();
            public static void GetNews(ChromeDriver cd)
            {
            }
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
        private void Write_Info_ToFile(IWebElement element, string path)
        {
            List<string> pictures = GetPostMainContent(element);
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
            {
                sw.WriteLine(GetPostAuthor(element) + " :");
                sw.WriteLine();
                sw.WriteLine(GetPostText(element));
                sw.WriteLine();
                for (int i = 0; i < pictures.Count; i++) sw.WriteLine(pictures[i]);
                sw.WriteLine();
                sw.WriteLine("---------------------------------------\n");
                sw.Close();
            }
        }
        private string GetPostAuthor(IWebElement element)
        {
            return element.FindElement(By.ClassName("author")).Text;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string WriteFilePath = @"output.txt";
            ChromeDriver chromeDriver = new ChromeDriver();
            chromeDriver.Navigate().GoToUrl("https://vk.com/feed"); 
            LogIn("79852981725", "Dealer4465", chromeDriver); //регистрация vk.com            
            Thread.Sleep(1000);
            var posts = (from item in chromeDriver.FindElementsByClassName("feed_row") where item.Displayed select item).ToList(); //новостные блоки
            Queue<Post> Posts = new Queue<Post>();
            foreach (IWebElement post in posts)
            {
                if (post.Text == "") continue;    //Если новость "существует", бывает пустой блок, а это от него защита
                else
                {
                    if (AdLock(post, "post_date")) continue;  //Если это рекламные пост или пост с возвожными друзьями, то мы его пропускаем
                    else
                    {
                        Post p = new Post() {Author = GetPostAuthor(post), Text = GetPostText(post), MainContent = GetPostMainContent(post)};
                        Posts.Enqueue(p);
                        Write_Info_ToFile(post, WriteFilePath);
                    }
                }
            }
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(Post[]));
            using (FileStream fs = new FileStream("posts.json", FileMode.OpenOrCreate))
            {
                Post[] PostToSerialize = Posts.ToArray();
                jsonFormatter.WriteObject(fs, PostToSerialize);
            }
        }
    }
}
