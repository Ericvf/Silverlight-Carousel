using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace ImageSearch
{
    public class SearchResultCollection : List<SearchResult>
    {
        public SearchResultCollection()
        {
            
        }

        public SearchResultCollection(IEnumerable<SearchResult> collection) : base(collection)
        {

        }
    }
}
