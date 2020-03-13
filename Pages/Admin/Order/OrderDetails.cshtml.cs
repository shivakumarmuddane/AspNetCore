using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.DataAccess.Data.Repository.IRepositoy;
using AspNetCore.Models;
using AspNetCore.Models.ViewModels;
using AspNetCore.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe;

namespace AspNetCore
{
    public class OrderDetailsModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderDetailsModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public OrderDetailsViewModel orderDetailsVM{ get; set; }
        public void OnGet( int id)
        {
            orderDetailsVM = new OrderDetailsViewModel()
            {
                OrderHeader = _unitOfWork.OrderHeader.GetFirstORDefault(u => u.Id == id),
                OrderDetails = _unitOfWork.OrderDetails.GetAll(m => m.OrderId == id).ToList()
            };
            orderDetailsVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetFirstORDefault(u => u.Id == orderDetailsVM.OrderHeader.UserId);
        }


        public IActionResult OnPostOrderConfirm(int orderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstORDefault(o => o.Id == orderId);
            orderHeader.Status = SD.StatusCompleted;
            _unitOfWork.Save();
            return RedirectToPage("orderlist");

        }

        public IActionResult OnPostOrderCancel(int orderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstORDefault(o => o.Id == orderId);
            orderHeader.Status = SD.StatusCancelled;
            _unitOfWork.Save();
            return RedirectToPage("orderlist");

        }

        public IActionResult OnPostOrderRefund(int orderId)
        {
            // Refund Application
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstORDefault(o => o.Id == orderId);
            var totlrefudamt = Convert.ToDouble(orderHeader.OrderTotal);

            var options = new RefundCreateOptions
            {
                Amount = Convert.ToInt32(orderHeader.OrderTotal * 100),
                Reason = RefundReasons.RequestedByCustomer,
                Charge = orderHeader.TransactionId
            };
            var service = new RefundService();
            Refund refund = service.Create(options);


            orderHeader.Status = SD.StatusRefunded;
            _unitOfWork.Save();
            return RedirectToPage("orderlist");

        }

    }
}