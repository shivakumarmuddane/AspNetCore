using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.DataAccess.Data.Repository.IRepositoy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Get()
        {
          var aa=  Json(new { data = _unitOfWork.Category.GetAll() });
            return aa;
        }

        [HttpDelete("{CategoryId}")]
        public IActionResult Delete(int Id)
        {
            var objFromDb = _unitOfWork.Category.GetFirstORDefault(u => u.Id == Id);
            if(objFromDb == null)
            {
                return Json(new { success = false, message = "Error While Deleting" });

            }
            _unitOfWork.Category.Remove(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Deleted Successfully" });


        }

    }
}