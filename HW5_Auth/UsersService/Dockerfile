FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5003
ENV ASPNETCORE_URLS=http://+:5003

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["UsersService.csproj", "./"]
RUN dotnet restore "./UsersService.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "UsersService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UsersService.csproj" -c Release -o /app/publish

FROM base as final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UsersService.dll"]