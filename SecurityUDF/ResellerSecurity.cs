using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;
using Microsoft.AnalysisServices.AdomdServer;

namespace SecurityUDF {
    public class ResellerSecurity {

        private class RegionSecurity : Dictionary<String, List<String>> {
            public void AddSecurity (string regionName, string userName) {
                regionName = regionName.ToUpper ();
                userName = userName.ToUpper ();

                if (!this.ContainsKey (userName)) {
                    this.Add (userName, new List<string> ());
                }
                this[userName].Add (regionName);
            }

            public bool isRegionEnabled (string regionName, string userName) {
                regionName = regionName.ToUpper ();
                userName = userName.ToUpper ();
                return this[userName].Contains (regionName);
            }
        }

        private static RegionSecurity RegionsForUsers = new RegionSecurity ();
        static ResellerSecurity () {
            RegionsForUsers.AddSecurity ("Canada", "NOEMANET\\ChrisWebb");
            RegionsForUsers.AddSecurity ("Australia", "NOEMANET\\ChrisWebb");
            RegionsForUsers.AddSecurity ("Canada", "NOEMANET\\Alberto");
            RegionsForUsers.AddSecurity ("Australia", "NOEMANET\\Marco");
            RegionsForUsers.AddSecurity ("Australia", "NOEMANET\\Caterina");
        }

        public static Set SecuritySet () {
            try {
                string userName = (new Expression ("UserName ()")
                    .Calculate (null))
                    .ToString ();
                MemberCollection members = Context.CurrentCube
                    .Dimensions["Sales Territory"]
                    .AttributeHierarchies["Sales Territory Country"]
                    .Levels[1]
                    .GetMembers ();

                SetBuilder sb = new SetBuilder ();
                foreach (Member m in members) {
                    if (RegionsForUsers.isRegionEnabled (m.Caption, userName)) {
                        TupleBuilder tb = new TupleBuilder ();
                        tb.Add (m);
                        sb.Add (tb.ToTuple ());
                    }
                }
                return sb.ToSet ();
            } catch (Exception) {
                return null;
            }
        }
    }
}
