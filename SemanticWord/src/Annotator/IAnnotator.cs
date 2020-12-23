using System.Collections.Generic;
using System.Threading.Tasks;

namespace SemanticWord
{
    namespace Annotation
    {
        /// <summary>
        ///     The interface that annotators should implement.
        /// </summary>
        interface IAnnotator
        {
            Task<IList<Metatag>> AnnotateAsync(
                string text
            );
        }
    }
}
