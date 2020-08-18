namespace OTUS_SoftwareArchitect_Client.DTO.WorkingHours
{
    public class UpdateWorkingHoursRecordDto : CreateWorkingHoursRecordDto
    {
        public string Id { get; set; }

        public int Version { get; set; }
    }
}
