using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using YounesCo_Backend.Data;
using YounesCo_Backend.Models;

namespace YounesCo_Backend.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly YounesCo_Backend.Data.ApplicationDbContext _context;

        public ResetPasswordModel(YounesCo_Backend.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public ResetPasswordViewModel ResetPasswordViewModel { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // _context.ResetPasswordViewModel.Add(ResetPasswordViewModel);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}