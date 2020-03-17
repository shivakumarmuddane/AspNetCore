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
    public class ManageOrderModel : PageModel
    {

        private readonly IUnitOfWork _unitOfWork;

        public ManageOrderModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [BindProperty]
        public List<OrderDetailsViewModel> orderDetailsVM { get; set; }

        public void OnGet()
        {
            orderDetailsVM = new List<OrderDetailsViewModel>();
            List<OrderHeader> orderHeaderList = _unitOfWork.OrderHeader.GetAll(o => o.Status == SD.StatusSubmited ||
            o.Status == SD.StatusInprogress).OrderByDescending(u => u.PickUpTime).ToList();

            foreach (OrderHeader item in orderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel
                {
                    OrderHeader = item,
                    OrderDetails = _unitOfWork.OrderDetails.GetAll(o => o.OrderHeaderId == item.Id).ToList()
                };
                orderDetailsVM.Add(individual);
            }
        }


        public IActionResult OnPostOrderPrepared(int orderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstORDefault(o => o.Id == orderId);
            orderHeader.Status = SD.StatusInprogress;
            _unitOfWork.Save();
            return RedirectToPage("ManageOrder");

        }

        public IActionResult OnPostOrderReady(int orderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstORDefault(o => o.Id == orderId);
            orderHeader.Status = SD.StatusReady;
            _unitOfWork.Save();
            return RedirectToPage("ManageOrder");

        }

        public IActionResult OnPostCancelorder(int orderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetFirstORDefault(o => o.Id == orderId);
            orderHeader.Status = SD.StatusCancelled;
            _unitOfWork.Save();
            return RedirectToPage("ManageOrder");

        }

        public IActionResult OnPostRefundOrder(int orderId)
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
            return RedirectToPage("ManageOrder");

        }

    }
}