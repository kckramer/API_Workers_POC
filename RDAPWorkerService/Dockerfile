#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["RDAPWorkerService/RDAPWorkerService.csproj", "RDAPWorkerService/"]
RUN dotnet restore "RDAPWorkerService/RDAPWorkerService.csproj"
COPY . .
WORKDIR "/src/RDAPWorkerService"
RUN dotnet build "RDAPWorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RDAPWorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RDAPWorkerService.dll"]