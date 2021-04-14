using DocumentFormat.OpenXml.Packaging;

namespace Kumo
{
    class CustomXml
    {
        private MainDocumentPart _mainPart;

        public CustomXml(MainDocumentPart mainPart)
        {
            _mainPart = mainPart;
        }
    }
}
