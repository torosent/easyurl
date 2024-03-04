FROM mcr.microsoft.com/dotnet/sdk:8.0-cbl-mariner2.0 AS build-env

WORKDIR /app

COPY *.csproj ./
COPY . .

RUN dotnet restore

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0-cbl-mariner2.0-distroless

WORKDIR /app

COPY --from=build-env /app/out ./

ENTRYPOINT ["dotnet", "easyurl.dll"]
