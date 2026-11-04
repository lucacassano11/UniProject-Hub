# build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY shared/ shared/
COPY services/sicurezza/ services/sicurezza/
RUN dotnet publish services/sicurezza/Sicurezza.csproj -c Release -o /app

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Sicurezza.dll"]
