FROM mcr.microsoft.com/dotnet/core/sdk:3.1

COPY . /src

RUN dotnet publish /src/Cognite.Jetfire.Cli/Cognite.Jetfire.Cli.csproj --output /app --configuration Release

ENTRYPOINT [ "/app/Cognite.Jetfire.Cli" ]
