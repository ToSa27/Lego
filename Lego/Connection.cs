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
//            bs = new Brickset.Connection(bsUser, bsPass);
            rb = new Rebrickable.Connection(rbEmail, rbPass);
            ds = new LegoDS();
//            ds.bs = bs;
            ds.rb = rb;
            if (File.Exists(dsFile))
                ds.ReadXml(dsFile);
            else
                NewDataSet();
        }

        public static void Disconnect()
        {
            Save();
        }

        public static void Save()
        {
            ds.WriteXml(dsFile);
        }

        public static void NewDataSet()
        {
            if (ds.Color.Rows.Count == 0)
                ds.Color.FetchAll();
            ds.Set.FetchMySets();
            ds.SetContent.FetchMySoloParts();
            ds.SetContent.FetchMyLostParts();
        }
    }
}
