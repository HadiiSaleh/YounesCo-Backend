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
            int size = data.Count();

            if (size == 0) return new List<Product>();

            List<Product> products = new List<Product>(size);

            foreach (Favorite favorite in data)
            {
                favorite.Product.Favorites = null;
                favorite.Product.OrderItems = null;
                favorite.Product.Colors = null;
                products.Add(favorite.Product);
            }


            return products;
        }

    }
}
