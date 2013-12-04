﻿using GAPPSF.Core;
using GAPPSF.Core.Data;
using GAPPSF.Core.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GAPPSF.Utils
{
    class DataAccess
    {
        public static bool AddGeocache(Database db, GeocacheData gd)
        {
            bool result = true;
            Geocache gc = db.GeocacheCollection.GetGeocache(gd.Code);
            if (gc == null)
            {
                gc = new Geocache(db, gd);
                gc.Selected = Core.Settings.Default.AutoSelectNewGeocaches;
            }
            else
            {
                if (gc.DataFromDate<gd.DataFromDate)
                {
                    gc.BeginUpdate();
                    GeocacheData.Copy(gd, gc);
                    gc.EndUpdate();
                }
            }
            Utils.Calculus.SetDistanceAndAngleGeocacheFromLocation(gc, ApplicationData.Instance.CenterLocation);
            return result;
        }

        public static bool AddLog(Database db, LogData gd)
        {
            bool result = true;
            Log gc = db.LogCollection.GetLog(gd.ID);
            if (gc == null)
            {
                gc = new Log(db, gd);
            }
            else
            {
                if (gc.DataFromDate < gd.DataFromDate)
                {
                    gc.BeginUpdate();
                    LogData.Copy(gd, gc);
                    gc.EndUpdate();
                }
            }
            return result;
        }

        public static bool AddWaypoint(Database db, WaypointData gd)
        {
            bool result = true;
            Waypoint gc = db.WaypointCollection.GetWaypoint(gd.ID);
            if (gc == null)
            {
                gc = new Waypoint(db, gd);
            }
            else
            {
                if (gc.DataFromDate < gd.DataFromDate)
                {
                    gc.BeginUpdate();
                    WaypointData.Copy(gd, gc);
                    gc.EndUpdate();
                }
            }
            return result;
        }

        public static bool AddLogImage(Database db,LogImageData gd)
        {
            bool result = true;
            LogImage gc = db.LogImageCollection.GetLogImage(gd.ID);
            if (gc == null)
            {
                gc = new LogImage(db, gd);
            }
            else
            {
                if (gc.DataFromDate < gd.DataFromDate)
                {
                    gc.BeginUpdate();
                    LogImageData.Copy(gd, gc);
                    gc.EndUpdate();
                }
            }
            return result;
        }

        public static bool AddGeocacheImage(Database db, GeocacheImageData gd)
        {
            bool result = true;
            GeocacheImage gc = db.GeocacheImageCollection.GetGeocacheImage(gd.ID);
            if (gc == null)
            {
                gc = new GeocacheImage(db, gd);
            }
            else
            {
                if (gc.DataFromDate < gd.DataFromDate)
                {
                    gc.BeginUpdate();
                    GeocacheImageData.Copy(gd, gc);
                    gc.EndUpdate();
                }
            }
            return result;
        }

        public static bool AddUserWaypoint(Database db, UserWaypointData gd)
        {
            bool result = true;
            UserWaypoint gc = db.UserWaypointCollection.GetUserWaypoint(gd.ID);
            if (gc == null)
            {
                gc = new UserWaypoint(db, gd);
            }
            else
            {
                gc.BeginUpdate();
                UserWaypointData.Copy(gd, gc);
                gc.EndUpdate();
            }
            return result;
        }

        public static GAPPSF.Core.Data.WaypointType GetWaypointType(int typeId)
        {
            GAPPSF.Core.Data.WaypointType result = null;

            result = (from gt in Core.ApplicationData.Instance.WaypointTypes
                      where gt.ID == typeId
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in Core.ApplicationData.Instance.WaypointTypes
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }

        public static GAPPSF.Core.Data.WaypointType GetWaypointType(string keyWord)
        {
            GAPPSF.Core.Data.WaypointType result = null;

            keyWord = keyWord.ToLower();
            result = (from gt in Core.ApplicationData.Instance.WaypointTypes
                      where gt.Name.ToLower().Contains(keyWord)
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = GetWaypointType(0);
            }
            return result;
        }


        public static GAPPSF.Core.Data.LogType GetLogType(int typeId)
        {
            GAPPSF.Core.Data.LogType result = null;

            result = (from gt in ApplicationData.Instance.LogTypes
                      where gt.ID == typeId
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in ApplicationData.Instance.LogTypes
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }


        public static Core.Data.LogType GetLogType(string keyWord)
        {
            Core.Data.LogType result = null;

            keyWord = keyWord.ToLower();
            result = (from gt in ApplicationData.Instance.LogTypes
                      where gt.Name.ToLower().Contains(keyWord)
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = GetLogType(0);
            }
            return result;
        }


        public static GAPPSF.Core.Data.GeocacheType GetGeocacheType(int typeId)
        {
            GAPPSF.Core.Data.GeocacheType result = null;

            result = (from gt in ApplicationData.Instance.GeocacheTypes
                      where gt.ID == typeId
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in ApplicationData.Instance.GeocacheTypes
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }

        public static GAPPSF.Core.Data.GeocacheType GetGeocacheType(string keyWord)
        {
            return GetGeocacheType(keyWord, 0);
        }
        public static GAPPSF.Core.Data.GeocacheType GetGeocacheType(string keyWord, int minID)
        {
            GAPPSF.Core.Data.GeocacheType result = null;

            keyWord = keyWord.ToLower();
            result = (from gt in ApplicationData.Instance.GeocacheTypes
                      where gt.ID >= minID && gt.Name.ToLower().Contains(keyWord)
                      orderby gt.ID
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = GetGeocacheType(0);
            }
            return result;
        }


        public static GAPPSF.Core.Data.GeocacheContainer GetGeocacheContainer(int containerId)
        {
            GAPPSF.Core.Data.GeocacheContainer result = null;

            result = (from gt in ApplicationData.Instance.GeocacheContainers
                      where gt.ID == containerId
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = (from gt in ApplicationData.Instance.GeocacheContainers
                          where gt.ID == 0
                          select gt).FirstOrDefault();
            }
            return result;
        }

        public static GAPPSF.Core.Data.GeocacheContainer GetGeocacheContainer(string container)
        {
            GAPPSF.Core.Data.GeocacheContainer result = null;

            container = container.ToLower();
            result = (from gt in ApplicationData.Instance.GeocacheContainers
                      where gt.Name.ToLower() == container
                      select gt).FirstOrDefault();
            if (result == null)
            {
                //take special ID
                result = GetGeocacheContainer(0);
            }
            return result;
        }

        public static List<GAPPSF.Core.Data.GeocacheImage> GetGeocacheImages(Database db, string GeocacheCode)
        {
            //grouping
            return db.GeocacheImageCollection.GetGeocacheImages(GeocacheCode);
        }


        public static List<GAPPSF.Core.Data.UserWaypoint> GetUserWaypointsFromGeocache(Database db, string geocacheCode)
        {
            return db.UserWaypointCollection.GetWaypoints(geocacheCode);
        }

        public static List<Core.Data.Waypoint> GetWaypointsFromGeocache(Database db, string geocacheCode)
        {
            return db.WaypointCollection.GetWaypoints(geocacheCode);
        }

        public static List<Core.Data.Log> GetLogs(Database db, string geocacheCode)
        {
            return db.LogCollection.GetLogs(geocacheCode);
        }

        public static List<Core.Data.LogImage> GetLogImages(Database db, string logId)
        {
            return db.LogImageCollection.GetLogImages(logId);
        }


        public static List<string> GetImagesOfGeocache(Database db, string geocacheCode)
        {
            List<string> result = new List<string>();

            Core.Data.Geocache gc = db.GeocacheCollection.GetGeocache(geocacheCode);

            if (gc != null)
            {
                StringBuilder sb = new StringBuilder();
                if (gc.ShortDescriptionInHtml && gc.ShortDescription != null)
                {
                    sb.Append(gc.ShortDescription);
                }
                if (gc.LongDescriptionInHtml && gc.LongDescription != null)
                {
                    sb.Append(gc.LongDescription);
                }
                if (sb.Length > 0)
                {

                    Regex r = new Regex(@"</?\w+\s+[^>]*>", RegexOptions.Multiline);
                    MatchCollection mc = r.Matches(sb.ToString());
                    foreach (Match m in mc)
                    {
                        string s = m.Value.Substring(1).Replace('\r', ' ').Replace('\n', ' ').Trim();
                        if (s.StartsWith("img ", StringComparison.OrdinalIgnoreCase))
                        {
                            int pos = s.IndexOf(" src", StringComparison.OrdinalIgnoreCase);
                            pos = s.IndexOfAny(new char[] { '\'', '"' }, pos);
                            int pos2 = s.IndexOfAny(new char[] { '\'', '"' }, pos + 1);
                            result.Add(s.Substring(pos + 1, pos2 - pos - 1));
                        }
                    }
                }
                List<Core.Data.GeocacheImage> imgList = DataAccess.GetGeocacheImages(db, geocacheCode);
                if (imgList != null)
                {
                    foreach (Core.Data.GeocacheImage img in imgList)
                    {
                        if (!result.Contains(img.Url))
                        {
                            result.Add(img.Url);
                        }
                    }
                }
            }
            return result;
        }

        public static void SetCenterLocation(double lat, double lon)
        {
            Core.ApplicationData.Instance.CenterLocation.Lat = lat;
            Core.ApplicationData.Instance.CenterLocation.Lon = lon;
            UpdateDistanceAndAngle();
        }

        public static void UpdateDistanceAndAngle()
        {
            foreach (Core.Storage.Database db in Core.ApplicationData.Instance.Databases)
            {
                using (DataUpdater upd = new DataUpdater(db))
                {
                    Calculus.SetDistanceAndAngleGeocacheFromLocation(db.GeocacheCollection, Core.ApplicationData.Instance.CenterLocation);
                }
            }
        }
    }
}