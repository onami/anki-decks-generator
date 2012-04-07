﻿using System.Collections.Generic;

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
}
