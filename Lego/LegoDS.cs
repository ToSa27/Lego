using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Xml;

namespace Lego
{
    public partial class LegoDS {

        partial class ExtIdTypeDataTable
        {
            public ExtIdTypeRow GetByName(string Name)
            {
                var q = from r in this.AsEnumerable()
                        where r.Name == Name
                        select r;
                if (q.Any())
                    return q.First();
                ExtIdTypeRow er = NewExtIdTypeRow();
                er.Name = Name;
                AddExtIdTypeRow(er);
                return er;
            }
        }
    
        partial class CategoryDataTable
        {
            public CategoryRow GetByName(string Name)
            {
                var q = from r in this.AsEnumerable()
                        where r.Name == Name
                        select r;
                if (q.Any())
                    return q.First();
                CategoryRow cr = NewCategoryRow();
                cr.Name = Name;
                AddCategoryRow(cr);
                return cr;
            }
        }
    
        partial class ElementDataTable
        {
            private ElementRow Fetch(string ElementId)
            {
                LegoDS ds = (LegoDS)this.DataSet;
                var pq = from p in ds.Element.AsEnumerable()
                         where p.Number == ElementId
                         select p;
                LegoDS.ElementRow er;
                if (pq.Any())
                    er = pq.First();
                else
                {
                    try
                    {
                        XmlNode rbPart = ds.rb.GetElement(ElementId)[0];
                        er = ds.Element.NewElementRow();
                        er.Number = ElementId;
                        er.PartRow = ds.Part.GetById(rbPart.SelectSingleNode("part_id").InnerText);
                        er.ColorRow = ds.Color.GetByLDrawColorId(rbPart.SelectSingleNode("color").InnerText);
                        ds.Element.AddElementRow(er);
                    }
                    catch
                    {
                        er = null;
                    }
                }
                return er;
            }

            public ElementRow GetById(string ElementId)
            {
                if (ElementId == "-1")
                    return null;
                var q = from r in this.AsEnumerable()
                        where r.Number == ElementId
                        select r;
                if (q.Any())
                    return q.First();
                return Fetch(ElementId);
            }

            public ElementRow GetByPartAndColor(string PartId, string ColorId)
            {
                LegoDS ds = (LegoDS)this.DataSet;
                PartRow pr = ds.Part.GetById(PartId);
                ColorRow cr = ds.Color.GetByLDrawColorId(ColorId);
                return GetByPartAndColor(pr, cr);
            }

            public ElementRow GetByPartAndColor(PartRow pr, ColorRow cr)
            {
                var q = from r in this.AsEnumerable()
                        where r.PartRow == pr && r.ColorRow == cr
                        select r;
                if (q.Any())
                    return q.First();
                return null;
            }

            public ElementRow Create(string ElementId, string PartId, string ColorId)
            {
                LegoDS ds = (LegoDS)this.DataSet;
                PartRow pr = ds.Part.GetById(PartId);
                ColorRow cr = ds.Color.GetByLDrawColorId(ColorId);
                return Create(ElementId, pr, cr);
            }

            public ElementRow Create(string ElementId, PartRow pr, ColorRow cr)
            {
                ElementRow er = NewElementRow();
                er.Number = ElementId;
                er.PartRow = pr;
                er.ColorRow = cr;
                AddElementRow(er);
                return er;
            }

            public ElementRow GetOrCreate(string ElementId, string PartId, string ColorId)
            {
                LegoDS ds = (LegoDS)this.DataSet;
                PartRow pr = ds.Part.GetById(PartId);
                ColorRow cr = ds.Color.GetByLDrawColorId(ColorId);
                ElementRow er = GetById(ElementId);
                if (er == null)
                    er = Create(ElementId, PartId, ColorId);
                return er;
            }
        }

        partial class SetContentRow
        {
            public string ImageUrl
            {
                get
                {
                    if (ElementRow != null)
                        return string.Format("http://img.rebrickable.com/img/pieces/elements/{0}.jpg", ElementRow.Number);
                    if (PartRow != null && ColorRow != null)
                        return string.Format("http://img.rebrickable.com/img/pieces/{0}/{1}.png", ColorRow.LDrawColorId, PartRow.Number);
                    return string.Empty;
                }
            }
        }

        partial class BuildDiffRow
        {
            public string ImageUrl
            {
                get
                {
                    if (ElementRow != null)
                        return string.Format("http://img.rebrickable.com/img/pieces/elements/{0}.jpg", ElementRow.Number);
                    if (PartRow != null && ColorRow != null)
                        return string.Format("http://img.rebrickable.com/img/pieces/{0}/{1}.png", ColorRow.LDrawColorId, PartRow.Number);
                    return string.Empty;
                }
            }
        }

        partial class ElementRow
        {
            public string ImageUrl
            {
                get
                {
                    return string.Format("http://img.rebrickable.com/img/pieces/elements/{0}.jpg", Number);
                }
            }

            public int CountInSets
            {
                get
                {
                    LegoDS ds = this.Table.DataSet as LegoDS;
                    var q = from r in this.GetSetContentRows().AsEnumerable()
                            where r.SetRow != ds.Set.GetSoloSet() && r.SetRow != ds.Set.GetWishlistSet()
                            select r;
                    int res = 0;
                    foreach (SetContentRow scr in q)
                        res += (scr.SetRow.Count * (scr.Count + scr.CountSpare));
                    // ToDo : count missing
                    return res;
                }
            }

            public int CountBuilt
            {
                get
                {
                    LegoDS ds = this.Table.DataSet as LegoDS;
                    var q = from r in this.GetSetContentRows().AsEnumerable()
                            where r.SetRow != ds.Set.GetSoloSet() && r.SetRow != ds.Set.GetWishlistSet()
                            select r;
                    int res = 0;
                    foreach (SetContentRow scr in q)
                        res += (scr.SetRow.CountBuilt * scr.Count);
                    // ToDo : count missing
                    return res;
                }
            }

            public int CountAvailable
            {
                get
                {
                    return CountTotal - CountBuilt;
                }
            }

            public int CountSolo
            {
                get
                {
                    LegoDS ds = this.Table.DataSet as LegoDS;
                    var q = from r in this.GetSetContentRows().AsEnumerable()
                            where r.SetRow == ds.Set.GetSoloSet()
                            select r;
                    if (q.Any())
                        return q.First().Count;
                    else
                        return 0;
                }
            }

            public int CountTotal
            {
                get
                {
                    LegoDS ds = this.Table.DataSet as LegoDS;
                    var q = from r in this.GetSetContentRows().AsEnumerable()
                            where r.SetRow != ds.Set.GetWishlistSet()
                            select r;
                    int res = 0;
                    foreach (SetContentRow scr in q)
                        res += (scr.SetRow.Count * (scr.Count + scr.CountSpare));
                    // ToDo : count missing
                    return res;
                }
            }
        }

        partial class BuildRow
        {
            public void Unbuild()
            {
                LegoDS ds = this.Table.DataSet as LegoDS;
                List<BuildDiffRow> drs = new List<BuildDiffRow>(this.GetBuildDiffRows());
                while (drs.Count > 0)
                {
                    BuildDiffRow dr = drs[0];
                    drs.Remove(dr);
                    ds.BuildDiff.RemoveBuildDiffRow(dr);
                }
                ds.Build.RemoveBuildRow(this);
            }
        }

        partial class SetRow : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public void OnPropertyChanged(string Property)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(Property));
            }

            public int CountBuilt
            {
                get
                {
                    return GetBuildRows().Count();
                }
            }

            public BuildRow AddBuild()
            {
                LegoDS ds = this.Table.DataSet as LegoDS;
                LegoDS.BuildRow br = ds.Build.NewBuildRow();
                br.SetRow = this;
                ds.Build.AddBuildRow(br);
                OnPropertyChanged("CountBuilt");
                return br;
            }
        }

        partial class PartRow
        {
            public string ImageUrl
            {
                get
                {
                    return string.Format("http://img.rebrickable.com/img/pieces/-1/{0}.png", Number);
                }
            }

            public int CountInSets
            {
                get
                {
                    LegoDS ds = this.Table.DataSet as LegoDS;
                    var q = from r in this.GetSetContentRows().AsEnumerable()
                            where r.SetRow != ds.Set.GetSoloSet() && r.SetRow != ds.Set.GetWishlistSet()
                            select r;
                    int res = 0;
                    foreach (SetContentRow scr in q)
                        res += (scr.SetRow.Count * (scr.Count + scr.CountSpare));
                    // ToDo : count missing
                    return res;
                }
            }

            public int CountBuilt
            {
                get
                {
                    LegoDS ds = this.Table.DataSet as LegoDS;
                    var q = from r in this.GetSetContentRows().AsEnumerable()
                            where r.SetRow != ds.Set.GetSoloSet() && r.SetRow != ds.Set.GetWishlistSet()
                            select r;
                    int res = 0;
                    foreach (SetContentRow scr in q)
                        res += (scr.SetRow.CountBuilt * scr.Count);
                    // ToDo : count missing
                    return res;
                }
            }

            public int CountAvailable
            {
                get
                {
                    return CountTotal - CountBuilt;
                }
            }

            public int CountSolo
            {
                get
                {
                    LegoDS ds = this.Table.DataSet as LegoDS;
                    var q = from r in this.GetSetContentRows().AsEnumerable()
                            where r.SetRow == ds.Set.GetSoloSet()
                            select r;
                    if (q.Any())
                        return q.First().Count;
                    else
                        return 0;
                }
            }

            public int CountTotal
            {
                get
                {
                    LegoDS ds = this.Table.DataSet as LegoDS;
                    var q = from r in this.GetSetContentRows().AsEnumerable()
                            where r.SetRow != ds.Set.GetWishlistSet()
                            select r;
                    int res = 0;
                    foreach (SetContentRow scr in q)
                        res += (scr.SetRow.Count * (scr.Count + scr.CountSpare));
                    // ToDo : count missing
                    return res;
                }
            }
        }

        partial class PartDataTable
        {
            private PartRow Fetch(string PartId)
            {
                LegoDS ds = (LegoDS)this.DataSet;
                var pq = from m in ds.Part.AsEnumerable()
                         where m.Number == PartId
                         select m;
                LegoDS.PartRow pr;
                if (pq.Any())
                    pr = pq.First();
                else
                {
                    try
                    {
                        XmlNode rbPart = null;
                        rbPart = ds.rb.GetPart(PartId)[0];
                        pr = ds.Part.NewPartRow();
                        pr.Number = PartId;
                        XmlNode xnName = rbPart.SelectSingleNode("name");
                        if (xnName != null)
                            pr.Name = xnName.InnerText;
                        XmlNode xnCategory = rbPart.SelectSingleNode("category");
                        if (xnCategory != null)
                            pr.CategoryRow = ds.Category.GetByName(xnCategory.InnerText);
                        ds.Part.AddPartRow(pr);
                        ExtIdTypeRow rtr = ds.ExtIdType.GetByName("rebrickable_part_id");
                        foreach (XmlNode rbExtId in rbPart.SelectNodes("rebrickable_part_ids/part_id"))
                        {
                            string eid = rbExtId.InnerText.Trim();
                            var eq = from e in ds.ExtId.AsEnumerable()
                                     where e.Name == eid && e.ExtIdTypeRow == rtr
                                     select e;
                            if (!eq.Any())
                            {
                                ExtIdRow er = ds.ExtId.NewExtIdRow();
                                er.Name = eid;
                                er.ExtIdTypeRow = rtr;
                                er.PartRow = pr;
                                ds.ExtId.AddExtIdRow(er);
                            }
                        }
                        foreach (XmlNode rbExtId in rbPart.SelectSingleNode("external_part_ids").ChildNodes)
                        {
                            if (rbExtId.Name == "lego_element_ids")
                            {
                                /*
                                foreach (XmlNode LEId in rbExtId.SelectNodes("element"))
                                {
                                    string ElementId = LEId.SelectSingleNode("element_id").InnerText;
                                    string ColorId = LEId.SelectSingleNode("color").InnerText;
                                    ElementRow er = ds.Element.GetOrCreate(ElementId, PartId, ColorId);
                                }
                                */
                            }
                            else
                            {
                                string eid = rbExtId.InnerText.Trim();
                                ExtIdTypeRow tr = ds.ExtIdType.GetByName(rbExtId.Name.Trim());
                                var eq = from e in ds.ExtId.AsEnumerable()
                                         where e.Name == eid && e.ExtIdTypeRow == tr
                                         select e;
                                if (!eq.Any())
                                {
                                    ExtIdRow er = ds.ExtId.NewExtIdRow();
                                    er.Name = eid;
                                    er.ExtIdTypeRow = tr;
                                    er.PartRow = pr;
                                    ds.ExtId.AddExtIdRow(er);
                                }
                            }
                        }
                    }
                    catch
                    {
                        pr = null;
                    }
                }
                return pr;
            }

            public PartRow GetById(string PartId)
            {
                var q = from r in this.AsEnumerable()
                        where r.Number == PartId
                        select r;
                if (q.Any())
                    return q.First();
                return Fetch(PartId);
            }
        }
    
        public Rebrickable.Connection rb;

        partial class SetContentDataTable
        {
            private enum PartCountType { normal, spare, missing }

            public void Fetch(SetRow sr)
            {
                LegoDS ds = (LegoDS)this.DataSet;
                XmlNodeList rbSetParts = ds.rb.GetSetParts(sr.Number);
                foreach (XmlNode rbSetPart in rbSetParts)
                {
                    string ElementId = rbSetPart.SelectSingleNode("element_id").InnerText;
                    string PartId = rbSetPart.SelectSingleNode("part_id").InnerText;
                    int Count = int.Parse(rbSetPart.SelectSingleNode("qty").InnerText);
                    string sType = rbSetPart.SelectSingleNode("type").InnerText;
                    PartCountType Type;
                    if (sType == "1")
                        Type = PartCountType.normal;
                    else if (sType == "2")
                        Type = PartCountType.spare;
                    else
                        throw new Exception("Unknown Part Count Type");
                    AddToSet(sr, ElementId, PartId, null, Count, Type);
                }
            }

            public void FetchMySoloParts()
            {
                LegoDS ds = (LegoDS)this.DataSet;
                SetRow sr = ds.Set.GetSoloSet();
                XmlNodeList rbMySoloParts = ds.rb.GetUserParts();
                foreach (XmlNode rbMySoloPart in rbMySoloParts)
                {
                    string PartId = rbMySoloPart.SelectSingleNode("part_id").InnerText;
                    string ColorId = rbMySoloPart.SelectSingleNode("ldraw_color_id").InnerText;
                    int Count = int.Parse(rbMySoloPart.SelectSingleNode("qty").InnerText);
                    AddToSet(sr, null, PartId, ColorId, Count, PartCountType.normal);
                }
                AcceptChanges();
            }

            public void FetchMyLostParts()
            {
                LegoDS ds = (LegoDS)this.DataSet;
                XmlNodeList rbMyLostParts = ds.rb.GetUserLostParts();
                foreach (XmlNode rbMyLostPart in rbMyLostParts)
                {
                    string PartId = rbMyLostPart.SelectSingleNode("part_id").InnerText;
                    string SetId = rbMyLostPart.SelectSingleNode("set_id").InnerText;
                    SetRow sr = ds.Set.GetById(SetId);
                    string ColorId = rbMyLostPart.SelectSingleNode("ldraw_color_id").InnerText;
                    int Count = int.Parse(rbMyLostPart.SelectSingleNode("qty").InnerText);
                    AddToSet(sr, null, PartId, ColorId, Count, PartCountType.missing);
                }
                AcceptChanges();
            }

            private void AddToSet(SetRow sr, string ElementId, string PartId, string ColorId, int Count, PartCountType Type)
            {
                LegoDS ds = (LegoDS)this.DataSet;
                PartRow pr = ds.Part.GetById(PartId);
                ColorRow cr = null;
                if (!string.IsNullOrEmpty(ColorId))
                    cr = ds.Color.GetByLDrawColorId(ColorId);
                ElementRow er = null;
                if (!string.IsNullOrEmpty(ElementId))
                    er = ds.Element.GetById(ElementId);
                else
                    er = ds.Element.GetByPartAndColor(pr, cr);
                var q = from r in ds.SetContent.AsEnumerable()
                        where r.SetRow == sr && r.ElementRow == er && r.PartRow == pr
                        select r;
                bool newrow = true;
                LegoDS.SetContentRow scr;
                if (q.Any())
                {
                    scr = q.First();
                    newrow = false;
                }
                else
                {
                    scr = NewSetContentRow();
                    scr.SetRow = sr;
                    scr.ElementRow = er;
                    scr.PartRow = pr;
                    scr.ColorRow = cr;
                }
                switch (Type)
                {
                    case PartCountType.normal:
                        scr.Count = Count;
                        break;
                    case PartCountType.spare:
                        scr.CountSpare = Count;
                        break;
                    case PartCountType.missing:
                        scr.CountBuiltMissing = Count;
                        break;
                }
                if (newrow)
                    ds.SetContent.AddSetContentRow(scr);
            }
        }

        partial class SetDataTable
        {
            public SetRow GetWishlistSet()
            {
                var q = from r in this.AsEnumerable()
                        where r.Number == "wishlist"
                        select r;
                if (q.Any())
                    return q.First();
                SetRow sr = NewSetRow();
                sr.Number = "wishlist";
                sr.Name = "Wunschliste";
                sr.Count = 0;
                sr.ImageUrl = "";
                sr.IsCustom = true;
                AddSetRow(sr);
                return sr;
            }

            public SetRow GetSoloSet()
            {
                var q = from r in this.AsEnumerable()
                        where r.Number == "solo"
                        select r;
                if (q.Any())
                    return q.First();
                SetRow sr = NewSetRow();
                sr.Number = "solo";
                sr.Name = "Einzelteile";
                sr.Count = 1;
                sr.ImageUrl = "";
                sr.IsCustom = true;
                AddSetRow(sr);
                return sr;
            }

            private SetRow Fetch(string SetId)
            {
                LegoDS ds = (LegoDS)this.DataSet;
                XmlNode rbSet = ds.rb.GetSet(SetId)[0];
                SetRow sr = NewSetRow();
                sr.Number = SetId;
                sr.Name = rbSet.SelectSingleNode("descr").InnerText.Trim();
                sr.ImageUrl = rbSet.SelectSingleNode("img_big").InnerText.Trim();
                AddSetRow(sr);
                AcceptChanges();
                ds.SetContent.Fetch(sr);
                return sr;
            }

            public SetRow GetById(string SetId)
            {
                var q = from r in this.AsEnumerable()
                        where r.Number == SetId
                        select r;
                if (q.Any())
                    return q.First();
                return Fetch(SetId);
            }

            public void FetchMySets()
            {
                LegoDS ds = (LegoDS)this.DataSet;
                XmlNodeList rbMySets = ds.rb.GetUserSets();
                foreach (XmlNode rbMySet in rbMySets)
                {
                    SetRow sr = GetById(rbMySet.SelectSingleNode("set_id").InnerText);
                    sr.Count = int.Parse(rbMySet.SelectSingleNode("qty").InnerText);
                }
                AcceptChanges();
            }
        }
    
        partial class ColorRow
        {
            private Bitmap _Image = null;
            public Bitmap Image
            {
                get
                {
                    if (_Image == null)
                    {
                        LegoDS ds = (LegoDS)this.Table.DataSet;
                        _Image = ds.rb.GetColorImage(this.LDrawColorId);
                    }
                    return _Image;
                }
            }
        }

        partial class ColorDataTable
        {
            public void FetchAll()
            {
                LegoDS ds = (LegoDS)this.DataSet;
                XmlNodeList rbColors = ds.rb.GetColors();
                foreach (XmlNode rbColor in rbColors)
                {
                    string lcid = rbColor.SelectSingleNode("ldraw_color_id").InnerText.Trim();
                    var q = from r in this.AsEnumerable()
                            where r.LDrawColorId == lcid
                            select r;
                    LegoDS.ColorRow cr;
                    if (q.Any())
                    {
                        cr = q.First();
                        if (Lego.Connection.Refresh)
                        {
                            cr.Name = rbColor.SelectSingleNode("color_name").InnerText.Trim();
                        }
                    }
                    else
                    {
                        cr = NewColorRow();
                        cr.LDrawColorId = lcid;
                        cr.Name = rbColor.SelectSingleNode("color_name").InnerText.Trim();
                        AddColorRow(cr);
                    }
                }
                AcceptChanges();
            }

            public ColorRow GetByLDrawColorId(string LDrawColorId)
            {
                var q = from r in this.AsEnumerable()
                        where r.LDrawColorId == LDrawColorId
                        select r;
                if (q.Any())
                    return q.First();
                return null;
            }
        }

        partial class InventoryRow : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public void OnPropertyChanged(string Property)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(Property));
            }

            public void SetCount(int NewCount)
            {
                this.Count = NewCount;
                OnPropertyChanged("Count");
            }

            private int _CountBin = 0;
            public int CountBin
            {
                get
                { 
                    return _CountBin; 
                }
                set 
                { 
                    _CountBin = value;
                    OnPropertyChanged("CountBin");
                }
            }
        }
    }
}
