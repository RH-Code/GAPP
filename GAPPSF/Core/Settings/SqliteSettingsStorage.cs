﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using System.Data.Common;
using System.Globalization;

namespace GAPPSF.Core
{
    public class SqliteSettingsStorage: ISettingsStorage
    {
        private  Utils.DBCon _dbcon = null;
        private Hashtable _availableKeys;
        private Hashtable _ignoredGeocacheCodes;
        private Hashtable _ignoredGeocacheNames;
        private Hashtable _ignoredGeocacheOwners;

        public SqliteSettingsStorage()
        {
            _availableKeys = new Hashtable();
            _ignoredGeocacheCodes = new Hashtable();
            _ignoredGeocacheNames = new Hashtable();
            _ignoredGeocacheOwners = new Hashtable();
            try
            {
                string sf = Properties.Settings.Default.SettingsFolder;
                if (string.IsNullOrEmpty(sf))
                {
                    sf = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "GAPPSF");
                }
                if (!Directory.Exists(sf))
                {
                    Directory.CreateDirectory(sf);
                }
                Properties.Settings.Default.SettingsFolder = sf;
                Properties.Settings.Default.Save();

                sf = Path.Combine(sf, "settings.db3");

                _dbcon = new Utils.DBConComSqlite(sf);

                if (!_dbcon.TableExists("settings"))
                {
                    _dbcon.ExecuteNonQuery("create table 'settings' (item_name text, item_value text)");
                    _dbcon.ExecuteNonQuery("create index idx_key on settings (item_name)");
                }
                else
                {
                    DbDataReader dr = _dbcon.ExecuteReader("select item_name, item_value from settings");
                    while (dr.Read())
                    {
                        _availableKeys[dr[0] as string] = dr[1] as string;
                    }
                }
                if (!_dbcon.TableExists("ignoregc"))
                {
                    _dbcon.ExecuteNonQuery("create table 'ignoregc' (item_name text, item_value text)");
                }
                else
                {
                    DbDataReader dr = _dbcon.ExecuteReader("select item_name, item_value from ignoregc");
                    while (dr.Read())
                    {
                        string item = dr[0] as string;
                        if (item == "code")
                        {
                            _ignoredGeocacheCodes[dr[1] as string] = true;
                        }
                        else if (item == "name")
                        {
                            _ignoredGeocacheNames[dr[1] as string] = true;
                        }
                        else if (item == "owner")
                        {
                            _ignoredGeocacheOwners[dr[1] as string] = true;
                        }
                    }
                }
                if (!_dbcon.TableExists("gccombm"))
                {
                    _dbcon.ExecuteNonQuery("create table 'gccombm' (bm_id text, bm_name text, bmguid text)");
                    _dbcon.ExecuteNonQuery("create index idx_bmid on gccombm (bm_id)");
                }
                if (!_dbcon.TableExists("gccomgc"))
                {
                    _dbcon.ExecuteNonQuery("create table 'gccomgc' (bm_id text, gccode text)");
                    _dbcon.ExecuteNonQuery("create index idx_bmgcid on gccomgc (bm_id)");
                }
                if (!_dbcon.TableExists("attachm"))
                {
                    _dbcon.ExecuteNonQuery("create table 'attachm' (gccode text, filename text, comments text)");
                    _dbcon.ExecuteNonQuery("create index idx_att on attachm (gccode)");
                }
                if (!_dbcon.TableExists("formulasolv"))
                {
                    _dbcon.ExecuteNonQuery("create table 'formulasolv' (gccode text, formula text)");
                    _dbcon.ExecuteNonQuery("create index idx_form on formulasolv (gccode)");
                }
                if (!_dbcon.TableExists("gcnotes"))
                {
                    _dbcon.ExecuteNonQuery("create table 'gcnotes' (gccode text, notes text)");
                    _dbcon.ExecuteNonQuery("create index idx_note on gcnotes (gccode)");
                }
                if (!_dbcon.TableExists("gccollection"))
                {
                    _dbcon.ExecuteNonQuery("create table 'gccollection' (col_id integer primary key autoincrement, name text)");
                    //_dbcon.ExecuteNonQuery("create index idx_col on gccollection (name)");
                }
                if (!_dbcon.TableExists("gcincol"))
                {
                    _dbcon.ExecuteNonQuery("create table 'gcincol' (col_id integer, gccode text)");
                    _dbcon.ExecuteNonQuery("create index idx_gccol on gcincol (col_id)");
                }
                if (!_dbcon.TableExists("gcdist"))
                {
                    _dbcon.ExecuteNonQuery("create table 'gcdist' (gccode text, dist float)");
                    _dbcon.ExecuteNonQuery("create index idx_dist on gcdist (gccode)");
                }
                if (!_dbcon.TableExists("gcvotes"))
                {
                    _dbcon.ExecuteNonQuery("create table 'gcvotes' (gccode text, VoteMedian float, VoteAvg float, VoteCnt integer, VoteUser float)");
                    _dbcon.ExecuteNonQuery("create unique index idx_gcvotes on gcvotes (gccode)");
                }

                object o = _dbcon.ExecuteScalar("PRAGMA integrity_check");
                if (o as string == "ok")
                {
                    //what is expected
                }
                else
                {
                    //oeps?
                    _dbcon.Dispose();
                    _dbcon = null;
                }
            }
            catch//(Exception e)
            {
                //Core.ApplicationData.Instance.Logger.AddLog(this, e);
                _dbcon = null;
            }
        }

        public bool IsStorageOK 
        {
            get
            {
                bool result;
                lock (this)
                {
                    result = _dbcon != null;
                }
                return result;
            }
        }

        public bool CreateBackup()
        {
            bool result = false;
            try
            {
                File.Copy(Path.Combine(Properties.Settings.Default.SettingsFolder, "settings.db3"), Path.Combine(Properties.Settings.Default.SettingsFolder, string.Format("settings.{0}.db3", DateTime.Now.ToString("s").Replace(" ", "-").Replace(":", "-"))), true);
                result = true;
            }
            catch(Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return result;
        }

        public List<string> AvailableBackups 
        {
            get
            {
                List<string> result;
                try
                {
                    string[] fls = Directory.GetFiles(Properties.Settings.Default.SettingsFolder, "settings.*.db3");
                    result = (from s in fls select Path.GetFileName(s)).OrderBy(x => x).ToList();
                }
                catch (Exception e)
                {
                    result = new List<string>();
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
                return result;
            }
        }

        public bool RemoveBackup(string id)
        {
            bool result = false;
            try
            {
                File.Delete(Path.Combine(Properties.Settings.Default.SettingsFolder, id));
                result = true;
            }
            catch (Exception e)
            {
                Core.ApplicationData.Instance.Logger.AddLog(this, e);
            }
            return result;
        }

        public bool PrepareRestoreBackup(string id)
        {
            bool result = false;
            lock (this)
            {
                bool restoreConnection = _dbcon != null;
                try
                {
                    if (restoreConnection)
                    {
                        //copy settings.db3 to settings.db3.bak
                        if (_dbcon != null)
                        {
                            _dbcon.Dispose();
                            _dbcon = null;
                        }
                        File.Copy(Path.Combine(Properties.Settings.Default.SettingsFolder, "settings.db3"), Path.Combine(Properties.Settings.Default.SettingsFolder, "settings.db3.bak"), true);
                    }
                    File.Copy(Path.Combine(Properties.Settings.Default.SettingsFolder, id), Path.Combine(Properties.Settings.Default.SettingsFolder, "settings.db3"), true);

                    if (restoreConnection)
                    {
                        //connect to previous settings file, so the backup is not overwritten.
                        //application needs to restart
                        //after restart the backup is used
                        string sf = Path.Combine(Properties.Settings.Default.SettingsFolder, "settings.db3.bak");
                        _dbcon = new Utils.DBConComSqlite(sf);
                    }

                    result = true;
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);

                    if (restoreConnection)
                    {
                        string sf = Path.Combine(Properties.Settings.Default.SettingsFolder, "settings.db3");
                        if (_dbcon != null)
                        {
                            _dbcon.Dispose();
                            _dbcon = null;
                        }
                        _dbcon = new Utils.DBConComSqlite(sf);
                    }
                }
            }
            return result;
        }

        public void StoreSetting(string name, string value)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_availableKeys.ContainsKey(name))
                    {
                        _dbcon.ExecuteNonQuery(string.Format("update settings set item_value={1} where item_name='{0}'", name, value == null ? "NULL" : string.Format("'{0}'", value.Replace("'", "''"))));
                    }
                    else
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into settings (item_name, item_value) values ('{0}', {1})", name, value == null ? "NULL" : string.Format("'{0}'", value.Replace("'", "''"))));
                    }
                    _availableKeys[name] = value;
                }
            }
        }

        public Hashtable LoadSettings()
        {
            Hashtable result = new Hashtable();
            foreach (DictionaryEntry kp in _availableKeys)
            {
                result.Add(kp.Key as string, kp.Value as string);
            }
            return result;
        }

        public void Dispose()
        {
            if (_dbcon!=null)
            {
                _dbcon.Dispose();
                _dbcon = null;
            }
        }


        public Hashtable LoadIgnoredGeocacheCodes()
        {
            Hashtable result = new Hashtable();
            foreach (DictionaryEntry kp in _ignoredGeocacheCodes)
            {
                result.Add(kp.Key as string, kp.Value as string);
            }
            return result;
        }

        public List<string> LoadIgnoredGeocacheNames()
        {
            return (from string a in _ignoredGeocacheNames.Keys select a).ToList();
        }

        public List<string> LoadIgnoredGeocacheOwners()
        {
            return (from string a in _ignoredGeocacheOwners.Keys select a).ToList();
        }

        public void ClearGeocacheIgnoreFilters()
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery("delete from ignoregc");
                    _ignoredGeocacheCodes.Clear();
                    _ignoredGeocacheNames.Clear();
                    _ignoredGeocacheOwners.Clear();
                }
            }
        }

        public void AddIgnoreGeocacheCode(string code)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_ignoredGeocacheCodes[code] == null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into ignoregc (item_name, item_value) values ('code', '{0}')", code));
                        _ignoredGeocacheCodes[code] = true;
                    }
                }
            }
        }

        public void AddIgnoreGeocacheName(string name)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_ignoredGeocacheNames[name] == null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into ignoregc (item_name, item_value) values ('name', '{0}')", name.Replace("'", "''")));
                        _ignoredGeocacheNames[name] = true;
                    }
                }
            }
        }

        public void AddIgnoreGeocacheOwner(string owner)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_ignoredGeocacheOwners[owner] == null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into ignoregc (item_name, item_value) values ('owner', '{0}')", owner.Replace("'", "''")));
                        _ignoredGeocacheOwners[owner] = true;
                    }
                }
            }
        }

        public void DeleteIgnoreGeocacheCode(string code)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_ignoredGeocacheCodes[code] != null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from ignoregc where item_name='code' and item_value='{0}'", code));
                        _ignoredGeocacheCodes.Remove(code);
                    }
                }
            }
        }

        public void DeleteIgnoreGeocacheName(string name)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_ignoredGeocacheNames[name] != null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from ignoregc where item_name='name' and item_value='{0}'", name.Replace("'", "''")));
                        _ignoredGeocacheNames.Remove(name);
                    }
                }
            }
        }

        public void DeleteIgnoreGeocacheOwner(string owner)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_ignoredGeocacheOwners[owner] != null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from ignoregc where item_name='owner' and item_value='{0}'", owner.Replace("'", "''")));
                        _ignoredGeocacheOwners.Remove(owner);
                    }
                }
            }
        }

        public List<GCComBookmarks.Bookmark> LoadGCComBookmarks()
        {
            List<GCComBookmarks.Bookmark> result = new List<GCComBookmarks.Bookmark>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    DbDataReader dr = _dbcon.ExecuteReader("select bm_id, bm_name, bmguid from gccombm");
                    while (dr.Read())
                    {
                        GCComBookmarks.Bookmark bm = new GCComBookmarks.Bookmark();
                        bm.Guid = dr["bmguid"] as string;
                        bm.ID = dr["bm_id"] as string;
                        bm.Name = dr["bm_name"] as string;
                        result.Add(bm);
                    }
                }
            }
            return result;
        }

        public void AddGCComBookmark(GCComBookmarks.Bookmark bm)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery(string.Format("insert into gccombm (bm_id, bm_name, bmguid) values ('{0}','{1}','{2}')", bm.ID, bm.Name.Replace("'", "''"), bm.Guid));
                }
            }
        }

        public void DeleteGCComBookmark(GCComBookmarks.Bookmark bm)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery(string.Format("delete from gccomgc where bm_id='{0}'", bm.ID));
                    _dbcon.ExecuteNonQuery(string.Format("delete from gccombm where bm_id='{0}'", bm.ID));
                }
            }
        }

        public List<string> LoadGCComBookmarkGeocaches(GCComBookmarks.Bookmark bm)
        {
            List<string> result = new List<string>();
            lock (this)
            {
                DbDataReader dr = _dbcon.ExecuteReader(string.Format("select gccode from gccomgc where bm_id='{0}'", bm.ID));
                while (dr.Read())
                {
                    result.Add(dr[0] as string);
                }
            }
            return result;
        }

        public void SaveGCComBookmarkGeocaches(GCComBookmarks.Bookmark bm, List<string> gcCodes)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    List<string> gcav = LoadGCComBookmarkGeocaches(bm);
                    foreach (string gc in gcav)
                    {
                        if (!gcCodes.Contains(gc))
                        {
                            _dbcon.ExecuteNonQuery(string.Format("delete from gccomgc where bm_id='{0}' and gccode='{1}'", bm.ID, gc));
                        }
                    }
                    foreach (string gc in gcCodes)
                    {
                        if (!gcav.Contains(gc))
                        {
                            _dbcon.ExecuteNonQuery(string.Format("insert into gccomgc (bm_id, gccode) values ('{0}', '{1}')", bm.ID, gc));
                        }
                    }
                }
            }
        }


        public List<Attachement.Item> GetAttachements(string gcCode)
        {
            List<Attachement.Item> result = new List<Attachement.Item>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    DbDataReader dr;
                    if (string.IsNullOrEmpty(gcCode))
                    {
                        dr = _dbcon.ExecuteReader("select gccode, filename, comments from attachm");
                    }
                    else
                    {
                        dr = _dbcon.ExecuteReader(string.Format("select gccode, filename, comments from attachm where gccode='{0}'", gcCode));
                    }
                    while (dr.Read())
                    {
                        Attachement.Item it = new Attachement.Item();
                        it.GeocacheCode = dr["gccode"] as string;
                        it.FileName = dr["filename"] as string;
                        it.Comment = dr["comments"] as string;
                        result.Add(it);
                    }
                }
            }
            return result;
        }
        public void AddAttachement(Attachement.Item item)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery(string.Format("insert into attachm (gccode, filename, comments) values ('{0}','{1}','{2}')", item.GeocacheCode, item.FileName, item.Comment.Replace("'", "''")));
                }
            }
        }
        public void DeleteAttachement(Attachement.Item item)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery(string.Format("delete from attachm where gccode='{0}' and filename='{1}' and comments='{2}'", item.GeocacheCode, item.FileName, item.Comment.Replace("'", "''")));
                }
            }
        }


        public string GetFormula(string gcCode)
        {
            string result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = _dbcon.ExecuteScalar(string.Format("select formula from formulasolv where gccode='{0}'", gcCode)) as string;
                }
            }
            return result;
        }
        public void SetFormula(string gcCode, string formula)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (string.IsNullOrEmpty(formula))
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from formulasolv where gccode='{0}'", gcCode));
                    }
                    else if (_dbcon.ExecuteNonQuery(string.Format("update formulasolv set formula='{1}' where gccode='{0}'", gcCode, formula.Replace("'","''"))) == 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into formulasolv (gccode, formula) values ('{0}', '{1}')", gcCode, formula.Replace("'", "''")));
                    }
                }
            }
        }

        public string GetGeocacheNotes(string gcCode)
        {
            string result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = _dbcon.ExecuteScalar(string.Format("select notes from gcnotes where gccode='{0}'", gcCode)) as string;
                }
            }
            return result;
        }

        public void SetGeocacheNotes(string gcCode, string notes)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (string.IsNullOrEmpty(notes))
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from gcnotes where gccode='{0}'", gcCode));
                    }
                    else if (_dbcon.ExecuteNonQuery(string.Format("update gcnotes set notes='{1}' where gccode='{0}'", gcCode, notes.Replace("'", "''"))) == 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into gcnotes (gccode, notes) values ('{0}', '{1}')", gcCode, notes.Replace("'", "''")));
                    }
                }
            }
        }

        private int getGCCollectionID(string name)
        {
            int result = -1;
            DbDataReader dr = _dbcon.ExecuteReader(string.Format("select col_id from gccollection where name='{0}'", name.Replace("'", "''")));
            if (dr.Read())
            {
                result = dr.GetInt32(0);
            }
            return result;
        }
        public int GetCollectionID(string collectionName)
        {
            int result = -1;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = getGCCollectionID(collectionName);
                }
            }
            return result;
        }
        public List<string> AvailableCollections()
        {
            List<string> result = new List<string>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    DbDataReader dr = _dbcon.ExecuteReader("select name from gccollection");
                    while (dr.Read())
                    {
                        result.Add(dr[0] as string);
                    }
                }
            }
            return result;
        }
        public List<string> GetGeocachesInCollection(string collectionName)
        {
            List<string> result = new List<string>();
            lock (this)
            {
                if (_dbcon != null)
                {
                    int id = getGCCollectionID(collectionName);
                    if (id >= 0)
                    {
                        DbDataReader dr = _dbcon.ExecuteReader(string.Format("select gccode from gcincol where col_id={0}", id));
                        while (dr.Read())
                        {
                            result.Add(dr[0] as string);
                        }
                    }
                }
            }
            return result;
        }
        public void AddCollection(string name)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    int id = getGCCollectionID(name);
                    if (id < 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into gccollection (name) values ('{0}')", name.Replace("'", "''")));
                    }
                }
            }
        }
        public void DeleteCollection(string name)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    int id = getGCCollectionID(name);
                    if (id >= 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from gcincol where col_id={0}", id));
                        _dbcon.ExecuteNonQuery(string.Format("delete from gccollection where col_id={0}", id));
                    }
                }
            }
        }
        public void AddToCollection(string collectionName, string geocacheCode)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    int id = getGCCollectionID(collectionName);
                    if (id >= 0)
                    {
                        if (!InCollection(id, geocacheCode))
                        {
                            _dbcon.ExecuteNonQuery(string.Format("insert into gcincol (col_id, gccode) values ({0}, '{1}')", id, geocacheCode.Replace("'", "''")));
                        }
                    }
                }
            }
        }
        public void AddToCollection(int collectionID, string geocacheCode)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (!InCollection(collectionID, geocacheCode))
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into gcincol (col_id, gccode) values ({0}, '{1}')", collectionID, geocacheCode.Replace("'", "''")));
                    }
                }
            }
        }
        public void RemoveFromCollection(string collectionName, string geocacheCode)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    int id = getGCCollectionID(collectionName);
                    if (id >= 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from gcincol where col_id={0} and gccode='{1}'", id, geocacheCode.Replace("'", "''")));
                    }
                }
            }
        }
        public void RemoveFromCollection(int collectionID, string geocacheCode)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery(string.Format("delete from gcincol where col_id={0} and gccode='{1}'", collectionID, geocacheCode.Replace("'", "''")));
                }
            }
        }
        public bool InCollection(string collectionName, string geocacheCode)
        {
            bool result = false;
            lock (this)
            {
                if (_dbcon != null)
                {
                    int id = getGCCollectionID(collectionName);
                    if (id >= 0)
                    {
                        result = (long)_dbcon.ExecuteScalar(string.Format("select count(1) from gcincol where col_id={0} and gccode='{1}'", id, geocacheCode.Replace("'", "''"))) > 0;
                    }
                }
            }
            return result;
        }
        public bool InCollection(int collectionID, string geocacheCode)
        {
            bool result = false;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = (long)_dbcon.ExecuteScalar(string.Format("select count(1) from gcincol where col_id={0} and gccode='{1}'", collectionID, geocacheCode.Replace("'", "''"))) > 0;
                }
            }
            return result;
        }

        public double? GetGeocacheDistance(string gcCode)
        {
            double? result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = (double?)_dbcon.ExecuteScalar(string.Format("select dist from gcdist where gccode='{0}'", gcCode));
                }
            }
            return result;
        }
        public void SetGeocacheDistance(string gcCode, double? dist)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (dist==null)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("delete from gcdist where gccode='{0}'", gcCode));
                    }
                    else if (_dbcon.ExecuteNonQuery(string.Format("update gcdist set dist={1} where gccode='{0}'", gcCode, dist.ToString().Replace(',','.'))) == 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into gcdist (gccode, dist) values ('{0}', {1})", gcCode, dist.ToString().Replace(',', '.')));
                    }
                }
            }
        }


//                    _dbcon.ExecuteNonQuery("create table 'gcvotes' (gccode text, VoteMedian float, VoteAvg float, VoteCnt integer, VoteUser float)");
        public double? GetGCVoteMedian(string gcCode)
        {
            double? result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = (double?)_dbcon.ExecuteScalar(string.Format("select VoteMedian from gcvotes where gccode='{0}'", gcCode));
                }
            }
            return result;
        }
        public double? GetGCVoteAverage(string gcCode)
        {
            double? result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = (double?)_dbcon.ExecuteScalar(string.Format("select VoteAvg from gcvotes where gccode='{0}'", gcCode));
                }
            }
            return result;
        }
        public int? GetGCVoteCount(string gcCode)
        {
            int? result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = (int?)_dbcon.ExecuteScalar(string.Format("select VoteCnt from gcvotes where gccode='{0}'", gcCode));
                }
            }
            return result;
        }
        public double? GetGCVoteUser(string gcCode)
        {
            double? result = null;
            lock (this)
            {
                if (_dbcon != null)
                {
                    result = (double?)_dbcon.ExecuteScalar(string.Format("select VoteUser from gcvotes where gccode='{0}'", gcCode));
                }
            }
            return result;
        }

        public void SetGCVote(string gcCode, double median, double average, int cnt, double? user)
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    if (_dbcon.ExecuteNonQuery(string.Format("update gcvotes set VoteMedian={0}, VoteAvg={1}, VoteCnt={2}, VoteUser={3} where gccode='{4}'", median.ToString(CultureInfo.InvariantCulture), average.ToString(CultureInfo.InvariantCulture), cnt, user == null ? "null" : ((double)user).ToString(CultureInfo.InvariantCulture), gcCode)) == 0)
                    {
                        _dbcon.ExecuteNonQuery(string.Format("insert into gcvotes (VoteMedian, VoteAvg, VoteCnt, VoteUser, gccode) values ({0}, {1}, {2}, {3}, '{4}')", median.ToString(CultureInfo.InvariantCulture), average.ToString(CultureInfo.InvariantCulture), cnt, user == null ? "null" : ((double)user).ToString(CultureInfo.InvariantCulture), gcCode));
                    }
                }
            }
        }

        public void ClearGCVotes()
        {
            lock (this)
            {
                if (_dbcon != null)
                {
                    _dbcon.ExecuteNonQuery("delete from gcvotes");
                }
            }
        }

    }
}
