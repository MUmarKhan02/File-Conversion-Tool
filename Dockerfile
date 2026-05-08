FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["FileConversionTool_Web/FileConversionTool.csproj", "FileConversionTool_Web/"]
RUN dotnet restore "FileConversionTool_Web/FileConversionTool.csproj"
COPY . .
WORKDIR "/src/FileConversionTool_Web"
RUN dotnet publish "FileConversionTool.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
ENTRYPOINT ["dotnet", "FileConversionTool.dll"]