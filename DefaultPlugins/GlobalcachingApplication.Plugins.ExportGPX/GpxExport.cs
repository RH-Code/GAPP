﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;

namespace GlobalcachingApplication.Plugins.ExportGPX
{
    public class GpxExport: Utils.BasePlugin.BaseExportFilter
    {
        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_EXPORTINGGPX = "Exporting GPX...";
        public const string STR_CREATINGFILE = "Creating file...";

        public const string ACTION_EXPORT_ALL = "Export GPX|All";
        public const string ACTION_EXPORT_SELECTED = "Export GPX|Selected";
        public const string ACTION_EXPORT_ACTIVE = "Export GPX|Active";

        private string _filename = null;
        private Utils.GPXGenerator _gpxGenerator = null;

        public override string FriendlyName
        {
            get { return "Export GPX"; }
        }

        public override bool Initialize(Framework.Interfaces.ICore core)
        {
            if (Properties.Settings.Default.UpgradeNeeded)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeNeeded = false;
                Properties.Settings.Default.Save();
            }

            AddAction(ACTION_EXPORT_ALL);
            AddAction(ACTION_EXPORT_SELECTED);
            AddAction(ACTION_EXPORT_ACTIVE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXPORTINGGPX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATINGFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_GPXVERSION));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ZIPFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ADDWPTTODESCR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_USEHINTSDESCR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_INCLNOTES));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_ADDWAYPOINTS));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(SettingsPanel.STR_EXTRACOORDNAMEPREFIX));

            return base.Initialize(core);
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_EXPORT_ALL || action == ACTION_EXPORT_SELECTED || action == ACTION_EXPORT_ACTIVE)
                {
                    List<Framework.Data.Geocache> gcList = null;
                    if (action == ACTION_EXPORT_ALL)
                    {
                        gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                    }
                    else if (action == ACTION_EXPORT_SELECTED)
                    {
                        gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    else
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            gcList = new List<Framework.Data.Geocache>();
                            gcList.Add(Core.ActiveGeocache);
                        }
                    }
                    if (gcList == null || gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        using (System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
                        {
                            dlg.FileName = "";
                            if (Properties.Settings.Default.ZipFile)
                            {
                                dlg.Filter = "*.zip|*.zip";
                            }
                            else
                            {
                                dlg.Filter = "*.gpx|*.gpx";
                            }
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                _filename = dlg.FileName;
                                _gpxGenerator = new Utils.GPXGenerator(Core, gcList, string.IsNullOrEmpty(Properties.Settings.Default.GPXVersionStr) ? Utils.GPXGenerator.V101: Version.Parse(Properties.Settings.Default.GPXVersionStr));
                                _gpxGenerator.MaxNameLength = Properties.Settings.Default.MaxGeocacheNameLength;
                                _gpxGenerator.MinStartOfname = Properties.Settings.Default.MinStartOfGeocacheName;
                                _gpxGenerator.UseNameForGCCode = Properties.Settings.Default.UseNameAndNotCode;
                                _gpxGenerator.AddAdditionWaypointsToDescription = Properties.Settings.Default.AddWaypointsToDescription;
                                _gpxGenerator.UseHintsForDescription = Properties.Settings.Default.UseHintsForDescription;
                                _gpxGenerator.AddFieldnotesToDescription = Properties.Settings.Default.AddFieldnotesToDescription;
                                _gpxGenerator.ExtraCoordPrefix = Properties.Settings.Default.CorrectedNamePrefix;
                                PerformExport();
                            }
                        }
                    }
                }
            }
            return result;
        }

        protected override void ExportMethod()
        {
            string gpxFile;
            string wptFile;
            System.IO.TemporaryFile tmp = null;
            System.IO.TemporaryFile tmpwpt = null;
            if (Properties.Settings.Default.ZipFile)
            {
                tmp = new System.IO.TemporaryFile(false);
                gpxFile = tmp.Path;
                tmpwpt = new System.IO.TemporaryFile(false);
                wptFile = tmpwpt.Path;
            }
            else
            {
                gpxFile = _filename;
                wptFile = string.Format("{0}-wpts.gpx", gpxFile.Substring(0,gpxFile.Length-4));
            }
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_EXPORTINGGPX, STR_CREATINGFILE, _gpxGenerator.Count, 0))
            {
                //create file stream (if not zipped actual file and if zipped tmp file
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(gpxFile))
                using (System.IO.StreamWriter swwp = System.IO.File.CreateText(wptFile))
                {
                    int block = 0;
                    //generate header
                    sw.Write(_gpxGenerator.Start());
                    if (Properties.Settings.Default.AddWaypoints)
                    {
                        swwp.Write(_gpxGenerator.WaypointData());
                    }
                    //preserve mem and do for each cache the export
                    for (int i = 0; i < _gpxGenerator.Count; i++)
                    {
                        sw.WriteLine(_gpxGenerator.Next());
                        if (Properties.Settings.Default.AddWaypoints)
                        {
                            string s = _gpxGenerator.WaypointData();
                            if (!string.IsNullOrEmpty(s))
                            {
                                swwp.WriteLine(s);
                            }
                        }
                        block++;
                        if (block > 50)
                        {
                            block = 0;
                            progress.UpdateProgress(STR_EXPORTINGGPX, STR_CREATINGFILE, _gpxGenerator.Count, i + 1);
                        }
                    }
                    //finalize
                    sw.Write(_gpxGenerator.Finish());
                    if (Properties.Settings.Default.AddWaypoints)
                    {
                        swwp.Write(_gpxGenerator.Finish());
                    }
                }

                if (Properties.Settings.Default.ZipFile)
                {
                    try
                    {
                        List<string> filenames = new List<string>();
                        filenames.Add(gpxFile);
                        if (Properties.Settings.Default.AddWaypoints)
                        {
                            filenames.Add(wptFile);
                        }

                        using (ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(_filename)))
                        {
                            s.SetLevel(9); // 0-9, 9 being the highest compression

                            byte[] buffer = new byte[4096];
                            bool wpt = false;

                            foreach (string file in filenames)
                            {

                                ZipEntry entry = new ZipEntry(System.IO.Path.GetFileName(wpt ? _filename.ToLower().Replace(".zip", "-wpts.gpx") : _filename.ToLower().Replace(".zip", ".gpx")));

                                entry.DateTime = DateTime.Now;
                                s.PutNextEntry(entry);

                                using (System.IO.FileStream fs = System.IO.File.OpenRead(file))
                                {
                                    int sourceBytes;
                                    do
                                    {
                                        sourceBytes = fs.Read(buffer, 0,
                                        buffer.Length);

                                        s.Write(buffer, 0, sourceBytes);

                                    } while (sourceBytes > 0);
                                }

                                wpt = true;
                            }
                            s.Finish();
                            s.Close();
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    if (!Properties.Settings.Default.AddWaypoints)
                    {
                        System.IO.File.Delete(wptFile);
                    }
                }
            }
            if (tmp != null)
            {
                tmp.Dispose();
                tmp = null;
            }
            if (tmpwpt != null)
            {
                tmpwpt.Dispose();
                tmpwpt = null;
            }
        }

        public override bool ApplySettings(List<System.Windows.Forms.UserControl> configPanels)
        {
            foreach (System.Windows.Forms.UserControl uc in configPanels)
            {
                if (uc is SettingsPanel)
                {
                    (uc as SettingsPanel).Apply();
                    break;
                }
            }
            return true;
        }

        public override List<System.Windows.Forms.UserControl> CreateConfigurationPanels()
        {
            List<System.Windows.Forms.UserControl> pnls = base.CreateConfigurationPanels();
            if (pnls == null) pnls = new List<System.Windows.Forms.UserControl>();
            pnls.Add(new SettingsPanel());
            return pnls;
        }

    }
}
