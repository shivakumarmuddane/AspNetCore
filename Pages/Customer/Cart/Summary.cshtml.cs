using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.DataAccess.Data.Repository.IRepositoy;
using AspNetCore.Models;
using AspNetCore.Models.ViewModels;
using AspNetCore.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Stripe;

namespace AspNetCore.Pages.Customer.Cart
{
    public class SummaryModel : PageModel
    {
        private readonly IUnitOfWork _unitOfWork;

        public SummaryModel(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

        }
        [BindProperty]
        public OrderDetailsCart detailsCart { get; set; }
        public IActionResult OnGet()
        {
            detailsCart = new OrderDetailsCart()
            {
                OrderHeader = new Models.OrderHeader()
            };

            detailsCart.OrderHeader.OrderTotal = 0;

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            

            IEnumerable<ShoppingCart> cart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value);
            if (cart != null)
            {
                detailsCart.listCart = cart.ToList();

            }
            foreach (var cartList in detailsCart.listCart)
            {
                cartList.MenuItem = _unitOfWork.MenuItem.GetFirstORDefault(m => m.Id == cartList.MenuItemId);
                detailsCart.OrderHeader.OrderTotal += (cartList.MenuItem.Price * cartList.Count);
            }

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.GetFirstORDefault(c => c.Id == claim.Value);
            detailsCart.OrderHeader.PickupName = applicationUser.FullName;
            detailsCart.OrderHeader.PickUpTime = DateTime.Now;
            detailsCart.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;

            return Page();

        }

        public IActionResult OnPost( string stripeToken)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            detailsCart.listCart = _unitOfWork.ShoppingCart.GetAll(c => c.ApplicationUserId == claim.Value).ToList();

            detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            detailsCart.OrderHeader.OrderDate = DateTime.Now;
            detailsCart.OrderHeader.UserId = claim.Value;
            detailsCart.OrderHeader.Status = SD.PaymentStatusPending;
            //detailsCart.OrderHeader.ApplicationUser = claim.Value;
            detailsCart.OrderHeader.PickUpTime = Convert.ToDateTime(detailsCart.OrderHeader.PickUpDate.ToShortDateString() + " " + detailsCart.OrderHeader.PickUpTime.ToShortTimeString());

            List<OrderDetails> orderDetailsList = new List<OrderDetails>();
            _unitOfWork.OrderHeader.add(detailsCart.OrderHeader);
            _unitOfWork.Save();

            foreach(var item in detailsCart.listCart)
            {
                item.MenuItem = _unitOfWork.MenuItem.GetFirstORDefault(m => m.Id == item.MenuItemId);
                OrderDetails orderDetails = new OrderDetails
                {
                    OrderId = detailsCart.OrderHeader.Id,
                    MenuItemId =item.MenuItemId,
                    Count = item.Count,                   
                    Name=item.MenuItem.Name,
                    Price=item.MenuItem.Price,
                    Description = item.MenuItem.Description
                };

                detailsCart.OrderHeader.OrderTotal += (orderDetails.Count * orderDetails.Price);
                _unitOfWork.OrderDetails.add(orderDetails);
              
            }

            //detailsCart.OrderHeader.OrderTotal = Convert.ToDouble(String.Format("{0:##}", detailsCart.OrderHeader.OrderTotal));
            _unitOfWork.ShoppingCart.RemoveRange(detailsCart.listCart);
            HttpContext.Session.SetInt32(SD.ShoppingCart, 0);
            _unitOfWork.Save();

            if (stripeToken != null)
            {
                var options = new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(detailsCart.OrderHeader.OrderTotal * 100),
                    Currency = "INR",
                    Description = "Your Order ID :" + detailsCart.OrderHeader.Id,
                    Source = stripeToken

                };

                var service = new ChargeService();
                Charge charge = service.Create(options);

                detailsCart.OrderHeader.TransactionId = charge.Id;
                if (charge.Status.ToLower() == "succeeded")
                {
                    //email
                    detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    detailsCart.OrderHeader.Status = SD.StatusSubmited;

                }
                else
                {
                    detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
                }
            }
            else
            {
                detailsCart.OrderHeader.PaymentStatus = SD.PaymentStatusRejected;
            }

            _unitOfWork.Save();

            return RedirectToPage("/customer/cart/OrderConformation", new { id = detailsCart.OrderHeader.Id });
        }
    }
}