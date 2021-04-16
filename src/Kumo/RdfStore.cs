using System;
using DocumentFormat.OpenXml.Packaging;
using VDS.RDF;

namespace Kumo
{
    class RdfStore
    {
        private MainDocumentPart _mainPart;
        private CustomXmlPart _textAnnotContainer;
        private IGraph _annotGraph;

        public RdfStore(MainDocumentPart mainPart)
        {
            _mainPart = mainPart;
        }

        public void Assert(Triple[] triples)
        {
            throw new NotImplementedException();
        }

        public void Retract(string id)
        {
            throw new NotImplementedException();
        }

        public Triple[] Get(string id)
        {
            throw new NotImplementedException();
        }

        private void Load()
        {
            throw new NotImplementedException();
        }

        private bool IsContainerInitialized()
        {
            throw new NotImplementedException();
        }

        private void InitializeContainer()
        {
            _mainPart.AddCustomXmlPart(
                CustomXmlPartType.CustomXml,
                "text-annotations"
            );
        }
    }
}
