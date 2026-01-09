FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["project.csproj", "./"]
RUN dotnet restore "project.csproj"

COPY . .
RUN dotnet build "project.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/build .

EXPOSE 80
ENTRYPOINT ["dotnet", "project.dll"]
