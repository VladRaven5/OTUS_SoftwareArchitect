namespace SoftwareArchitectHW1.DTO
{
    public class HealthcheckResult
    {
        public static HealthcheckResult Ok()
        {
            return new HealthcheckResult("OK");
        }
        
        protected HealthcheckResult(string status)
        {
            Status = status;
        }
        
        public string Status { get; }
    }
}