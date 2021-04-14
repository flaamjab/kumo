using DocumentFormat.OpenXml.Packaging;

namespace Kumo
{
    class AnnotationStore
    {
        private CustomXml _customXml;
        private Bookmarks _bookmarks;

        public AnnotationStore(MainDocumentPart mainPart)
        {
            _customXml = new CustomXml(mainPart);
            _bookmarks = new Bookmarks(mainPart.Document);
        }

        
    }
}
