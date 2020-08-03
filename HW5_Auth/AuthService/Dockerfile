FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["AuthService.csproj", "./"]
RUN dotnet restore "./AuthService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "AuthService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AuthService.csproj" -c Release -o /app/publish

FROM base as final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuthService.dll"]