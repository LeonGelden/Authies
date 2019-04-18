FROM mcr.microsoft.com/dotnet/core/aspnet:2.1-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.1-stretch AS build
WORKDIR /src
COPY ["src/ResourceApi2/ResourceApi2.csproj", "src/ResourceApi2/"]
RUN dotnet restore "src/ResourceApi2/ResourceApi2.csproj"
COPY . .
WORKDIR "/src/src/ResourceApi2"
RUN dotnet build "ResourceApi2.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "ResourceApi2.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ResourceApi2.dll"]