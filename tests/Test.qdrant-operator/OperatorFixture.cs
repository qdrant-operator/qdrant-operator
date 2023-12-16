using System;

using Neon.Operator.Xunit;
using Neon.Xunit;

using Qdrant.Client;

namespace QdrantOperator.Xunit
{
    public class OperatorFixture : ComposedFixture, IDisposable
    {
        public QdrantClient QdrantClient { get; private set; }
        public TestOperatorFixture TestOperatorFixture { get; private set; }
        public QdrantFixture QdrantFixture { get; private set; }

        private ComposedFixture fixture;

        public OperatorFixture()
        {
        }

        public TestFixtureStatus Start()
        {
            base.CheckDisposed();

            return base.Start(
                () =>
                {
                    AddFixture("operator", new TestOperatorFixture(),
                        operatorFixture =>
                        {
                            this.TestOperatorFixture = operatorFixture;
                            this.TestOperatorFixture.Operator.AddController<QdrantCollectionController>();
                            this.TestOperatorFixture.Operator.AddFinalizer<QdrantCollectionFinalizer>();

                            operatorFixture.Start();
                        });

                    AddFixture("qdrant", new QdrantFixture(),
                        qdrantFixture =>
                        {
                            this.QdrantFixture = qdrantFixture;
                            qdrantFixture.StartAsComposed();
                            QdrantClient = qdrantFixture.QdrantClient;
                        });

                });
        }

        new public void Dispose()
        {
        }
    }
}