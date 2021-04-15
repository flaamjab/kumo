using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    record Bookmark(
        string id,
        Word.BookmarkStart start,
        Word.BookmarkEnd end
    );
}
