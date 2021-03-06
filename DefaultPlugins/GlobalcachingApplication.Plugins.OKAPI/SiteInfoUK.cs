﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GlobalcachingApplication.Plugins.OKAPI
{
    public class SiteInfoUK: SiteInfo
    {
        public const string STR_INFO = "opencaching.org.uk";

        public SiteInfoUK()
        {
            ID = "5";
            Info = STR_INFO;
            OKAPIBaseUrl = "http://www.opencaching.org.uk/okapi/";
            GeocodePrefix = "OK";
        }

        public override void LoadSettings()
        {
            Username = Properties.Settings.Default.SiteInfoUKUsername ?? "";
            UserID = Properties.Settings.Default.SiteInfoUKUserID ?? "";
            Token = Properties.Settings.Default.SiteInfoUKToken ?? "";
            TokenSecret = Properties.Settings.Default.SiteInfoUKTokenSecret ?? "";

            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            Properties.Settings.Default.SiteInfoUKUsername = Username;
            Properties.Settings.Default.SiteInfoUKUserID = UserID;
            Properties.Settings.Default.SiteInfoUKToken = Token;
            Properties.Settings.Default.SiteInfoUKTokenSecret = TokenSecret;
            Properties.Settings.Default.Save();
        }

    }
}
