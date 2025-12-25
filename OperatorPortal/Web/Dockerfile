FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Web/Web.fsproj", "Web/"]
RUN dotnet restore "Web/Web.fsproj"
COPY . .
WORKDIR "/src/Web"
RUN dotnet build "Web.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Web.fsproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web.dll"]
