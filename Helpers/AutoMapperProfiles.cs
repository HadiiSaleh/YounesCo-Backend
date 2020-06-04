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
            CreateMap<CategoryToUpdateViewModel, Category>();
           
            
            // CreateMap<TypesForListViewModel, Models.Type>().ReverseMap();
            // CreateMap<CreateTypeViewModel, Models.Type>();
            


            CreateMap<CreateProductViewModel, Product > ();
            CreateMap<Product, ProductDetailsViewModel>().ForMember(dest => dest.Images, opt => 
            {
                opt.MapFrom(src => src.Images.Find(i => i.ColorId == src.ColorId));
            });

            CreateMap<Product, ProductToListViewModel>()
                .ForMember(dest => dest.DefaultColor, opt =>
            {
                opt.MapFrom(src => src.Color.ColorName);
            }).ForMember(dest => dest.ImageUrl, opt =>
            {
                opt.MapFrom(src => src
                .Images
                .Find(p => p.ProductId == src.ProductId && p.Default)
                .ImageUrl
                );
            });

            CreateMap<Product, ProductDetailsViewModel>();

            CreateMap<ImageForCreationViewModel, Image>();
            CreateMap<Image, ImageForListViewModel>();
            
            CreateMap<ColorForCreationViewModel, Color>();
            CreateMap<Color, ColorsForListViewModel>();
            CreateMap<Color, ColorsForDetailsViewModel>();

        }
    }
}
