using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace Kumo
{
    record Bookmark(
        int id,
        Word.BookmarkStart start,
        Word.BookmarkEnd end
    );
}
