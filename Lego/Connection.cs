using Brickset.ApiV2;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Lego
{
    public class Connection
    {
        internal static LegoDS ds;
        internal static Brickset.Connection bs;
        internal static Rebrickable.Connection rb;

        internal static bool Refresh = false;

        private const string dsFile = @"ToSaLego.xml";
        private const string bsUser = "ToSa";
        private const string bsPass = "heiner";
        private const string rbEmail = "tobias@die-hoffs.net";
        private const string rbPass = "rebrickable";

        public static void Connect()
        {
            ds = new LegoDS();
            if (File.Exists(dsFile))
                ds.ReadXml(dsFile);
            bs = new Brickset.Connection(bsUser, bsPass);
            rb = new Rebrickable.Connection(rbEmail, rbPass);
        }

        public static void Disconnect()
        {
            Save();
        }

        public static void Save()
        {
            ds.WriteXml(dsFile);
        }

        private static void LoadColorsRb()
        {
            XmlNodeList rbColors = rb.GetColors();
            foreach (XmlNode rbColor in rbColors)
            {
                string lcid = rbColor.SelectSingleNode("ldraw_color_id").InnerText.Trim();
                var cq = from c in ds.Color.AsEnumerable()
                         where c.LDrawColorId == lcid
                         select c;
                LegoDS.ColorRow cr;
                if (cq.Any())
                {
                    cr = cq.First();
                    if (Refresh)
                    {
                        cr.Name = rbColor.SelectSingleNode("color_name").InnerText.Trim();
                    }
                }
                else
                {
                    cr = ds.Color.NewColorRow();
                    cr.LDrawColorId = lcid;
                    cr.Name = rbColor.SelectSingleNode("color_name").InnerText.Trim();
                    ds.Color.AddColorRow(cr);
                }
            }
            ds.AcceptChanges();
        }

        private static void LoadThemesBs()
        {
            themes[] bsThemes = bs.GetThemes();
            foreach (themes bsTheme in bsThemes)
            {
                var tq = from t in ds.Theme.AsEnumerable()
                         where t.Name == bsTheme.theme
                         select t;
                LegoDS.ThemeRow tr;
                if (tq.Any())
                {
                    tr = tq.First();
                    if (Refresh)
                    {
                    }
                }
                else
                {
                    tr = ds.Theme.NewThemeRow();
                    tr.Name = bsTheme.theme;
                    ds.Theme.AddThemeRow(tr);
                }
                subthemes[] bsSubThemes = bs.GetSubThemes(bsTheme.theme);
                foreach (subthemes bsSubTheme in bsSubThemes)
                {
                    var sq = from s in ds.SubTheme.AsEnumerable()
                             where s.ThemeRow == tr && s.Name == bsSubTheme.theme
                             select s;
                    LegoDS.SubThemeRow sr;
                    if (sq.Any())
                    {
                        sr = sq.First();
                        if (Refresh)
                        {
                        }
                    }
                    else
                    {
                        sr = ds.SubTheme.NewSubThemeRow();
                        sr.ThemeRow = tr;
                        sr.Name = bsSubTheme.theme;
                        ds.SubTheme.AddSubThemeRow(sr);
                    }
                }
            }
            ds.AcceptChanges();
        }

        /*
        private static LegoDS.CategoryRow GetCategory(string category)
        {
            var cq = from c in ds.Category.AsEnumerable()
                     where c.Name == category
                     select c;
            if (cq.Any())
                return cq.First();
            LegoDS.CategoryRow cr = ds.Category.NewCategoryRow();
            cr.Name = category;
            ds.Category.AddCategoryRow(cr);
            ds.AcceptChanges();
            return cr;
        }

        private static LegoDS.ExtTypeRow GetExtType(string type)
        {
            var tq = from t in ds.ExtType.AsEnumerable()
                     where t.Name == type
                     select t;
            if (tq.Any())
                return tq.First();
            LegoDS.ExtTypeRow tr = ds.ExtType.NewExtTypeRow();
            tr.Name = type;
            ds.ExtType.AddExtTypeRow(tr);
            ds.AcceptChanges();
            return tr;
        }
        */

        private static LegoDS.ColorRow GetColor(string colorid)
        {
            var cq = from c in ds.Color.AsEnumerable()
                     where c.LDrawColorId == colorid
                     select c;
            if (cq.Any())
                return cq.First();
            return null;
        }

        /*
        private static void LoadPartsRb()
        {
            List<XmlNode> rbSParts = new List<XmlNode>();
            for (int i = 0; i < 99; i++)
                foreach (XmlNode xn in rb.SearchParts(string.Format("{0:00}", i)))
                    rbSParts.Add(xn);
            foreach (XmlNode rbSPart in rbSParts)
            {
                string pid = rbSPart.SelectSingleNode("part_id").InnerText.Trim();
                XmlNode rbPart = rb.GetPart(pid)[0];
                var mq = from m in ds.Mold.AsEnumerable()
                         where m.Number == pid
                         select m;
                LegoDS.MoldRow mr;
                if (mq.Any())
                {
                    mr = mq.First();
                    mr.Name = rbPart.SelectSingleNode("name").InnerText.Trim();
                    mr.CategoryRow = GetCategory(rbPart.SelectSingleNode("category").InnerText.Trim());
                }
                else
                {
                    mr = ds.Mold.NewMoldRow();
                    mr.Number = pid;
                    mr.Name = rbPart.SelectSingleNode("name").InnerText.Trim();
                    mr.CategoryRow = GetCategory(rbPart.SelectSingleNode("category").InnerText.Trim());
                    ds.Mold.AddMoldRow(mr);
                }
                foreach(XmlNode rbExtId in rbPart.SelectSingleNode("external_part_ids").ChildNodes)
                {
                    if (rbExtId.Name == "lego_element_ids")
                    {
                        foreach (XmlNode rbElement in rbExtId.SelectNodes("element"))
                        {
                            string eid = rbElement.SelectSingleNode("element_id").InnerText.Trim();
                            var pq = from p in ds.Part.AsEnumerable()
                                     where p.Number == eid
                                     select p;
                            LegoDS.PartRow pr;
                            if (pq.Any())
                            {
                                pr = pq.First();
                                pr.ColorRow = GetColor(rbElement.SelectSingleNode("color").InnerText.Trim());
                                pr.MoldRow = mr;
                            }
                            else
                            {
                                pr = ds.Part.NewPartRow();
                                pr.Number = eid;
                                pr.ColorRow = GetColor(rbElement.SelectSingleNode("color").InnerText.Trim());
                                pr.MoldRow = mr;
                                ds.Part.AddPartRow(pr);
                            }
                        }
                    }
                    else
                    {
                        string eid = rbExtId.InnerText.Trim();
                        LegoDS.ExtTypeRow tr = GetExtType(rbExtId.Name.Trim());
                        var eq = from e in ds.ExtId.AsEnumerable()
                                 where e.Number == eid && e.ExtTypeRow == tr
                                 select e;
                        LegoDS.ExtIdRow er;
                        if (eq.Any())
                        {
                        }
                        else
                        {
                            er = ds.ExtId.NewExtIdRow();
                            er.Number = eid;
                            er.ExtTypeRow = tr;
                            er.MoldRow = mr;
                            ds.ExtId.AddExtIdRow(er);
                        }

                    }
                }
            }
            ds.AcceptChanges();
        }

        private static void LoadSetsRb()
        {
            XmlNodeList rbSSets = rb.SearchSets("");
            foreach (XmlNode rbSSet in rbSSets)
            {
                string sid = rbSSet.SelectSingleNode("set_id").InnerText.Trim();
                GetSet(sid);
            }
            ds.AcceptChanges();
        }
         */

        private static LegoDS.SetRow LoadSet(string setid)
        {
            XmlNode rbSet = rb.GetSet(setid)[0];
            var sq = from s in ds.Set.AsEnumerable()
                     where s.Number == setid
                     select s;
            LegoDS.SetRow sr;
            if (sq.Any())
            {
                sr = sq.First();
                if (Refresh)
                {
                    sr.Name = rbSet.SelectSingleNode("descr").InnerText.Trim();
                    sr.ImageUrl = rbSet.SelectSingleNode("set_img_url").InnerText.Trim();
                    LoadSetParts(setid);
                }
            }
            else
            {
                sr = ds.Set.NewSetRow();
                sr.Number = setid;
                sr.Name = rbSet.SelectSingleNode("descr").InnerText.Trim();
                sr.ImageUrl = rbSet.SelectSingleNode("set_img_url").InnerText.Trim();
                ds.Set.AddSetRow(sr);
                LoadSetParts(setid);
            }
            return sr;
        }

        private static LegoDS.SetRow GetSet(string setid)
        {
            var sq = from s in ds.Set.AsEnumerable()
                     where s.Number == setid
                     select s;
            if (sq.Any())
                return sq.First();
            return LoadSet(setid);
        }

        private static void LoadSetParts(string setid)
        {
            XmlNodeList rbSetParts = rb.GetSetParts(setid);
            foreach (XmlNode rbSetPart in rbSetParts)
            {
            }
        }

        private static void LoadMySetsRb()
        {
            XmlNodeList rbMySets = rb.GetUserSets();
            foreach (XmlNode rbMySet in rbMySets)
            {
                LegoDS.SetRow sr = GetSet(rbMySet.SelectSingleNode("set_id").InnerText);
                sr.Count = int.Parse(rbMySet.SelectSingleNode("qty").InnerText);
            }
        }

        public static void LoadReferenceData()
        {
            LoadColorsRb();
            LoadThemesBs();
//            LoadPartsRb();
//            LoadSetsRb();
        }

        public static void LoadMySets()
        {
            LoadMySetsRb();
        }

    }
}
