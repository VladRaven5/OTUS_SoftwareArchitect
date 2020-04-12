FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app
EXPOSE 8000
ENV ASPNETCORE_URLS=http://+:8000

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY ["SoftwareArchitectHW1.csproj", "./"]
RUN dotnet restore "./SoftwareArchitectHW1.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "SoftwareArchitectHW1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SoftwareArchitectHW1.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SoftwareArchitectHW1.dll"]
