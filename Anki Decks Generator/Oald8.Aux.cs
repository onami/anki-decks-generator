using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace deckgen
{
    public partial class Oald8
    {
        string GetName(HtmlNode node)
        {
            return node.Attributes["class"].Value;
        }

        string PrintList(List<string> list)
        {
            string ret = "";

            for (int i = 0; i < list.Count; i++)
            {
                ret += list[i].Trim();
                if (i + 1 < list.Count) ret += " → ";
            }

            ret = ret.Replace("    ", " ").Replace("  ", " ");

            return ret;
        }
    }
}
