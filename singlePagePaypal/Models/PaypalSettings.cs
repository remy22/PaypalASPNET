﻿//PaypalSettings.cs
//Setters and Getters for paypal API settings for Seller Account (from appSettings in web.config)

using System;
using System.Configuration;
using System.ComponentModel;
using System.Globalization;

public static class PayPalSettings
{
    public static string ApiDomain
    {
        get
        {
            return Setting<bool>("PayPal:Sandbox") ? "api-3t.sandbox.paypal.com"
               : "api-3t.paypal.com";
        }
    }

    public static string CgiDomain
    {
        get
        {
            return Setting<bool>("PayPal:Sandbox") ? "www.sandbox.paypal.com" : "www.paypal.com";
        }
    }

    public static string Signature
    {
        get
        {
            return Setting<string>("PayPal:Signature");
        }
    }

    public static string Username
    {
        get
        {
            return Setting<string>("PayPal:Username");
        }
    }

    public static string Password
    {
        get
        {
            return Setting<string>("PayPal:Password");
        }
    }

    public static string ReturnUrl
    {
        get
        {
            return Setting<string>("PayPal:ReturnUrl");
        }
    }

    public static string CancelUrl
    {
        get
        {
            return Setting<string>("PayPal:CancelUrl");
        }
    }

    public static string Version
    {
        get
        {
            return Setting<string>("PayPal:Version");
        }
    }

    private static T Setting<T>(string name)
    {
        string value = ConfigurationManager.AppSettings[name];

        if (value == null)
        {
            throw new Exception(String.Format("Could not find setting '{0}',", name));
        }

        return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
    }
}