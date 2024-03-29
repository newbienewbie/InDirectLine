FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 3000 

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/Itminus.InDirectLine.Samples/Itminus.InDirectLine.IntegrationBotSample
RUN dotnet build Itminus.InDirectLine.IntegrationBotSample.csproj -c Release -o /app/build

FROM build AS publish
WORKDIR /src/Itminus.InDirectLine.Samples/Itminus.InDirectLine.IntegrationBotSample
RUN dotnet publish Itminus.InDirectLine.IntegrationBotSample.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Itminus.InDirectLine.IntegrationBotSample.dll"]
CMD [ "--urls", "http://0.0.0.0:5000" ]
