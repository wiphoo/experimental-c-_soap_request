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
using System.Xml.Linq;

//  ref -   https://www.c-sharpcorner.com/article/calling-web-service-using-soap-request/
//          https://stackoverflow.com/questions/4791794/client-to-send-soap-request-and-received-response?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa

namespace Calling_Web_Service_using_SOAP_Request
{
    class Program
    {
        static void Main(string[] args)
        {
            //creating object of program class to access methods    
            //Program obj = new Program();

            //Console.WriteLine("Please Enter Input values..");
            ////Reading input values from console

            ////  Year
            //Console.Write("Enter from year : ");
            //int fromYear = Convert.ToInt32(Console.ReadLine());
            //Console.Write("      to year : ");
            //int toYear = Convert.ToInt32(Console.ReadLine());

            ////  Month
            //Console.Write("Enter from month : ");
            //int fromMonth = Convert.ToInt32(Console.ReadLine());
            //Console.Write("      to month : ");
            //int toMonth = Convert.ToInt32(Console.ReadLine());

#warning UNCOMMENT_ME
            ////  harmonize code
            //Console.Write("Enter harmonize code (csv file name) : ");
            //string harmonizeCodeCsvFileName = Convert.ToString(Console.ReadLine());
            //List<string> harmonizeCodeList = new List<string>();
            ////  parsing csv file to a list of harmonize code
            //{
            //    //  read a csv harmonize file
            //    string[] harmonizeLines = System.IO.File.ReadAllLines( harmonizeCodeCsvFileName );

            //    //  strip the harmonize code that is a first element in the csv file
            //    foreach( string harmonizeLine in harmonizeLines )
            //    {
            //        //  strip the comma ','
            //        List<string> harmonizeLineSplit = harmonizeLine.Split( ',' ).ToList();
            //        Debug.Assert( harmonizeLineSplit.Count >= 1 );

            //        //  get the harmonize code
            //        string harmonizeCode = harmonizeLineSplit.ElementAt( 0 ).ToString();
            //        Debug.Assert( !string.IsNullOrEmpty( harmonizeCode ) );

            //        //  get the first element
            //        harmonizeCodeList.Add( harmonizeCode );
            //    }
            //}

            //#warning REMOVE_ME :: TESTING PURPOSE
            //            //  harmonize code
            //            Console.Write("Enter harmonize code : ");
            //            string harmonizeCodeStr = Convert.ToString(Console.ReadLine());
            //            List<string> harmonizeCodeList = new List<string>();
            //            harmonizeCodeList.Add( harmonizeCodeStr );

            //            //  rank
            //            //Console.Write("Enter from rank : ");
            //            //int fromRank = Convert.ToInt32(Console.ReadLine());
            //            //Console.Write("      to rank : ");
            //            //int toRank = Convert.ToInt32(Console.ReadLine());
            //            Console.Write( "Enter number of rank : " );
            //            int numRanks = Convert.ToInt32(Console.ReadLine());

            //            //  loop over all year/month/rank and call the service
            //            for ( int year = fromYear ; year <= toYear ; ++ year )
            //            {
            //                for( int month = fromMonth ; month <= toMonth ; ++ month )
            //                {
            //                    foreach( string harmonizeCode in harmonizeCodeList )
            //                    {
            //                        //  Calling InvokeService method    
            //                        obj.InvokeServiceExportHarmonizeCountry( year, month, numRanks, harmonizeCode );
            //                    }
            //                }
            //            }

            invokeServiceImportHarmonizeCountry( 2017, 8, 99999, "390410" );
            Console.WriteLine( "--------------------------------------------------" );

            string urlGetExportHamonizeCountry = @"http://www2.ops3.moc.go.th/tradeWebservice/ServiceExportHarmonizeCountry.asmx",
                    actionGetExportHamonizeCountry = "http://tempuri.org/GetExportHarmonizeCountry";

            HttpWebRequest requestGetExportHamonizeCountry = createSOAPWebRequest( urlGetExportHamonizeCountry, actionGetExportHamonizeCountry );
            XmlDocument envelopeGetExportHamonizeCountry = createSOAPEnvelopeForGetExportHarmonizeCountry( 2017, 8, "390410", 10 );
            callWebService( requestGetExportHamonizeCountry, envelopeGetExportHamonizeCountry );

            Console.ReadLine();
        }

        public static void callWebService( HttpWebRequest request, XmlDocument envelope )
        {
            //  insert the envelope into web request
            using ( Stream stream = request.GetRequestStream() )
            {
                envelope.Save( stream );
            }

            // begin async call to web request.
            IAsyncResult asyncResult = request.BeginGetResponse( null, null );

            // suspend this thread until call is complete. You might want to
            // do something usefull here like update your UI.
            asyncResult.AsyncWaitHandle.WaitOne();

            // get the response from the completed web request.
            string soapResponse;
            using ( WebResponse webResponse = request.EndGetResponse( asyncResult ) )
            {
                using ( StreamReader streamReader = new StreamReader( webResponse.GetResponseStream() ) )
                {
                    soapResponse = streamReader.ReadToEnd();
                }

                //  parse response
                XElement response = XElement.Parse( soapResponse );

                //  writting stream result on console
                Console.WriteLine( response );
            }
        }

        //-----------------------------------------------------------------------------------------------------------------
        //  GetExportHarmonizeCountry

        public static HttpWebRequest createSOAPWebRequest( string url, string action )
        {
            //  create the http web request header
            HttpWebRequest webRequest = ( HttpWebRequest )WebRequest.Create( url );
            webRequest.Headers.Add( "SOAPAction", action );
            webRequest.ContentType = "text/xml; charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }
       
        public static XmlDocument createSOAPEnvelopeForGetExportHarmonizeCountry( int year, int month, string harmonizeCode, int numRanks )
        {
            Console.WriteLine( String.Format( "createSOAPEnvelopeForGetExportHarmonizeCountry : ( year = {0}, month = {1}, rank = {2}, harmonize code = {3} )", year.ToString(), month.ToString(), harmonizeCode, numRanks.ToString()  ) );
            //  create empty SOAP envelope document
            XmlDocument soapEnvelopeDocument = new XmlDocument();

            //  set the SOAP envelope
            soapEnvelopeDocument.LoadXml( @"<?xml version=""1.0"" encoding=""utf-8"" ?>
    <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
        <soap12:Body>
            <GetExportHarmonizeCountry xmlns=""http://tempuri.org/"">
                <Yearno>" + year + @"</Yearno>
                <Monthno>" + month + @"</Monthno>
                <HarmonizeCode>" + harmonizeCode + @"</HarmonizeCode>
                <NoRank>" + numRanks + @" </NoRank>
            </GetExportHarmonizeCountry>
        </soap12:Body>
    </soap12:Envelope>" );

            //  return xml soap document
            return soapEnvelopeDocument;
        }

        //-----------------------------------------------------------------------------------------------------------------
        //  GetImportHarmonizeCountry

        public static HttpWebRequest createSOAPWebRequestForImportHarmonizeCountry()
        {
            //  making Web Request    
            HttpWebRequest httpWebRequest = ( HttpWebRequest )WebRequest.Create( @"http://www2.ops3.moc.go.th/tradeWebservice/ServiceImportHarmonizeCountry.asmx" );
            //SOAPAction    
            httpWebRequest.Headers.Add( @"SOAPAction:http://tempuri.org/GetImportHarmonizeCountry" );
            httpWebRequest.Host = "www2.ops3.moc.go.th";
            //Content_type    
            httpWebRequest.ContentType = "text/xml;charset=\"utf-8\"";
            httpWebRequest.Accept = "text/xml";
            //HTTP method    
            httpWebRequest.Method = "POST";
            //return HttpWebRequest    
            return httpWebRequest;
        }

        public static void invokeServiceImportHarmonizeCountry( int year, int month, int rank, string harmonizeCode )
        {
            Console.WriteLine( String.Format("Getting the export harmonize country : ( year = {0}, month = {1}, rank = {2}, harmonize code = {3} )", year.ToString(), month.ToString(), rank.ToString(), harmonizeCode ) );
            //Calling CreateSOAPWebRequest method
            HttpWebRequest request = createSOAPWebRequestForImportHarmonizeCountry();

            XmlDocument SOAPReqBody = new XmlDocument();
            SOAPReqBody.LoadXml(@"<?xml version=""1.0"" encoding=""utf-8""?>
   <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
             
               <soap:Body>
              
                  <GetExportHarmonizeCountry xmlns=""http://tempuri.org/"">
               
                     <Yearno>" + year.ToString() + @"</Yearno>
               
                     <Monthno>" + month.ToString() + @"</Monthno>
               
                     <HarmonizeCode>" + harmonizeCode + @"</HarmonizeCode>
               
                     <NoRank>" + rank.ToString() + @"</NoRank>
               
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
                        var serviceResult = rd.ReadToEnd();

                        XElement parsingServiceResult = XElement.Parse( serviceResult );
                        //writting stream result on console
                        Console.WriteLine( parsingServiceResult );
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
