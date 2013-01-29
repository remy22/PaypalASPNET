using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using singlePagePaypal.com.paypal.sandbox.www;
using singlePagePaypal.Models;

namespace singlePagePaypal.Controllers
{
    public class PaidController : Controller
    {
        //
        // GET: /Paid/

        public ActionResult Index()
        {
            return View();
        }

        //If checkout confirmed, do express payment and show transaction details
        public ActionResult SendExpCheck(string token, string payerID, string amount)
        {
            //send the checkout in (FINAL STEP)
            NameValueCollection paymentResponse = PaypalPayment.DoExpressCheckout(token, payerID, amount);

            string ack = paymentResponse["ACK"].ToLower(); //Status of Payment after submission
            if (ack == "success" || ack == "successwithwarning")
            {
                ViewBag.TransIdMessage = "Your transaction ID is: " + paymentResponse["PAYMENTINFO_0_TRANSACTIONID"];
                ViewBag.AmountMessage = "Amount Paid (in USD): $" + amount;

                return View();
            }
            else
            {
                string redirectURL = String.Format("http://localhost:51072/Paid/Failure?ErrMsg={0}",
                                                    paymentResponse["L_LONGMESSAGE0"]);
                return new RedirectResult(redirectURL);
            }
        }

        //confirmation page
        public ActionResult ConfirmExpCheck(string token, string payerID)
        {
            //get checkout details
            NameValueCollection checkoutDetails = PaypalPayment.GetExpressCheckout(token);
            string ack = checkoutDetails["ACK"].ToLower(); //Status of Payment after submission
            if (ack == "success" || ack == "successwithwarning")
            {
                String amount = checkoutDetails["PAYMENTREQUEST_0_AMT"];

                ViewBag.confMessage = String.Format("Please confirm payment amount (in USD) ${0} to EN",
                                                        amount);

                ViewBag.PayerID = payerID;
                ViewBag.Token = token;
                ViewBag.Amount = amount;

                return View();
            }

            else
            {
                string redirectURL = String.Format("http://localhost:51072/Paid/Failure?ErrMsg={0}", 
                                                                    checkoutDetails["L_LONGMESSAGE0"]);
                return new RedirectResult(redirectURL);
            }
        }

        //Call this function if payment succesful
        public ActionResult Success(string transID = "NONE", string amt = "0.00")
        {
            ViewBag.TransIdMessage = "Your transaction ID is: " + transID;
            ViewBag.AmountMessage = "Amount Paid (in USD): $" + amt;

            return View();
        }

        //Call this function if payment failed
        public ActionResult Failure(string errMsg = "Please try payment again later")
        {
            ViewBag.ErrMsg = errMsg;
            
            return View();
        }

    }
}
