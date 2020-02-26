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
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IActionResult Get()
        {
          var aa=  Json(new { data = _unitOfWork.ApplicationUser.GetAll() });
            return aa;
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody]string Id)
        {
            var objFromDb = _unitOfWork.ApplicationUser.GetFirstORDefault(u => u.Id == Id);
            if(objFromDb == null)
            {
                return Json(new { success = false, message = "Error While Locking/Unlocking" });

            }

            
            if(objFromDb.LockoutEnd != null &&  objFromDb.LockoutEnd> DateTime.Now)
            {

                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {

                objFromDb.LockoutEnd = DateTime.Now.AddYears(100);

            }
           
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation Successful." });


        }

    }
}