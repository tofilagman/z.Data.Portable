using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace z.Data.Converters
{
    /// <summary>
    /// @LJ20170915
    /// Intended for InSys Base Paradigm
    /// </summary>
    public class SqlToJs
    {
        private tmps nTmp;

        public string Translate(string SQL)
        {
            if (SQL == null) return "NULL";

            nTmp = new tmps();

            var g = Match(SQL, @"(\$)?\b(?<!\$\.|@|\:|''|\~)([^\d|,]\w+)\b").Where(x =>
            {
                var gg = true;
                foreach (var sd in new string[] { "OR", "AND", "NOT", "IN", "IS", "NULL" })
                    if (x.ToUpper().Trim() == sd) gg = false;
                return gg;
            }).OrderByDescending(x => x);


            foreach (string gg in g)
                nTmp.Add(gg.Trim(), gg.Replace("$", "").Replace(" ", ""));


            foreach (var h in nTmp)
            {
                h.matchtext = "$." + h.matchtext.Replace("$", "");
            }


            // include OR and AND
            nTmp.Add("OR", "||");
            nTmp.Add("AND", "&&");

            //replace
            foreach (var t in nTmp)
            {
                if (t.NParam.Substring(0, 1) != "$")
                    SQL = Replace(SQL, @"\b(?<!\$|\$\.|@|\:|'')" + t.NParam + @"\b", t.matchtext);
                else
                    SQL = Replace(SQL, @"\" + t.NParam, t.matchtext);
            }

            SQL = SQL.Replace("=", "==").Replace("<>", "!==");
            SQL = Replace(SQL, "Is( )+NULL", "== null");
            SQL = Replace(SQL, "Is( )+not( )+NULL", "!= null");

            //get num collection
            nTmp.Clear();

            var g2 = Match(SQL, @"([^\&\| ]\w*( )+)+([NOT( )+]?)(IN( )+\(([^()]+)\))");
            foreach (string g22 in g2)
            {
                var hh = g22;
                hh = hh.Substring(0, 1).TrimStart() == "." ? "$" + g22.TrimStart() : g22.TrimStart();
                nTmp.Add(g22, hh);
            }

            foreach (var t in nTmp)
            {
                var a = Match(t.matchtext, @"\([^(]+\)");
                var n = Match(t.matchtext, @"(\S\.|\@|\:|\$)\b\w+\b");

                foreach (var aa in a)
                {
                    var fr = Replace("[@List].indexOf(@Param) @Compare -1", "@List", aa.Replace("(", "").Replace(")", ""));
                    foreach (var nn in n)
                    {
                        fr = Replace(fr, "@Param", nn);
                        t.matchtext = Replace(fr, "@Compare", t.NParam.Contains("not") ? "==" : "!=");
                    }
                }
            }

            // group by 1 or 0

            var j = Match(SQL, @"(\$.|:|@|\~)\b\w+\b( )+(==|\!=|\<|\>)( )+\b(0|1)\b");
            foreach (var jj in j)
            {
                var n = Match(jj, @"(\S\.|\@|\:|\$|\~)\b\w+\b");
                foreach (var nn in n)
                {
                    var BText = "(" + nn + " @Compare " + (jj.Contains("1") ? "1" : "0") + " || " + nn + " @Compare " + (jj.Contains("1") ? "true" : "false") + ")";

                    string BComp = "";
                    foreach (var jjj in new string[] { "==", "!=", "<", ">" })
                    {
                        if (jj.Contains(jjj))
                            BComp = jjj;
                    }

                    nTmp.Add(jj, BText.Replace("@Compare", BComp));
                }
            }

            // replace

            foreach (var h in nTmp)
            {
                SQL = SQL.Replace(h.NParam, h.matchtext);
            }

            SQL = SQL.Replace(@"$[", "[");

            return SQL;
        }

        public static string Convert(string SQL) => new SqlToJs().Translate(SQL);

        protected IEnumerable<string> Match(string Command, string Pattern)
        {
            return Regex.Matches(Command.ToString(), Pattern.ToString()).Cast<Match>().Select(x => x.Value);
        }

        protected string Replace(string Command, string Pattern, string Value)
        {
            return Regex.Replace(Command, Pattern, Value, RegexOptions.IgnoreCase);
        }

        private class tmp
        {
            public int ID;
            public string NParam;
            public string matchtext;
        }

        private class tmps : List<tmp>
        {
            public void Add(string nParam, string matchText)
            {
                base.Add(new tmp
                {
                    ID = base.Count + 1,
                    NParam = nParam,
                    matchtext = matchText
                });
            }

        }
    }
}
