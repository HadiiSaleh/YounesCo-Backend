using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YounesCo_Backend.ViewModels
{
    public class ImageForCreationViewModel
    {

        public string ImageSource { get; set; }
        public bool Default { get; set; }
        public List<IFormFile> files { get; set; }

        public int productId { get; set; }
        public int colorId { get; set; }

    }
}
