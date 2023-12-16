#Requires -Version 7.1.3 -RunAsAdministrator

dotnet publish -c Release src\qdrant-operator\qdrant-operator.csproj -o app/publish

docker build -t ghcr.io/myinstep/qdrant-operator:main .
docker push ghcr.io/myinstep/qdrant-operator:main
