namespace OTUS_SoftwareArchitect_Client.DTO.TaskDtos
{
    public class UpdateTaskDto : CreateTaskDto
    {
        public string Id { get; set; }

        public int Version { get; set; }
    }
}
