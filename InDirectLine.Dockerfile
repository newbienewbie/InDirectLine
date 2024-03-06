FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 3000 

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
WORKDIR /src/Itminus.InDirectLine.Web
RUN dotnet build Itminus.InDirectLine.Web.csproj -c Release -o /app/build

FROM build AS publish
WORKDIR /src/Itminus.InDirectLine.Web
RUN dotnet publish Itminus.InDirectLine.Web.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Itminus.InDirectLine.Web.dll"]
CMD [ "--urls", "http://0.0.0.0:3000" ]
