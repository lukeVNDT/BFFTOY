using Tfood.Models;

namespace Tfood.ViewModel
{
    public class CartVM
    {
        public Product product { set; get; }
        public int amount { set; get; }
        public float total => amount * product.Price.Value;
    }
}
