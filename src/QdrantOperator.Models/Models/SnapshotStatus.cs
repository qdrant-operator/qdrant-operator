using System.Collections.Generic;

namespace QdrantOperator.Models
{
    public class SnapshotStatus
    {
        // unfortunately using the qdrant model here makes the generator barf, we'll have to
        // create a model that represents the same data
        public Dictionary<string, string> Nodes { get; set; }
    }
}
