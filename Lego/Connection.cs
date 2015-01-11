using Brickset.ApiV2;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lego
{
    public class Connection
    {
        public static LegoDS ds;
//        internal static Brickset.Connection bs;
//        internal static Rebrickable.Connection rb;

        internal static bool Refresh = false;

        private const string dsFile = @"ToSaLego.xml";
        private const string bsUser = "ToSa";
        private const string bsPass = "heiner";
        private const string rbEmail = "tobias@die-hoffs.net";
        private const string rbPass = "rebrickable";

        public static void Connect()
        {
            ds = new LegoDS();
//            ds.bs = new Brickset.Connection(bsUser, bsPass);
            ds.rb = new Rebrickable.Connection(rbEmail, rbPass);
        }

        public static void LoadAsync(object state)
        {
            if (File.Exists(dsFile))
                ds.ReadXml(dsFile);
            else
                NewDataSet();
            Update();
            IsReady = true;
        }

        public static void Disconnect()
        {
            Save();
        }

        public static void Save()
        {
            if (IsReady)
            {
                if (File.Exists(dsFile))
                {
                    string bFile = dsFile + ".backup." + DateTime.Now.ToString("yyyyMMddHHmmss");
                    File.Copy(dsFile, bFile);
                }
                ds.WriteXml(dsFile);
            }
        }

        public static bool IsReady = false;

        public static string Status
        {
            get
            {
                string s = "";
                if (ds != null)
                {
                    foreach (DataTable dt in ds.Tables)
                        s += string.Format("{0} : {1}\n", dt.TableName, dt.Rows.Count);
                }
                return s;
            }
        }

        public static void NewDataSet()
        {
            if (ds.Color.Rows.Count == 0)
                ds.Color.FetchAll();
        }

        public static void Update()
        {
            ds.Set.FetchMySets();
            ds.SetContent.FetchMySoloParts();
            ds.SetContent.FetchMyLostParts();
        }
    }
}
