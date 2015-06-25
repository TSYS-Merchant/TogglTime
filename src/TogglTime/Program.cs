using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TogglTime
{
    class Program
    {
        private static string m_apiTokenB64;
        private static int m_wid = 0;
        private static int m_pid = 0;
        private static int? m_tid = null;
        private static bool m_billable = false;
        private static double m_start = 9; // work day starts at 9am

        static void Main( string[] args )
        {
            Console.WriteLine( "Welcome to Kyle's TogglTime App!" );
            Console.WriteLine( "================================" );
            Console.WriteLine( "" );

            // load configs

            var apiToken = ConfigurationManager.AppSettings["ApiToken"];
            if( string.IsNullOrWhiteSpace( apiToken ) )
            {
                Console.WriteLine( "API token not set in config. Press any key to exit." );
                Console.ReadLine();
                return;
            }
            else
            {
                // base 64 encode the api key from toggl
                m_apiTokenB64 = Base64Encode( string.Format( "{0}:api_token", apiToken ) );
            }

            if( args.Length > 1 )
            {
                int.TryParse( args[0], out m_wid );
                int.TryParse( args[1], out m_pid );

                if( m_wid == 0 || m_pid == 0 )
                {
                    Console.WriteLine( "Invalid workspace/project args. Press any key to exit." );
                    Console.ReadLine();
                    return;
                }
            }
            else
            {
                var wid = ConfigurationManager.AppSettings["TogglWorkspaceId"];
                Console.Write( "Workspace Id: [{0}] ", wid );
                var widInput = Console.ReadLine();
                if( !string.IsNullOrWhiteSpace( widInput ) )
                {
                    wid = widInput;
                }
                int.TryParse( wid, out m_wid );

                var pid = ConfigurationManager.AppSettings["TogglProjectId"];
                Console.Write( "Project Id: [{0}] ", pid );
                var pidInput = Console.ReadLine();
                if( !string.IsNullOrWhiteSpace( pidInput ) )
                {
                    pid = pidInput;
                }
                int.TryParse( pid, out m_pid );

                if( m_wid == 0 || m_pid == 0 )
                {
                    Console.WriteLine( "Invalid workspace/project input. Press any key to exit." );
                    Console.ReadLine();
                    return;
                }
            }

            if( args.Length > 2 )
            {
                if( args[2] != "-" )
                {
                    m_tid = int.Parse( args[2] );
                }
            }
            else
            {
                var tid = ConfigurationManager.AppSettings["TogglTaskId"];
                Console.Write( "Task Id: [{0}] ", string.IsNullOrWhiteSpace( tid ) ? "none" : tid );
                var tidInput = Console.ReadLine();
                if( !string.IsNullOrWhiteSpace( tidInput ) )
                {
                    tid = tidInput;
                }

                if( !string.IsNullOrWhiteSpace( tid ) )
                {
                    m_tid = int.Parse( tid );
                }
            }

            if( args.Length > 3 )
            {
                bool.TryParse( args[3], out m_billable );
            }
            else
            {
                var billable = ConfigurationManager.AppSettings["TogglBillable"];
                Console.Write( "Billable? [{0}] ", billable );
                var billableInput = Console.ReadLine();
                if( !string.IsNullOrWhiteSpace( billableInput ) )
                {
                    billable = billableInput;
                }
                bool.TryParse( billable, out m_billable );
            }

            var weekStart = DateTime.Today.AddDays( -1 * (int)DateTime.Now.DayOfWeek ).AddDays( 1 ); // monday
            var weekEnd = weekStart.AddDays( 4 ); // friday

            Console.Write( "Time date range: [{0}-{1}] ", weekStart.ToShortDateString(), weekEnd.ToShortDateString() );
            var dateRangeInput = Console.ReadLine();

            if( !string.IsNullOrWhiteSpace( dateRangeInput ) )
            {
                if( dateRangeInput.Contains( "-" ) )
                {
                    var dateRangeSplit = dateRangeInput.Split( '-' );
                    if( dateRangeSplit.Count() != 2 )
                    {
                        Console.WriteLine( "Invalid date range input. Press any key to exit." );
                        Console.ReadLine();
                        return;
                    }

                    if( !DateTime.TryParse( dateRangeSplit[0], out weekStart ) || !DateTime.TryParse( dateRangeSplit[1], out weekEnd ) )
                    {
                        Console.WriteLine( "Invalid date range input. Press any key to exit." );
                        Console.ReadLine();
                        return;
                    }
                }
                else
                {
                    if( !DateTime.TryParse( dateRangeInput, out weekStart ) )
                    {
                        Console.WriteLine( "Invalid date input. Press any key to exit." );
                        Console.ReadLine();
                        return;
                    }

                    weekEnd = weekStart;
                }
            }

            Console.Write( "How many hours per day? [8] " );
            var hoursInput = Console.ReadLine();
            double hours;
            if( string.IsNullOrWhiteSpace( hoursInput ) )
            {
                hours = 8;
            }
            else if( !double.TryParse( hoursInput, out hours ) )
            {
                Console.WriteLine( "Invalid hours input. Press any key to exit." );
                Console.ReadLine();
                return;
            }

            Console.Write( "What hour to start your work days on? [9] " );
            var startInput = Console.ReadLine();
            if( !string.IsNullOrWhiteSpace( startInput ) && !double.TryParse( startInput, out m_start ) )
            {
                Console.WriteLine( "Invalid start hour input. Press any key to exit." );
                Console.ReadLine();
                return;
            }

            Console.WriteLine( "" );
            Console.WriteLine( "Submitting Toggl time of " + hours + " hrs /day for date range " + weekStart.ToShortDateString() + "-" + weekEnd.ToShortDateString() + "." );
            Console.WriteLine( "Workspace: {0}, Project: {1}, Task: {2}, Billable: {3}, Start hour: {4}", m_wid, m_pid, m_tid, m_billable, m_start );

            DateTime day = weekStart;
            while( day <= weekEnd )
            {
                if( !PostTogglTime( day, hours ) )
                {
                    Console.WriteLine( "Unable to submit time. Check your parameters. Press any key to exit." );
                    Console.ReadLine();
                    return;
                }

                day = day.AddDays( 1 );
            }

            Console.WriteLine( "Done. Press any key to exit." );
            Console.ReadLine();
        }

        public static bool PostTogglTime( DateTime startDate, double hours = 8 )
        {
            var url = new Uri( "https://www.toggl.com/api/v8/time_entries" );
            var request = WebRequest.Create( url.AbsoluteUri ) as HttpWebRequest;
            request.Method = "POST";
            request.Headers.Add( HttpRequestHeader.Authorization, string.Format( "Basic {0}", m_apiTokenB64 ) );
            request.ContentType = "application/json";
            request.Accept = "application/json";

            using( var streamWriter = new StreamWriter( request.GetRequestStream() ) )
            {
                string json = JsonConvert.SerializeObject( new
                {
                    time_entry = new
                    {
                        wid = m_wid,
                        pid = m_pid,
                        tid = m_tid,
                        start = startDate.AddHours( m_start ).ToString( "yyyy-MM-ddTHH:mm:sszzz" ),
                        duration = (int)( 60 * 60 * hours ), // hours
                        created_with = "Kyle's TogglTime App",
                        at = DateTime.Now.ToString( "yyyy-MM-ddTHH:mm:sszzz" ),
                        billable = m_billable
                    }
                } );

                streamWriter.Write( json );
                streamWriter.Flush();
                streamWriter.Close();

                try
                {
                    var httpResponse = (HttpWebResponse)request.GetResponse();
                    using( var streamReader = new StreamReader( httpResponse.GetResponseStream() ) )
                    {
                        return true;
                    }
                }
                catch( WebException )
                {
                    return false;
                }
            }
        }

        public static string Base64Encode( string plainText )
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes( plainText );
            return System.Convert.ToBase64String( plainTextBytes );
        }
    }
}
