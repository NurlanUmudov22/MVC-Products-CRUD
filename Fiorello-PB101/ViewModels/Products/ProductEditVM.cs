using System.ComponentModel.DataAnnotations;

namespace Fiorello_PB101.ViewModels.Products
{
    public class ProductEditVM
    {
        [Required(ErrorMessage = "This input can't be empty")]
        [StringLength(50)]
        public string Name { get; set; }

        public string Description { get; set; }

        public string Price { get; set; }

        public int CategoryId { get; set; }

        public string Image { get; set; }

        //public List<IFormFile> NewImage { get; set; }

        public IFormFile  NewImage { get; set; }
    }
}
