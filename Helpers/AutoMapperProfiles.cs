using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YounesCo_Backend.Models;
using YounesCo_Backend.ViewModels;

namespace YounesCo_Backend.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Category, CategoryDetailsViewModel>();
            CreateMap<CategoryToCreateViewModel, Category>();
            CreateMap<CategoryToCreateViewModel, CategoryDetailsViewModel>();
            CreateMap<CategoryToUpdateViewModel, Category>();
            CreateMap<TypesForListViewModel, Models.Type>().ReverseMap();
            CreateMap<CreateTypeViewModel, Models.Type>();
            


            CreateMap<CreateProductViewModel, Product > ();
            CreateMap<Product, ProductToListViewModel> ();

        }
    }
}
