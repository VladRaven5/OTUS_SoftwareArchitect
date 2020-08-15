using System;

using OTUS_SoftwareArchitect_Client.Models;

namespace OTUS_SoftwareArchitect_Client.ViewModels
{
    public class ItemDetailViewModel : BaseViewModel
    {
        public Item Item { get; set; }
        public ItemDetailViewModel(Item item = null)
        {
            Title = item?.Text;
            Item = item;
        }
    }
}
