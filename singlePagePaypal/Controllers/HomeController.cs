//HomeController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using singlePagePaypal.com.paypal.sandbox.www;
using singlePagePaypal.Models;

namespace singlePagePaypal.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {

            return View();
        }

        //If Submit button pressed:
        [HttpPost]
        public ActionResult Index(FormCollection billingForm)
        {

            //create paypal payment object
            PayPalOrder order = new PayPalOrder();
            
            //collect billing info form data
            order.amount = billingForm["amount"];
            order.firstname = billingForm["firstname"];
            order.lastname = billingForm["lastname"];
            order.email = billingForm["email"];
            order.street1 = billingForm["street1"];
            order.street2 = billingForm["street2"];
            order.city = billingForm["city"];
            order.state = billingForm["state"];
            order.zip = billingForm["zip"];
            order.country = billingForm["country"];
            
            //collect CC card info
            order.ccType = billingForm["cardType"];
            order.accountNum = billingForm["accountNum"];
            order.cvv = billingForm["cvv"];
            order.expDate = billingForm["expMonth"] + billingForm["expYear"];

            //Get ip address
            string ipAddress;
            //Check for proxy first
            ipAddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //if no proxy
            if (ipAddress == null)
            {
                ipAddress = Request.ServerVariables["REMOTE_ADDR"];
            }

            //System.Diagnostics.Debug.WriteLine(ipAddress);
            ipAddress = "192.168.0.1"; //comment out if not running locally
            order.ip = ipAddress;

            PayPalRedirect redirect = new PayPalRedirect();

            //send order to paypal
            if (Request.Form["submit"] == "Pay With Credit Card") //want direct payment api
            {
                //check for complete form (billing information and such)
                foreach (string key in billingForm.AllKeys)
                {
                    HashSet<string> optionalFields = new HashSet<string> { "street2" };
                    if (!optionalFields.Contains(key) && billingForm[key] == "")
                    {
                        string errorMsg = 
                            String.Format("Entered {0} not valid. Please return to previous page and fix errors.", key);
                        string redirectURL = String.Format("http://localhost:51072/Paid/Failure?ErrMsg={0}", errorMsg);
                        return new RedirectResult(redirectURL); 
                    }
                }
                //form is complete , move on with direct payment
                redirect = PaypalPayment.DoDirectPayment(order);
            }

            else //want express checkout api
            {
                //check if there is an amount. If not, then nothing to pay!
                if (billingForm["amount"] == "" || Convert.ToDouble(billingForm["amount"]) <= 0 ) 
                {
                    string errorMsg = "This transaction cannot be processed. The amount to be charged is zero.";
                    string redirectURL = String.Format("http://localhost:51072/Paid/Failure?ErrMsg={0}", errorMsg);
                    return new RedirectResult(redirectURL); 
                }

                //amount is good, move on with order
                redirect = PaypalPayment.SetExpressCheckout(order);
            }
            
            //get response and redirect user
            Session["token"] = redirect.Token;

            return new RedirectResult(redirect.Url);
            
        }

        public ActionResult Cancel()
        {
            return View();
        }
    }
}
