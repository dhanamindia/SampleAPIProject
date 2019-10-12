using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using System.Collections;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using System.Configuration;



namespace MovieList
{
    
    class Program
    {
        public static HttpClient webclient = new HttpClient();
        static void Main(string[] args)
        {
            Console.Write("Enter Email Address:");
            string email = Console.ReadLine();
            Console.Write("Enter movie name to search:");
            string movie_search = Console.ReadLine();

            /***********************************************************/

            //Getting the API response into string
            string strurl = string.Format("https://www.omdbapi.com/?s=" + movie_search + "&apikey=1727522c");
            WebRequest requestobject = WebRequest.Create(strurl);
            requestobject.Method = "Get";
            HttpWebResponse responseobject = (HttpWebResponse)requestobject.GetResponse();
            

            string strresult = null;
            using (Stream stream = responseobject.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                strresult = sr.ReadToEnd();
                sr.Close();
            }

            //Handler, if the API response is false;
            if (strresult.Substring(strresult.IndexOf("Response") + 11, 5) == "False")
            { 
                Console.WriteLine(strresult.Substring(strresult.IndexOf("Error") + 7));
                Console.ReadLine();
            }

            //Removing the "Total Results and response status" from the API response string
            string strlentodelete = strresult.Substring(strresult.IndexOf("totalResults") - 2);
            strresult = strresult.Remove(strresult.IndexOf("totalResults") - 2, strlentodelete.Length - 1);
 
            //Deserialising the format string to list object
            JObject jo = JObject.Parse(strresult);
            JArray ja = (JArray)jo["Search"];
            IList<movieclass> movielist = ja.ToObject<IList<movieclass>>();

            //Creating and writing to csv file
            StringBuilder csvline = new StringBuilder();
            csvline.AppendLine("Title;Year;Type");
            foreach (movieclass mc in movielist)
              {
                csvline.AppendLine(mc.Title + ";" + mc.Year + ";" + mc.Type);
                }
            File.AppendAllText(@"MovieList.csv", csvline.ToString());
            Console.WriteLine("File created");
            
            //calling Mail method to send email
            Program.send_mail(email);
            Console.WriteLine("Mail Sent");

            //Deleting the file once mail sent
            File.Delete(@"MovieList.csv");

        }
        
        public static void send_mail(string tomailid)
        {
            MailMessage message = new MailMessage(tomailid, tomailid, "MovieSearchList", "PFA CSV file for your movie search");
            message.IsBodyHtml = true;
            message.Attachments.Add(new Attachment(@"MovieList.csv"));
            string password = ConfigurationManager.AppSettings["password"];

            try
            {
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(tomailid, password);
                client.Send(message);

                message.Dispose();
                client.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }

        }

    }

    class movieclass
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string imdbID { get; set; }
        public string Type { get; set; }
        public string Poster { get; set; }
    }

 }
