# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#dossier du code source
WORKDIR /src
# Copy SharedEvents project
COPY ITANIS.SharedEvents ITANIS.SharedEvents
# Copy ModuleCRM project
COPY ModuleCRM ModuleCRM
WORKDIR /src/ModuleCRM
RUN dotnet restore
RUN dotnet publish -c Release -o out 

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
#dossier de l'application compilée
WORKDIR /app
COPY --from=build /src/ModuleCRM/out .

#Pour garantir que ton API écoute bien dans le container.
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080 
ENTRYPOINT ["dotnet", "ModuleCRM.dll"]