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
            CreateMap<Product, ProductDetailsViewModel>();


            CreateMap<Product, ProductToListViewModel>().ForMember(dest => dest.DefaultColor, opt =>
            {
                opt.MapFrom(src => src.Colors.FirstOrDefault(c => c.Default).ColorName);
            }).ForMember(dest => dest.ImageUrl, opt => {
                opt.MapFrom(src => src
                .Colors
                .FirstOrDefault(c => c.Default)
                .Images
                .FirstOrDefault(i=>i.Default)
                .ImageSource
                );
            });



            CreateMap<ImageForCreationViewModel, Image>();
            CreateMap<ColorForCreationViewModel, Color>();

        }
    }
}
