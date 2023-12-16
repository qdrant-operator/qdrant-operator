#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0.0-jammy-amd64
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV TZ=UTC
ENV DEBIAN_FRONTEND noninteractive
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

COPY app/publish              /app/

ENTRYPOINT ["dotnet", "qdrant-operator.dll"]