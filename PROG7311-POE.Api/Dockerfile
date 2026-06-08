# Use the ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PROG7311-POE.Shared/PROG7311-POE.Shared.csproj", "PROG7311-POE.Shared/"]
COPY ["PROG7311-POE.Api/PROG7311-POE.Api.csproj", "PROG7311-POE.Api/"]
RUN dotnet restore "PROG7311-POE.Api/PROG7311-POE.Api.csproj"
COPY . .
WORKDIR "/src/PROG7311-POE.Api"
RUN dotnet build "PROG7311-POE.Api.csproj" -c Release -o /app/build

# Publish the build output
FROM build AS publish
RUN dotnet publish "PROG7311-POE.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Setup the runtime container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PROG7311-POE.Api.dll"]
