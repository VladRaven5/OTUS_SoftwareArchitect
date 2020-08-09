using Shared;

namespace LabelsService
{
    public class LabelModel : BaseVersionedModel
    {
        public string Title { get; set; }
        public string Color { get; set; }
    }
}