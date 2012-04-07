using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using HtmlAgilityPack;
using System.IO;

namespace Anki_Decks_Generator
{
    class Card
    {
        public string sentence;
        public string full;
        public string definition;
        public List<string> structure;
        public List<string> register;
        public string usaTranscription;
        public string gbrTranscription;

        public Card()
        {
            register = new List<string>();
            structure = new List<string>();
        }
    }

    class Oald8Parser
    {
        //Недоделано::Берет общий для всей статьи регистр, который находится сразу после div#h-g и до любого определения 
        List<string> getGeneralRegister(HtmlDocument document)
        {
            var ret = new List<string>();

            var register = document.DocumentNode.SelectSingleNode("//div[@class='top-container']");
            register = register.NextSibling;
            Console.WriteLine("{0}", register.InnerHtml);
            register = register.NextSibling;
            Console.ReadKey();
            Console.WriteLine("{0}", register.InnerHtml);
            register = register.NextSibling;
            Console.ReadKey();
            Console.WriteLine("{0}", register.InnerHtml);
            register = register.NextSibling;
            Console.ReadKey();
            
            if (register.InnerText.Contains("(") == false)
            {
                return ret;
            }            

            register = register.NextSibling;

            while (register.InnerText.Contains(")") != true)
            {
                ret.Add(register.InnerText);
                register = register.NextSibling.NextSibling;
            }

            return ret;
        }

        string GetName(HtmlNode node)
        {
            return node.Attributes["class"].Value;
        }

        string PrintList(List<string> list)
        {
            string ret = "";
            
            for(int i = 0; i < list.Count; i++)
            {
                ret += list[i];
                if (i + 1 < list.Count) ret += " → ";
            }

            return ret;
        }

        public void GetPage(string path)
        {
            var document = (new HtmlWeb()).Load(path);
            var examples = document.DocumentNode.SelectNodes("//span[@class='x-g']");
            if (examples == null) return;

            //Init
            HtmlNode definition;
            var file = new StreamWriter("1.txt");

            var usaTranscription = document.DocumentNode.SelectSingleNode("//span[@class='y']").InnerText;
            var gbrTranscription = document.DocumentNode.SelectSingleNode("//span[@class='i']").InnerText;
                        
            foreach (HtmlNode example in examples)
            {
                var card = new Card();
                var parentNode = example.ParentNode;

                //A structure
                var structure1 = example.SelectSingleNode("span[@class='cf']");
                if (GetName(parentNode) == "pv-g")
                {
                    var structure2 = parentNode.SelectSingleNode("h4[@class='pv']");
                    if (structure2 != null) card.structure.Add(structure2.InnerText);
                    if (structure1 != null) card.structure.Add(structure1.InnerText);
                }
                else if (GetName(parentNode) == "n-g" && GetName(parentNode.ParentNode) == "pv-g")
                {
                    var structure2 = parentNode.SelectSingleNode("span[@class='vs-g']");
                    var structure3 = parentNode.ParentNode.SelectSingleNode("h4[@class='pv']");
                    if (structure3 != null) card.structure.Add(structure3.InnerText);
                    if (structure2 != null) card.structure.Add(structure2.InnerText);
                    if (structure1 != null) card.structure.Add(structure1.InnerText);
                }
                else if (GetName(parentNode) == "n-g")
                {
                    var structure2 = parentNode.SelectSingleNode("span[@class='cf']");
                    if (structure2 != null) card.structure.Add(structure2.InnerText);
                    if (structure1 != null) card.structure.Add(structure1.InnerText);
                }
                else if (GetName(parentNode) == "id-g")
                {
                    var structure2 = parentNode.SelectSingleNode("h4[@class='id']");
                    if (structure2 != null) card.structure.Add(structure2.InnerText);
                    if (structure1 != null) card.structure.Add(structure1.InnerText);
                }

                //An example itself
                var sentence = example.SelectSingleNode("span[@class='x']");
                card.sentence = (sentence != null) ? sentence.InnerText : "";

                //Definition
                if (GetName(parentNode) == "id-g")
                {
                    var temp = parentNode.SelectSingleNode("div[@class='def_block']");

                    if (temp == null)
                    {
                        temp = parentNode.SelectSingleNode("span[@class='ud']");
                        if (temp == null)
                        {
                            definition = parentNode.SelectSingleNode("span[@class='d']");
                        }
                        else
                        {
                            definition = temp;
                        }
                    }
                    else
                    {
                        definition = temp.SelectSingleNode("span[@class='ud']");
                        if (definition == null)
                        {
                            definition = temp.SelectSingleNode("span[@class='d']");
                        }
                    }
                }
                else
                {
                    definition = parentNode.SelectSingleNode("span[@class='ud']");
                    if (definition == null)
                    {
                        definition = parentNode.SelectSingleNode("span[@class='d']");
                    }
                }

                card.definition = (definition != null) ? definition.InnerText : "";
                card.definition.Replace("    ", " ");

                card.usaTranscription = usaTranscription;
                card.gbrTranscription = gbrTranscription;

                file.WriteLine("Structure: {1}\nDefinition: {2}\nExample: {0}\nBr: {3}\nAm: {4}\n\n", card.sentence, PrintList(card.structure).Replace("    ", " "), card.definition, card.gbrTranscription, card.usaTranscription);
                //card.structure = (structure != null) ? generalStructure.InnerText : "";
            }
            file.Close();
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Oald8Parser();
            parser.GetPage("http://oald8.oxfordlearnersdictionaries.com/dictionary/fuck");
           // Console.ReadKey();
        }
    }
}
