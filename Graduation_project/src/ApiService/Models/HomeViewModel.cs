namespace ApiService.Models
{
    public class HomeViewModel
    {
        public Endpoint[] Endpoints { get; }

        public HomeViewModel()
        {
            Endpoints = EndpointsProvider.Instance.Endpoints;
        }
    }
}