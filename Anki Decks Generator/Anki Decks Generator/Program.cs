using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using HtmlAgilityPack; 

namespace Anki_Decks_Generator
{
    class Card
    {
        public string sentence;
        public string interpretation;
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
        string searchPath = "http://oald8.oxfordlearnersdictionaries.com/dictionary/";
        StreamWriter stream;
        public Oald8Parser(StreamWriter stream)
        {
            this.stream = stream;
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

            ret = ret.Replace("    ", " ").Replace("  ", " ");

            return ret;
        }

        public void Process(List<string> wordlist)
        {
            foreach (string word in wordlist)
                GetPage(word);
        }

        void GetPage(string word)
        {
            var document = (new HtmlWeb()).Load(searchPath + word);
            var examples = document.DocumentNode.SelectNodes("//span[@class='x-g']");
            if (examples == null) return;

            //Init
            HtmlNode definition;
           
            var usaTranscription = document.DocumentNode.SelectSingleNode("//span[@class='y']").InnerText;
            var gbrTranscription = document.DocumentNode.SelectSingleNode("//span[@class='i']").InnerText;
            usaTranscription = (new Regex("^ ")).Replace(usaTranscription, "");
            gbrTranscription = (new Regex("^ ")).Replace(gbrTranscription, "");
                        
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
                var interpretation = example.SelectSingleNode("span[@class='x']");
                card.interpretation = (interpretation != null) ? interpretation.InnerText : "";
                card.sentence = (new Regex(" \\(=.*?\\)")).Replace(card.interpretation, "");

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

                //file.WriteLine("Structure: {1}\nDefinition: {2}\nExample: {0}\nFull: {5}\nBr: {3}\nAm: {4}\n\n", card.sentence, PrintList(card.structure), card.definition, card.gbrTranscription, card.usaTranscription, card.interpretation);
                stream.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", card.sentence, card.interpretation, PrintList(card.structure), card.definition, card.gbrTranscription, card.usaTranscription, "oald8 " + word);
                
                //card.structure = (structure != null) ? generalStructure.InnerText : "";
            }
        }

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

    }

    class Word
    {
        public string value;
        public bool isVisited = false;
    }
    class Program
    {
        static void Main(string[] args)
        {
            var oaldFlag = false;
            var inputPath = "";
            var outputPath = "./deck " + DateTime.Now.ToString("yyyy.MM.dd HH-mm-ss") + ".txt";
            var wordlist = new List<String>();

            StreamWriter output;
            StreamReader input;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-oald8")
                {
                    oaldFlag = true;
                }
                else if (args[i] == "-p" && i + 1 < args.Length)
                {
                    inputPath = args[i+1];
                    i++;
                }
              //  Console.WriteLine("{0}", args[i]);

            }

            if (inputPath == "")
            {
                Console.Write("Where's a listname, uh? You should type -p <path_to_you_wordlist>");
                Console.ReadKey();
                return;
            }
            else
            {
                try
                {
                    input = new StreamReader(inputPath);
                    while (input.EndOfStream == false)
                    {
                        wordlist.Add(input.ReadLine().Trim());
                    }
                }
                catch(FileNotFoundException e) 
                {
                    Console.Write("Wrong path: {0}", inputPath);
                }
            }           

            output = new StreamWriter(outputPath);

            if(oaldFlag == true)
            {
            var parser = new Oald8Parser(output);
            parser.Process(wordlist);
            }

            output.Close();
            Console.ReadKey();
        }
    }
}
