using System;
using System.Text;
using System.Xml;

namespace TestClient
{
  public  class Program
    {
        static void Main(string[] args)
        {
            SendData();        
        }

        public static void SendData()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(@"D:\\XmlTestDocCopy.xml");
            
            string _strInerXml = doc.InnerXml;
            SR1.XmlServiceClient sc2 = new SR1.XmlServiceClient();
            byte[] bytearr = Encoding.UTF8.GetBytes(_strInerXml);


            string response1 =  sc2.Xml_Data(bytearr);
            doc.Load(@"D:\\XmlTestDocCopy2.xml");
            _strInerXml = doc.InnerXml;
            bytearr = Encoding.UTF8.GetBytes(_strInerXml);
            string response2 = sc2.Xml_Data(bytearr);
            Console.WriteLine("" + "\r\n" + response1 + response2 + "\r\n");
            Console.ReadKey();
            sc2.Close();     
        }
      }
}
