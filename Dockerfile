FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/ src/
RUN dotnet publish src/Despacho.Api/Despacho.Api.csproj -c Release -o /app/api
RUN dotnet publish src/Despacho.EnviadorCorreos/Despacho.EnviadorCorreos.csproj -c Release -o /app/enviador

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/api ./api
COPY --from=build /app/enviador ./enviador
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
WORKDIR /app/api
ENTRYPOINT ["dotnet", "Despacho.Api.dll"]
