using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace deckgen
{
    public partial class Macmillan
    {
        //Сделать только геттер
        public int count;
        Hashtable pages;
        string searchPath = "http://www.macmillandictionary.com/dictionary/british/";
        CardsStream reportStream = new CardsStream("./report " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss") + ".txt", 10000);

        public Macmillan()
        {
            count = 0;
            pages = new Hashtable();
        }

        string getCleanUrl(HtmlNode link_)
        {
            return (new Regex("(.*/)?(.+?)(#.*)?")).Replace(link_.Attributes["href"].Value, "$2");
        }

        public void ProcessWordlist(ref CardsStream stream, List<string> wordlist, string labels, bool crossreferenceFlag)
        {
            //Разбираем исходные страницы. Возможно, удастся ускорить работу засчёт распараллеливания.
            //Собираем ссылки в теле статьи и добавляем в список.
            //downloadPages(ref stream, GetArticleCrossrefenceLinkList(), labels);

            //Работаем с ссылками, находящиеся в блоке Search Results, в т.ч. и на само word)
            var crossreferenceLinks = new Hashtable();

            foreach (String word_ in wordlist)
            {
                var word = (new Regex("[^- 0-9a-zA-Z']+")).Replace(word_, "").Replace(" ", "-");
                var page = (new HtmlWeb()).Load(searchPath + word);

                ParsePage(ref stream, page, word, labels);
                Console.WriteLine("{0}", word);
                reportStream.Write("Success. Page was parsed. Link: " + word + ".\n");

                //Берём все слова из блока search results
                var linksNodes = page.DocumentNode.SelectNodes("//div[@class='entrylist']/ul/li/a");
                if (linksNodes != null)
                {
                    foreach (HtmlNode link in linksNodes)
                    {
                        var url = getCleanUrl(link);
                        if (crossreferenceLinks.ContainsKey(url) == false)
                        {
                            crossreferenceLinks.Add(url, false);
                        }
                    }
                }


                var updatedWordList = new Hashtable();

                foreach (string link in crossreferenceLinks.Keys)
                {
                    if ((bool)crossreferenceLinks[link] == true)
                    {
                        continue;
                    }

                    var tranformedWordRegExp = new Regex("^(" + word.Replace(' ', '-') + "_\\d+)$", RegexOptions.IgnoreCase);

                    if (crossreferenceFlag == true || tranformedWordRegExp.Match(link).Success)
                    {
                        ParsePage(ref stream, (new HtmlWeb()).Load(searchPath + link), link, labels);

                        Console.WriteLine("{0}", link);
                        updatedWordList.Add(link, true);
                        reportStream.Write("Success. Page was parsed. Link: " + link + ".\n");
                    }
                }

                foreach (DictionaryEntry update in updatedWordList)
                {
                    crossreferenceLinks[update.Key] = update.Value;
                }
            }
            reportStream.Write("Total: " + count + "\n");
            reportStream.Save();
        }
    }
}
