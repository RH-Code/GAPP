﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class SiteInfoUSA: SiteInfo
    {
        public const string STR_INFO = "opencaching.us";

        public SiteInfoUSA()
        {
            ID = "4";
            Info = STR_INFO;
            OKAPIBaseUrl = "http://www.opencaching.us/okapi/";
            GeocodePrefix = "OU";
        }

        public override void LoadSettings()
        {
            Username = Properties.Settings.Default.SiteInfoUSAUsername ?? "";
            UserID = Properties.Settings.Default.SiteInfoUSAUserID ?? "";
            Token = Properties.Settings.Default.SiteInfoUSAToken ?? "";
            TokenSecret = Properties.Settings.Default.SiteInfoUSATokenSecret ?? "";

            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            Properties.Settings.Default.SiteInfoUSAUsername = Username;
            Properties.Settings.Default.SiteInfoUSAUserID = UserID;
            Properties.Settings.Default.SiteInfoUSAToken = Token;
            Properties.Settings.Default.SiteInfoUSATokenSecret = TokenSecret;
            Properties.Settings.Default.Save();
        }

    }
}
