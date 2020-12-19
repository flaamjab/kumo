using DocumentFormat.OpenXml.Packaging;

namespace SemanticWord {
    namespace Types {
        public class Document {
            private readonly DocumentXMLEditor _docXmlEditor;
            private readonly WordprocessingDocument _document;

            public Document(string path) {
                _document = WordprocessingDocument.Open(path, false);
                _docXmlEditor = new ParaIDReferenceEditor(
                    _document.MainDocumentPart
                );
            }

            public string Text() {
                return _docXmlEditor.Text();
            }

            public void Close()
            {
                _document.Close();
            }
        }
    }
}
