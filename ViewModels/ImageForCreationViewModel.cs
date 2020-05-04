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
        public IFormFile File { get; set; }

    }
}
