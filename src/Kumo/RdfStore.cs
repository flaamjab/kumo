using System;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using VDS.RDF;

namespace Kumo
{
    class RdfStore
    {
        private MainDocumentPart _mainPart;
        private IGraph _annotationGraph;

        public RdfStore(MainDocumentPart mainPart)
        {
            _mainPart = mainPart;
        }

        public void Assert(Annotation annotation)
        {
            throw new NotImplementedException();
        }

        public void Retract(Annotation annotation)
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
