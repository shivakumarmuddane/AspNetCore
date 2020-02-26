using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AspNetCore.DataAccess.Data.Repository.IRepositoy;
using Microsoft.AspNetCore.Authorization;
using AspNetCore.Utility;

namespace AspNetCore.Pages.Admin.FoodType
{
    [Authorize(Roles = SD.ManagerRole)]
    public class UpsertModel : PageModel
    {

        private readonly IUnitOfWork _unitOfWork;


        public UpsertModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [BindProperty]
        public AspNetCore.Models.FoodType FoodTypeObj { get; set; }

        public IActionResult OnGet(int? id)
        {
            FoodTypeObj = new AspNetCore.Models.FoodType();
            if (id != null)
            {

                FoodTypeObj = _unitOfWork.FoodType.GetFirstORDefault(u => u.Id == id);
                if (FoodTypeObj == null)
                {
                    return NotFound();
                }
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (FoodTypeObj.Id == 0)
            {
                _unitOfWork.FoodType.add(FoodTypeObj);
            }
            else
            {
                _unitOfWork.FoodType.Update(FoodTypeObj);
            }
            _unitOfWork.Save();
            return RedirectToPage("./Index");
        }
    }
}