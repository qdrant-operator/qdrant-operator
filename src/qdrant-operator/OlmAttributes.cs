using Neon.Operator.Attributes;
using Neon.Operator.OperatorLifecycleManager;

using QdrantOperator;

[assembly: Name(Name = "qdrant-operator")]
[assembly: DisplayName(DisplayName = "Qdrant Operator")]
[assembly: Version(Constants.Version)]
[assembly: Maturity(Constants.Maturity)]
[assembly: MinKubeVersion(Constants.MinKubeVersion)]
[assembly: Keyword("qdrant", "vector", "database", "ai")]
[assembly: DefaultChannel(Constants.DefaultChannel)]
[assembly: OwnedEntity<V1QdrantCluster>(
    Description = Constants.QdrantClusterDescription,
    DisplayName = "QdrantCluster",
    ExampleYaml = Constants.QdrantClusterExample)]
[assembly: OwnedEntity<V1QdrantCollection>(
    Description = Constants.QdrantCollectionDescription,
    DisplayName = "QdrantCollection",
    ExampleYaml = Constants.QdrantCollectionExample)]
[assembly: OwnedEntity<V1QdrantCollectionFieldIndex>(
    Description = Constants.QdrantCollectionFieldIndexDescription,
    DisplayName = "QdrantCollectionFieldIndex",
    ExampleYaml = Constants.QdrantFieldIndexExample)]
[assembly: Description(
    FullDescription = Constants.FullDescription,
    ShortDescription = Constants.ShortDescription)]
[assembly: Provider(
    Name = "qdrant-operator",
    Url = "https://github.com/qdrant-operator")]
[assembly: Maintainer(
    Name = "Marcus Bowyer",
    Email = "marcus@bowyer.me",
    GitHub = "marcusbooyah",
    Reviewer = true)]
[assembly: Maintainer(
    Name = "Carolina Haggerty",
    Email = "carolina.vhaggerty@gmail.com",
    GitHub = "carohagg",
    Reviewer = true)]
[assembly: Icon(
    Path = "../../logo.png",
    MediaType = "image/png")]
[assembly: Category(
    Category = Category.AiMachineLearning
    | Category.BigData
    | Category.Database)]
[assembly: Capabilities(
    Capability = CapabilityLevel.FullLifecycle)]
[assembly: ContainerImage(
    Repository = "ghcr.io/qdrant-operator/qdrant-operator",
    Tag = Constants.Version)]
[assembly: Repository(
    Repository = "https://github.com/qdrant-operator/qdrant-operator")]
[assembly: InstallMode(
    Type = InstallModeType.OwnNamespace
    | InstallModeType.SingleNamespace
    | InstallModeType.MultiNamespace
    | InstallModeType.AllNamespaces)]
