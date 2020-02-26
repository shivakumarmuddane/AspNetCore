using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AspNetCore
{
    public class OrderConformationModel : PageModel
    {
        [BindProperty]
        public int orderId { get; set; }
        public void OnGet(int id)
        {
            orderId = id;
        }
    }
}