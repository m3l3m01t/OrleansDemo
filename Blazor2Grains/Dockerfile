FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Blazor2Grains/Blazor2Grains.csproj", "Blazor2Grains/"]
RUN dotnet restore "Blazor2Grains/Blazor2Grains.csproj"
COPY . .
WORKDIR "/src/Blazor2Grains"
RUN dotnet build "Blazor2Grains.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Blazor2Grains.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Blazor2Grains.dll"]
