using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calling_Web_Service_using_SOAP_Request
{
    class Configs
    {
        //////////////////////////////////////////////////////////////////////////////////
        //  log file

        //  log file name
        public static string logFileName = @"calling_web_service.log";


        //////////////////////////////////////////////////////////////////////////////////
        //  call webservice properties

        //  number of ranks
        public static int numRanks = 10;

        //////////////////////////////////////////////////////////////////////////////////
        //  database

        //  database host name
        public static string databaseHostName = @"FARADAY-PC";

        //  database name
        public static string databaseName = @"WebServices";

        //  database user name / password
        public static string databaseUserName = @"Gullaya";
        public static string databaseUserPassword = @"49211505";

        //  table namespace
        public static string tableNamespace = @"dbo";

        //////////////////////////////////////////////////////////////////////////////////
        //  HS code table

        //  harmonized system code table name
        public static string hsCodeTableName = @"hs_code";

        //  harmonized system code column name
        public static string hsCodeColumnName = @"HS Code";

        //////////////////////////////////////////////////////////////////////////////////
        //  export table

        //  export table name
        public static string exportTableName = @"MOC_Export";

        //  export columns
        //      HS code
        public static string exportHsCodeColumnName = @"HS_Code";
        //      year
        public static string exportYearColumnName = @"YearNo";
        //      month
        public static string exportMonthColumnName = @"MonthNo";
        //      abbr code
        public static string exportAbbrColumnName = @"AbbrCode";
        //      en name
        public static string exportEnNameColumnName = @"EnName";
        //      qty
        public static string exportQtyColumnName = @"QTY";
        //      acc qty
        public static string exportAccQtyColumnName = @"AccQTY";
        //      value baht
        public static string exportValueBahtColumnName = @"ValueBaht";
        //      acc value baht
        public static string exportAccValueBahtColumnName = @"AccValueBaht";
        //      value usd
        public static string exportValueUsdColumnName = @"ValueUsd";
        //      acc value usd
        public static string exportAccValueUsdColumnName = @"AccValueUSD";

    }
}
