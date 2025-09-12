FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["Soap.csproj", "./"]
RUN dotnet restore "Soap.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Soap.csproj" -c ${configuration} -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "Soap.csproj" -c ${configuration} -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Soap.dll"]