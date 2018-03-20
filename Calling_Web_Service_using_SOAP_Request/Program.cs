using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

//  assert
using System.Diagnostics;

//  ref - https://www.c-sharpcorner.com/article/calling-web-service-using-soap-request/

namespace Calling_Web_Service_using_SOAP_Request
{
    class Program
    {
        static void Main(string[] args)
        {
            //creating object of program class to access methods    
            Program obj = new Program();
            Console.WriteLine("Please Enter Input values..");
            //Reading input values from console
            
            //  Year
            Console.Write("Enter from year : ");
            int fromYear = Convert.ToInt32(Console.ReadLine());
            Console.Write("      to year : ");
            int toYear = Convert.ToInt32(Console.ReadLine());
            
            //  Month
            Console.Write("Enter from month : ");
            int fromMonth = Convert.ToInt32(Console.ReadLine());
            Console.Write("      to month : ");
            int toMonth = Convert.ToInt32(Console.ReadLine());
            
            //  harmonize code
            Console.Write("Enter harmonize code (csv file name) : ");
            string harmonizeCodeCsvFileName = Convert.ToString(Console.ReadLine());
            List<string> harmonizeCodeList = new List<string>();
            //  parsing csv file to a list of harmonize code
            {
                //  read a csv harmonize file
                string[] harmonizeLines = System.IO.File.ReadAllLines( harmonizeCodeCsvFileName );
                
                //  strip the harmonize code that is a first element in the csv file
                foreach( string harmonizeLine in harmonizeLines )
                {
                    //  strip the comma ','
                    List<string> harmonizeLineSplit = harmonizeLine.Split( ',' ).ToList();
                    Debug.Assert( harmonizeLineSplit.Count >= 1 );

                    //  get the harmonize code
                    string harmonizeCode = harmonizeLineSplit.ElementAt( 0 ).ToString();
                    Debug.Assert( !string.IsNullOrEmpty( harmonizeCode ) );

                    //  get the first element
                    harmonizeCodeList.Add( harmonizeCode );
                }
            }

            //  rank
            Console.Write("Enter from rank : ");
            int fromRank = Convert.ToInt32(Console.ReadLine());
            Console.Write("      to rank : ");
            int toRank = Convert.ToInt32(Console.ReadLine());

            //  loop over all year/month/rank and call the service
            for( int year = fromYear ; year <= toYear ; ++ year )
            {
                for( int month = fromMonth ; month <= toMonth ; ++ month )
                {
                    foreach( string harmonizeCode in harmonizeCodeList )
                    {
                        for( int rank = fromRank ; rank <= toRank ; ++ rank )
                        {
                            //  Calling InvokeService method    
                            obj.InvokeServiceExportHarmonizeCountry( year, month, rank, harmonizeCode );
                        }
                    }
                }
            }
        }

        public HttpWebRequest CreateSOAPWebRequestForExportHarmonizeCountry()
        {
            //  making Web Request    
            HttpWebRequest httpWebRequest = ( HttpWebRequest )WebRequest.Create(@"http://www2.ops3.moc.go.th/tradeWebservice/ServiceExportHarmonizeCountry.asmx");
            //SOAPAction    
            httpWebRequest.Headers.Add(@"SOAPAction:http://tempuri.org/GetExportHarmonizeCountry");
            //httpWebRequest.Host = "www2.ops3.moc.go.th";
            //Content_type    
            httpWebRequest.ContentType = "text/xml;charset=\"utf-8\"";
            httpWebRequest.Accept = "text/xml";
            //HTTP method    
            httpWebRequest.Method = "POST";
            //return HttpWebRequest    
            return httpWebRequest;
        }

        public void InvokeServiceExportHarmonizeCountry( int year, int month, int rank, string harmonizeCode )
        {
            Console.WriteLine( String.Format("Getting the export harmonize country : ( year = {0}, month = {1}, rank = {2}, harmonize code = {3} )", year.ToString(), month.ToString(), rank.ToString(), harmonizeCode ) );
            //Calling CreateSOAPWebRequest method
            HttpWebRequest request = CreateSOAPWebRequestForExportHarmonizeCountry();

            XmlDocument SOAPReqBody = new XmlDocument();
            SOAPReqBody.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
   <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
             
               <soap:Body>
              
                  <GetExportHarmonizeCountry xmlns=""http://tempuri.org/"">
               
                     <Yearno>" + year + @"</Yearno>
               
                     <Monthno>" + month + @"</Monthno>
               
                     <HarmonizeCode>" + harmonizeCode + @"</HarmonizeCode>
               
                     <NoRank>" + rank + @"</NoRank>
               
                   </GetExportHarmonizeCountry>
               
                 </soap:Body>
                </soap:Envelope>" );

            using (Stream stream = request.GetRequestStream())
            {
                SOAPReqBody.Save(stream);
            }
            //  geting response from request    
            try
            {

                using (WebResponse Serviceres = request.GetResponse())
                {
                    using (StreamReader rd = new StreamReader(Serviceres.GetResponseStream()))
                    {
                        //reading stream    
                        var ServiceResult = rd.ReadToEnd();
                        //writting stream result on console    
                        Console.WriteLine(ServiceResult);
                        Console.WriteLine( "------------------------------------------------\n" );
                        //Console.ReadLine();
                    }
                }
            }
            catch( System.Net.WebException e )
            {
                Console.WriteLine( String.Format( "ERROR!!! {0}\n", e.ToString() ) );
                Console.WriteLine( "------------------------------------------------\n" );
            }
        }
    }
}
