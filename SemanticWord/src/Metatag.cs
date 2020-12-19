using System;
using System.Text;

namespace SemanticWord
{
    /**<summary>This class represents metatags that
    can be manipulated within an Open XML document.</summary>*/
    public class Metatag
    {
        /// <summary>Get or sets the part of the entire corpus which contains
        /// a sequence of annotated words.</summary>
        public string Context { get; set; }

        /// <summary>Gets or sets the part of the
        /// context that is being tagged.</summary>
        public string Text { get; set; }

        /// <summary>Gets or sets the type of the metatag.</summary>
        public string Type { get; set; }

        /// <summary>Gets or sets the tagged entity class</summary>
        public string Class { get; set; }

        /// <summary>Gets or sets the tagged entity instance</summary>
        public string Instance { get; set; }

        /// <summary>Creates a <c>Metatag object</c>.
        /// Throws <c>ArgumentException</c> 
        /// if the <c>text</c> string is not
        /// a part of the <c>context</c> string.</summary>
        public Metatag(
            string text,
            string context,
            string type,
            string class_ = "",
            string instance = ""
        )
        {
            if (text == null || context == null || type == null)
                throw new ArgumentNullException(
                    "The provided strings cannot be null"
                );
            if (
                !TextTransform.RemoveNonAlphanum(context).ToLower().Contains(
                    TextTransform.RemoveNonAlphanum(text).ToLower()
                )
            )
                throw new ArgumentException(
                    "\"Text\" must be a part of \"Context\""
                );

            Text = text;
            Context = context;
            Type = type;
            Class = class_;
            Instance = instance;
        }

        override public string ToString()
        {
            return $"Metatag {{Text: \"{Text}\", "
                + $"Context: \"{Context}\", Type: \"{Type}\"}}";
        }

        private Metatag() { }
    }

    class MetatagBundle
    {
        public Metatag Metatag { get; set; }

        public int ID { get; set; }

        public int[] PIDs { get; set; }

        public MetatagBundle(Metatag mt, int id, int[] pids)
        {
            Metatag = mt;
            ID = id;
            PIDs = pids;
        }

        override public string ToString()
        {
            var sw = new StringBuilder("[");
            for (int ix = 0; ix < PIDs.Length; ix++)
            {
                sw.Append(PIDs[ix]);
                if (ix < PIDs.Length - 1)
                    sw.Append(" ");
            }
            sw.Append("]");

            return $"MetatagBundle {{{Metatag}, ID: {ID}, PIDs: {sw}";
        }

        public static int GenerateID()
        {
            return (new Random()).Next();
        }
    }
}
