using OTUS_SoftwareArchitect_Client.Models.BaseModels;
using Xamarin.Forms;

namespace OTUS_SoftwareArchitect_Client.Models
{
    public class TaskLabelModel : BaseModel
    {
        public string Title { get; set; }
        public string Color { get; set; }

        public Color RealColor => Xamarin.Forms.Color.FromHex($"#{Color}");
    }
}
