using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YounesCo_Backend.Data;
using YounesCo_Backend.Models;
using YounesCo_Backend.ViewModels;

namespace YounesCo_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {

        #region Attributes

        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        #endregion

        #region Constructor
        public CategoriesController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        #endregion

        #region GetCategoriesAsync

        [HttpGet("[action]")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _db.Categories
                .Where(c => c.Deleted == false)
                .Include(cat => cat.Types)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
            if (result == null) return NotFound("No categories existed.");
            var categoriesToReturn = _mapper.Map<List<CategoryDetailsViewModel>>(result);

            return Ok(categoriesToReturn);

        }

        #endregion

        #region GetCategoryById

        [HttpGet("[action]/{id}", Name = "GetCategoryById")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Category>> GetCategoryById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id <= 0) return BadRequest("Invalid Id!");

            var category = await _db.Categories
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Types = c.Types.Select(t => new Models.Type
                    {
                        TypeId = t.TypeId,
                        TypeName = t.TypeName,
                        Deleted = t.Deleted,
                        Products = t.Products.Select(p => new Product
                        {
                            Name = p.Name,
                            ProductId = p.ProductId,
                            Description = p.Description,
                            Price = p.Price,
                            OutOfStock = p.OutOfStock,
                            Deleted = p.Deleted
                        })
                        .Where(p => p.Deleted == false)
                        .Where(p=>p.OutOfStock == false)
                        .ToList()

                    }).Where(t=>t.Deleted == false).ToList()
                })
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null || category.Deleted)
            {
                return NotFound("Category Not Found");
            }

            return Ok(category);
        }

        #endregion


        #region GetCategoryByNameAsync

        [HttpGet("[action]/{name}")]
        [Authorize(Policy = "RequireLoggedIn")]
        public async Task<ActionResult<Category>> GetCategoryByName([FromRoute] string name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(name)) return BadRequest("Invalid name!");

            var category = await _db.Categories
                .Select(c => new Category
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Types = c.Types.Select(t => new Models.Type
                    {
                        TypeId = t.TypeId,
                        TypeName = t.TypeName,
                        Deleted = t.Deleted,
                        Products = t.Products.Select(p => new Product
                        {
                            Name = p.Name,
                            ProductId = p.ProductId,
                            Description = p.Description,
                            Price = p.Price,
                            OutOfStock = p.OutOfStock,
                            Deleted = p.Deleted
                        })
                        .Where(p => p.Deleted == false)
                        .Where(p => p.OutOfStock == false)
                        .ToList()

                    }).Where(t => t.Deleted == false).ToList()
                })
                .FirstOrDefaultAsync(c => c.CategoryName == name);

            if (category == null || category.Deleted)
            {
                return NotFound("Category Not Found");
            }

            return Ok(category);
        }

        #endregion


        #region CreateCategoryAsync

        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryToCreateViewModel data)
        {
            var categoryFromDB = await _db.Categories.FirstOrDefaultAsync(c => c.CategoryName.ToLower() == data.CategoryName.ToLower());

            if(categoryFromDB != null)
            {
                return Conflict("Category name already exist");
            }

          var categoryToCreate = _mapper.Map<Category>(data);

            var createCat = await _db.Categories.AddAsync(categoryToCreate);


            if (createCat == null)
                return BadRequest("Category Creation Failed");

            await _db.SaveChangesAsync();

            var categoryToReturn = _mapper.Map<CategoryDetailsViewModel>(categoryToCreate);


            return CreatedAtRoute("GetCategoryById", new { controller = "Categories", id= categoryToReturn.CategoryId}, categoryToReturn);
        }

        #endregion

        #region UpdateCategoryAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> UpdateCategory([FromRoute] int id, [FromBody] CategoryToUpdateViewModel category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var findCategory = await _db.Categories.FindAsync(id);

            if (findCategory == null)
            {
                return NotFound("Category Not Found");
            }

            // If the category was found
            _mapper.Map(category, findCategory);
           
            ////findCategory.CategoryName = category.CategoryName;
            //findCategory.Deleted = category.Deleted;
            //findCategory.UpdatedAt = DateTime.Now;

            _db.Entry(findCategory).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            return NoContent();
        }

        #endregion

        #region DeleteCategoryAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<ActionResult<Category>> DeleteCategory([FromRoute] int id)
        {
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            if (category.Deleted == true)
            {
                return BadRequest($"{category.CategoryName} is already Deleted");
            }

            category.Deleted = true;
            category.UpdatedAt = DateTime.Now;

            _db.Entry(category).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok($"{category.CategoryName} is Deleted");
        }

        #endregion

        #region UnDeleteCategoryAsync

        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<ActionResult<Category>> UnDeleteCategory([FromRoute] int id)
        {
            var category = await _db.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            if(category.Deleted == false)
            {
                return BadRequest($"{category.CategoryName} is already UnDeleted");
            }

            category.Deleted = false;
            category.UpdatedAt = DateTime.Now;

            _db.Entry(category).State = EntityState.Modified;

            await _db.SaveChangesAsync();

            // Finally return the result to client
            return Ok($"{category.CategoryName} is UnDeleted");
        }

        #endregion


        #region AddTypesToCategory
        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> AddTypesToCategory(AddTypeToCategoryViewModel types)
        {
            var typesList = types.Types;
            var category = await _db.Categories
                .Where(c => c.Deleted == false)
                .FirstOrDefaultAsync(c=>c.CategoryId == types.CategoryId);

            if (category == null)
            {
                return NotFound("Category not found");
            }

            for (int i = 0; i < typesList.Count; i++)
            {
                var type = _db.Types.Include(c=>c.Category).FirstOrDefault(t => t.TypeId == typesList[i].TypeId);

                if (type != null)
                {
                    if(type.Category == null)
                    type.CategoryId = types.CategoryId;
                }
                else
                {
                    var typeToCreate = new Models.Type()
                    {
                        TypeName = type.TypeName,
                        CategoryId = types.CategoryId,
                    };
                    _db.Types.Add(typeToCreate);
                }
            }

            await _db.SaveChangesAsync();
            return Ok($"Types add to {category.CategoryName} successfully");
        }
        #endregion


        #region CreateTypeAsync
        [HttpPost("[action]")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> CreateType(CreateTypeViewModel typeFromBody)
        {
            if(typeFromBody == null)
            {
                return BadRequest("Invalid type name");
            }
            var type = await _db.Types.FirstOrDefaultAsync(t => t.TypeName.ToLower() == typeFromBody.TypeName.ToLower());

            if (type != null)
            {
                return Conflict($"Type of name {type.TypeName} already exist");
            }

           var typeToCreate = _mapper.Map<Models.Type>(typeFromBody);

            await _db.Types.AddAsync(typeToCreate);

            var result = await _db.SaveChangesAsync();

            if (result > 0)
            {
                return Ok($"{typeToCreate.TypeName} created successfully");
            }
            else
            {
                return BadRequest($"Error while trying to save {typeToCreate.TypeName}");
            }
        }
        #endregion


        //#region GetTypes

        //[HttpGet("[action]")]
        //[Authorize(Policy = "RequireAdministratorRole")]
        //public async Task<IActionResult> GetTypes()
        //{
        //need to include products + modify the output result
        //    var result = await _db.Types
        //        .Include(t => t.Category) 
        //        .Where(t => t.Deleted == false)
        //        .ToListAsync();

        //    return Ok(result);
        //}

        //#endregion


        #region GetTypeById

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> GetTypeById(int id)
        {
            var type = await _db.Types
                .Include(t=>t.Category)
                .Include(t => t.Products)
                .FirstOrDefaultAsync(t=>t.TypeId == id);

            if(type == null)
            {
                return NotFound($"Type with id {id} not found");
            }

            return Ok(type);
        }

        #endregion


        #region GetTypeProducts

        [HttpGet("[action]/{id}")]
        [Authorize(Policy = "RequireAdministratorRole")]
        public async Task<IActionResult> GetTypeProducts(int id)
        {
            var type = await _db.Types
                .Include(t => t.Products)
                .FirstOrDefaultAsync(t => t.TypeId == id);

            if (type == null)
            {
                return NotFound($"Type with id {id} not found");
            }

            return Ok(type.Products);
        }

        #endregion

    }
}