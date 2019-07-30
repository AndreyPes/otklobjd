using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.ServiceModel.Channels;
using System.Web.Configuration;

namespace WcfDataService
{

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall,
       ConcurrencyMode = ConcurrencyMode.Multiple)]

    public class XmlService : IXmlService
    {
        public const String SQLSecond_ConnectionString = "Data Source=server; Initial Catalog=basa; User ID=Andre; pwd=pass!";


        public string insertQuery;
        public string selectQuery;
        public string logMessage;
        public string clientMessage;
        public string Query;
        string error = "";
        string interval = "";
        char fatal = '\u2205';
        DateTime dtimenow = DateTime.Now;
        string fileName = DateTime.Now.AddDays(-7).ToShortDateString();
        string Sub = "";
        string Name = "";
        string errormessage = "";
        bool Ipchecker(string address)
        {
            bool fl = false;
            try
            {
                foreach (var s in ipConfigFile())
                {
                    if (s[0] == address)
                    {
                        Sub = s[1];
                        Name = s[2];

                    }
                }
                if (String.IsNullOrWhiteSpace(Sub) == false && String.IsNullOrWhiteSpace(Name) == false)
                {
                    fl = true;
                }
            }
            catch (Exception ex)
            {
                errormessage += "\r\n Ошибка IP адреса:" + address + " Sub=" + Sub + " Name=" + Name + " " + ex.ToString();
                clientMessage += "\r\n Ошибка IP адреса:" + address;
            }

            return fl;
        }

        public string Xml_Data(byte[] bytearr)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            string strd = Encoding.UTF8.GetString(bytearr);
            XmlTextReader reader = new XmlTextReader(new System.IO.StringReader(strd));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(strd);
            OperationContext context = OperationContext.Current;
            MessageProperties messageProperties = context.IncomingMessageProperties;
            RemoteEndpointMessageProperty endpointProperty =
            messageProperties[RemoteEndpointMessageProperty.Name]
            as RemoteEndpointMessageProperty;
            string hjfyt = messageProperties.Via.ToString();
            OperationContext context2 = OperationContext.Current;
            RemoteEndpointMessageProperty endpoint3 = (RemoteEndpointMessageProperty)context2.IncomingMessageProperties[RemoteEndpointMessageProperty.Name];
            string ip = endpoint3.Address;
            string ipString = string.Format("Client IP address is: {1} and Client port is: {0}", endpointProperty.Port, endpointProperty.Address);
            string[] ipaddressArr = endpointProperty.Address.Split('.');
            DisableDayObject dsobj = new DisableDayObject(0, 0, 0, 0);
            Ipchecker(ipaddressArr[0] + '.' + ipaddressArr[1]);
            errormessage += " " + Name;
            if (Ipchecker(ipaddressArr[0] + '.' + ipaddressArr[1]) != true)
            {
                errormessage += "\r\n Ошибка IP";
                clientMessage += "\r\n Ошибка IP";
            }
            if (IsInRange(dtimenow) != true)
            {
                errormessage += "\r\n Ошибка даты " + dtimenow;
                clientMessage += "\r\n Ошибка даты: " + dtimenow;

            }
            if (Ipchecker(ipaddressArr[0] + '.' + ipaddressArr[1]) == true && IsInRange(dtimenow) == true)
            {
                try
                {
                    reader.WhitespaceHandling = WhitespaceHandling.None;
                    while (reader.Read())
                        if (reader.NodeType == XmlNodeType.Element)
                        {


                            if (reader.Name == "Otkl_Obj")
                            {
                                try
                                {
                                    dsobj = new DisableDayObject(Int32.Parse(reader.GetAttribute("NP50")), Int32.Parse(reader.GetAttribute("NP50_200")), Int32.Parse(reader.GetAttribute("NP200_1000")), Int32.Parse(reader.GetAttribute("NP1000_plus")));

                                    errormessage += "\r\n NP1:" + dsobj.NP1 + "\r\n NP2:" + dsobj.NP2 + "\r\n NP3:" + dsobj.NP3 + "\r\n NP4:" + dsobj.NP4;

                                }

                                catch (Exception ex)
                                {
                                    errormessage += "\r\n Ошибка считывания данных: " + ex.ToString() + "\r\n " + strd;
                                    clientMessage += "\r\n Ошибка считывания данных! ";
                                }
                            }

                        }
                    DateTime dtnf = DateTime.Now.AddDays(-1);
                    string query = @"SELECT top 1 Z" + dtnf.Day + " FROM N_03 WHERE SUB=@SUB and dat=@dat and pok =@pok and ut=@ut  and obj=@obj and vid =@vid and per=@per";
                    using (SqlConnection SqlConnect = new SqlConnection(SQLSecond_ConnectionString))
                    {
                        using (SqlCommand SelectCommand = new SqlCommand(query, SqlConnect))
                        {
                            using (SqlDataAdapter adapter = new SqlDataAdapter(SelectCommand))
                            {
                                SelectCommand.CommandTimeout = 10000;
                                SelectCommand.Parameters.Add("@POK", System.Data.SqlDbType.NVarChar).Value = "0600";
                                SelectCommand.Parameters.Add("@UT", System.Data.SqlDbType.NVarChar).Value = "60";
                                SelectCommand.Parameters.Add("@SUB", System.Data.SqlDbType.NVarChar).Value = Sub;
                                SelectCommand.Parameters.Add("@otn", System.Data.SqlDbType.NVarChar).Value = "00";
                                SelectCommand.Parameters.Add("@OBJ", System.Data.SqlDbType.NVarChar).Value = "0000000000000000";
                                SelectCommand.Parameters.Add("@VID", System.Data.SqlDbType.NVarChar).Value = "05";
                                SelectCommand.Parameters.Add("@Per", System.Data.SqlDbType.NVarChar).Value = "03";/*03*/
                                SelectCommand.Parameters.Add("@DAT", System.Data.SqlDbType.NVarChar).Value = Convert.ToString(dtnf.ToString("yyyy-MM-01 00:00:00"));
                                if (SqlConnect.State == ConnectionState.Closed)
                                {
                                    SqlConnect.Open();
                                }
                                object objresult = SelectCommand.ExecuteScalar();
                                if (objresult != null)
                                {
                                    string resultVal = objresult.ToString();
                                    SqlConnect.Close();

                                    if (dsobj.Sum() != Convert.ToInt32(resultVal))
                                    {

                                        errormessage += "\r\n Ошибка суммы: возможно неправильное количество отключённых объектов (UT61+UT62+UT63+UT64!=UT60) \r\n Ut60=" + dsobj.Sum();
                                        clientMessage += "\r\n  Ошибка суммы: возможно неправильное количество отключённых объектов (UT61+UT62+UT63+UT64!=UT60) \r\n Ut60=" + dsobj.Sum();

                                    }

                                    if (resultVal == "9797979797,9797")
                                    {
                                        errormessage += "\r\n Ошибка возможно нет значения UT показателя в базе ";
                                        clientMessage += "\r\n Ошибка возможно нет значения UT показателя в базе " + resultVal.ToString();

                                    }



                                    if (resultVal != "9797979797,9797" && dsobj.Sum() == Convert.ToInt32(resultVal))
                                    {

                                        /// next connection str
                                        //SqlConnect = new SqlConnection("Data Source=server; Initial Catalog=base; User ID=Andrey; pwd=password");
                                        SqlCommandBuilder sqlcmdBuilder = new SqlCommandBuilder(adapter);
                                        try
                                        {
                                            if (SqlConnect.State == ConnectionState.Closed)
                                            {
                                                SqlConnect.Open();
                                            }
                                            SqlCommand InsertCommand = new SqlCommand("proc", SqlConnect);
                                            adapter.InsertCommand = InsertCommand;
                                            InsertCommand.CommandType = CommandType.StoredProcedure;
                                            InsertCommand.CommandTimeout = 10000;
                                            ;
                                            int v = 61;
                                            int iterator = 0;
                                            foreach (var s in dsobj)
                                            {
                                                if (iterator == 0)
                                                { v = 61; }
                                                if (iterator == 1)
                                                { v = 62; }
                                                if (iterator == 2)
                                                { v = 63; }
                                                if (iterator == 3)
                                                { v = 64; }
                                                SqlParameter[] sqlParameters = new SqlParameter[11];
                                                sqlParameters[0] = new SqlParameter("@IST", SqlDbType.VarChar);
                                                sqlParameters[0].Value = Convert.ToString("016");
                                                sqlParameters[1] = new SqlParameter("@TABL", SqlDbType.VarChar);
                                                sqlParameters[1].Value = Convert.ToString("n_03");
                                                sqlParameters[2] = new SqlParameter("@POK", SqlDbType.VarChar);
                                                sqlParameters[2].Value = Convert.ToString("0600");
                                                sqlParameters[3] = new SqlParameter("@UT", SqlDbType.VarChar);
                                                sqlParameters[3].Value = Convert.ToString(v);
                                                sqlParameters[4] = new SqlParameter("@SUB", SqlDbType.VarChar);
                                                sqlParameters[4].Value = Convert.ToString(Sub);
                                                sqlParameters[5] = new SqlParameter("@otn", SqlDbType.VarChar);
                                                sqlParameters[5].Value = Convert.ToString("00");
                                                sqlParameters[6] = new SqlParameter("@OBJ", SqlDbType.VarChar);
                                                sqlParameters[6].Value = Convert.ToString("0000000000000000");
                                                sqlParameters[7] = new SqlParameter("@VID", SqlDbType.VarChar);
                                                sqlParameters[7].Value = Convert.ToString("05");
                                                sqlParameters[8] = new SqlParameter("@PER", SqlDbType.VarChar);
                                                sqlParameters[8].Value = Convert.ToString("03");
                                                sqlParameters[9] = new SqlParameter("@DATV", SqlDbType.VarChar);
                                                sqlParameters[9].Value = Convert.ToString(dtnf.ToString("yyyy-MM-dd HH:mm:ss"));
                                                sqlParameters[10] = new SqlParameter("@ZNC", SqlDbType.VarChar);
                                                sqlParameters[10].Value = Convert.ToString(s);
                                                if (iterator == 4)
                                                { }
                                                iterator++;
                                                InsertCommand.Parameters.AddRange(sqlParameters);
                                                InsertCommand.ExecuteNonQuery();
                                                InsertCommand.Parameters.Clear();

                                                errormessage += "\r\n" + " IST:" + sqlParameters[0].Value + " TABL:" + sqlParameters[1].Value + " POK:" + sqlParameters[2].Value + " UT:" +
                                                    sqlParameters[3].Value + " SUB:" + sqlParameters[4].Value + " Otn:" + sqlParameters[5].Value + " OBJ:" + sqlParameters[6].Value + " VID:" + sqlParameters[7].Value + " PER:" +
                                                    sqlParameters[8].Value + " DATV:" + sqlParameters[9].Value + " ZNC:" + sqlParameters[10].Value;

                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            SqlConnect.Close();
                                            errormessage += "\r\n " + ex.ToString();
                                            clientMessage += "\r\n Ошибка записиси! ";
                                        }
                                        finally
                                        {


                                            SqlConnect.Close();
                                        }

                                    }
                                }

                                if (objresult == null)
                                {
                                    errormessage += "\r\n Ошибка записи. Запись по уточнению UT" + null + " ненайдена. Нет вариантов для сравнения суммы.";
                                    clientMessage += "\r\n Ошибка записи. В таблицы нет записи для проверки суммы по UT60.";
                                }
                            }
                        }
                    }
                }

                catch (ArgumentException ex)
                {
                    errormessage += "\r\n Ошибка записи" + ex.ToString();
                    clientMessage += "\r\n Ошибка записи";


                }
            }

            string filepath1 = AppDomain.CurrentDomain.BaseDirectory + "LogFile\\\\" + dtimenow.ToString("yyyy-MM-01") + ".txt";

            try
            {
                string filepath = AppDomain.CurrentDomain.BaseDirectory + "LogFile\\\\" + dtimenow.AddYears(-1).ToString("yyyy-MM-01") + ".txt";


                if (!File.Exists(filepath1))
                {
                    if (File.Exists(filepath))
                    {
                        try { System.IO.File.Delete(filepath); }
                        catch { }
                    }
                    using (StreamWriter file = new StreamWriter(@"" + filepath1, false))
                    {
                        file.WriteLine("");
                    }
                }



                string text = File.ReadAllText(@"" + filepath1) + "\r\n" + "\r\n" + ipString + "\r\n";
                File.WriteAllText(@"" + filepath1, text);



                using (StreamWriter w = File.AppendText(@"" + filepath1))
                {
                    Log(errormessage, w, dtimenow, interval);
                }
            }

            catch (Exception ex)
            {
                errormessage += "\r\n " + ex.ToString();
            }
            if (clientMessage == null || clientMessage == "")
            {
                clientMessage = "\r\n Операция успешно выполнена для всех записей!\r\n";

            }

            return clientMessage;
        }

        public static List<string[]> ipConfigFile()
        {
            List<string[]> listSArr = new List<string[]>();

            string filepathIP = AppDomain.CurrentDomain.BaseDirectory + "IpConfigFile\\\\" + "IpConfigFile" + ".txt";

            string[] linesIP;

            if (File.Exists(filepathIP))
            {

                linesIP = File.ReadAllLines(@"" + AppDomain.CurrentDomain.BaseDirectory + WebConfigurationManager.AppSettings["IpPath"]);
                foreach (var s in linesIP)
                {

                    string[] valArr = s.Split('|');
                    if (valArr.Count() == 3)
                    {
                        listSArr.Add(valArr);
                    }
                }


            }
            return listSArr;
        }


        public static void Log(string logMessage, TextWriter w, DateTime dat, string interval)
        {
            w.Write("\r\nLog Entry : ");
            w.WriteLine("{0} {1}", dat.ToLongTimeString(),
                DateTime.Now.ToLongDateString());
            w.WriteLine("  :");
            w.WriteLine("interval : " + interval);
            w.WriteLine("  :{0}", "\r\n" + logMessage);
            w.WriteLine("-------------------------------");
        }


        public bool IsInRange(DateTime dtimenow)
        {
            bool bVal = false;

            string[] lines = File.ReadAllLines(@"" + AppDomain.CurrentDomain.BaseDirectory + WebConfigurationManager.AppSettings["IntervalPath"]);
            interval = lines[0] + " - " + lines[1] + "\r\n" + "At time: " + dtimenow;


            if (dtimenow > Convert.ToDateTime(lines[1]))
            {
                DateTime dt1 = new DateTime();
                DateTime dt2 = new DateTime();
                string interv1;
                int interv2;

                while (Convert.ToDateTime(lines[0]).Date < dtimenow.Date)
                {
                    dt1 = Convert.ToDateTime(lines[0]).AddHours(Int32.Parse(lines[2]));
                    dt2 = Convert.ToDateTime(lines[1]).AddHours(Int32.Parse(lines[2]));
                    interv1 = lines[2].ToString();
                    interv2 = new DateTime().AddHours(Convert.ToInt32(lines[2]) + Convert.ToInt32(lines[3])).Hour;


                    lines[0] = dt1.ToString();
                    lines[1] = dt2.ToString();
                    lines[2] = interv1;
                    lines[3] = interv2.ToString();

                }

                string interval = lines[0].ToString() + "\r\n"
                        + lines[1].ToString() + "\r\n" + lines[2].ToString() + "\r\n" + lines[3].ToString() +
                          "\r\n";

                File.WriteAllText(@"" + AppDomain.CurrentDomain.BaseDirectory + WebConfigurationManager.AppSettings["IntervalPath"], interval);
            }


            if (Convert.ToDateTime(dtimenow) > Convert.ToDateTime(lines[0]) && Convert.ToDateTime(dtimenow) < Convert.ToDateTime(lines[1]))
            {
                bVal = true;
            }
            else
            {
                bVal = false;
            }

            return bVal;

        }

    }
}
