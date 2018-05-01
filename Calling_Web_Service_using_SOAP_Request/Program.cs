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


            //  construct the connection string to database
            string connectionString = String.Format( @"Data Source={0}; Initial Catalog={1}; User Id={2}; Password={3};",
                                                            Configs.databaseHostName, Configs.databaseName,
                                                            Configs.databaseUserName, Configs.databaseUserPassword );
            //  get the hamonize code
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

            //  export hamonize country
            {
                //  create the url and action
                string urlGetExportHamonizeCountry = @"http://www2.ops3.moc.go.th/tradeWebservice/ServiceExportHarmonizeCountry.asmx",
                        actionGetExportHamonizeCountry = "http://tempuri.org/GetExportHarmonizeCountry";

                //  call a request
                
                //  open a connection for updating the harmonize code data including 
                //      harmonizeCode, year, month, abbrCode (as primary keys)
                //      enName, qty, accQty, valueBaht, accBath, valueUSD, accUSD

                //  loop over all homonize code and call the web service
                foreach( string hsCode in hsCodeList )
                {
                    //  construct the steam for request
                    HttpWebRequest requestGetExportHamonizeCountry = createSOAPWebRequest( urlGetExportHamonizeCountry, actionGetExportHamonizeCountry );

                    //  construct the xml envelope based on the year, month, harmonize code and number of ranks
                    XmlDocument envelopeGetExportHamonizeCountry = createSOAPEnvelopeForGetExportHarmonizeCountry( 2017, 8, hsCode, 2 );

                    //  call web service
                    string response = callWebService( requestGetExportHamonizeCountry, envelopeGetExportHamonizeCountry );

                    //  parse response
                    parseAndStoreSOAPGetExportHarmonizeCountryResponse( hsCode, response, connectionString );

                    //  delay a bit
                    System.Threading.Thread.Sleep( 5000 );
                }
                Console.WriteLine( "--------------------------------------------------" );
            }

            //  import hamonize country
            {
                //  create the url and action
                string urlGetImportHamonizeCountry = @"http://www2.ops3.moc.go.th/tradeWebservice/ServiceImportHarmonizeCountry.asmx",
                        actionGetImportHamonizeCountry = "http://tempuri.org/GetImportHarmonizeCountry";

                //  call a request
                HttpWebRequest requestGetImportHamonizeCountry = createSOAPWebRequest( urlGetImportHamonizeCountry, actionGetImportHamonizeCountry );
                XmlDocument envelopeGetImportHamonizeCountry = createSOAPEnvelopeForGetImportHarmonizeCountry( 2017, 8, "271114", 5 );
                callWebService( requestGetImportHamonizeCountry, envelopeGetImportHamonizeCountry );

                Console.WriteLine( "--------------------------------------------------" );
            }

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
            using ( WebResponse webResponse = request.EndGetResponse( asyncResult ) )
            {
                //  create a reader stream and read response
                using ( StreamReader streamReader = new StreamReader( webResponse.GetResponseStream() ) )
                {
                    //  read response
                    soapResponse = streamReader.ReadToEnd();
                }
            }

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

            //  get the body, response and resolt element
            XElement body = xmlElementReponse.Element( soapEnvelopNamespace + @"Body" ),
                        getExportHarmonizeCountryResponse = body.Element( tempuriNamespace + @"GetExportHarmonizeCountryResponse"  ),
                        getExportHarmonizeCountryResult = getExportHarmonizeCountryResponse.Element( tempuriNamespace + @"GetExportHarmonizeCountryResult" );


            //  open a connection to sql server to query the hamonize code
            using ( SqlConnection connection = new SqlConnection( connectionString ) )
            {
                //  open a conneciton with database
                connection.Open();

                //  create a sql statement string
                string insertOrUpdateSqlStatement = String.Format( @"INSERT INTO {11}.{12}.{13} ( {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10} ) "
                                                                    + @"VALUES( @hsCode, @year, @month, @abbrCode, @enName, @qty, @accQty, "
                                                                               + @"@valueBaht, @accValueBaht, @valueUsd, @accValueUsd ) ",
                                                                    //+ @"ON DUPLICATE KEY UPDATE {4} = @enName, {5} = @qty, {6} = @accQty, "
                                                                    //                            + @"{7} = @valueBaht, {8} = @accValueBaht, "
                                                                    //                            + @"{9} = @valueUsd, {10} = @accValueUsd",
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
