FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ProyectoSantaMonica_Cesar.csproj", "."]
RUN dotnet restore "ProyectoSantaMonica_Cesar.csproj"
COPY . .
RUN dotnet publish "ProyectoSantaMonica_Cesar.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProyectoSantaMonica_Cesar.dll"]
