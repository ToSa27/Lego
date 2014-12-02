using Brickset.ApiV2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Brickset
{
    public class Connection
    {
        private const string _ApiKey = "pkWv-jYp1-hgRB";

        private BricksetAPIv2SoapClient _Client;
        private string _Hash = string.Empty;

        private bool _IsOpen = false;
        public bool IsOpen { get { return _IsOpen; } }

        public Connection(string user, string password)
        {
            Open(user, password);
        }

        public bool Open(string user, string password)
        {
            _Client = new BricksetAPIv2SoapClient();
            _Client.Open();
            String res = _Client.login(_ApiKey, user, password);
            _IsOpen = !(res == "INVALIDKEY" || res.StartsWith("Error"));
            if (_IsOpen)
                _Hash = res;
            return _IsOpen;
        }

        public themes[] GetThemes()
        {
            if (!_IsOpen)
                return null;
            return _Client.getThemes(_ApiKey);
        }

        public subthemes[] GetSubThemes(string theme)
        {
            if (!_IsOpen)
                return null;
            return _Client.getSubthemes(_ApiKey, theme);
        }

        public sets[] GetSets()
        {
            if (!_IsOpen)
                return null;
            return _Client.getSets(_ApiKey, _Hash, "", "", "", "", "", "1", "", "", "1000", "", "");
        }

        public sets[] GetSet(string setid)
        {
            if (!_IsOpen)
                return null;
            return _Client.getSet(_ApiKey, _Hash, setid);
        }
    }
}
