FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY PourEvents.sln ./
COPY src/Pours.Domain/Pours.Domain.csproj src/Pours.Domain/
COPY src/Pours.Application/Pours.Application.csproj src/Pours.Application/
COPY src/Pours.Infrastructure/Pours.Infrastructure.csproj src/Pours.Infrastructure/
COPY src/Pours.Api/Pours.Api.csproj src/Pours.Api/

RUN dotnet restore src/Pours.Api/Pours.Api.csproj

COPY src/ src/
RUN dotnet publish src/Pours.Api/Pours.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Non-root user for security
RUN adduser --disabled-password --gecos "" appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "Pours.Api.dll"]
