using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BomberService
{
    class Program
    {
        private static ConcurrentStack<string> _tasksIds = new ConcurrentStack<string>();    
        private static List<Bombarder> _bombareds = new List<Bombarder>();        
        
        private static string _hostUri;
        private static string _hostName;
        private static int _rps;
        private static int _streamsCount;

        static void Main(string[] args)
        {
            Console.WriteLine($"Run with args: {string.Join(", ", args)}");

            if(args.Length < 4)
            {
                Console.WriteLine("No hosturl or hostname or RPS or parallel streams count arguments provided. Exit");
                return;
            }

            _hostUri = args[0];

            _hostName = args[1];

            string rpsString = args[2];

            if(!int.TryParse(rpsString, out _rps))
            {                        
                Console.WriteLine($"Can't parse {rpsString} to RpS. Exit");
                return;
            }

            string streamsCountString = args[3];

            if(!int.TryParse(streamsCountString, out _streamsCount))
            {                        
                Console.WriteLine($"Can't parse {rpsString} to RpS. Exit");
                return;
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
