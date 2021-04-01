using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LoadTest
{
    class Program
    {
        const int DefaultIterations = 100;
        const string UrlStem = "/api/diagscenario/memleak/";

        static Random s_r = new Random();
        static HttpClient s_cli = new HttpClient();

        static int s_Iterations;
        static string s_Url;

        static bool CheckSyntax(string[] args)
        {
            if (args.Length == 0 || args.Length > 2)
            {
                Console.WriteLine("Syntax: LoadTest Url [nIter]");
                return false;
            }
            s_Url = args[0];
            if (args.Length == 1)
                s_Iterations = DefaultIterations;
            else // args.Length == 2
            {
                bool bRet = int.TryParse(args[1], out s_Iterations);
                if (!bRet)
                {
                    Console.WriteLine("Syntax: LoadTest Url [nIter]. nIter must be a number");
                    return false;
                }
            }
            return true;
        }


        static void Main(string[] args)
        {
            if (!CheckSyntax(args))
                return;

            string sFullUrl = s_Url + UrlStem;
            Console.WriteLine("Calling " + sFullUrl + "<number> for " + s_Iterations + " times");

            Task[] at = new Task[s_Iterations];
            for (int i = 0; i < s_Iterations; i++)
            {
                int val = s_r.Next(500, 5000);
                at[i] = MakeACall(sFullUrl + val);
                Thread.Sleep(val / 3 * 30);
            }

            try
            {
                Task.WaitAll(at);
            }
            catch(AggregateException ae)
            {
                Console.WriteLine("Number of exceptions: " + ae.InnerExceptions.Count);
            }
            catch(Exception e)
            {

            }
        }

        public static async Task MakeACall(string uri)
        {
            try
            {
                HttpResponseMessage response = await s_cli.GetAsync(new Uri(uri));
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                throw;
            }
            return;
        }


    }
}
