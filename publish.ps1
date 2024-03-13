#Requires -Version 7.1.3 -RunAsAdministrator

dotnet publish -c Release src\qdrant-operator\qdrant-operator.csproj -o app/publish

docker build -t ghcr.io/qdrant-operator/qdrant-operator:main .
docker push ghcr.io/qdrant-operator/qdrant-operator:main
