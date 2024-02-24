using Neon.Operator.Attributes;
using Neon.Operator.OperatorLifecycleManager;

using QdrantOperator;

[assembly: Name(Name = "qdrant-operator")]
[assembly: DisplayName(DisplayName = "Qdrant Operator")]
[assembly: OwnedEntity<V1QdrantCluster>(Description = Constants.QdrantClusterDescription, DisplayName = "QdrantCluster")]
[assembly: OwnedEntity<V1QdrantCollection>(Description = Constants.QdrantCollectionDescription, DisplayName = "QdrantCollection")]
[assembly: OwnedEntity<V1QdrantCollectionFieldIndex>(Description = Constants.QdrantCollectionFieldIndexDescription, DisplayName = "QdrantCollectionFieldIndex")]
[assembly: Description(FullDescription = Constants.FullDescription, ShortDescription = Constants.ShortDescription)]
[assembly: Provider(Name = "qdrant-operator", Url = "https://github.com/qdrant-operator")]
[assembly: Maintainer(Name = "Marcus", Email = "marcus@bowyer.me", GitHub = "marcusbooyah", Reviewer = true)]
[assembly: Maintainer(Name = "Carolina", Email = "carolina.vhaggerty@gmail.com", GitHub = "carohagg", Reviewer = true)]
[assembly: Version("0.0.1")]
[assembly: Maturity("beta")]
[assembly: MinKubeVersion("")]
[assembly: Icon(Path = "../../logo.png", MediaType = "image/png")]
[assembly: Keyword("qdrant")]
[assembly: Category(Category = Category.AiMachineLearning | Category.BigData | Category.Database)]
[assembly: Capabilities(Capability = CapabilityLevel.FullLifecycle)]
[assembly: ContainerImage(Repository = "ghcr.io/qdrant-operator/qdrant-operator", Tag = "0.0.1")]
[assembly: Repository(Repository = "https://github.com/qdrant-operator/qdrant-operator")]
[assembly: InstallMode(Type = InstallModeType.OwnNamespace | InstallModeType.SingleNamespace | InstallModeType.MultiNamespace | InstallModeType.AllNamespaces)]
[assembly: DefaultChannel("beta")]
