using DocumentFormat.OpenXml.Packaging;

namespace Kumo
{
    class Annotations
    {
        public Annotations(MainDocumentPart mainPart)
        {
            _mainPart = mainPart;
        }

        private MainDocumentPart _mainPart;
    }
}
