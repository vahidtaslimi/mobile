﻿using Bit.App.Abstractions;
using Foundation;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace Bit.iOS.Services
{
    public class iOSPushNotificationHandler
    {
        private const string TokenSetting = "token";
        private const string DomainName = "iOSPushNotificationService";

        private readonly IPushNotificationListener _pushNotificationListener;

        public iOSPushNotificationHandler(
            IPushNotificationListener pushNotificationListener)
        {
            _pushNotificationListener = pushNotificationListener;
        }

        public void OnMessageReceived(NSDictionary userInfo)
        {
            var json = DictionaryToJson(userInfo);
            var values = JObject.Parse(json);

            var keyAps = new NSString("aps");
            if(userInfo.ContainsKey(keyAps) && userInfo.ValueForKey(keyAps) is NSDictionary aps)
            {
                foreach(var apsKey in aps)
                {
                    if(!values.TryGetValue(apsKey.Key.ToString(), out JToken temp))
                    {
                        values.Add(apsKey.Key.ToString(), apsKey.Value.ToString());
                    }
                }
            }

            _pushNotificationListener.OnMessage(values, Device.iOS);
        }

        public void OnErrorReceived(NSError error)
        {
            Debug.WriteLine("{0} - Registration Failed.", DomainName);
            _pushNotificationListener.OnError(error.LocalizedDescription, Device.iOS);
        }

        public void OnRegisteredSuccess(NSData token)
        {
            Debug.WriteLine("{0} - Successfully Registered.", DomainName);

            var trimmedDeviceToken = token.Description;
            if(!string.IsNullOrWhiteSpace(trimmedDeviceToken))
            {
                trimmedDeviceToken = trimmedDeviceToken.Trim('<').Trim('>').Trim().Replace(" ", string.Empty);
            }

            Console.WriteLine("{0} - Token: {1}", DomainName, trimmedDeviceToken);
            _pushNotificationListener.OnRegistered(trimmedDeviceToken, Device.iOS);
            NSUserDefaults.StandardUserDefaults.SetString(trimmedDeviceToken, TokenSetting);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        private static string DictionaryToJson(NSDictionary dictionary)
        {
            var json = NSJsonSerialization.Serialize(dictionary, NSJsonWritingOptions.PrettyPrinted, out NSError error);
            return json.ToString(NSStringEncoding.UTF8);
        }
    }
}
