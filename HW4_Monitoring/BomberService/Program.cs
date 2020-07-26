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
        private static int _rps;
        private static int _streamsCount;

        static void Main(string[] args)
        {
            Console.WriteLine($"Run with args: {string.Join(", ", args)}");

            if(args.Length < 3)
            {
                Console.WriteLine("No hostname or RPS or parallel streams count arguments provided. Exit");
                return;
            }

            _hostUri = args[0];

            string rpsString = args[1];

            if(!int.TryParse(rpsString, out _rps))
            {                        
                Console.WriteLine($"Can't parse {rpsString} to RpS. Exit");
                return;
            }

            string streamsCountString = args[2];

            if(!int.TryParse(streamsCountString, out _streamsCount))
            {                        
                Console.WriteLine($"Can't parse {rpsString} to RpS. Exit");
                return;
            }

            InitBombardment(_rps, _streamsCount);
        }

        private static void InitBombardment(int rps, int bombardersCount)
        {          
            int msecPerBombarderRequest = (1000 / rps) * bombardersCount;

            Console.WriteLine($"Bombing {_hostUri} started with RpS = {rps} in {bombardersCount} streams");

            for(int i = 0; i < bombardersCount; i++)
            {
                _bombareds.Add(new Bombarder(_hostUri, msecPerBombarderRequest, _tasksIds));
            }


            Task.Run(async () => {

                List<Task> bombardingTasks = new List<Task>(); 

                foreach(var bombard in _bombareds)
                {
                    bombardingTasks.Add(bombard.StartBombardment());
                }    
                await Task.WhenAll(bombardingTasks);            
            })
            .GetAwaiter()
            .GetResult();           
        }        
    }
}
