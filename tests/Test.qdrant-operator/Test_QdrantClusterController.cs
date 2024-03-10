using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using k8s;
using k8s.Models;

using Neon.Operator.Xunit;

using QdrantOperator;
using QdrantOperator.Entities;
using QdrantOperator.Models;

namespace Test_QdrantOperator
{
    [Collection("Test_QdrantClusterController")]
    [CollectionDefinition("Test_QdrantClusterController", DisableParallelization = true)]
    [Trait("Category", "QdrantClusterController")]
    public class Test_QdrantClusterController : IClassFixture<TestOperatorFixture>
    {
        private TestOperatorFixture fixture;

        public Test_QdrantClusterController(TestOperatorFixture fixture)
        {
            this.fixture = fixture;
            fixture.Operator.AddController<QdrantClusterController>();
            fixture.Operator.AddFinalizer<QdrantClusterFinalizer>();
            fixture.RegisterType<V1ConfigMap>();
            fixture.RegisterType<V1Secret>();
            fixture.RegisterType<V1Service>();
            fixture.RegisterType<V1PersistentVolumeClaim>();
            fixture.RegisterType<V1ServiceAccount>();
            fixture.RegisterType<V1StatefulSet>();
            fixture.Start();
        }

        [Fact]
        public async Task TestReconcileCreatesStatefulset()
        {
            fixture.ClearResources();

            var controller = fixture.Operator.GetController<QdrantClusterController>();

            var qdrantCluster = new V1QdrantCluster()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Spec = new V1QdrantCluster.V1QdrantClusterSpec()
                {
                    Image = new ImageSpec()
                    {
                        PullPolicy = "Always",
                        Repository = "test/image",
                        Tag = "not-latest"
                    },
                    Persistence = new PersistenceSpec()
                    {
                        Size = "9999Gi",
                        StorageClassName = "test-storage-class"
                    }
                }
            };
            await controller.ReconcileInternalAsync(qdrantCluster);

            // verify result
            var statefulsets   = fixture.GetResources<V1StatefulSet>();
            var services       = fixture.GetResources<V1Service>();
            var clusterIp      = fixture.GetResource<V1Service>(qdrantCluster.GetFullName(), qdrantCluster.Metadata.NamespaceProperty);
            var headless       = fixture.GetResource<V1Service>(Constants.HeadlessServiceName(qdrantCluster.GetFullName()), qdrantCluster.Metadata.NamespaceProperty);
            var configMap      = fixture.GetResources<V1ConfigMap>();
            var serviceAccount = fixture.GetResources<V1ServiceAccount>();

            statefulsets.Should().HaveCount(1);
            statefulsets.First().Metadata.Name.Should().Be(qdrantCluster.GetFullName());

            services.Should().HaveCount(2);
            clusterIp.Should().NotBeNull();
            headless.Should().NotBeNull();
            headless.Spec.ClusterIP.Should().Be("None");

            configMap.Should().HaveCount(1);
            configMap.First().Metadata.Name.Should().Be(qdrantCluster.GetFullName());

            serviceAccount.Should().HaveCount(1);
            serviceAccount.First().Metadata.Name.Should().Be(qdrantCluster.GetFullName());
        }

        [Fact]
        public async Task TestCreatesSecrets()
        {
            fixture.ClearResources();

            var controller = fixture.Operator.GetController<QdrantClusterController>();

            var qdrantCluster = new V1QdrantCluster()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Spec = new V1QdrantCluster.V1QdrantClusterSpec()
                {
                    Image = new ImageSpec()
                    {
                        PullPolicy = "Always",
                        Repository = "test/image",
                        Tag = "not-latest"
                    },
                    Persistence = new PersistenceSpec()
                    {
                        Size = "9999Gi",
                        StorageClassName = "test-storage-class"
                    },
                    ApiKey = new ApiKeyOptions()
                    {
                        Enabled = true
                    },
                    ReadApiKey = new ApiKeyOptions()
                    {
                        Enabled = true
                    }
                }
            };
            await controller.ReconcileInternalAsync(qdrantCluster);

            // verify result
            var statefulsets   = fixture.GetResources<V1StatefulSet>();
            var secrets        = fixture.GetResources<V1Secret>();

            statefulsets.Should().HaveCount(1);
            statefulsets.First().Metadata.Name.Should().Be(qdrantCluster.GetFullName());

            var env = statefulsets.First().Spec.Template.Spec.Containers.First().Env.Where(e => e.Name == Constants.ApiKeyEnvName).FirstOrDefault();
            env.ValueFrom.SecretKeyRef.Should().NotBeNull();
            env.ValueFrom.SecretKeyRef.Name.Should().Be(qdrantCluster.GetFullName());
            env.ValueFrom.SecretKeyRef.Key.Should().Be(Constants.ApiKeySecretKey);

            env = statefulsets.First().Spec.Template.Spec.Containers.First().Env.Where(e => e.Name == Constants.ReadApiKeyEnvName).FirstOrDefault();
            env.ValueFrom.SecretKeyRef.Should().NotBeNull();
            env.ValueFrom.SecretKeyRef.Name.Should().Be(qdrantCluster.GetFullName());
            env.ValueFrom.SecretKeyRef.Key.Should().Be(Constants.ReadApiKeySecretKey);

            secrets.Should().HaveCount(1);

            secrets.FirstOrDefault().Data.Keys.Should().HaveCount(2);
            secrets.FirstOrDefault().Data.Keys.Should().Contain(Constants.ApiKeySecretKey);
            secrets.FirstOrDefault().Data.Keys.Should().Contain(Constants.ReadApiKeySecretKey);
        }

        [Fact]
        public async Task TestCreatesCustomSecrets()
        {
            fixture.ClearResources();

            var controller = fixture.Operator.GetController<QdrantClusterController>();

            var qdrantCluster = new V1QdrantCluster()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Spec = new V1QdrantCluster.V1QdrantClusterSpec()
                {
                    Image = new ImageSpec()
                    {
                        PullPolicy = "Always",
                        Repository = "test/image",
                        Tag = "not-latest"
                    },
                    Persistence = new PersistenceSpec()
                    {
                        Size = "9999Gi",
                        StorageClassName = "test-storage-class"
                    },
                    ApiKey = new ApiKeyOptions()
                    {
                        Enabled = true,
                        Secret = new V1SecretKeySelector()
                        {
                            Name = "foo",
                            Key = "bar"
                        }
                    },
                    ReadApiKey = new ApiKeyOptions()
                    {
                        Enabled = true,
                        Secret = new V1SecretKeySelector()
                        {
                            Name = "bar",
                            Key = "baz",
                        },
                        KeyLength = 5
                    }
                }
            };
            await controller.ReconcileInternalAsync(qdrantCluster);

            // verify result
            var statefulsets   = fixture.GetResources<V1StatefulSet>();
            var secrets        = fixture.GetResources<V1Secret>();

            statefulsets.Should().HaveCount(1);
            statefulsets.First().Metadata.Name.Should().Be(qdrantCluster.GetFullName());

            var env = statefulsets.First().Spec.Template.Spec.Containers.First().Env.Where(e => e.Name == Constants.ApiKeyEnvName).FirstOrDefault();
            env.ValueFrom.SecretKeyRef.Should().NotBeNull();
            env.ValueFrom.SecretKeyRef.Name.Should().Be("foo");
            env.ValueFrom.SecretKeyRef.Key.Should().Be("bar");

            env = statefulsets.First().Spec.Template.Spec.Containers.First().Env.Where(e => e.Name == Constants.ReadApiKeyEnvName).FirstOrDefault();
            env.ValueFrom.SecretKeyRef.Should().NotBeNull();
            env.ValueFrom.SecretKeyRef.Name.Should().Be("bar");
            env.ValueFrom.SecretKeyRef.Key.Should().Be("baz");

            secrets.Should().HaveCount(2);

            var fooSecret = secrets.Where(s => s.Name() == "foo").FirstOrDefault();
            fooSecret.Data.Keys.Should().HaveCount(1);
            var barSecret = secrets.Where(s => s.Name() == "bar").FirstOrDefault();
            barSecret.Data.Keys.Should().HaveCount(1);
            barSecret.Data.Values.FirstOrDefault().Length.Should().Be(5);
        }

        [Fact]
        public async Task TestUpdateVolumePersistence()
        {
            fixture.ClearResources();

            var controller = fixture.Operator.GetController<QdrantClusterController>();

            var qdrantCluster = new V1QdrantCluster()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Spec = new V1QdrantCluster.V1QdrantClusterSpec()
                {
                    Image = new ImageSpec()
                    {
                        PullPolicy = "Always",
                        Repository = "test/image",
                        Tag = "not-latest"
                    },
                    Persistence = new PersistenceSpec()
                    {
                        Size = "9999Gi",
                        StorageClassName = "test-storage-class"
                    },
                    Replicas = 3
                }
            };
            await controller.ReconcileInternalAsync(qdrantCluster);

            for (int i = 0; i < qdrantCluster.Spec.Replicas; i++)
            {
                var pvc = new V1PersistentVolumeClaim().Initialize();
                pvc.Metadata.Name = $"{Constants.QdrantStorage}-{qdrantCluster.GetFullName()}-{i}";
                pvc.Metadata.NamespaceProperty = qdrantCluster.Metadata.NamespaceProperty;
                pvc.Spec = new V1PersistentVolumeClaimSpec()
                {
                    Resources = new V1ResourceRequirements()
                    {
                        Requests = new Dictionary<string, ResourceQuantity>()
                        {
                            { "storage", new ResourceQuantity(qdrantCluster.Spec.Persistence.Size) }
                        }
                    }
                };
                fixture.Resources.Add(pvc);
            }
            // verify result
            var statefulsets = fixture.Resources.OfType<V1StatefulSet>();

            statefulsets.Should().HaveCount(1);
            statefulsets.First().Metadata.Name.Should().Be(qdrantCluster.GetFullName());

            qdrantCluster.Spec.Persistence.Size = "100Gi";
            await controller.CheckVolumesAsync(qdrantCluster);

            var volumes = fixture.GetResources<V1PersistentVolumeClaim>();
            volumes.First().Spec.Resources.Requests["storage"].Value.Should().Be("100Gi");
        }

        [Fact]
        public async Task TestFinalizeAsync()
        {
            fixture.ClearResources();

            var finalizer = fixture.Operator.GetFinalizer<QdrantClusterFinalizer>();

            var qdrantCluster = new V1QdrantCluster()
            {
                Metadata = new V1ObjectMeta()
                {
                    Name = "test",
                    NamespaceProperty = "test",
                },
                Spec = new V1QdrantCluster.V1QdrantClusterSpec()
                {
                    Image = new ImageSpec()
                    {
                        PullPolicy = "Always",
                        Repository = "test/image",
                        Tag = "not-latest"
                    },
                    Persistence = new PersistenceSpec()
                    {
                        Size = "9999Gi",
                        StorageClassName = "test-storage-class"
                    }
                }
            };

            fixture.AddResource(qdrantCluster);

            var statefulSet = new V1StatefulSet().Initialize();
            statefulSet.Metadata.Name = qdrantCluster.GetFullName();
            statefulSet.Metadata.NamespaceProperty = qdrantCluster.Metadata.NamespaceProperty;

            var service = new V1Service().Initialize();
            service.Metadata.Name = qdrantCluster.GetFullName();
            service.Metadata.NamespaceProperty = qdrantCluster.Metadata.NamespaceProperty;
            
            var serviceHeadless = new V1Service().Initialize();
            serviceHeadless.Metadata.Name = Constants.HeadlessServiceName(qdrantCluster.GetFullName());
            serviceHeadless.Metadata.NamespaceProperty = qdrantCluster.Metadata.NamespaceProperty;

            var configMap = new V1ConfigMap().Initialize();
            configMap.Metadata.Name = qdrantCluster.GetFullName();
            configMap.Metadata.NamespaceProperty = qdrantCluster.Metadata.NamespaceProperty;

            var serviceAccount = new V1ServiceAccount().Initialize();
            serviceAccount.Metadata.Name = qdrantCluster.GetFullName();
            serviceAccount.Metadata.NamespaceProperty = qdrantCluster.Metadata.NamespaceProperty;

            fixture.AddResource(statefulSet);
            fixture.AddResource(service);
            fixture.AddResource(serviceHeadless);
            fixture.AddResource(configMap);
            fixture.AddResource(serviceAccount);

            await finalizer.FinalizeAsync(qdrantCluster);

            fixture.GetResource<V1StatefulSet>(statefulSet.GetFullName(), statefulSet.Metadata.NamespaceProperty).Should().BeNull();
            fixture.GetResource<V1Service>(service.GetFullName(), service.Metadata.NamespaceProperty).Should().BeNull();
            fixture.GetResource<V1Service>(serviceHeadless.GetFullName(), serviceHeadless.Metadata.NamespaceProperty).Should().BeNull();
            fixture.GetResource<V1ConfigMap>(configMap.GetFullName(), configMap.Metadata.NamespaceProperty).Should().BeNull();
            fixture.GetResource<V1ServiceAccount>(serviceAccount.GetFullName(), serviceAccount.Metadata.NamespaceProperty).Should().BeNull();
        }
    }
}