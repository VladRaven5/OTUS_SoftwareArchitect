FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5010
ENV ASPNETCORE_URLS=http://+:5010

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["BomberService.csproj", "./"]
RUN dotnet restore "./BomberService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "BomberService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BomberService.csproj" -c Release -o /app/publish

FROM base as final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BomberService.dll"]