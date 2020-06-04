using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using YounesCo_Backend.Data;
using YounesCo_Backend.Models;

namespace YounesCo_Backend.Services
{
    public class DataCleaner
    {
        public Dictionary<string, List<BaseUser>> cleanAllUsersListBasedOnRoles(List<BaseUser> data)
        {
            Dictionary<string, List<BaseUser>> result = new Dictionary<string, List<BaseUser>>();

            foreach (BaseUser user in data)
            {
                if (!result.ContainsKey(user.role))
                {
                    result.Add(user.role, new List<BaseUser>());
                    result.GetValueOrDefault(user.role).Add(user);
                }

                else
                {
                    result.GetValueOrDefault(user.role).Add(user);
                }

            }

            if (result.Count() == 0 || result == null)
                return null;

            else
                return result;

        }

        public List<Product> cleanFavorites(List<Favorite> data)
        {
            if (data == null) return new List<Product>();

            int size = data.Count();

            if (size == 0) return new List<Product>();

            List<Product> products = new List<Product>(size);

            foreach (Favorite favorite in data)
            {
                favorite.Product.Favorites = null;
                favorite.Product.OrderItems = null;
                favorite.Product.Color = null;
                products.Add(favorite.Product);
            }


            return products;
        }

        public BaseUser cleanUser(AppUser user, string firstRole)
        {
            var result = new BaseUser
            {
                Id = user.Id,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                DisplayName = user.DisplayName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Location = user.Location,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Deleted = user.Deleted,
                role = firstRole,
                Orders = user.Orders != null ? user.Orders : new List<Order>(),
                Favorites = cleanFavorites(user.Favorites)
            };

            return result;
        }

    }
}
