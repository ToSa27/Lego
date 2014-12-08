using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Rebrickable
{
    public class Connection
    {
        private const string _ApiKey = "or4qbDFWMy";
        private const string _ApiUrl = "http://rebrickable.com/api/";
        private const string _ApiFormat = "xml";

        private string _Hash = string.Empty;

        private bool _IsOpen = false;
        public bool IsOpen { get { return _IsOpen; } }

        public Connection(string email, string password)
        {
            Open(email, password);
        }

        public bool Open(string email, string password)
        {
            String res = Call("get_user_hash", false, new Dictionary<string, string>() { { "email", email }, { "pass", password } });
            _IsOpen = !(res == "INVALIDKEY" || res == "INVALIDUSERPASS");
            if (_IsOpen)
                _Hash = res;
            return _IsOpen;
        }

        private String Call(string function, bool hash, Dictionary<String,String> parameters)
        {
            UriBuilder ub = new UriBuilder(_ApiUrl);
            ub.Path += function;
            String query = string.Format("key={0}&format={1}", _ApiKey, _ApiFormat);
            if (hash)
                query += string.Format("&hash={0}", _Hash);
            if (parameters != null)
                foreach (KeyValuePair<String, String> parameter in parameters)
                    query += string.Format("&{0}={1}", parameter.Key, parameter.Value);
            ub.Query = query;
            WebRequest req = WebRequest.Create(ub.Uri);
            WebResponse res = req.GetResponse();
            StreamReader sr = new StreamReader(res.GetResponseStream());
            return sr.ReadToEnd();
        }

        private XmlNodeList Call(string function, bool hash, Dictionary<String, String> parameters, String xpath)
        {
            try
            {
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(Call(function, hash, parameters));
                return xd.SelectNodes(xpath);
            }
            catch
            {
                return null;
            }
        }

        public XmlNodeList GetColors()
        {
            if (!_IsOpen)
                return null;
            return Call("get_colors", false, null, "root/color");
        }

        public XmlNodeList SearchParts(string pattern)
        {
            if (!_IsOpen)
                return null;
            return Call("search", false, new Dictionary<String,String>() { { "type", "P" }, { "query", pattern } }, "root/results/part");
        }

        public XmlNodeList GetPart(string partid)
        {
            if (!_IsOpen)
                return null;
            return Call("get_part", false, new Dictionary<String, String>() { { "part_id", partid }, { "inc_rels", "1" }, { "inc_ext", "1" } }, "root");
        }

        public XmlNodeList GetElement(string elementid)
        {
            if (!_IsOpen)
                return null;
            return Call("get_element", false, new Dictionary<String, String>() { { "element_id", elementid } }, "root");
        }

        public XmlNodeList SearchSets(string pattern)
        {
            if (!_IsOpen)
                return null;
            return Call("search", false, new Dictionary<String, String>() { { "type", "S" }, { "query", pattern } }, "root/results/set");
        }

        public XmlNodeList GetSet(string setid)
        {
            if (!_IsOpen)
                return null;
            return Call("get_set", false, new Dictionary<String, String>() { { "set_id", setid } }, "root/set");
        }

        public XmlNodeList GetSetParts(String setid)
        {
            if (!_IsOpen)
                return null;
            return Call("get_set_parts", false, new Dictionary<String, String>() { { "set", setid } }, "set/parts/part");
        }

        public XmlNodeList GetUserSets()
        {
            if (!_IsOpen)
                return null;
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(Call("get_user_sets", true, null));
            return xd.SelectNodes("root/sets/set");
        }

        public XmlNodeList GetUserParts()
        {
            if (!_IsOpen)
                return null;
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(Call("get_user_parts", true, null));
            return xd.SelectNodes("root/parts/part");
        }

        public XmlNodeList GetUserLostParts()
        {
            if (!_IsOpen)
                return null;
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(Call("get_user_lost_parts", true, null));
            return xd.SelectNodes("root/parts/part");
        }

        public Bitmap GetColorImage(string LDrawColorId)
        {
            WebRequest req = WebRequest.Create(string.Format("http://rebrickable.com/img/pieces/{0}/3003.png", LDrawColorId));
            return new Bitmap(req.GetResponse().GetResponseStream());
        }
    }
}
