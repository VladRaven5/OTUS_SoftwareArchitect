using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BomberService
{
    class Program
    {
        private const string targetHostNameVariable = "TARGET_HOST_NAME";
        private const string targetHostUrlVariable = "TARGET_HOST_URL";
        private const string rpsVariable = "RPS";
        private const string streamCountVariable = "STREAM_COUNT";

        private const int _defaultRps = 10;
        private const int _defaultStreams = 2;


        private static ConcurrentStack<string> _tasksIds = new ConcurrentStack<string>();    
        private static List<Bombarder> _bombareds = new List<Bombarder>();        
        
        private static string _hostUri;
        private static string _hostName;
        private static int _rps;        
        private static int _streamsCount;

        static void Main(string[] args)
        {
            _hostUri = Environment.GetEnvironmentVariable(targetHostUrlVariable);
            _hostName = Environment.GetEnvironmentVariable(targetHostNameVariable);

            if(string.IsNullOrWhiteSpace(_hostName) || string.IsNullOrWhiteSpace(_hostUri))
            {
                Console.WriteLine($"Target hostname ({targetHostNameVariable}) or url ({targetHostUrlVariable}) not provided. Exit");
                return;
            }

            string rpsString = Environment.GetEnvironmentVariable(rpsVariable);
            if(!int.TryParse(rpsString, out _rps))
            {                        
                Console.WriteLine($"Can't parse \"{rpsString}\" to RpS. Use default value {_defaultRps}");
                _rps = _defaultRps;
            }

            string streamsCountString = Environment.GetEnvironmentVariable(streamCountVariable);
            if(!int.TryParse(streamsCountString, out _streamsCount))
            {                        
                Console.WriteLine($"Can't parse \"{rpsString}\" to streams count. Use default value {_defaultStreams}");
                _streamsCount = _defaultStreams;
            }

            InitBombardment(_rps, _streamsCount);
        }

        private static void InitBombardment(int rps, int bombardersCount)
        {          
            int rpsPerBomber = rps / bombardersCount;

            Console.WriteLine($"Bombing {_hostUri} with host {_hostName} started with RpS = {rps} in {bombardersCount} streams");

            for(int i = 0; i < bombardersCount; i++)
            {
                _bombareds.Add(new Bombarder(_hostUri, _hostName, rpsPerBomber, _tasksIds));
            }

            Task.Run(async () => {

                List<Task> bombardingTasks = new List<Task>(); 

                foreach(var bombard in _bombareds)
                {
                    var bombardingTask = Task.Run(bombard.StartBombardment);
                    bombardingTasks.Add(bombardingTask);                    
                }          
                await Task.WhenAll(bombardingTasks);
            })
            .GetAwaiter()
            .GetResult();          
        }        
    }
}
