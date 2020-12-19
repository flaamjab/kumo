using System.Collections.Generic;

namespace SemanticWord
{
    namespace Annotation
    {
        /// <summary>
        ///     The interface that annotators should implement.
        /// </summary>
        interface IAnnotator
        {
            IList<Metatag> Annotations { get; }
        }
    }
}