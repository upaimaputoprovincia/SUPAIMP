FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY SUPAI_MP/SUPAI_MP.csproj SUPAI_MP/

RUN dotnet restore SUPAI_MP/SUPAI_MP.csproj

COPY SUPAI_MP/ SUPAI_MP/

WORKDIR /src/SUPAI_MP

RUN dotnet publish SUPAI_MP.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "SUPAI_MP.dll"]