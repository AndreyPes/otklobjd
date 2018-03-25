using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Web;
using System.Threading;
using System.IO;



namespace TestClient
{
  public  class Program
    {

 

        static void Main(string[] args)
        {
            streamMD1();        
        }


        public static void streamMD1()
        {

            XmlDocument doc = new XmlDocument();

            doc.Load(@"D:\\XmlTestDocCopy.xml");
            
            string strd = doc.InnerXml;
             SR1.XmlServiceClient sc2 = new SR1.XmlServiceClient();
            byte[] bytearr = Encoding.UTF8.GetBytes(strd);


            string jhasas=  sc2.Xml_Data(bytearr);
            doc.Load(@"D:\\XmlTestDocCopy2.xml");
            strd = doc.InnerXml;
            bytearr = Encoding.UTF8.GetBytes(strd);
            string teststr = sc2.Xml_Data(bytearr);
            Console.WriteLine("" + "\r\n" + teststr+ jhasas + "\r\n");
            Console.ReadKey();
            sc2.Close();     
        }
      }
}
