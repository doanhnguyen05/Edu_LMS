FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["EcduLMS.Web/EduLMS.Web.csproj", "EcduLMS.Web/"]
RUN dotnet restore "EcduLMS.Web/EduLMS.Web.csproj"

COPY . .
WORKDIR /src/EcduLMS.Web
RUN dotnet publish "EduLMS.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EduLMS.Web.dll"]
