FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 3000 

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY . .
WORKDIR /src/Itminus.InDirectLine.Samples/Itminus.InDirectLine.EchoBotApp
RUN dotnet build Itminus.InDirectLine.EchoBotApp.csproj -c Release -o /app/build

FROM build AS publish
WORKDIR /src/Itminus.InDirectLine.Samples/Itminus.InDirectLine.EchoBotApp
RUN dotnet publish Itminus.InDirectLine.EchoBotApp.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Itminus.InDirectLine.EchoBotApp.dll"]
CMD [ "--urls", "http://0.0.0.0:5000" ]
