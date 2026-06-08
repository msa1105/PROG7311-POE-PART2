# Use the ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PROG7311-POE.Shared/PROG7311-POE.Shared.csproj", "PROG7311-POE.Shared/"]
COPY ["PROG7311-POE.csproj", "."]
RUN dotnet restore "PROG7311-POE.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "PROG7311-POE.csproj" -c Release -o /app/build

# Publish the build output
FROM build AS publish
RUN dotnet publish "PROG7311-POE.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Setup the runtime container
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PROG7311-POE.dll"]
