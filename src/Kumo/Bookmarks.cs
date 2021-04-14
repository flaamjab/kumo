using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    class Bookmarks
    {
        private Word.Document _content;

        public Bookmarks(Word.Document content)
        {
            _content = content;
        }
    }
}
