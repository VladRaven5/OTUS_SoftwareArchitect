using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BomberService
{
    public class Bombarder
    {
        private readonly Dictionary<RequestType, Func<string, Task>> _allRequests = new Dictionary<RequestType, Func<string, Task>>();
        private HttpClient _httpClient;
        private readonly Random _random;
        private readonly string _hostUrl;
        private readonly string _hostName;
        private readonly int _rps;
        private readonly ConcurrentStack<string> _tasksIds;

        private int requestsCount = 0;

        public Bombarder(string hostUrl, string hostName, int rps, ConcurrentStack<string> tasksIds)
        {
            _hostUrl = hostUrl;
            _hostName = hostName;
            _tasksIds = tasksIds;
            _rps = rps;
            
            _allRequests.Add(RequestType.GetTasks, GetTasksRequest);
            _allRequests.Add(RequestType.PostTask, PostTaskRequest);
            _allRequests.Add(RequestType.GetTask, GetTaskRequest);
            _allRequests.Add(RequestType.PutTask, PutTaskRequest);
            _allRequests.Add(RequestType.DeleteTask, DeleteTaskRequest);

            _random = new Random(GetHashCode());  

            InitHttpClient();
        }
        
        public async Task StartBombardment()
        {              
            Console.WriteLine($"Bombarder {GetHashCode()} has started at {DateTime.Now}. RpS ={_rps}");

            Stopwatch timer = Stopwatch.StartNew();            

            while(true)
            {
                double elapsedSec = timer.ElapsedMilliseconds / 1000;

                double currentRps = elapsedSec > 0
                    ? requestsCount / elapsedSec
                    : 0;

                if(currentRps > _rps)
                {
                    await Task.Delay(15);
                    continue;
                }

                await PerformRandomRequest();
                requestsCount++;
            }
        }

        private void InitHttpClient()
        {
            var handler = new HttpClientHandler()
            { 
                ServerCertificateCustomValidationCallback 
                    = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _httpClient = new HttpClient(handler);
        }

        private async Task PerformRandomRequest()
        {
            int availableRequestsCount = _allRequests.Count;                      

            while(true)
            {
                RequestType requestType = (RequestType)_random.Next(availableRequestsCount);

                if(!_tasksIds.Any() &&
                     (requestType == RequestType.DeleteTask ||
                     requestType == RequestType.GetTask ||
                     requestType == RequestType.PutTask))
                     {
                         continue;
                     }

                switch(requestType)
                {
                    case RequestType.DeleteTask:
                        if(_tasksIds.TryPop(out string deleteTaskId))
                        {
                            await _allRequests[requestType](deleteTaskId);
                        }                 
                    break;

                    case RequestType.GetTask:
                    case RequestType.PutTask:
                        if(_tasksIds.TryPop(out string getTaskId))
                        {
                            await _allRequests[requestType](getTaskId);
                            //items stays in db
                            _tasksIds.Push(getTaskId);
                        }
                    break;

                    case RequestType.PostTask:
                    case RequestType.GetTasks:
                        await _allRequests[requestType](string.Empty);
                    break;
                }

                break;
            }
        }

        private Task GetTasksRequest(string _)
        {
            var request =  new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{GetEndpointString()}")
            };

            return SendRequest(request); 
        }

        private Task GetTaskRequest(string taskId)
        {
            var request =  new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{GetEndpointString()}/{taskId}")
            };

            return SendRequest(request); 
        }

        private async Task PostTaskRequest(string _)
        {
            string title = $"Task_{DateTime.Now.Millisecond}";
            string assignedTo = "Bomber";

            var request =  new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{GetEndpointString()}/?title={title}&assignedTo={assignedTo}")
            };            

            var response = await SendRequest(request); 

            if(response.StatusCode != HttpStatusCode.OK)
                return;

            var newTask = JsonConvert.DeserializeObject<TaskModel>(await response.Content.ReadAsStringAsync());

            _tasksIds.Push(newTask.Id);            
        }

        private async Task PutTaskRequest(string taskId)
        {
            string title = $"Task_{DateTime.Now.Millisecond}";
            string assignedTo = "Bomber_other";

            var updatedTask = new TaskModel
            {
                Id = taskId,
                AssignedTo = assignedTo,
                Title = title,
                CreatedDate = DateTimeOffset.Now,
                State = TaskState.Active
            };

            var request =  new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri($"{GetEndpointString()}")
            };           

            var bodyContent = new StringContent(JsonConvert.SerializeObject(updatedTask), Encoding.UTF8, "application/json");
            request.Content = bodyContent;

            await SendRequest(request); 

            bodyContent.Dispose();           
        }

        private Task DeleteTaskRequest(string taskId)
        {
            var request =  new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri($"{GetEndpointString()}/{taskId}")
            };

            return SendRequest(request);            
        }

        private Task<HttpResponseMessage> SendRequest(HttpRequestMessage request)
        {
            AddHostIfNecessary(request, _hostName);
            return _httpClient.SendAsync(request);
        }


        [Conditional("RELEASE")]
        private static void AddHostIfNecessary(HttpRequestMessage request, string host)
        {
            request.Headers.Add("Host", host);            
        }

        private string GetEndpointString()
        {
            return 
#if DEBUG
            $"https://{_hostUrl}/tasks";
#else
            $"http://{_hostUrl}/otusapp/tasks";
#endif
        }

        private enum RequestType
        {
            GetTask = 0,
            GetTasks = 1,
            PostTask = 2,
            PutTask = 3,
            DeleteTask = 4, 
        }
    }
}