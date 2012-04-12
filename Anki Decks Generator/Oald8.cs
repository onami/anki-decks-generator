using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace deckgen
{
    public partial class Oald8
    {
        //Сделать только геттер
        public int count;
        Hashtable pages;
        string searchPath = "http://oald8.oxfordlearnersdictionaries.com/dictionary/";
        CardsStream reportStream = new CardsStream("./report " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss") + ".txt", 10000);

        public Oald8()
        {
            count = 0;
            pages = new Hashtable();
        }

        string getCleanUrl(HtmlNode link_)
        {
            return (new Regex("(.+?)#.*")).Replace(link_.Attributes["href"].Value, "$1");
        }

        public void ProcessWordlist(ref CardsStream stream, List<string> wordlist, string labels, bool crossreferenceFlag)
        {
            //Разбираем исходные страницы. Возможно, удастся ускорить работу засчёт распараллеливания.
            //Собираем ссылки в теле статьи и добавляем в список.
            //downloadPages(ref stream, GetArticleCrossrefenceLinkList(), labels);

            //Работаем с ссылками, находящиеся в блоке Search Results, в т.ч. и на само word)
            var crossreferenceLinks = new Hashtable();
            string currentPageLink;

            foreach(String word_ in wordlist)
            {
                var word = (new Regex("[^- 0-9a-zA-Z']+")).Replace(word_, "");
                var page = (new HtmlWeb()).Load(searchPath + word);
                
                //Определяем, на какой по какой ссылке находится наше слово
                var currentPageLink_ = page.DocumentNode.SelectSingleNode("//li[@class='currentpage']/a");
                currentPageLink = (currentPageLink_ != null) ? getCleanUrl(currentPageLink_) : "";

                //Берём все слова из блока search results
                var linksNodes = page.DocumentNode.SelectNodes("//div[@id='relatedentries']/ul/li/a");
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
                else
                {
                    reportStream.Write("Failure. Page not found. Link: " + word_ + ".\n");
                }

                //Всё нужное получили, начинаем обрабатывать searchCrossreferenceLinkList
                var updatedWordList = new Hashtable();
                foreach (string link in crossreferenceLinks.Keys)
                {
                    if ((bool)crossreferenceLinks[link] == true)
                    {
                        continue;
                    }

                    var tranformedWord = (new Regex("^(" + word.Replace(' ', '-') + "_\\d+)$", RegexOptions.IgnoreCase));
                    
                    //Первую страницу мы уже скачали.
                    if (currentPageLink == link)
                    {
                        ParsePage(ref stream, page, word, labels);
                    }
                    else if (tranformedWord.Match(link).Success)
                    {
                        ParsePage(ref stream, (new HtmlWeb()).Load(searchPath + link), link, labels);
                    }
                    else if (crossreferenceFlag == true)
                    {
                        ParsePage(ref stream, (new HtmlWeb()).Load(searchPath + link), link, labels);
                    }
                    if (currentPageLink == link || tranformedWord.Match(link).Success || crossreferenceFlag == true)
                    {
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
