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
using System.Data.SqlClient;
using System.Data;

//  ref -   https://www.c-sharpcorner.com/article/calling-web-service-using-soap-request/
//          https://stackoverflow.com/questions/4791794/client-to-send-soap-request-and-received-response?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa

//  TODO
//      - implement for import hs code
//      - do a config file

namespace Calling_Web_Service_using_SOAP_Request
{
    public class FileLogger
    {
        //  log file stream
        protected string logFileName = string.Empty;

        //  constructor
        public FileLogger( string logFileName )
        {
            //  initialize log file name
            string executePath = Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly().Location );

            //  construct the stream writer
            this.logFileName = Path.Combine( executePath, logFileName );

            //  log start message
            this.log( "" );
            this.log( "==================================================================================================================================" );
            this.log( String.Format( "# log start at {0}", DateTime.Now ) );
            
        }

        //  write log message to file
        public void log( string message )
        {
            //  open file and append message
            using ( StreamWriter streamWriter = File.AppendText( this.logFileName ) )
            {
                //  write message to stream writer
                streamWriter.WriteLine( message );
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //  this program will query the data from last 2 months

            //  create file logger
            FileLogger logger = new FileLogger( Configs.logFileName );

            //  get the current date
            DateTime currentLocalDate = DateTime.Now;
            logger.log( String.Format( @"# calling web service using SOAP request - run at {0}", currentLocalDate ) );

            //  construct the connection string to database
            string connectionString = String.Format( @"Data Source={0}; Initial Catalog={1}; User Id={2}; Password={3};",
                                                            Configs.databaseHostName, Configs.databaseName,
                                                            Configs.databaseUserName, Configs.databaseUserPassword );
            logger.log( String.Format( @"# connect to database host name = {0}, database name = {1}", Configs.databaseHostName, Configs.databaseName ) );

            //  get the hamonize code
            logger.log( @"# lising all interested hs code................................." );
            List<string> hsCodeList = new List<string>();
            {
                //  get the hamonize code from database

                //  construct the query for the hamonize code
                string queryString = String.Format( @"SELECT DISTINCT [{0}] FROM [{1}].[{2}].[{3}]", 
                                                        Configs.hsCodeColumnName, Configs.databaseName, 
                                                        Configs.tableNamespace, Configs.hsCodeTableName );

                //  open a connection to sql server to query the hamonize code
                using ( SqlConnection connection = new SqlConnection( connectionString ) )
                {
                    //  open a conneciton with database
                    connection.Open();

                    //  create query command
                    SqlCommand command = new SqlCommand( queryString, connection );
                    SqlDataReader reader = command.ExecuteReader();
                    try
                    {
                        while ( reader.Read() )
                        {
                            //  get a hamonize code
                            string hsCode = reader[Configs.hsCodeColumnName].ToString();
                            //  add it to the list
                            hsCodeList.Add( hsCode );
                        }
                    }
                    finally
                    {
                        // Always call Close when done reading.
                        reader.Close();
                    }
                }
            }

            //  export hs code country
            logger.log( @"# query export hs code from web service................................." );
            {
                //  create the url and action
                string urlGetExportHamonizeCountry = @"http://www2.ops3.moc.go.th/tradeWebservice/ServiceExportHarmonizeCountry.asmx",
                        actionGetExportHamonizeCountry = "http://tempuri.org/GetExportHarmonizeCountry";

                //  call a request
                
                //  open a connection for updating the harmonize code data including 
                //      harmonizeCode, year, month, abbrCode (as primary keys)
                //      enName, qty, accQty, valueBaht, accBath, valueUSD, accUSD

                //  get the data from last 2 months

                //  get current year

                //  calculate start/end month
                DateTime startDate = currentLocalDate.AddMonths( -2 ),
                            endDate = currentLocalDate.AddMonths( -1 );
                logger.log( String.Format( "# query start date = {0}, end data = {1}.................", startDate, endDate ) );

                //  loop from last 2 month to last month and call webservice query then store the response data to database
                for ( DateTime currentDate = startDate ;
                                currentDate <= endDate ;
                                currentDate = currentDate.AddMonths( 1 ) )
                {
                    Console.WriteLine( String.Format( "getting / storing export HS data on year = {0}, month = {1}.................", currentDate.Year, currentDate.Month ) );
                    logger.log( String.Format( "        getting / storing export HS data on year = {0}, month = {1}.................", currentDate.Year, currentDate.Month ) );

                    //  loop over all homonize code and call the web service
                    foreach( string hsCode in hsCodeList )
                    {
                        logger.log( String.Format( "    current hs code = {0}", hsCode ) );

                        //  loop until we can get the response from web service
                        //      mostly except we found now is "The remote server returned an error: (500) Internal Server Error." 
                        int wait_mins = 90;
                        int wait_ms = wait_mins * 60 * 1000;
                        string response = null;
                        while( true )
                        {
                            //  catch the exception from webservice
                            try
                            {
                                //  construct the steam for request
                                HttpWebRequest requestGetExportHamonizeCountry = createSOAPWebRequest( urlGetExportHamonizeCountry, actionGetExportHamonizeCountry );

                                //  construct the xml envelope based on the year, month, harmonize code and number of ranks
                                XmlDocument envelopeGetExportHamonizeCountry = createSOAPEnvelopeForGetExportHarmonizeCountry( currentDate.Year, currentDate.Month, hsCode, Configs.numRanks );

                                //  call web service
                                response = callWebService( requestGetExportHamonizeCountry, envelopeGetExportHamonizeCountry );

                                //  received response
                                break;
                            }
                            catch ( System.Net.WebException e )
                            //  got an excpetion when call the web service, so wait
                            {
                                logger.log( String.Format( "         FAILED!!! query hs code = {0}, at {1}", hsCode, DateTime.Now ) );

                                //  get current time
                                DateTime gotWebServiceExceptionLocalDate = DateTime.Now;
                                //  calculate wait minutes
                                Console.WriteLine( String.Format( "ERROR!!! Cannot get a response from webservice.\n Message = {0}\n Waiting {1} mins before call again.\n Got exception from web service at {2} and it will call web service again around {3}", 
                                                                                    e, wait_mins, gotWebServiceExceptionLocalDate, gotWebServiceExceptionLocalDate.AddMinutes( wait_mins  ) ) );

                                //  delay a bit
                                logger.log( String.Format( "                      waiting for {0} mins, retire again at {1}", wait_mins, gotWebServiceExceptionLocalDate.AddMinutes( wait_mins ) ) );
                                System.Threading.Thread.Sleep( wait_ms );

                                //  increase wait secs
                                wait_ms *= 3;
                            }
                        }

                        //  parse response
                        logger.log( String.Format( "        parsing and storing result in database [{0}]", hsCode ) );
                        parseAndStoreSOAPGetExportHarmonizeCountryResponse( hsCode, response, connectionString );

                        //  delay a bit
                        System.Threading.Thread.Sleep( 2500 );
                    }
                }
                Console.WriteLine( "--------------------------------------------------" );
            }

            ////  import hamonize country
            //{
            //    //  create the url and action
            //    string urlGetImportHamonizeCountry = @"http://www2.ops3.moc.go.th/tradeWebservice/ServiceImportHarmonizeCountry.asmx",
            //            actionGetImportHamonizeCountry = "http://tempuri.org/GetImportHarmonizeCountry";

            //    //  call a request
            //    HttpWebRequest requestGetImportHamonizeCountry = createSOAPWebRequest( urlGetImportHamonizeCountry, actionGetImportHamonizeCountry );
            //    XmlDocument envelopeGetImportHamonizeCountry = createSOAPEnvelopeForGetImportHarmonizeCountry( 2017, 8, "271114", 5 );
            //    callWebService( requestGetImportHamonizeCountry, envelopeGetImportHamonizeCountry );

            //    Console.WriteLine( "--------------------------------------------------" );
            //}

            Console.WriteLine( "== DONE ==" );
            logger.log( "== DONE ==" );

            //  wait for a key press to exit
            Console.ReadLine();
        }

        //-----------------------------------------------------------------------------------------------------------------
        //  Helper funtions

        public static HttpWebRequest createSOAPWebRequest( string url, string action )
        {
            //  create the http web request header
            HttpWebRequest webRequest = ( HttpWebRequest )WebRequest.Create( url );
            webRequest.Headers.Add( @"SOAPAction", action );
            webRequest.ContentType = @"application/soap+xml; charset=""utf-8""";
            webRequest.Method = @"POST";

            return webRequest;
        }

        public static string callWebService( HttpWebRequest request, XmlDocument envelope )
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
            using( WebResponse webResponse = request.EndGetResponse( asyncResult ) )
            {
                //  create a reader stream and read response
                using( StreamReader streamReader = new StreamReader( webResponse.GetResponseStream() ) )
                {
                    //  read response
                    soapResponse = streamReader.ReadToEnd();
                }
            }

            //  we got a response, done

            //  return response
            return soapResponse;
        }
        
        //-----------------------------------------------------------------------------------------------------------------
        //  GetExportHarmonizeCountry
        
        public static XmlDocument createSOAPEnvelopeForGetExportHarmonizeCountry( int year, int month, string hsCode, int numRanks )
        {
            Console.WriteLine( String.Format( "createSOAPEnvelopeForGetExportHarmonizeCountry : ( year = {0}, month = {1}, hs code = {2}, number of ranks = {3} )", year.ToString(), month.ToString(), hsCode, numRanks.ToString()  ) );
            //  create empty SOAP envelope document
            XmlDocument soapEnvelopeDocument = new XmlDocument();

            //  set the SOAP envelope
            soapEnvelopeDocument.LoadXml( @"<?xml version=""1.0"" encoding=""utf-8""?>
    <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
        <soap12:Body>
            <GetExportHarmonizeCountry xmlns=""http://tempuri.org/"">
                <Yearno>" + year + @"</Yearno>
                <Monthno>" + month + @"</Monthno>
                <HarmonizeCode>" + hsCode + @"</HarmonizeCode>
                <NoRank>" + numRanks + @" </NoRank>
            </GetExportHarmonizeCountry>
        </soap12:Body>
    </soap12:Envelope>" );

            //  return xml soap document
            return soapEnvelopeDocument;
        }

        public static void parseAndStoreSOAPGetExportHarmonizeCountryResponse( string hsCode, string response, string connectionString )
        {
            //  parse reponse to XElement
            XElement xmlElementReponse = XElement.Parse( response );
            Console.WriteLine( xmlElementReponse );

            //  construct namespace for xElement
            XNamespace soapEnvelopNamespace = @"http://www.w3.org/2003/05/soap-envelope",
                            xsiNamespace = @"http://www.w3.org/2001/XMLSchema-instance",
                            xsdNamespace = @"http://www.w3.org/2001/XMLSchema",
                            tempuriNamespace = @"http://tempuri.org/",
                            msdataNamespace = @"urn:schemas-microsoft-com:xml-msdata",
                            diffgrNamespace = @"urn:schemas-microsoft-com:xml-diffgram-v1";

            //  get the body, response element
            XElement body = xmlElementReponse.Element( soapEnvelopNamespace + @"Body" ),
                        getExportHarmonizeCountryResponse = body.Element( tempuriNamespace + @"GetExportHarmonizeCountryResponse"  );

            //  check if the response export element has any result element
            if( !getExportHarmonizeCountryResponse.Elements( tempuriNamespace + @"GetExportHarmonizeCountryResult" ).Any() )
            //  the result doesn't exist, so skip
                return;

            //  the result exists, so get the result element
            XElement getExportHarmonizeCountryResult = getExportHarmonizeCountryResponse.Element( tempuriNamespace + @"GetExportHarmonizeCountryResult" );

            //  open a connection to sql server to query the hamonize code
            using ( SqlConnection connection = new SqlConnection( connectionString ) )
            {
                //  open a conneciton with database
                connection.Open();

                //  create a sql statement string
                string insertOrUpdateSqlStatement = String.Format( @"INSERT INTO {11}.{12}.{13} ( {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10} ) "
                                                                    + @"VALUES( @hsCode, @year, @month, @abbrCode, @enName, @qty, @accQty, "
                                                                               + @"@valueBaht, @accValueBaht, @valueUsd, @accValueUsd ) ",
                                                                    //  values
                                                                    Configs.exportHsCodeColumnName, Configs.exportYearColumnName, Configs.exportMonthColumnName,
                                                                    Configs.exportAbbrColumnName, Configs.exportEnNameColumnName,
                                                                    Configs.exportQtyColumnName, Configs.exportAccQtyColumnName,
                                                                    Configs.exportValueBahtColumnName, Configs.exportAccValueBahtColumnName,
                                                                    Configs.exportValueUsdColumnName, Configs.exportAccValueUsdColumnName,
                                                                    // database/table
                                                                    Configs.databaseName, Configs.tableNamespace, Configs.exportTableName );


                //  construct a sql command to do insert or update
                SqlCommand sqlCommand = new SqlCommand( insertOrUpdateSqlStatement, connection );
                sqlCommand.Parameters.Add( "@hsCode", SqlDbType.NVarChar, 100 );
                sqlCommand.Parameters.Add( "@year", SqlDbType.Int );
                sqlCommand.Parameters.Add( "@month", SqlDbType.TinyInt );
                sqlCommand.Parameters.Add( "@abbrCode", SqlDbType.NVarChar, 255 );
                sqlCommand.Parameters.Add( "@enName", SqlDbType.NVarChar, 255 );
                sqlCommand.Parameters.Add( "@qty", SqlDbType.Decimal, 28 );
                sqlCommand.Parameters.Add( "@accQty", SqlDbType.Decimal, 28 );
                sqlCommand.Parameters.Add( "@valueBaht", SqlDbType.Decimal, 28 );
                sqlCommand.Parameters.Add( "@accValueBaht", SqlDbType.Decimal, 28 );
                sqlCommand.Parameters.Add( "@valueUsd", SqlDbType.Decimal, 28 );
                sqlCommand.Parameters.Add( "@accValueUsd", SqlDbType.Decimal, 28 );


                //  loop over all results, and create or update database
                foreach ( XElement results in getExportHarmonizeCountryResult.Elements( diffgrNamespace + @"diffgram" ) )
                {
                    //  get document and export element
                    XElement document = results.Element( @"DocumentElement" ),
                            export = document.Element( @"Export" );

                    //  extract data
                    int year = Convert.ToInt32( export.Element( @"YearNo").Value ),
                        month = Convert.ToInt32( export.Element( @"MonthNo" ).Value );
                    string abbrCode = export.Element( @"AbbrCode" ).Value,
                            enName = export.Element( @"EnName" ).Value;
                    decimal qty = Convert.ToDecimal( export.Element( @"QTY" ).Value ),
                            accQty = Convert.ToDecimal( export.Element( @"AccQTY" ).Value ),
                            valueBaht = Convert.ToDecimal( export.Element( @"ValueBaht" ).Value ),
                            accValueBaht = Convert.ToDecimal( export.Element( @"AccValueBaht").Value ),
                            valueUsd = Convert.ToDecimal( export.Element( @"ValueUSD" ).Value ),
                            accValueUsd = Convert.ToDecimal( export.Element( @"AccValueUSD").Value );

                    //  create a sql command for insert or update if exists

                    //  update parameters
                    sqlCommand.Parameters["@hsCode"].Value = hsCode;
                    sqlCommand.Parameters["@year"].Value = year;
                    sqlCommand.Parameters["@month"].Value = month;
                    sqlCommand.Parameters["@abbrCode"].Value = abbrCode;
                    sqlCommand.Parameters["@enName"].Value = enName;
                    sqlCommand.Parameters["@qty"].Value = qty;
                    sqlCommand.Parameters["@accQty"].Value = accQty;
                    sqlCommand.Parameters["@valueBaht"].Value = valueBaht;
                    sqlCommand.Parameters["@accValueBaht"].Value = accValueBaht;
                    sqlCommand.Parameters["@valueUsd"].Value = valueUsd;
                    sqlCommand.Parameters["@accValueUsd"].Value = accValueUsd;

                    //  execulte sql command
                    sqlCommand.ExecuteNonQuery();

                }
            }

        }

        //-----------------------------------------------------------------------------------------------------------------
        //  GetImportHarmonizeCountry

        public static XmlDocument createSOAPEnvelopeForGetImportHarmonizeCountry( int year, int month, string hsCode, int numRanks )
        {
            Console.WriteLine( String.Format( "createSOAPEnvelopeForGetImportHarmonizeCountry : ( year = {0}, month = {1}, hs code = {2}, number of rank = {3} )", year.ToString(), month.ToString(), hsCode, numRanks.ToString() ) );
            //  create empty SOAP envelope document
            XmlDocument soapEnvelopeDocument = new XmlDocument();

            //  set the SOAP envelope
            soapEnvelopeDocument.LoadXml( @"<?xml version=""1.0"" encoding=""utf-8""?>
    <soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope"">
        <soap12:Body>
            <GetImportHarmonizeCountry xmlns=""http://tempuri.org/"">
                <Yearno>" + year + @"</Yearno>
                <Monthno>" + month + @"</Monthno>
                <HarmonizeCode>" + hsCode + @"</HarmonizeCode>
                <NoRank>" + numRanks + @" </NoRank>
            </GetImportHarmonizeCountry>
        </soap12:Body>
    </soap12:Envelope>" );

            //  return xml soap document
            return soapEnvelopeDocument;
        }
    }
}
