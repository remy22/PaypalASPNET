//PaypalPayment.cs
//paypal payment object here and supporting classes & submit request
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Specialized;
using System.Net;
using System.IO;
using System.Globalization;

namespace singlePagePaypal.Models
{
    //class for redirect info after payment
    public class PayPalRedirect
    {
        public string Url { get; set; }
        public string Token { get; set; }
    }

    //class for info needed from customer
    public class PayPalOrder
    {
        
        public string amount { get; set; }
        public string ip { get; set; }

        //Customer Info
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }

        //Billing Address Info
        public string street1 { get; set; }
        public string street2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string country { get; set; }

        //CCard Info
        public string ccType { get; set; }
        public string accountNum { get; set; }
        public string expDate { get; set; }
        public string cvv { get; set; }
        
    }

    //paypal payment class (includes all needed parameters and passes the request to paypal
    //then returns a redirect address & paypal response
    public class PaypalPayment
    {
        public static PayPalRedirect DoDirectPayment(PayPalOrder order)
        {
            NameValueCollection values = new NameValueCollection();

            values["METHOD"] = "DoDirectPayment";
            
            //Seller PayPal API Info
            values["RETURNURL"] = PayPalSettings.ReturnUrl;
            values["CANCELURL"] = PayPalSettings.CancelUrl;
            values["CURRENCYCODE"] = "USD";
            values["BUTTONSOURCE"] = "PP-ECWizard";
            values["USER"] = PayPalSettings.Username;
            values["PWD"] = PayPalSettings.Password;
            values["SIGNATURE"] = PayPalSettings.Signature;
            values["SUBJECT"] = "";
            values["VERSION"] = PayPalSettings.Version;
            
            //Order Info
            values["AMT"] = order.amount;

            //Customer Info
            values["FIRSTNAME"] = order.firstname;
            values["LASTNAME"] = order.lastname;
            values["STREET"] = order.street1;
            values["STREET2"] = order.street2;
            values["CITY"] = order.city;
            values["STATE"] = order.state;
            values["ZIP"] = order.zip;
            values["COUNTRYCODE"] = order.country;
            values["EMAIL"] = order.email;
            values["IPADDRESS"] = order.ip;

            //Shipping Info
            /*
            values["SHIPTONAME"] = order.firstname + order.lastname;
            values["SHIPTOSTREET"] = order.street;
            values["SHIPTOCITY"] = order.city;
            values["SHIPTOSTATE"] = order.state;
            values["SHIPTOZIP"] = order.zip;
            values["SHIPTOCOUNTRY"] = order.country;
             */

            //Customer CC Info
            values["CREDITCARDTYPE"] = order.ccType;
            values["ACCT"] = order.accountNum;
            values["EXPDATE"] = order.expDate;
            values["CVV2"] = order.cvv;

            //submit info for payment
            values = Submit(values);

            string ack = values["ACK"].ToLower(); //Status of Payment after submission

            //Redirect customer to new page based on whether payment successful or not
            if (ack == "success" || ack == "successwithwarning")
            {
                return new PayPalRedirect
                {
                    /* //For paypal express checkout
                    Token = values["TOKEN"],
                    Url = String.Format("https://{0}/cgi-bin/webscr?cmd=_express-checkout&token={1}",
                       PayPalSettings.CgiDomain, values["TOKEN"])
                     */

                    //Payment sucessful!
                    //Direct user to successful transaction page with transaction info
                    
                    Url = String.Format("http://localhost:51072/Paid/Success?TransID={0}&amt={1}", 
                                 values["TRANSACTIONID"],values["AMT"])
                    
                };
            }
            else
            {
                //throw new Exception(values["L_LONGMESSAGE0"]);

                return new PayPalRedirect
                {
                    //Payment Failed :(
                    //Direct user to error page
                    Url = String.Format("http://localhost:51072/Paid/Failure?ErrMsg={0}", values["L_LONGMESSAGE0"])

                };
            }
        }



        //Express Checkout
        public static PayPalRedirect SetExpressCheckout(PayPalOrder order)
        {
            NameValueCollection values = new NameValueCollection();

            values["METHOD"] = "SetExpressCheckout";

            //Seller PayPal API Info
            values["RETURNURL"] = PayPalSettings.ReturnUrl;
            values["CANCELURL"] = PayPalSettings.CancelUrl;
            values["CURRENCYCODE"] = "USD";
            values["BUTTONSOURCE"] = "PP-ECWizard";
            values["USER"] = PayPalSettings.Username;
            values["PWD"] = PayPalSettings.Password;
            values["SIGNATURE"] = PayPalSettings.Signature;
            values["SUBJECT"] = "";
            values["VERSION"] = PayPalSettings.Version;

            //Order Info
            values["PAYMENTREQUEST_0_AMT"] = order.amount;
            values["PAYMENTREQUEST_0_ITEMAMT"] = order.amount;
            values["PAYMENTREQUEST_0_CURRENCYCODE"] = "USD";
            values["PAYMENTREQUEST_0_SHIPPINGAMT"] = "0.00";
            values["REQCONFIRMSHIPPING"] = "0";
            values["NOSHIPPING"] = "1";
            
            //Optional Order details
            //values["L_PAYMENTREQUEST_0_NAMEian"] = "buy this";
            //values["L_PAYMENTREQUEST_0_DESCian"] = "purchasing this";
            //values["L_PAYMENTREQUEST_0_AMTian"] = order.amount;
            //values["L_PAYMENTREQUEST_0_QTYian"] = "1";
            
            //submit info for payment
            values = Submit(values);

            string ack = values["ACK"].ToLower(); //Status of Payment after submission

            //Redirect customer to new page based on whether payment successful or not
            if (ack == "success" || ack == "successwithwarning")
            {
                return new PayPalRedirect
                {
                    //For paypal express checkout
                    Token = values["TOKEN"],
                    Url = String.Format("https://{0}/webscr?cmd=_express-checkout&token={1}",
                       PayPalSettings.CgiDomain, values["TOKEN"])

                };
            }
            else
            {
                //throw new Exception(values["L_LONGMESSAGE0"]);

                return new PayPalRedirect
                {
                    //Payment Failed :(
                    //Direct user to error page
                    Url = String.Format("http://localhost:51072/Paid/Failure?ErrMsg={0}", values["L_LONGMESSAGE0"])
                };
            }
        }

        //get checkout details of order
        public static NameValueCollection GetExpressCheckout(string token)
        {
            NameValueCollection values = new NameValueCollection();

            //seller paypal api info
            values["METHOD"] = "GetExpressCheckoutDetails";
            values["USER"] = PayPalSettings.Username;
            values["PWD"] = PayPalSettings.Password;
            values["SIGNATURE"] = PayPalSettings.Signature;
            values["VERSION"] = PayPalSettings.Version;

            //transaction details
            values["TOKEN"] = token;

            //submit values
            values = Submit(values);

            return values;
        }

        //do the express checkout for actual payment
        public static NameValueCollection DoExpressCheckout(String token, String payerID, String amount)
        {
            NameValueCollection values = new NameValueCollection();
            
            //seller paypal API info
            values["METHOD"] = "DoExpressCheckoutPayment";
            values["USER"] = PayPalSettings.Username;
            values["PWD"] = PayPalSettings.Password;
            values["SIGNATURE"] = PayPalSettings.Signature;
            values["VERSION"] = PayPalSettings.Version;

            //transaction details
            values["TOKEN"] = token;
            values["PAYERID"] = payerID;
            values["PAYMENTACTION"] = "sale";
            values["PAYMENTREQUEST_0_AMT"] = amount;
            values["PAYMENTREQUEST_0_CURRENCYCODE"] = "USD";

            //submit values
            values = Submit(values);
            return values;

            /*
            string ack = values["ACK"].ToLower(); //Status of Payment after submission

            //Redirect customer to new page based on whether payment successful or not
            if (ack == "success" || ack == "successwithwarning")
            {
                return values;
            }
            else
            {
                throw new Exception(values["L_LONGMESSAGE0"]);
            }*/
        }

        //send request to Paypal and get response
        private static NameValueCollection Submit(NameValueCollection values)
        {
            string data = String.Join("&", values.Cast<string>()
              .Select(key => String.Format("{0}={1}", key, HttpUtility.UrlEncode(values[key]))));

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(
               String.Format("https://{0}/nvp", PayPalSettings.ApiDomain));

            request.Method = "POST";
            request.ContentLength = data.Length;

            using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
            {
                writer.Write(data);
            }

            using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                return HttpUtility.ParseQueryString(reader.ReadToEnd());
            }
        }
    }
}