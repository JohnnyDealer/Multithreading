using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace MyNewService
{
    class Elements
    {
    }
    [DataContract]
    public class Post      //новостной блок
    {
        public static int _id = 0;
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string Author { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public List<string> MainContent = new List<string>();
        [DataMember]
        public List<string> Links = new List<string>();
        [DataMember]
        public string Data_Post_Id { get; set; }
        public Post()
        {
            _id++;
            ID = _id;
        }
    }
    [DataContract]
    class Texts
    {
        public int ID { get; set; }
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public string Data_Post_Id { get; set; }
    }
    [DataContract]
    public class PicVids
    {

        public int ID { get; set; }
        [DataMember]
        public List<string> MainContent = new List<string>();
        [DataMember]
        public string Data_Post_Id { get; set; }
    }
    [DataContract]
    public class Authors
    {

        public int ID { get; set; }
        [DataMember]
        public string Author { get; set; }
        [DataMember]
        public string Data_Post_Id { get; set; }
    }
    [DataContract]
    public class Link
    {
        public int ID { get; set; }
        [DataMember]
        public string Data_Post_Id { get; set; }
        [DataMember]
        public List<string> Links = new List<string>();
    }
}
