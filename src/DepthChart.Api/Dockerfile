FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY src/DepthChart.Domain/DepthChart.Domain.csproj src/DepthChart.Domain/
COPY src/DepthChart.Application/DepthChart.Application.csproj src/DepthChart.Application/
COPY src/DepthChart.Contracts/DepthChart.Contracts.csproj src/DepthChart.Contracts/
COPY src/DepthChart.Infrastructure/DepthChart.Infrastructure.csproj src/DepthChart.Infrastructure/
COPY src/DepthChart.Api/DepthChart.Api.csproj src/DepthChart.Api/

RUN dotnet restore src/DepthChart.Api/DepthChart.Api.csproj

COPY . .
RUN dotnet publish src/DepthChart.Api/DepthChart.Api.csproj -c Release -o /out

FROM base AS final
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "DepthChart.Api.dll"]
