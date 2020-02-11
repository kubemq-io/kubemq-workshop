FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src

COPY . .
RUN dotnet restore rate_generate.csproj
WORKDIR /src
RUN dotnet build rate_generate.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish rate_generate.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "rate_generate.dll"]
